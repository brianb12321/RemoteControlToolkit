using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crayon;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.VirtualFileSystem;
using Zio;

namespace RemoteControlToolkitCore.Common.Commandline.Commands
{
    [PluginModule(Name = "text", ExecutingSide = NetworkSide.Server | NetworkSide.Proxy)]
    [CommandHelp("RCT's own text editor.")]
    public class TextCommand : RCTApplication
    {
        private IFileSystem _fileSystem;
        private int _cursorX = 0;
        private int _renderX = 0;
        private int _cursorY = 0;
        private int _rowOff = 0;
        private int _colOff = 0;
        private int _quitTimes = QUIT_TIME;
        private bool _dirty = false;
        private string _statusMessage = string.Empty;
        private TimeSpan _statusTime = TimeSpan.Zero;
        private ITerminalHandler _handler;
        private RCTProcess _process;
        private StringBuilder _buffer;
        private List<TextRow> _rows;
        private string _fileName = string.Empty;
        private string _filePath = string.Empty;
        private const int TAB_STOP = 8;
        private const int QUIT_TIME = 3;

        private int ScreenRows
        {
            get => (int)_handler.TerminalRows - 2;
        }
        private class TextRow
        {
            public string Text { get; set; }
            public string Render { get; set; } = string.Empty;
        }

        enum editorKey
        {
            BACKSPACE = 127,
            ARROW_LEFT = 1,
            ARROW_RIGHT = 2,
            ARROW_UP = 3,
            ARROW_DOWN = 4,
            HOME_KEY = 5,
            END_KEY = 6,
            DELETE_KEY = 7
        };


        public override string ProcessName => "Text";

        void editorRowInsertChar(TextRow row, int at, char c)
        {
            if (at < 0 || at > row.Text.Length) at = row.Text.Length;
            row.Text = row.Text.Insert(at, c.ToString());
            editorUpdateRow(row);
            _dirty = true;
        }
        private void editorInsertNewline()
        {
            if (_cursorX == 0)
            {
                _rows.Insert(_cursorY, new TextRow() {Text = string.Empty});
                editorUpdateRow(_rows[_cursorY]);
            }
            else
            {
                TextRow row = _rows[_cursorY];
                if (_cursorX == row.Text.Length)
                {
                    _rows.Insert(_cursorY + 1, new TextRow() { Text = string.Empty });
                    editorUpdateRow(_rows[_cursorY + 1]);
                }
                else
                {
                    if (row.Text.Length < _cursorX) _rows.Insert(_cursorY + 1, new TextRow() { Text = string.Empty });
                    else _rows.Insert(_cursorY + 1, new TextRow()
                    {
                        Text = row.Text.Substring(_cursorX)
                    });
                    editorUpdateRow(_rows[_cursorY + 1]);
                    if (row.Text.Length >= _cursorX) row.Text = row.Text.Substring(0, _cursorX + 1);
                    editorUpdateRow(row);
                }
            }

            _dirty = true;
            _cursorY++;
            _cursorX = 0;
        }
        void editorFreeRow(TextRow row)
        {
            _rows.Remove(row);
        }
        void editorDelRow(int at)
        {
            if (at < 0 || at >= _rows.Count) return;
            editorFreeRow(_rows[at]);
            _dirty = true;
        }
        void editorRowAppendString(TextRow row, string s)
        {
            row.Text = row.Text.Insert((row.Text.Length - 1 >= 0) ? row.Text.Length - 1 : 0, s);
            editorUpdateRow(row);
            _dirty = true;
        }

        /*** editor operations ***/
        void editorInsertChar(char c)
        {
            if (_cursorY == _rows.Count)
            {
                _rows.Add(new TextRow() {Text = string.Empty});
                _dirty = true;
            }
            editorRowInsertChar(_rows[_cursorY], _cursorX, c);
            _cursorX++;
        }

        void editorDrawMessageBar()
        {
            _buffer.Append("\u001b[K");
            string newMessage = _statusMessage;
            int msglen = _statusMessage.Length;
            if (msglen > _handler.TerminalColumns) msglen = (int)_handler.TerminalColumns;
            if (_statusMessage.Length < msglen) newMessage = _statusMessage.Substring(0, _statusMessage.Length);
            else newMessage = _statusMessage.Substring(0, msglen);
            if (msglen != 0 && DateTime.Now.TimeOfDay.Ticks - _statusTime.Ticks < 5)
                _buffer.Append(newMessage);
        }
        void editorSetStatusMessage(string message)
        {
            _statusMessage = message;
            _statusTime = DateTime.Now.TimeOfDay;
        }
        void editorDrawStatusBar()
        {
            _buffer.Append("\u001b[7m");
            int len = 0;
            int rlen = 0;
            string position = string.Empty;
            if (string.IsNullOrWhiteSpace(_fileName)) _buffer.Append("[No Name]");
            else
            {
                _fileName += (_dirty) ? " (modified)" : string.Empty;
                len = (_fileName.Length > 22) ? 22 : _fileName.Length;
                position = $"{_cursorY + 1}/{_rows.Count}";
                rlen = position.Length;
                if (len > _handler.TerminalColumns) len = (int)_handler.TerminalColumns;
                _buffer.Append(_fileName.Substring(0, len));
            }
            while (len < _handler.TerminalColumns)
            {
                if (_handler.TerminalColumns - len == rlen)
                {
                    _buffer.Append(position);
                    break;
                }
                else
                {
                    _buffer.Append(' ');
                    len++;
                }
            }
            _buffer.Append("\u001b[m");
            _buffer.Append("\r\n");
        }
        int editorRowCxToRx(TextRow row, int cx)
        {
            int rx = 0;
            int j;
            for (j = 0; j < cx; j++)
            {
                if (row.Text[j] == '\t')
                    rx += (TAB_STOP - 1) - (rx % TAB_STOP);
                rx++;
            }
            return rx;
        }

        void editorUpdateRow(TextRow row)
        {
            StringBuilder renderBuilder = new StringBuilder();
            row.Render = string.Empty;
            int j;
            int tabs = 0;
            if (string.IsNullOrEmpty(row.Text))
            {
                renderBuilder.Append(' ');
                row.Render = renderBuilder.ToString();
                return;
            }
            for (j = 0; j < row.Text.Length; j++)
            {
                if (row.Text[j] == '\t') tabs++;
            }

            int idx = 0;
            for (j = 0; j < row.Text.Length; j++)
            {
                if (row.Text[j] == '\t')
                {
                    renderBuilder.Append(' ');
                    while (idx % TAB_STOP != 0)
                    {
                        renderBuilder.Append(' ');
                        idx++;
                    }
                }
                else
                {
                    renderBuilder.Append(row.Text[j]);
                }
            }

            row.Render = renderBuilder.ToString();
        }
        private void setupRawMode()
        {
            _handler.TerminalModes.SIGINT = false;
            _handler.TerminalModes.ECHO = false;
            _handler.TerminalModes.ICANON = false;
            _handler.TerminalModes.OPOST = false;
            _handler.TerminalModes.SIGTSTP = false;
        }
        private void editorRowDelChar(TextRow row, int at)
        {
            if (at < 0 || at >= row.Text.Length) return;
            if (at == row.Text.Length - 1) row.Text = row.Text.Remove(at);
            else row.Text = row.Text.Remove(at, 1);
            editorUpdateRow(row);
            _dirty = true;
        }
        void editorDelChar()
        {
            if (_cursorY == _rows.Count) return;
            if (_cursorX == 0 && _cursorY == 0) return;
            TextRow row = _rows[_cursorY];
            if (_cursorX > 0)
            {
                editorRowDelChar(row, _cursorX - 1);
                _cursorX--;
            }
            else
            {
                _cursorX = _rows[_cursorY - 1].Text.Length;
                editorRowAppendString(_rows[_cursorY - 1], row.Text);
                editorDelRow(_cursorY);
                _cursorY--;
            }
        }
        /*** input ***/
        void editorProcessKeypress()
        {
            char[] buffer = new char[4];
            char c = (char) 0;
            _process.In.Read(buffer, 0, buffer.Length);
            if (buffer[0] == '\u001b')
            {
                if (buffer[1] == '[')
                {
                    if (char.IsDigit(buffer[2]))
                    {
                        switch (buffer[2])
                        {
                            case '1':
                                c = (char) editorKey.HOME_KEY;
                                break;
                            case '4':
                                c = (char) editorKey.END_KEY;
                                break;
                            case '3':
                                c = (char) editorKey.DELETE_KEY;
                                break;
                        }
                    }
                    else
                    {
                        switch (buffer[2])
                        {
                            case 'A':
                                c = (char)editorKey.ARROW_UP;
                                break;
                            case 'B':
                                c = (char)editorKey.ARROW_DOWN;
                                break;
                            case 'C':
                                c = (char)editorKey.ARROW_RIGHT;
                                break;
                            case 'D':
                                c = (char)editorKey.ARROW_LEFT;
                                break;
                        }
                    }
                }
                else
                {
                    c = '\u001b';
                }
            }
            else c = buffer[0];
            switch (c)
            {
                case '\r':
                    editorInsertNewline();
                    break;
                case '\f':
                case '\u001b':
                    break;
                case '\u0011':
                    if (_dirty && _quitTimes > 0)
                    {
                        editorSetStatusMessage($"WARNING!!! File has unsaved changes. Press Ctrl-Q {_quitTimes} more times to quit.");
                        _quitTimes--;
                        return;
                    }
                    resetMode();
                    _process.Out.Write("\u001b[2J");
                    _process.Out.Write("\u001b[H");
                    _process.Close();
                    break;
                case '\u0013':
                    editorSave();
                    break;

                case (char)editorKey.BACKSPACE:
                case (char)editorKey.DELETE_KEY:
                case '\b':
                    if (c == (char)editorKey.DELETE_KEY) editorMoveCursor((char)editorKey.ARROW_RIGHT);
                    editorDelChar();
                    break;
                case (char)editorKey.ARROW_LEFT:
                case (char)editorKey.ARROW_DOWN:
                case (char)editorKey.ARROW_RIGHT:
                case (char)editorKey.ARROW_UP:
                case (char)editorKey.HOME_KEY:
                case (char)editorKey.END_KEY:
                    editorMoveCursor(c);
                    break;
                default:
                    editorInsertChar(c);
                    break;
                
            }

            _quitTimes = QUIT_TIME;
        }
        void editorMoveCursor(char key)
        {
            TextRow currentRow = (_cursorY >= _rows.Count) ? null : _rows[_cursorY];
            switch (key)
            {
                case (char)editorKey.ARROW_LEFT:
                    if (_cursorX != 0)
                        _cursorX--;
                    else if (_cursorY > 0)
                    {
                        _cursorY--;
                        _cursorX = _rows[_cursorY].Text.Length;
                    }
                    break;
                case (char)editorKey.ARROW_RIGHT:
                    if (currentRow != null && _cursorX < currentRow.Text.Length)
                    {
                        _cursorX++;
                    }
                    else if (currentRow != null && _cursorX == currentRow.Text.Length)
                    {
                        _cursorY++;
                        _cursorX = 0;
                    }
                    break;
                case (char)editorKey.ARROW_UP:
                    if (_cursorY != 0)
                        _cursorY--;
                    break;
                case (char)editorKey.ARROW_DOWN:
                    if (_cursorY < _rows.Count)
                        _cursorY++;
                    break;
                case (char)editorKey.HOME_KEY:
                    _cursorX = 0;
                    break;
                case (char)editorKey.END_KEY:
                    if (_cursorY < _rows.Count)
                        _cursorX = _rows[_cursorY].Text.Length;
                    break;
                case (char)editorKey.DELETE_KEY:

                    break;
            }
            currentRow = (_cursorY >= _rows.Count) ? null : _rows[_cursorY];
            int rowLen = currentRow?.Text.Length ?? 0;
            if (_cursorX > rowLen)
            {
                _cursorX = rowLen;
            }
        }


        private void editorRefreshScreen()
        {
            editorScroll();
            _buffer.Append("\u001b[?25l");
            _buffer.Append("\u001b[H");
            editorDrawRows();
            editorDrawStatusBar();
            editorDrawMessageBar();
            _buffer.Append($"\u001b[{(_cursorY - _rowOff) + 1};{(_renderX - _colOff) + 1}H");
            _buffer.Append("\u001b[?25h");
            _process.Out.Write(_buffer.ToString());
            _buffer.Clear();
        }
        private void editorDrawRows()
        {
            int y;
            for (y = 0; y < ScreenRows; y++)
            {
                int fileRow = y + _rowOff;
                if (fileRow < _rows.Count)
                {
                    int len = _rows[fileRow].Render.Length - _colOff;
                    if (len < 0)
                    {
                        _buffer.Append(' ');
                    }
                    else
                    {
                        if (len > _handler.TerminalColumns) len = (int)_handler.TerminalColumns;
                        _buffer.Append(_rows[fileRow].Render.Substring(_colOff, len));
                    }
                }
                else
                {
                    _buffer.Append("~");
                }
                _buffer.Append("\u001b[K");
                _buffer.Append("\r\n");
            }
        }

        private void editorScroll()
        {
            _renderX = 0;
            if (_cursorY < _rows.Count)
            {
                _renderX = editorRowCxToRx(_rows[_cursorY], _cursorX);
            }
            if (_cursorY < _rowOff)
            {
                _rowOff = _cursorY;
            }
            if (_cursorY >= _rowOff + ScreenRows)
            {
                _rowOff = _cursorY - (int)ScreenRows + 1;
            }
            if (_renderX < _colOff)
            {
                _colOff = _renderX;
            }
            if (_renderX >= _colOff + _handler.TerminalColumns)
            {
                _colOff = _renderX - (int)_handler.TerminalColumns + 1;
            }
        }
        private void editorSave()
        {
            try
            {
                if (string.IsNullOrEmpty(_fileName)) return;
                _fileSystem.WriteAllText(_filePath, editorRowsToString());
                editorSetStatusMessage("File saved.");
                _dirty = false;
            }
            catch (IOException ex)
            {
                editorSetStatusMessage($"Cannot save file: {ex.Message}");
            }
        }
        private string editorRowsToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (TextRow row in _rows)
            {
                builder.AppendLine(row.Text);
            }

            return builder.ToString();
        }
        private void editorOpen(string file)
        {
            _rows = new List<TextRow>();
            StreamReader sr = new StreamReader(_fileSystem.OpenFile(file, FileMode.OpenOrCreate, FileAccess.Read));
            while (!sr.EndOfStream)
            {
                var text = new TextRow()
                {
                    Text = sr.ReadLine()
                };
                _rows.Add(text);
                editorUpdateRow(text);
            }
            sr.Close();
            _fileName = _fileSystem.GetFileEntry(file).Name;
            _filePath = file;
        }
        public override CommandResponse Execute(CommandRequest args, RCTProcess context, CancellationToken token)
        {
            try
            {
                _process = context;
                _handler = context.ClientContext.GetExtension<ITerminalHandler>();
                if (args.Arguments.Length == 1)
                {
                    die("No file specified.");
                    token.ThrowIfCancellationRequested();
                }
                _buffer = new StringBuilder();
                _fileSystem = context.ClientContext.GetExtension<IExtensionFileSystem>().FileSystem;
                _handler.Clear();
                _handler.UpdateCursorPosition(0, 0);
                setupRawMode();
                editorOpen(args.Arguments[1].ToString());
                editorSetStatusMessage("HELP: Ctrl-Q = quit, Ctrl-S = save");
                while (!token.IsCancellationRequested)
                {
                    editorRefreshScreen();
                    editorProcessKeypress();
                }
                context.Out.Write("\u001b[2J");
                context.Out.Write("\u001b[H");
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            catch (Exception e)
            {
                die($"Error: {e.Message}".Red());
                token.ThrowIfCancellationRequested();
                return new CommandResponse(CommandResponse.CODE_FAILURE);
            }
        }

        private void resetMode()
        {
            _handler.TerminalModes.SIGINT = true;
            _handler.TerminalModes.ECHO = true;
            _handler.TerminalModes.ICANON = true;
            _handler.TerminalModes.OPOST = true;
            _handler.TerminalModes.SIGTSTP = true;
        }
        private void die(string message)
        {
            resetMode();
            _process.Out.Write("\u001b[2J");
            _process.Out.Write("\u001b[H");
            _process.Error.WriteLine(message.Red());
            _process.Close();
        }

        public override void InitializeServices(IServiceProvider kernel)
        {
            
        }
    }
}