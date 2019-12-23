using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline;

namespace RemoteControlToolkitCore.Common.Utilities
{
    public class ConsoleTextReader : TextReader
    {
        private ITerminalHandler _terminalHandler;

        public ConsoleTextReader(ITerminalHandler handler)
        {
            _terminalHandler = handler;
        }

        public override int Read()
        {
            return _terminalHandler.Read();
        }

        public override string ReadLine()
        {
            return _terminalHandler.ReadLine();
        }
    }
}