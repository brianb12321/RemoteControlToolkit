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
            int originalCol = int.Parse(_terminalHandler.GetCursorPosition().column);
            int originalRow = int.Parse(_terminalHandler.GetCursorPosition().row);
            string text = string.Empty;
            int cursorPosition = 0;
            //Read from the terminal
            while ((text = tr.ReadLine()) != "\n" && text != "\r")
            {
                //Check conditions
                switch (text)
                {
                    //Handle backspace
                    case "\u007f":
                        if (sb.Length > 0)
                        {
                            sb.Remove(cursorPosition - 1, 1);
                            cursorPosition--;
                        }

                        break;
                    //Cursor Right
                    case "\u001b[C":
                        cursorPosition = Math.Min(sb.Length, cursorPosition + 1);
                        break;
                    //Cursor Up
                    case "\u001b[A":
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
                    case "\u001b[B":
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
                    case "\u001b[1~":
                        cursorPosition = 0;
                        break;
                    //End
                    case "\u001b[4~":
                        cursorPosition = sb.Length;
                        break;
                    case "\u001b[11~":
                        sb.Clear();
                        cursorPosition = 0;
                        break;

                    //Cursor Left
                    case "\u001b[D":
                        if (cursorPosition > 0)
                        {
                            cursorPosition = Math.Max(0, cursorPosition - 1);
                        }
                        break;
                    default:
                        sb.Insert(cursorPosition, text);
                        cursorPosition += text.Length;
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