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
        public List<string> History { get; }

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
            _textOut.WriteLine("\u001b[2J\u001b[;H\u001b[0m");
        }

        public void Bell()
        {
            _textOut.Write("\a");
        }

        private void updateTerminal(StringBuilder sb, int cursorPosition)
        {
            int realStringLength = sb.Length + _originalCol;
            int realCursorPosition = (cursorPosition + _originalCol);
            int rowsToMove = realStringLength / (int)TerminalColumns;
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
                _textOut.Write($"\u001b[{_originalRow};{_originalCol}H");
                //Clear lines
                if (rowsToMove > 0)
                {
                    for (int i = 0; i <= rowsToMove; i++)
                    {
                        _textOut.Write("\u001b[0K");
                        _textOut.Write("\u001b[B");
                        _textOut.Write("\u001b[10000000000D");
                    }
                }
                else
                {
                    _textOut.Write("\u001b[0K");
                }
                UpdateCursorPosition(_originalCol, _originalRow);
                //Print data
                _textOut.Write(sb.ToString());
                UpdateCursorPosition(_originalCol, _originalRow);
                //Reposition cursor
                if (cursorPosition > 0)
                {
                    if (realCursorPosition > (int)TerminalColumns)
                    {
                        for (int i = 0; i < cursorRowsToMove; i++)
                        {
                            _textOut.Write("\u001b[E");
                        }

                        if (cellsToMove > 0)
                        {
                            _textOut.Write($"\u001b[{cellsToMove}C");
                        }
                    }
                    else
                    {
                        _textOut.Write("\u001b[" + cursorPosition + "C");
                    }
                }
            }
        }
        public string ReadLine()
        {
            StringBuilder sb = new StringBuilder();

            //Clean out memory pipe's buffer
            sb.Insert(0, Encoding.UTF8.GetString(_stdIn.GetBuffer()));
            int HistoryPosition = History.Count;
            var cursorDimensions = GetCursorPosition();
            _originalCol = int.Parse(cursorDimensions.column);
            _originalRow = int.Parse(cursorDimensions.row);
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
                        _textOut.Write(text);
                        _textOut.Write("\u001b[1C");
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
                                    string HistoryCommand = History[HistoryPosition];
                                    sb.Append(HistoryCommand);
                                    cursorPosition = HistoryCommand.Length;
                                }
                                break;
                            //Cursor Down
                            case "[B":
                                if (History.Count > 0 && HistoryPosition < History.Count - 1)
                                {
                                    HistoryPosition++;
                                    sb.Clear();
                                    cursorPosition = 0;
                                    string HistoryCommand = History[HistoryPosition];
                                    sb.Append(HistoryCommand);
                                    cursorPosition = HistoryCommand.Length;
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
                        sb.Insert(cursorPosition, text);
                        cursorPosition++;
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

        public void UpdateCursorPosition(int col, int row)
        {
            _textOut.Write($"\u001b[{row};{col}H");
        }

        public (string row, string column) GetCursorPosition()
        {
            //Send code for cursor position.
            _textOut.Write("\u001b[6n");
            char[] buffer = new char[8];
            _textIn.Read(buffer, 0, buffer.Length);
            string newString = new string(buffer);
            //Get rid of \0
            newString = newString.Replace("\0", string.Empty);
            //Get rid of ANSI escape code.
            newString = newString.Replace("\u001b", string.Empty);
            //Get rid of brackets
            newString = newString.Replace("[", string.Empty);
            //Split between the semicolon and R.
            string[] position = newString.Split(';', 'R');
            return (position[0], position[1]);
        }

        

        public void Attach(IInstanceSession owner)
        {
            
        }

        public void Detach(IInstanceSession owner)
        {
            
        }
    }
}