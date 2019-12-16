using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.ApplicationSystem;

namespace RemoteControlToolkitCore.Common.Commandline
{
    public class ProxyTerminalHandler : ITerminalHandler
    {
        private RCTProcess _process;
        public List<string> History { get; }

        public ProxyTerminalHandler()
        {
            History = new List<string>();
        }
        public void Attach(RCTProcess owner)
        {
            _process = owner;
        }

        public void Detach(RCTProcess owner)
        {
            
        }

        public string ReadLine()
        {
            return _process.In.ReadLine();
        }

        public void Bell()
        {
            _process.Out.Write("\a");
        }
        public void Clear()
        {
            _process.Out.WriteLine("\u001b[2J\u001b[;H\u001b[0m");
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
    }
}