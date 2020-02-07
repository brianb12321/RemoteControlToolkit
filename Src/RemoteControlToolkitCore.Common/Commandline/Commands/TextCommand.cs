using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
    [PluginModule(Name = "text", ExecutingSide = NetworkSide.Server | NetworkSide.Proxy)]
    [CommandHelp("RCT's own text editor.")]
    public class TextCommand : RCTApplication
    {
        private ILogger<TextCommand> _logger;
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
        private List<EditorSyntax> _syntaxDatabase;
        private EditorSyntax _currentSyntax;
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
            public editorHighlight[] HL;
        }
        private class EditorSyntax
        {
            public string FileType { get; set; }

            public highlightFlags Flags { get; set; }
            public string[] FileMatch { get; set; }
            public string[] Keywords { get; set; }
            public string SingleLineCommentStart { get; set; }
        };

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
        enum editorHighlight
        {
            HL_NORMAL = 0,
            HL_NUMBER,
            HL_STRING,
            HL_COMMENT,
            HL_KEYWORD1,
            HL_KEYWORD2,
        };
        [Flags]
        enum highlightFlags
        {
            HL_HIGHLIGHT_NUMBERS = 1,
            HL_HIGHLIGHT_STRINGS = 2
        }


        public override string ProcessName => "Text";
        void editorUpdateSyntax(TextRow row)
        {
            row.HL = new editorHighlight[row.Render.Length];
            if (_currentSyntax == null) return;
            string scs = _currentSyntax.SingleLineCommentStart;
            int scs_len = scs.Length >= 1 ? scs.Length : 0;
            string[] keywords = _currentSyntax.Keywords;
            bool prev_sep = true;
            int in_string = 0;
            for (int i = 0; i < row.HL.Length; i++)
            {
                row.HL[i] = editorHighlight.HL_NORMAL;
            }
            int j = 0;
            while (j < row.Render.Length)
            {
                char c = row.Render[j];
                editorHighlight prev_hl = (j > 0) ? row.HL[j - 1] : editorHighlight.HL_NORMAL;
                if (scs_len >= 1 && in_string <= 0)
                {
                    if (c.ToString() == scs)
                    {
                        for (int commentCount = j; commentCount < row.Render.Length; commentCount++)
                        {
                            row.HL[commentCount] = editorHighlight.HL_COMMENT;
                        }
                        break;
                    }
                }
                if (_currentSyntax.Flags.HasFlag(highlightFlags.HL_HIGHLIGHT_STRINGS))
                {
                    if (in_string >= 1)
                    {
                        row.HL[j] = editorHighlight.HL_STRING;
                        if (c == '\\' && j + 1 < row.Render.Length)
                        {
                            row.HL[j + 1] = editorHighlight.HL_STRING;
                            j += 2;
                            continue;
                        }
                        if (c == in_string) in_string = 0;
                        j++;
                        prev_sep = true;
                        continue;
                    }
                    else
                    {
                        if (c == '"' || c == '\'')
                        {
                            in_string = c;
                            row.HL[j] = editorHighlight.HL_STRING;
                            j++;
                            continue;
                        }
                    }
                }

                if (_currentSyntax.Flags.HasFlag(highlightFlags.HL_HIGHLIGHT_NUMBERS))
                {
                    if ((char.IsDigit(c) && (prev_sep || prev_hl == editorHighlight.HL_NUMBER)) ||
                        (c == '.' && prev_hl == editorHighlight.HL_NUMBER))
                    {
                        row.HL[j] = editorHighlight.HL_NUMBER;
                        j++;
                        prev_sep = false;
                        continue;
                    }

                    if (prev_sep)
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
                                        row.HL[keywordCounter] =
                                            kw2 ? editorHighlight.HL_KEYWORD2 : editorHighlight.HL_KEYWORD1;
                                    }

                                    j += klen;
                                    break;
                                }
                            }
                        }
                        if (keywords[k] != null)
                        {
                            prev_sep = false;
                            continue;
                        }
                    }

                    prev_sep = is_separator(c);
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
            switch (hl)
            {
                case (int)editorHighlight.HL_NUMBER: return 31;
                case (int)editorHighlight.HL_STRING: return 35;
                case (int)editorHighlight.HL_COMMENT: return 36;
                case (int)editorHighlight.HL_KEYWORD1: return 33;
                case (int)editorHighlight.HL_KEYWORD2: return 32;
                default: return 37;
            }
        }

        string editorPrompt(string prompt, Action<string, char> callback)
        {
            StringBuilder buf = new StringBuilder();
            int buflen = 0;
            while (true)
            {
                editorSetStatusMessage(prompt);
                editorRefreshScreen();
                char[] c = editorReadKey();
                if (c[0] == (char)editorKey.DELETE_KEY || c[0] == (char)editorKey.BACKSPACE)
                {
                    if (buflen != 0) buf[--buflen] = '\0';
                }
                else if (c[0] == '\x1b')
                {
                    editorSetStatusMessage("");
                    callback?.Invoke(buf.ToString(), c[0]);
                    buf = null;
                    return string.Empty;
                }
                else if (c[0] == '\r')
                {
                    if (buflen != 0)
                    {
                        editorSetStatusMessage("");
                        callback?.Invoke(buf.ToString(), c[0]);
                        break;
                    }
                }
                else if (!char.IsControl(c[0]))
                {
                    buf.Insert(++buflen, c[0]);
                }
                callback?.Invoke(buf.ToString(), c[0]);
            }
            return buf.ToString();
        }

        void editorFindCallback(string query, char key)
        {
            if (key == '\r' || key == '\x1b')
            {
                return;
            }
            int i;
            for (i = 0; i < _rows.Count; i++)
            {
                TextRow row = _rows[i];
                int match = row.Render.IndexOf(query, StringComparison.CurrentCulture);
                if (match != -1)
                {
                    _cursorY = i;
                    _cursorX = editorRowRxToCx(row, match - row.Render.Length);
                    _rowOff = _rows.Count;
                    break;
                }
            }
        }
        void editorFind()
        {
            int saved_cx = _cursorX;
            int saved_cy = _cursorY;
            int saved_coloff = _colOff;
            int saved_rowoff = _rowOff;
            string query = editorPrompt("Search: %s (ESC to cancel)", editorFindCallback);
            if (query == string.Empty) return;
            else
            {
                _cursorX = saved_cx;
                _cursorY = saved_cy;
                _colOff = saved_coloff;
                _rowOff = saved_rowoff;
            }
        }
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
        int editorRowRxToCx(TextRow row, int rx)
        {
            int cur_rx = 0;
            int cx;
            for (cx = 0; cx < row.Text.Length; cx++)
            {
                if (row.Text[cx] == '\t')
                    cur_rx += (TAB_STOP - 1) - (cur_rx % TAB_STOP);
                cur_rx++;
                if (cur_rx > rx) return cx;
            }
            return cx;
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

        char[] editorReadKey()
        {
            char[] buffer = new char[4];
            _process.In.Read(buffer, 0, buffer.Length);
            return buffer;
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
                                c = (char) editorKey.HOME_KEY;
                                break;
                            case '4':
                                c = (char) editorKey.END_KEY;
                                break;
                            case '3':
                                c = (char) editorKey.DELETE_KEY;
                                break;
                        }

                        _process.In.Read();
                    }
                    else
                    {
                        switch (buffer)
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
                        string c = _rows[fileRow].Render.Substring(_colOff);
                        editorHighlight[] hl;
                        hl = _rows[fileRow].HL.Skip(_colOff).ToArray();
                        int current_color = -1;
                        int j;
                        for (j = 0; j < len; j++)
                        {
                            if (char.IsControl(c[j]))
                            {
                                char sym = (c[j] <= 26) ? (char)('@' + c[j]) : '?';
                                _buffer.Append("\u001b[7m");
                                _buffer.Append(sym);
                                _buffer.Append("\u001b[m");
                                if (current_color != -1)
                                {
                                    _buffer.Append($"\u001b[{current_color}m");
                                }
                            }
                            else if (hl[j] == editorHighlight.HL_NORMAL)
                            {
                                if (current_color != -1)
                                {
                                    _buffer.Append("\u001b[39m");
                                    current_color = -1;
                                }
                                _buffer.Append(c[j], 1);
                            }
                            else
                            {
                                int color = editorSyntaxToColor((int)hl[j]);
                                if (color != current_color)
                                {
                                    current_color = color;
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
        private void editorOpen(string file, RCTProcess currentProc)
        {
            UPath path = new UPath(file);
            if(!path.IsAbsolute) path = UPath.Combine(currentProc.WorkingDirectory, path);
            _rows = new List<TextRow>();
            StreamReader sr = new StreamReader(_fileSystem.OpenFile(path, FileMode.OpenOrCreate, FileAccess.Read));
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
        public override CommandResponse Execute(CommandRequest args, RCTProcess context, CancellationToken token)
        {
            try
            {
                _syntaxDatabase = new List<EditorSyntax>
                {
                    new EditorSyntax()
                    {
                        FileType = "Python",
                        FileMatch = new[] {".py"},
                        SingleLineCommentStart = "#",
                        Keywords = new[]
                        {
                            "switch", "if", "while", "for", "break", "continue", "return", "elif", "import",
                            "static", "enum", "class", "case", "from", "void|", null
                        },
                        Flags = highlightFlags.HL_HIGHLIGHT_NUMBERS | highlightFlags.HL_HIGHLIGHT_STRINGS
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
            _logger = kernel.GetService<ILogger<TextCommand>>();
        }
    }
}