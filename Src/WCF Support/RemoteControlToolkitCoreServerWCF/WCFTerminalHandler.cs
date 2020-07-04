using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCoreLibraryWCF;

namespace RemoteControlToolkitCoreServerWCF
{
    public class WCFTerminalHandler : ITerminalHandler
    {
        public IExtensionCollection<ITerminalHandler> Extensions { get; }
        public TextWriter TerminalOut { get; }
        public TextReader TerminalIn { get; }
        public Stream RawTerminalIn { get; }
        public event EventHandler TerminalDimensionsChanged;
        public event EventHandler ReadLineInvoked;
        public event EventHandler<string> ReadLineCompleted;
        public string TerminalName { get; }
        private IRCTServiceCallback _callback;

        public WCFTerminalHandler(IRCTServiceCallback callback)
        {
            _callback = callback;
        }

        public void Attach(IInstanceSession owner)
        {
            
        }

        public void Detach(IInstanceSession owner)
        {
            
        }

        
        public (string row, string column) GetCursorPosition()
        {
            return _callback.GetCursorPosition();
        }

        public uint TerminalRows { get; set; }
        public uint TerminalColumns { get; set; }
        public PseudoTerminalMode TerminalModes { get; }
        public void Clear()
        {
            _callback.ClearScreen();
        }

        public string ClearScreenCursorDown(bool writeCode = false)
        {
            throw new NotImplementedException();
        }

        public void ClearRow()
        {
            _callback.Print("\u001b[K");
        }

        public void Bell()
        {
            _callback.Print("\a");
        }

        public string ReadLine()
        {
            return _callback.ReadLine();
        }

        public string ReadToEnd()
        {
            return _callback.ReadToEnd();
        }

        public char Read()
        {
            return _callback.Read();
        }

        public void BindKey(string key, KeyBindingDelegate function)
        {
            throw new NotImplementedException();
        }

        public int ReadFromPipe(char[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public void UpdateHomePosition(int col, int row)
        {
            throw new NotImplementedException();
        }

        public string UpdateCursorPosition(int col, int row, bool writeCode = false)
        {
            _callback.UpdateCursorPosition(col, row);
            return string.Empty;
        }

        public void MoveCursorLeft(int count = 1)
        {
            if (count > 0) _callback.Print($"\u001b[{count}D");
        }

        public void MoveCursorRight(int count = 1)
        {
            throw new NotImplementedException();
        }

        public void MoveCursorUp(int count = 1)
        {
            throw new NotImplementedException();
        }

        public void MoveCursorDown(int count = 1)
        {
            throw new NotImplementedException();
        }

        public void SetDisplayMode(int code)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void ScrollDown()
        {
            throw new NotImplementedException();
        }

        public void SetTitle(string title)
        {
            _callback.SetTitle(title);
        }

        public void SetForeground(ConsoleColor color)
        {
            throw new NotImplementedException();
        }

        public void SetBackground(ConsoleColor color)
        {
            throw new NotImplementedException();
        }

        public void HideCursor()
        {
            throw new NotImplementedException();
        }

        public void ShowCursor()
        {
            throw new NotImplementedException();
        }

        public void ResizeWindow(int column, int row)
        {
            throw new NotImplementedException();
        }
    }
}