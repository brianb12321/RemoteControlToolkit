using System;
using System.Collections.Generic;
using RemoteControlToolkitCore.Common.Commandline.Readline.Abstractions;

namespace RemoteControlToolkitCore.Common.Commandline.Readline
{
    public class GNUReadline
    {
        private List<string> _history;
        private readonly IConsole _console;

        public GNUReadline(IConsole console)
        {
            _history = new List<string>();
            _console = console;
        }

        public void AddHistory(params string[] text) => _history.AddRange(text);
        public List<string> GetHistory() => _history;
        public void ClearHistory() => _history = new List<string>();
        public bool HistoryEnabled { get; set; }
        public IAutoCompleteHandler AutoCompletionHandler { private get; set; }

        public string Read(string prompt = "", string @default = "")
        {
            _console.Write(prompt);
            KeyHandler keyHandler = new KeyHandler(_console, _history, AutoCompletionHandler);
            string text = GetText(keyHandler);

            if (String.IsNullOrWhiteSpace(text) && !String.IsNullOrWhiteSpace(@default))
            {
                text = @default;
            }
            else
            {
                if (HistoryEnabled)
                    _history.Add(text);
            }

            return text;
        }

        public string ReadPassword(string prompt = "")
        {
            _console.Write(prompt);
            _console.PasswordMode = true;
            KeyHandler keyHandler = new KeyHandler(_console, null, null);
            return GetText(keyHandler);
        }

        private string GetText(KeyHandler keyHandler)
        {
            ConsoleKeyInfo keyInfo = _console.ReadKey();
            while (keyInfo.Key != ConsoleKey.Enter)
            {
                keyHandler.Handle(keyInfo);
                keyInfo = _console.ReadKey();
            }
            keyHandler.MoveCursorEnd();
            _console.WriteLine(string.Empty);
            return keyHandler.Text;
        }
    }
}
