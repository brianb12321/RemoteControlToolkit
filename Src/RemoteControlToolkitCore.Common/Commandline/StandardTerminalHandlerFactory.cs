using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCore.Common.Commandline
{
    public class StandardTerminalHandlerFactory : ITerminalHandlerFactory
    {
        public ITerminalHandler CreateNewTerminalHandler(string name, Stream stdIn = null, Stream stdOut = null,
            uint terminalRows = 32, uint terminalColumns = 32, PseudoTerminalMode modes = null, int baudRate = 9600,
            object[] additionalArguments = null)
        {
            return new TerminalHandler(stdIn, stdOut, name, terminalColumns, terminalRows, modes);
        }
    }
}