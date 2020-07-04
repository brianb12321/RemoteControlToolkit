using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCoreLibraryWCF;

namespace RemoteControlToolkitCoreServerWCF
{
    public class WCFTerminalHandlerFactory : ITerminalHandlerFactory
    {
        public ITerminalHandler CreateNewTerminalHandler(string name, Stream stdIn = null, Stream stdOut = null,
            uint terminalRows = 32, uint terminalColumns = 32, PseudoTerminalMode modes = null,
            object[] additionalArguments = null)
        {
            if(additionalArguments != null && additionalArguments.Length < 1) throw new ArgumentException("The client callback must be passed to additional arguments.", nameof(additionalArguments));
            return new WCFTerminalHandler(additionalArguments[0] as IRCTServiceCallback);
        }
    }
}