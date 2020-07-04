using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.NSsh.Packets.Channel.RequestPayloads;

namespace RemoteControlToolkitCore.Common.Commandline
{
    /// <summary>
    /// Provides a mechanism for creating a new terminal session.
    /// </summary>
    public interface ITerminalHandlerFactory
    {
        ITerminalHandler CreateNewTerminalHandler(string name,
            Stream stdIn = null,
            Stream stdOut = null,
            uint terminalRows = 32,
            uint terminalColumns = 32,
            PseudoTerminalMode modes = null,
            object[] additionalArguments = null);
    }
}