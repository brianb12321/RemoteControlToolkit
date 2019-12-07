using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.ApplicationSystem;

namespace RemoteControlToolkitCore.Common.Commandline
{
    /// <summary>
    /// Provides helper methods for working with a terminal.
    /// </summary>
    public class TerminalHandler : ITerminalHandler
    {
        private RCTProcess _process;
        public List<string> History { get; }

        public TerminalHandler()
        {
            History = new List<string>();
        }

        public string ReadLine()
        {
            StringBuilder sb = new StringBuilder();
            TextReader tr = _process.In;
            TextWriter tw = _process.Out;
            int col = int.Parse(_process.EnvironmentVariables["TERMINAL_COLUMNS"]);
            int row = int.Parse(_process.EnvironmentVariables["TERMINAL_ROWS"]);
            int HistoryPosition = History.Count;
            int originalCol = int.Parse(GetCursorPosition().column);
            int originalRow = int.Parse(GetCursorPosition().row);
            string c;
            int cursorPosition = 0;
            //Read from the terminal
            while ((c = char.ConvertFromUtf32(tr.Read())) != "\n" && c != "\r")
            {
                //Check conditions
                switch (c)
                {
                    //Handle backspace
                    case "\u007f":
                        if (sb.Length > 0)
                        {
                            sb.Remove(cursorPosition - 1, 1);
                            cursorPosition--;
                        }

                        break;
                    case "\u001b":
                        char[] chars = new char[4];
                        tr.Read(chars, 0, chars.Length);
                        string charString = new string(chars).Replace("\0", string.Empty);
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
                            //Down Arrow
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
                            //F1
                            case "[11~":
                                sb.Clear();
                                cursorPosition = 0;
                                break;
                        }

                        break;
                    default:
                        sb.Insert(cursorPosition, c);
                        cursorPosition++;
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

        public void Clear()
        {
            _process.Out.WriteLine("\u001b[2J\u001b[;H\u001b[0m");
        }

        public void Bell()
        {
            _process.Out.Write("\a");
        }

        public (string row, string column) GetCursorPosition()
        {
            TextWriter tw = _process.Out;
            TextReader tr = _process.In;
            //Send code for cursor position.
            tw.Write("\u001b[6n");
            char[] buffer = new char[8];
            tr.Read(buffer, 0, buffer.Length);
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

        public void Attach(RCTProcess owner)
        {
            _process = owner;
        }

        public void Detach(RCTProcess owner)
        {
            
        }
    }
}