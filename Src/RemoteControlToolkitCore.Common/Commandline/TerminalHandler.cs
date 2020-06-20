﻿using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Text;
using RemoteControlToolkitCore.Common.Commandline.TerminalExtensions;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.NSsh.Packets.Channel.RequestPayloads;

namespace RemoteControlToolkitCore.Common.Commandline
{
    public class TerminalHandler : ITerminalHandler
    {
        
        public IExtensionCollection<ITerminalHandler> Extensions { get; }
        public PseudoTerminalPayload InitialTerminalConfig { get; }
        public PseudoTerminalMode TerminalModes { get; }
        public TextWriter TerminalOut => _textOut;
        public TextReader TerminalIn => _textIn;
        public MemoryStream RawTerminalIn => _stdIn;
        public event EventHandler TerminalDimensionsChanged;
        public event EventHandler ReadLineInvoked;
        public event EventHandler<string> ReadLineCompleted;
        private uint _terminalRows = 36;
        private uint _terminalColumns = 130;
        private uint _scrollOffset;
        private readonly uint _maxChars = 272;
        private int _cursorX;
        private int _cursorY;
        private readonly StringBuilder _renderBuffer = new StringBuilder();
        private readonly Dictionary<string, KeyBindingDelegate> _keyBindings;
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

        private readonly MemoryStream _stdIn;
        private readonly TextWriter _textOut;
        private readonly StreamReader _textIn;
        private int _originalCol;

        private int OriginalRow => _originalRow - (int)_scrollOffset;

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
            Extensions = new ExtensionCollection<ITerminalHandler>(this);
            _keyBindings = new Dictionary<string, KeyBindingDelegate>
            {
                {
                    // ReSharper disable once RedundantAssignment
                    "\u001b[11~", (StringBuilder sb, StringBuilder renderBuffer, ref int cursorPosition) =>
                    {
                        sb.Clear();
                        cursorPosition = 0;
                        return true;
                    }
                }
            };
            //Add History functionality.
            Extensions.Add(new TerminalHistory());
        }

        public void Clear()
        {
            _textOut.Write("\u001b[2J\u001b[;H\u001b[0m");
            if(TerminalModes.ClearScrollbackOnClear) _textOut.Write("\u001b[3J");
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
                _renderBuffer.Append(UpdateCursorPosition(_originalCol, OriginalRow, true));
                //Clear lines
                _renderBuffer.Append(ClearScreenCursorDown(true));
                _renderBuffer.Append(UpdateCursorPosition(_originalCol, OriginalRow, true));
                _renderBuffer.Append(sb);
                _renderBuffer.Append(UpdateCursorPosition(_originalCol, OriginalRow, true));
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
            ReadLineInvoked?.Invoke(this, EventArgs.Empty);
            var (row, column) = GetCursorPosition();
            _originalCol = int.Parse(column);
            _cursorX = _originalCol;
            _scrollOffset = 0;
            _originalRow = int.Parse(row);
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
                    case (char)26:
                        quit = true;
                        insertCharacter(sb, ref cursorPosition, text.ToString());
                        cursorPosition = sb.Length;
                        break;
                    //Handle backspace
                    case '\u007f':
                        if (sb.Length > 0 && cursorPosition > 0)
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
                            //Home
                            case "[H":
                            case "[1~":
                                cursorPosition = 0;
                                break;
                            //End
                            case "[F":
                            case "[4~":
                                cursorPosition = sb.Length;
                                break;

                            //Cursor Left
                            case "[D":
                                if (cursorPosition > 0)
                                {
                                    cursorPosition = Math.Max(0, cursorPosition - 1);
                                }
                                break;
                            default:
                                if (_keyBindings.ContainsKey($"\u001b{code}"))
                                {
                                    bool consume = _keyBindings[$"\u001b{code}"](sb, _renderBuffer, ref cursorPosition);
                                    if (!consume) insertCharacter(sb, ref cursorPosition, $"\u001b{code}");
                                    break;
                                }
                                else break;
                        }
                        break;

                    default:
                        if (_keyBindings.ContainsKey(text.ToString()))
                        {
                            bool consume = _keyBindings[text.ToString()](sb, _renderBuffer, ref cursorPosition);
                            if (!consume) insertCharacter(sb, ref cursorPosition, text.ToString());
                        }
                        else insertCharacter(sb, ref cursorPosition, text.ToString());
                        break;
                }
                updateTerminal(sb, cursorPosition);
                if (quit) break;
            }
            _textOut.WriteLine();
            ReadLineCompleted?.Invoke(this, sb.ToString());
            return sb.ToString();
        }

        private void insertCharacter(StringBuilder sb, ref int cursorPosition, string text)
        {
            if (sb.Length <= _maxChars)
            {
                sb.Insert(cursorPosition, text);
                cursorPosition++;
            }
        }
        public string ReadToEnd()
        {
            StringBuilder sb = new StringBuilder();
            string text;
            while (string.IsNullOrWhiteSpace(text = ReadLine()) || text[0] != (char)26)
            {
                sb.Append(text);
            }

            return sb.ToString();
        }

        public char Read()
        {
            _renderBuffer.Clear();
            var character = _textIn.Read();
            if (TerminalModes.ECHO)
            {
                _renderBuffer.Append((char)character);
                _renderBuffer.Append("\u001b[1C");
                _textOut.Write(_renderBuffer.ToString());
            }

            return (char)character;
        }

        public void BindKey(string key, KeyBindingDelegate function)
        {
            _keyBindings.Add(key, function);
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
            if (writeCode)
            {
                if (col == 0 && row == 0) return "\u001b[;H";
                else return $"\u001b[{row};{col}H";
            }
            else
            {
                if(col == 0 && row == 0) _textOut.Write("\u001b[;H");
                else _textOut.Write($"\u001b[{row};{col}H");
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

        public void SetForeground(ConsoleColor color)
        {
            switch(color)
            {
                case ConsoleColor.Black:
                    SetDisplayMode(30);
                    break;
                case ConsoleColor.Blue:
                    SetDisplayMode(34);
                    break;
                case ConsoleColor.Cyan:
                    SetDisplayMode(36);
                    break;
                case ConsoleColor.Magenta:
                    SetDisplayMode(35);
                    break;
                case ConsoleColor.Red:
                    SetDisplayMode(31);
                    break;
                case ConsoleColor.Green:
                    SetDisplayMode(32);
                    break;
                case ConsoleColor.White:
                    SetDisplayMode(37);
                    break;
                case ConsoleColor.Yellow:
                    SetDisplayMode(33);
                    break;
            }
        }
        public void SetBackground(ConsoleColor color)
        {
            switch (color)
            {
                case ConsoleColor.Black:
                    SetDisplayMode(40);
                    break;
                case ConsoleColor.Blue:
                    SetDisplayMode(44);
                    break;
                case ConsoleColor.Cyan:
                    SetDisplayMode(46);
                    break;
                case ConsoleColor.Magenta:
                    SetDisplayMode(45);
                    break;
                case ConsoleColor.Red:
                    SetDisplayMode(41);
                    break;
                case ConsoleColor.Green:
                    SetDisplayMode(42);
                    break;
                case ConsoleColor.White:
                    SetDisplayMode(47);
                    break;
                case ConsoleColor.Yellow:
                    SetDisplayMode(43);
                    break;
            }
        }

        public void HideCursor()
        {
            _textOut.Write("\u001b[?25l");
        }

        public void ShowCursor()
        {
            _textOut.WriteLine($"\u001b[?25h");
        }

        public void ResizeWindow(int column, int row)
        {
            _textOut.Write($"\x1B[8; {row}; {column}t");
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