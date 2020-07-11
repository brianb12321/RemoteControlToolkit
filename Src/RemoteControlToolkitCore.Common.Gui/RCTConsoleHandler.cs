using System;
using ConsoleGUI.Api;
using ConsoleGUI.Data;
using ConsoleGUI.Space;
using RemoteControlToolkitCore.Common.Commandline;

namespace RemoteControlToolkitCore.Common.Gui
{
    public class RctConsoleHandler : IConsole
    {
        private readonly ITerminalHandler _handler;
        public RctConsoleHandler(ITerminalHandler handler)
        {
            _handler = handler;
        }
		public Size Size
        {
            get => new Size((int)_handler.TerminalColumns, (int)_handler.TerminalRows);
            set
            {
                _handler.UpdateCursorPosition(0, 0);
                _handler.ResizeWindow((uint)value.Width, (uint)value.Height);
                Initialize();
            }
        }

        public bool KeyAvailable => true;

        public virtual void Initialize()
        {
            _handler.HideCursor();
            _handler.Clear();
        }

        public virtual void OnRefresh()
        {
            _handler.HideCursor();
        }

        public virtual void Write(Position position, in Character character)
        {
            var content = character.Content ?? ' ';
            var foreground = character.Foreground ?? Color.White;
            var background = character.Background ?? Color.Black;

            if (content == '\n') content = ' ';

            _handler.TerminalOut.Write($"{_handler.UpdateCursorPosition(position.X, position.Y, true)}\x1b[38;2;{foreground.Red};{foreground.Green};{foreground.Blue}m\x1b[48;2;{background.Red};{background.Green};{background.Blue}m{content}");
        }

        public ConsoleKeyInfo ReadKey()
        {
            char originalChar = (char)_handler.TerminalIn.Read();
            ConsoleKeyInfo info = new ConsoleKeyInfo(originalChar, parseKey(originalChar), false, false, false);
            return info;
        }

        private ConsoleKey parseKey(char info)
        {
            switch (info)
            {
                case '\r':
                    return ConsoleKey.Enter;
                case '\t':
                    return ConsoleKey.Tab;
                case '\u007f':
                    return ConsoleKey.Backspace;
                case '\u001b':
                    char[] buffer = new char[8];
                    _handler.TerminalIn.Read(buffer, 0, buffer.Length);
                    string code = new string(buffer).Replace("\0", string.Empty);
                    switch (code)
                    {
                        //Cursor Right
                        case "[C":
                            return ConsoleKey.RightArrow;
                        //Cursor Up
                        case "[A":
                            return ConsoleKey.UpArrow;
                        //Cursor Down
                        case "[B":
                            return ConsoleKey.DownArrow;
                        //Home
                        case "[1~":
                            return ConsoleKey.Home;
                        //End
                        case "[4~":
                            return ConsoleKey.End;

                        //Cursor Left
                        case "[D":
                            return ConsoleKey.LeftArrow;
                        default:
                            return ConsoleKey.Escape;
                    }
                case ' ':
                    return ConsoleKey.Spacebar;
                default:
                    Enum.TryParse(info.ToString(), out ConsoleKey ck);
                    return ck;
            }
        }
	}
}