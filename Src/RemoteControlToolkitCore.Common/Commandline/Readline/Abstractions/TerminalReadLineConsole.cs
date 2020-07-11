using System;

namespace RemoteControlToolkitCore.Common.Commandline.Readline.Abstractions
{
    public class TerminalReadLineConsole : IConsole
    {
        public bool PasswordMode { get; set; }

        public int CursorLeft => int.Parse(_handler.GetCursorPosition().column);
        public int CursorTop => int.Parse(_handler.GetCursorPosition().row);
        public int BufferWidth => (int)_handler.TerminalColumns;
        public int BufferHeight => (int) _handler.TerminalRows;

        private readonly ITerminalHandler _handler;
        public TerminalReadLineConsole(ITerminalHandler handler)
        {
            _handler = handler;
        }
        public void SetCursorPosition(int left, int top)
        {
            _handler.UpdateCursorPosition(left, top);
        }

        public void SetBufferSize(int width, int height)
        {
            _handler.ResizeWindow((uint)width, (uint)height);
        }

        public void Write(string value)
        {
            _handler.TerminalOut.Write(value);
        }

        public void WriteLine(string value)
        {
            _handler.TerminalOut.WriteLine(value);
        }

        public ConsoleKeyInfo ReadKey()
        {
            char character = (char)_handler.TerminalIn.Read();
            switch (character)
            {
                case '\r':
                    return new ConsoleKeyInfo(character, ConsoleKey.Enter, false, false, false);
                //case (char)26:
                //    quit = true;
                //    insertCharacter(sb, ref cursorPosition, text.ToString());
                //    cursorPosition = sb.Length;
                //    break;
                //Handle backspace
                case '\u007f':
                    return new ConsoleKeyInfo(character, ConsoleKey.Backspace, false, false, false);
                case '8':
                    return new ConsoleKeyInfo(character, ConsoleKey.D8, false, false, false);

                case '\u001b':
                    char[] buffer = new char[8];
                    _handler.TerminalIn.Read(buffer, 0, buffer.Length);
                    string code = new string(buffer).Replace("\0", string.Empty);
                    switch (code)
                    {
                        //Cursor Right
                        case "[C":
                            return new ConsoleKeyInfo('\0', ConsoleKey.RightArrow, false, false, false);
                        //Home
                        case "[H":
                        case "[1~":
                            return new ConsoleKeyInfo('\0', ConsoleKey.Home, false, false, false);
                        //End
                        case "[F":
                        case "[4~":
                            return new ConsoleKeyInfo('\0', ConsoleKey.End, false, false, false);

                        //Cursor Left
                        case "[D":
                            return new ConsoleKeyInfo('\0', ConsoleKey.LeftArrow, false, false, false);
                        //Cursor up
                        case "[A":
                            return new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false);
                        //Cursor down
                        case "[B":
                            return new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false);
                        default:
                            return new ConsoleKeyInfo(character, ConsoleKey.Escape, false, false, false);
                    }

                default:
                    if (Enum.TryParse<ConsoleKey>(character.ToString().ToUpper(), true, out ConsoleKey result))
                    {
                        return new ConsoleKeyInfo(character, result, false, false, false);
                    }
                    else return new ConsoleKeyInfo(character, ConsoleKey.A, false, false, false);
            }
        }
    }
}