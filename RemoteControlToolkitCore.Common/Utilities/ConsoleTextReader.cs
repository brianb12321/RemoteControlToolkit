using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline;

namespace RemoteControlToolkitCore.Common.Utilities
{
    public class ConsoleTextReader : TextReader
    {
        private ITerminalHandler _terminalHandler;
        private TextReader _textIn;
        private TextWriter _textOut;
        private int _col;
        private int _row;

        public ConsoleTextReader(ITerminalHandler handler, TextReader textIn, TextWriter textOut)
        {
            _terminalHandler = handler;
            _textIn = textIn;
            _textOut = textOut;
            handler.TerminalDimensionsChanged += Handler_TerminalDimensionsChanged;
        }

        private void Handler_TerminalDimensionsChanged(object sender, EventArgs e)
        {
            _col = (int)_terminalHandler.TerminalColumns;
            _row = (int) _terminalHandler.TerminalRows;
        }

        public override string ReadLine()
        {
            StringBuilder sb = new StringBuilder();
            TextReader tr = _textIn;
            TextWriter tw = _textOut;
            _col = (int)_terminalHandler.TerminalColumns;
            _row = (int)_terminalHandler.TerminalRows;
            int HistoryPosition = _terminalHandler.History.Count;
            var cursorDimensions = _terminalHandler.GetCursorPosition();
            int originalCol = int.Parse(cursorDimensions.column);
            int originalRow = int.Parse(cursorDimensions.row);
            char text;
            int cursorPosition = 0;
            //Read from the terminal
            while ((text = (char)tr.Read()) != '\n' && text != '\r')
            {
                //Check conditions
                switch (text)
                {
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
                        tr.Read(buffer, 0, buffer.Length);
                        string code = new string(buffer).Replace("\0", string.Empty);
                        switch (code)
                        {
                            //Cursor Right
                            case "[C":
                                cursorPosition = Math.Min(sb.Length, cursorPosition + 1);
                                break;
                            //Cursor Up
                            case "[A":
                                if (_terminalHandler.History.Count > 0 && HistoryPosition > 0)
                                {
                                    HistoryPosition--;
                                    sb.Clear();
                                    cursorPosition = 0;
                                    string HistoryCommand = _terminalHandler.History[HistoryPosition];
                                    sb.Append(HistoryCommand);
                                    cursorPosition = HistoryCommand.Length;
                                }
                                break;
                            //Cursor Down
                            case "[B":
                                if (_terminalHandler.History.Count > 0 && HistoryPosition < _terminalHandler.History.Count - 1)
                                {
                                    HistoryPosition++;
                                    sb.Clear();
                                    cursorPosition = 0;
                                    string HistoryCommand = _terminalHandler.History[HistoryPosition];
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

                int realStringLength = sb.Length + originalCol;
                int realCursorPosition = (cursorPosition + originalCol);
                int rowsToMove = realStringLength / _col;
                int cursorRowsToMove = realCursorPosition / _col;
                int cellsToMove = (realCursorPosition % _col) - 1;
                //The cursor position is a multiple of the column
                if (cellsToMove == -1)
                {
                    cursorRowsToMove--;
                    cellsToMove = _col;
                }
                //Restore saved cursor position.
                tw.Write($"\u001b[{originalRow};{originalCol}H");
                //Clear lines
                if (rowsToMove > 0)
                {
                    for (int i = 0; i <= rowsToMove; i++)
                    {
                        tw.Write("\u001b[0K");
                        tw.Write("\u001b[B");
                        tw.Write("\u001b[10000000000D");
                    }
                }
                else
                {
                    tw.Write("\u001b[0K");
                }

                tw.Write($"\u001b[{originalRow};{originalCol}H");
                //Print data
                tw.Write(sb.ToString());
                tw.Write($"\u001b[{originalRow};{originalCol}H");
                //Reposition cursor
                if (cursorPosition > 0)
                {
                    if (realCursorPosition > _col)
                    {
                        for (int i = 0; i < cursorRowsToMove; i++)
                        {
                            tw.Write("\u001b[E");

                        }

                        if (cellsToMove > 0)
                        {
                            tw.Write($"\u001b[{cellsToMove}C");
                        }
                    }
                    else
                    {
                        tw.Write("\u001b[" + cursorPosition + "C");
                    }
                }
            }

            tw.WriteLine();
            return sb.ToString();
        }
    }
}