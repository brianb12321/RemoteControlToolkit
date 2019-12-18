using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.NSsh.Packets.Channel.RequestPayloads;

namespace RemoteControlToolkitCore.Common.Commandline
{
    /// <summary>
    /// Provides helper methods for working with a terminal.
    /// </summary>
    public class TerminalHandler : ITerminalHandler
    {
        public PseudoTerminalPayload InitialTerminalConfig { get; }
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
        private TextReader _textIn;
        private TextWriter _textOut;

        public TerminalHandler(TextReader textIn, TextWriter textOut, PseudoTerminalPayload terminalConfig)
        {
            _textIn = textIn;
            _textOut = textOut;
            History = new List<string>();
            InitialTerminalConfig = terminalConfig;
            if (InitialTerminalConfig != null)
            {
                _terminalRows = InitialTerminalConfig.TerminalHeight;
                _terminalColumns = InitialTerminalConfig.TerminalWidth;
            }
        }

        public void Clear()
        {
            _textOut.WriteLine("\u001b[2J\u001b[;H\u001b[0m");
        }

        public void Bell()
        {
            _textOut.Write("\a");
        }

        public (string row, string column) GetCursorPosition()
        {
            TextWriter tw = _textOut;
            TextReader tr = _textIn;
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

        

        public void Attach(IInstanceSession owner)
        {
            
        }

        public void Detach(IInstanceSession owner)
        {
            
        }
    }
}