using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.NSsh.Packets.Channel.RequestPayloads;
using RemoteControlToolkitCore.Common.NSsh.Utility;
using RemoteControlToolkitCore.Common.Utilities;

namespace RemoteControlToolkitCore.Common.Commandline
{
    /// <summary>
    /// Provides helper methods for working with a terminal.
    /// </summary>
    public class TerminalHandler : ITerminalHandler
    {
        public PseudoTerminalPayload InitialTerminalConfig { get; }
        public PseudoTerminalMode TerminalModes { get; }
        public event EventHandler TerminalDimensionsChanged;
        private uint _terminalRows = 36;
        private uint _terminalColumns = 130;
        private uint _scrollOffset = 0;
        private uint _maxChars = 272;
        private int _cursorX;
        private int _cursorY;
        private StringBuilder _renderBuffer = new StringBuilder();
        public List<string> History { get; }

        public string TerminalName
        {
            get
            {
                try
                {
                    return InitialTerminalConfig.TerminalType;
                }
                catch (NullReferenceException)
                {
                    return "vt100";
                }
            }
        }

        public uint TerminalRows
        {
            get => _terminalRows;
            set
            {
                _terminalRows = value;
                TerminalDimensionsChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public uint TerminalColumns
        {
            get => _terminalColumns;
            set
            {
                _terminalColumns = value;
                TerminalDimensionsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private MemoryStream _stdIn;
        private Stream _stdOut;
        private TextWriter _textOut;
        private StreamReader _textIn;
        private int _originalCol;

        private int originalRow
        {
            get => _originalRow - (int)_scrollOffset;
            set => _originalRow = value;
        }

        private int _originalRow;
        public TerminalHandler(MemoryStream stdIn, TextWriter stdOut, PseudoTerminalPayload terminalConfig)
        {
            _stdIn = stdIn;
            _textOut = stdOut;
            _textIn = new StreamReader(_stdIn);
            History = new List<string>();
            InitialTerminalConfig = terminalConfig;
            if (InitialTerminalConfig != null)
            {
                _terminalRows = InitialTerminalConfig.TerminalHeight;
                _terminalColumns = InitialTerminalConfig.TerminalWidth;
            }
            TerminalModes = new PseudoTerminalMode();
        }

        public void Clear()
        {
            _textOut.Write("\u001b[2J\u001b[;H\u001b[0m");
        }

        public string ClearScreenCursorDown(bool writeCode = false)
        {
            if (writeCode) return "\u001b[J";
            else
            {
                _textOut.Write("\u001b[J");
                return string.Empty;
            }

        }

        public void ClearRow()
        {
            _textOut.Write("\u001b[K");
        }

        public void Bell()
        {
            _textOut.Write("\a");
        }

        private void updateTerminal(StringBuilder sb, int cursorPosition)
        {
            int realCursorPosition = (cursorPosition + _originalCol);
            int cursorRowsToMove = realCursorPosition / (int)TerminalColumns;
            int cellsToMove = (realCursorPosition % (int)TerminalColumns) - 1;
            //The cursor position is a multiple of the column
            if (cellsToMove == -1)
            {
                cursorRowsToMove--;
                cellsToMove = (int)TerminalColumns;
            }
            if (TerminalModes.ECHO)
            {
                //Restore saved cursor position.
                _renderBuffer.Append(UpdateCursorPosition(_originalCol, originalRow, true));
                //Clear lines
                _renderBuffer.Append(ClearScreenCursorDown(true));
                _renderBuffer.Append(UpdateCursorPosition(_originalCol, originalRow, true));
                _renderBuffer.Append(sb);
                _renderBuffer.Append(UpdateCursorPosition(_originalCol, originalRow, true));
                //Reposition cursor
                if (cursorPosition > 0)
                {
                    if (realCursorPosition > (int)TerminalColumns)
                    {
                        if(cellsToMove > 0) _cursorX = cellsToMove;
                        _cursorY += cursorRowsToMove - 1;
                        //Check if cursor is at the end of the row and must count the new scrolled coordinates.
                        if (_cursorY == TerminalRows && _cursorX == TerminalColumns)
                        {
                            _scrollOffset += 1;
                        }
                        
                        for (int i = 0; i < cursorRowsToMove; i++)
                        {
                            _renderBuffer.AppendLine();
                        }

                        if (cellsToMove > 0)
                        {
                            _renderBuffer.Append($"\u001b[{_cursorX}C");
                        }
                    }
                    else
                    {
                        _renderBuffer.Append("\u001b[" + cursorPosition + "C");
                        _cursorX = realCursorPosition;
                    }
                }
                _textOut.Write(_renderBuffer.ToString());
                _renderBuffer.Clear();
            }
        }

        private void cleanOutBuffers(StringBuilder sb)
        {
            sb.Insert(0, Encoding.UTF8.GetString(_stdIn.GetBuffer()));
        }

        public string ReadLine()
        {
            StringBuilder sb = new StringBuilder();

            //Clean out memory pipe's buffer
            cleanOutBuffers(sb);
            int HistoryPosition = History.Count;
            var cursorDimensions = GetCursorPosition();
            _originalCol = int.Parse(cursorDimensions.column);
            _cursorX = _originalCol;
            _scrollOffset = 0;
            _originalRow = int.Parse(cursorDimensions.row);
            _cursorY = _originalRow;
            char text;
            int cursorPosition = sb.Length;
            updateTerminal(sb, cursorPosition);
            bool quit = false;
            //Read from the terminal
            while (true)
            {
                text = (char)_textIn.Read();
                if (!TerminalModes.ICANON)
                {
                    if (TerminalModes.ECHO)
                    {
                        _renderBuffer.Append(text);
                        _renderBuffer.Append("\u001b[1C");
                    }
                    return text.ToString();
                }
                //Check conditions
                switch (text)
                {
                    case '\r':
                        quit = true;
                        cursorPosition = sb.Length;
                        break;
                    //Handle backspace
                    case '\u007f':
                        if (sb.Length > 0)
                        {
                            sb.Remove(cursorPosition - 1, 1);
                            cursorPosition--;
                        }

                        break;
                    case '\u001b':
                        char[] buffer = new char[8];
                        _textIn.Read(buffer, 0, buffer.Length);
                        string code = new string(buffer).Replace("\0", string.Empty);
                        switch (code)
                        {
                            //Cursor Right
                            case "[C":
                                cursorPosition = Math.Min(sb.Length, cursorPosition + 1);
                                break;
                            //Cursor Up
                            case "[A":
                                if (History.Count > 0 && HistoryPosition > 0)
                                {
                                    HistoryPosition--;
                                    sb.Clear();
                                    cursorPosition = 0;
                                    string historyCommand = History[HistoryPosition];
                                    byte[] historyCommandBytes = Encoding.UTF8.GetBytes(historyCommand);
                                    //Clean out buffer
                                    _stdIn.GetBuffer();
                                    _renderBuffer.Clear();
                                    _stdIn.Write(historyCommandBytes, 0, historyCommandBytes.Length);
                                }
                                break;
                            //Cursor Down
                            case "[B":
                                if (History.Count > 0 && HistoryPosition < History.Count - 1)
                                {
                                    HistoryPosition++;
                                    sb.Clear();
                                    cursorPosition = 0;
                                    string historyCommand = History[HistoryPosition];
                                    //Clean out buffer
                                    _stdIn.GetBuffer();
                                    _renderBuffer.Clear();
                                    byte[] historyCommandBytes = Encoding.UTF8.GetBytes(historyCommand);
                                    _stdIn.Write(historyCommandBytes, 0, historyCommandBytes.Length);
                                }
                                break;
                            //Home
                            case "[1~":
                                cursorPosition = 0;
                                break;
                            //End
                            case "[4~":
                                cursorPosition = sb.Length;
                                break;
                            case "[11~":
                                sb.Clear();
                                cursorPosition = 0;
                                break;

                            //Cursor Left
                            case "[D":
                                if (cursorPosition > 0)
                                {
                                    cursorPosition = Math.Max(0, cursorPosition - 1);
                                }
                                break;
                        }
                        break;

                    default:
                        if (sb.Length <= _maxChars)
                        {
                            sb.Insert(cursorPosition, text);
                            cursorPosition++;
                        }
                        break;
                }
                updateTerminal(sb, cursorPosition);
                if (quit) break;
            }
            _textOut.WriteLine();
            return sb.ToString();
        }

        public string ReadToEnd()
        {
            StringBuilder sb = new StringBuilder();
            string text = string.Empty;
            while ((text = ReadLine()) != ((char) 26).ToString())
            {
                sb.AppendLine(text);
            }

            return sb.ToString();
        }

        public char Read()
        {
            var character = _textIn.Read();
            if (TerminalModes.ECHO)
            {
                _textOut.Write((char)character);
                _textOut.Write("\u001b[1C");
            }

            return (char)character;
        }

        public int ReadFromPipe(char[] buffer, int offset, int length)
        {
            return _textIn.Read(buffer, offset, length);
        }

        public void UpdateHomePosition(int col, int row)
        {
            _originalCol = col;
            _originalRow = row;
            UpdateCursorPosition(col, row);
        }

        public string UpdateCursorPosition(int col, int row, bool writeCode = false)
        {
            if (writeCode) return $"\u001b[{row};{col}H";
            else
            {
                _textOut.Write($"\u001b[{row};{col}H");
                return string.Empty;
            }
        }

        public void MoveCursorLeft(int count = 1)
        {
            if(count > 0) _textOut.Write($"\u001b[{count}D");
        }

        public void MoveCursorRight(int count = 1)
        {
            _textOut.Write($"\u001b[{count}C");
        }

        public void MoveCursorUp(int count = 1)
        {
            _textOut.Write($"\u001b[{count}A");
        }

        public void MoveCursorDown(int count = 1)
        {
            _textOut.Write($"\u001b[{count}B");
        }

        public void SetDisplayMode(int code)
        {
            _textOut.Write($"\u001b[{code}m");
        }

        public void Reset()
        {
            _textOut.Write("\u001b[c");
        }

        public void ScrollDown()
        {
            _textOut.Write("\u001b[M");
        }

        public void SetTitle(string title)
        {
            _textOut.Write($"\u001b]0;{title}\x7");
        }

        public (string row, string column) GetCursorPosition()
        {
            //Send code for cursor position.
            _textOut.Write("\u001b[6n");
            char escapeChar = (char)_textIn.Read();
            if (escapeChar == '\u001b')
            {
                char[] buffer = new char[9];
                _textIn.Read(buffer, 0, buffer.Length);
                string newString = new string(buffer);
                //Get rid of \0
                newString = newString.Replace("\0", string.Empty);
                //Get rid of brackets
                newString = newString.Replace("[", string.Empty);
                //Split between the semicolon and R.
                string[] position = newString.Split(';', 'R');
                return (position[0], position[1]);
            }
            else
            {
                return ("-1", "-1");
            }
        }

        public void Attach(IInstanceSession owner)
        {
            
        }

        public void Detach(IInstanceSession owner)
        {
            
        }
    }
}