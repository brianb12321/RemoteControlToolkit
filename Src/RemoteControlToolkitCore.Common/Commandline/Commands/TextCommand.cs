using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Crayon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.VirtualFileSystem;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;

namespace RemoteControlToolkitCore.Common.Commandline.Commands
{
    [Plugin(PluginName = "text")]
    [CommandHelp("RCT's own text editor.")]
    public class TextCommand : RCTApplication
    {
        private IFileSystem _fileSystem;
        private int _cursorX;
        private int _renderX;
        private int _cursorY;
        private int _rowOff;
        private int _colOff;
        private int _quitTimes = QUIT_TIME;
        private bool _dirty;
        private string _statusMessage = string.Empty;
        private TimeSpan _statusTime = TimeSpan.Zero;
        private ITerminalHandler _handler;
        private RctProcess _process;
        private StringBuilder _buffer;
        private List<textRow> _rows;
        private string _fileName = string.Empty;
        private string _filePath = string.Empty;
        private List<editorSyntax> _syntaxDatabase;
        private editorSyntax _currentSyntax;
        private const int TAB_STOP = 8;
        private const int QUIT_TIME = 3;

        private int ScreenRows => (int)_handler.TerminalRows - 2;

        private class textRow
        {
            public string Text { get; set; }
            public string Render { get; set; } = string.Empty;
            public EditorHighlight[] Hl;
        }
        private class editorSyntax
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string FileType { get; set; }

            public HighlightFlags Flags { get; set; }
            public string[] FileMatch { get; set; }
            public string[] Keywords { get; set; }
            public string SingleLineCommentStart { get; set; }
        };

        enum EditorKey
        {
            Backspace = 127,
            ArrowLeft = 1,
            ArrowRight = 2,
            ArrowUp = 3,
            ArrowDown = 4,
            HomeKey = 5,
            EndKey = 6,
            DeleteKey = 7
        };
        enum EditorHighlight
        {
            HlNormal = 0,
            HlNumber,
            HlString,
            HlComment,
            HlKeyword1,
            HlKeyword2,
        };
        [Flags]
        enum HighlightFlags
        {
            HlHighlightNumbers = 1,
            HlHighlightStrings = 2
        }


        public override string ProcessName => "Text";
        void editorUpdateSyntax(textRow row)
        {
            row.Hl = new EditorHighlight[row.Render.Length];
            if (_currentSyntax == null) return;
            string scs = _currentSyntax.SingleLineCommentStart;
            int scsLen = scs.Length >= 1 ? scs.Length : 0;
            string[] keywords = _currentSyntax.Keywords;
            bool prevSep = true;
            int inString = 0;
            for (int i = 0; i < row.Hl.Length; i++)
            {
                row.Hl[i] = EditorHighlight.HlNormal;
            }
            int j = 0;
            while (j < row.Render.Length)
            {
                char c = row.Render[j];
                EditorHighlight prevHl = (j > 0) ? row.Hl[j - 1] : EditorHighlight.HlNormal;
                if (scsLen >= 1 && inString <= 0)
                {
                    if (c.ToString() == scs)
                    {
                        for (int commentCount = j; commentCount < row.Render.Length; commentCount++)
                        {
                            row.Hl[commentCount] = EditorHighlight.HlComment;
                        }
                        break;
                    }
                }
                if (_currentSyntax.Flags.HasFlag(HighlightFlags.HlHighlightStrings))
                {
                    if (inString >= 1)
                    {
                        row.Hl[j] = EditorHighlight.HlString;
                        if (c == '\\' && j + 1 < row.Render.Length)
                        {
                            row.Hl[j + 1] = EditorHighlight.HlString;
                            j += 2;
                            continue;
                        }
                        if (c == inString) inString = 0;
                        j++;
                        prevSep = true;
                        continue;
                    }
                    else
                    {
                        if (c == '"' || c == '\'')
                        {
                            inString = c;
                            row.Hl[j] = EditorHighlight.HlString;
                            j++;
                            continue;
                        }
                    }
                }

                if (_currentSyntax.Flags.HasFlag(HighlightFlags.HlHighlightNumbers))
                {
                    if ((char.IsDigit(c) && (prevSep || prevHl == EditorHighlight.HlNumber)) ||
                        (c == '.' && prevHl == EditorHighlight.HlNumber))
                    {
                        row.Hl[j] = EditorHighlight.HlNumber;
                        j++;
                        prevSep = false;
                        continue;
                    }

                    if (prevSep)
                    {
                        int k;
                        for (k = 0; keywords[k] != null; k++)
                        {
                            int klen = keywords[k].Length;
                            bool kw2 = keywords[k][klen - 1] == '|';
                            if (kw2) klen--;
                            int separatorCount = j + klen;
                            if(row.Render.Length >= klen)
                            {
                                if (row.Render.Substring(j).Contains(keywords[k]) && is_separator((row.Render.Length > separatorCount) ? row.Render[separatorCount] : row.Render[row.Render.Length - 1]))
                                {
                                    for (int keywordCounter = j; keywordCounter < ((row.Render.Length > separatorCount) ? separatorCount : row.Render.Length); keywordCounter++)
                                    {
                                        row.Hl[keywordCounter] =
                                            kw2 ? EditorHighlight.HlKeyword2 : EditorHighlight.HlKeyword1;
                                    }

                                    j += klen;
                                    break;
                                }
                            }
                        }
                        if (keywords[k] != null)
                        {
                            prevSep = false;
                            continue;
                        }
                    }

                    prevSep = is_separator(c);
                    j++;
                }
            }
        }
        private int? strchr(string originalString, char charToSearch)
        {
            int? found = originalString.IndexOf(charToSearch);
            return found > -1 ? found : null;
        }
        
        bool is_separator(char c)
        {
            return char.IsWhiteSpace(c) || c == '\0' || strchr(",.()+-/*=~%<>[];", c) != null;
        }
        int editorSyntaxToColor(int hl)
        {
            return hl switch
            {
                (int) EditorHighlight.HlNumber => 31,
                (int) EditorHighlight.HlString => 35,
                (int) EditorHighlight.HlComment => 36,
                (int) EditorHighlight.HlKeyword1 => 33,
                (int) EditorHighlight.HlKeyword2 => 32,
                _ => 37
            };
        }

        void editorRowInsertChar(textRow row, int at, char c)
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
                _rows.Insert(_cursorY, new textRow() {Text = string.Empty});
                editorUpdateRow(_rows[_cursorY]);
            }
            else
            {
                textRow row = _rows[_cursorY];
                if (_cursorX == row.Text.Length)
                {
                    _rows.Insert(_cursorY + 1, new textRow() { Text = string.Empty });
                    editorUpdateRow(_rows[_cursorY + 1]);
                }
                else
                {
                    if (row.Text.Length < _cursorX) _rows.Insert(_cursorY + 1, new textRow() { Text = string.Empty });
                    else _rows.Insert(_cursorY + 1, new textRow()
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
        void editorFreeRow(textRow row)
        {
            _rows.Remove(row);
        }
        void editorDelRow(int at)
        {
            if (at < 0 || at >= _rows.Count) return;
            editorFreeRow(_rows[at]);
            _dirty = true;
        }
        void editorRowAppendString(textRow row, string s)
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
                _rows.Add(new textRow() {Text = string.Empty});
                _dirty = true;
            }
            editorRowInsertChar(_rows[_cursorY], _cursorX, c);
            _cursorX++;
        }

        private void editorDrawMessageBar()
        {
            _buffer.Append("\u001b[K");
            // ReSharper disable once RedundantAssignment
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

        int editorRowCxToRx(textRow row, int cx)
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

        void editorUpdateRow(textRow row)
        {
            StringBuilder renderBuilder = new StringBuilder();
            row.Render = string.Empty;
            int j;
            if (string.IsNullOrEmpty(row.Text))
            {
                renderBuilder.Append(' ');
                row.Render = renderBuilder.ToString();
            }
            for (j = 0; j < row.Text.Length; j++)
            {
                if (row.Text[j] == '\t')
                {
                }
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
            editorUpdateSyntax(row);
        }
        private void setupRawMode()
        {
            _handler.TerminalModes.SIGINT = false;
            _handler.TerminalModes.ECHO = false;
            _handler.TerminalModes.ICANON = false;
            _handler.TerminalModes.OPOST = false;
            _handler.TerminalModes.SIGTSTP = false;
        }
        private void editorRowDelChar(textRow row, int at)
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
            textRow row = _rows[_cursorY];
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
            char buffer = (char)_process.In.Read();
            char c = (char)0;
            if (buffer == '\u001b')
            {
                buffer = (char) _process.In.Read();
                if (buffer == '[')
                {
                    buffer = (char) _process.In.Read();
                    if (char.IsDigit(buffer))
                    {
                        switch (buffer)
                        {
                            case '1':
                                c = (char) EditorKey.HomeKey;
                                break;
                            case '4':
                                c = (char) EditorKey.EndKey;
                                break;
                            case '3':
                                c = (char) EditorKey.DeleteKey;
                                break;
                        }

                        _process.In.Read();
                    }
                    else
                    {
                        switch (buffer)
                        {
                            case 'A':
                                c = (char)EditorKey.ArrowUp;
                                break;
                            case 'B':
                                c = (char)EditorKey.ArrowDown;
                                break;
                            case 'C':
                                c = (char)EditorKey.ArrowRight;
                                break;
                            case 'D':
                                c = (char)EditorKey.ArrowLeft;
                                break;
                        }
                    }
                }
                else
                {
                    c = '\u001b';
                }
            }
            else c = buffer;
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
                case '\u0015':
                    //editorFind();
                    break;
                case '\u0013':
                    editorSave();
                    break;

                case (char)EditorKey.Backspace:
                case (char)EditorKey.DeleteKey:
                case '\b':
                    if (c == (char)EditorKey.DeleteKey) editorMoveCursor((char)EditorKey.ArrowRight);
                    editorDelChar();
                    break;
                case (char)EditorKey.ArrowLeft:
                case (char)EditorKey.ArrowDown:
                case (char)EditorKey.ArrowRight:
                case (char)EditorKey.ArrowUp:
                case (char)EditorKey.HomeKey:
                case (char)EditorKey.EndKey:
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
            textRow currentRow = (_cursorY >= _rows.Count) ? null : _rows[_cursorY];
            switch (key)
            {
                case (char)EditorKey.ArrowLeft:
                    if (_cursorX != 0)
                        _cursorX--;
                    else if (_cursorY > 0)
                    {
                        _cursorY--;
                        _cursorX = _rows[_cursorY].Text.Length;
                    }
                    break;
                case (char)EditorKey.ArrowRight:
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
                case (char)EditorKey.ArrowUp:
                    if (_cursorY != 0)
                        _cursorY--;
                    break;
                case (char)EditorKey.ArrowDown:
                    if (_cursorY < _rows.Count)
                        _cursorY++;
                    break;
                case (char)EditorKey.HomeKey:
                    _cursorX = 0;
                    break;
                case (char)EditorKey.EndKey:
                    if (_cursorY < _rows.Count)
                        _cursorX = _rows[_cursorY].Text.Length;
                    break;
                case (char)EditorKey.DeleteKey:

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
                        string c = _rows[fileRow].Render.Substring(_colOff);
                        EditorHighlight[] hl;
                        hl = _rows[fileRow].Hl.Skip(_colOff).ToArray();
                        int currentColor = -1;
                        int j;
                        for (j = 0; j < len; j++)
                        {
                            if (char.IsControl(c[j]))
                            {
                                char sym = (c[j] <= 26) ? (char)('@' + c[j]) : '?';
                                _buffer.Append("\u001b[7m");
                                _buffer.Append(sym);
                                _buffer.Append("\u001b[m");
                                if (currentColor != -1)
                                {
                                    _buffer.Append($"\u001b[{currentColor}m");
                                }
                            }
                            else if (hl[j] == EditorHighlight.HlNormal)
                            {
                                if (currentColor != -1)
                                {
                                    _buffer.Append("\u001b[39m");
                                    currentColor = -1;
                                }
                                _buffer.Append(c[j], 1);
                            }
                            else
                            {
                                int color = editorSyntaxToColor((int)hl[j]);
                                if (color != currentColor)
                                {
                                    currentColor = color;
                                    string code = $"\u001b[{color}m";
                                    _buffer.Append(code);
                                }

                                _buffer.Append(c[j]);
                            }
                        }
                        _buffer.Append("\u001b[39m");
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
                _rowOff = _cursorY - ScreenRows + 1;
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
            foreach (textRow row in _rows)
            {
                builder.AppendLine(row.Text);
            }

            return builder.ToString();
        }
        private void editorOpen(string file, RctProcess currentProc)
        {
            UPath path = new UPath(file);
            if(!path.IsAbsolute) path = UPath.Combine(currentProc.WorkingDirectory, path);
            _rows = new List<textRow>();
            StreamReader sr = new StreamReader(_fileSystem.OpenFile(path, FileMode.OpenOrCreate, FileAccess.Read));
            while (!sr.EndOfStream)
            {
                var text = new textRow()
                {
                    Text = sr.ReadLine()
                };
                _rows.Add(text);
                editorUpdateRow(text);
            }
            sr.Close();
            _fileName = _fileSystem.GetFileEntry(path).Name;
            editorSelectSyntaxHighlight();
            _filePath = path.FullName;
        }
        void editorSelectSyntaxHighlight()
        {
            _currentSyntax = null;
            if (string.IsNullOrWhiteSpace(_fileName)) return;
            string ext = Path.GetExtension(_fileName);
            _currentSyntax = _syntaxDatabase.Find(s => s.FileMatch.Any(s2 => s2 == ext));
            int filerow;
            for (filerow = 0; filerow < _rows.Count; filerow++)
            {
                editorUpdateSyntax(_rows[filerow]);
            }
        }
        public override CommandResponse Execute(CommandRequest args, RctProcess context, CancellationToken token)
        {
            try
            {
                _syntaxDatabase = new List<editorSyntax>
                {
                    new editorSyntax()
                    {
                        FileType = "Python",
                        FileMatch = new[] {".py"},
                        SingleLineCommentStart = "#",
                        Keywords = new[]
                        {
                            "switch", "if", "while", "for", "break", "continue", "return", "elif", "import",
                            "static", "enum", "class", "case", "from", "void|", null
                        },
                        Flags = HighlightFlags.HlHighlightNumbers | HighlightFlags.HlHighlightStrings
                    }
                };
                _process = context;
                _handler = context.ClientContext.GetExtension<ITerminalHandler>();
                if (args.Arguments.Length == 1)
                {
                    die("No file specified.");
                    token.ThrowIfCancellationRequested();
                }
                _buffer = new StringBuilder();
                _fileSystem = context.Extensions.Find<IExtensionFileSystem>().GetFileSystem();
                _handler.Clear();
                _handler.UpdateCursorPosition(0, 0);
                setupRawMode();
                editorOpen(args.Arguments[1].ToString(), context);
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
                die($"Error: {e}".Red());
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