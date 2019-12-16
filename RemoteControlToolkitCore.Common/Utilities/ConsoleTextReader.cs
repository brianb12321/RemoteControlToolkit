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

        public ConsoleTextReader(ITerminalHandler handler, TextReader textIn, TextWriter textOut)
        {
            _terminalHandler = handler;
            _textIn = textIn;
            _textOut = textOut;
        }

        public override string ReadLine()
        {
            StringBuilder sb = new StringBuilder();
            TextReader tr = _textIn;
            TextWriter tw = _textOut;
            int col = _terminalHandler.TerminalColumns;
            int row = _terminalHandler.TerminalRows;
            int HistoryPosition = _terminalHandler.History.Count;
            int originalCol = int.Parse(_terminalHandler.GetCursorPosition().column);
            int originalRow = int.Parse(_terminalHandler.GetCursorPosition().row);
            char[] c = new char[1024];
            int cursorPosition = 0;
            //Read from the terminal
            while ((char.ConvertFromUtf32(tr.Read(c, 0, c.Length)) != "\n" && c[0] != '\r'))
            {
                //Check conditions
                switch (c[0])
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
                        string charString = new string(c.Skip(1).ToArray()).Replace("\0", string.Empty);
                        switch (charString)
                        {
                            //Cursor left
                            case "[D":
                                if (cursorPosition > 0)
                                {
                                    cursorPosition = Math.Max(0, cursorPosition - 1);
                                }

                                break;
                            //Cursor Right
                            case "[C":
                                cursorPosition = Math.Min(sb.Length, cursorPosition + 1);
                                break;
                            //Up Arrow
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
                            //Down Arrow
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
                            //F1
                            case "[11~":
                                sb.Clear();
                                cursorPosition = 0;
                                break;
                        }

                        break;
                    default:
                        string newString = new string(c).Replace("\0", string.Empty);
                        sb.Insert(cursorPosition, newString);
                        cursorPosition += newString.Length;
                        break;
                }

                int realStringLength = sb.Length + originalCol;
                int realCursorPosition = (cursorPosition + originalCol);
                int rowsToMove = realStringLength / col;
                int cursorRowsToMove = realCursorPosition / col;
                int cellsToMove = (realCursorPosition % col) - 1;
                //The cursor position is a multiple of the column
                if (cellsToMove == -1)
                {
                    cursorRowsToMove--;
                    cellsToMove = col;
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
                    if (realCursorPosition > col)
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