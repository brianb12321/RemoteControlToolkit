using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCore.Common.Commandline
{
    /// <summary>
    /// Provides a mechanism for creating a new terminal session.
    /// </summary>
    public interface ITerminalHandlerFactory
    {
        /// <summary>
        /// Allocates a new pseudo terminal for use with a communication channel.
        /// </summary>
        /// <param name="name">The name of the client terminal.</param>
        /// <param name="stdIn">The stream to read from the terminal.</param>
        /// <param name="stdOut">The stream to write to the terminal.</param>
        /// <param name="terminalRows">The number of rows allocated by the client terminal. Some handlers may send a dimension adjust when the handler is created.</param>
        /// <param name="terminalColumns">The number of columns allocated by the client terminal. Some handlers may send a dimension adjust when the handler is created.</param>
        /// <param name="modes">The default terminal modes.</param>
        /// <param name="baudRate">When connected to a serial line, specifies the current baud rate.</param>
        /// <param name="additionalArguments">Any additional arguments required by the terminal handler. Please see the documentation for additional arguments.</param>
        /// <returns></returns>
        ITerminalHandler CreateNewTerminalHandler(string name,
            Stream stdIn = null,
            Stream stdOut = null,
            uint terminalRows = 32,
            uint terminalColumns = 32,
            PseudoTerminalMode modes = null,
            int baudRate = 9600,
            object[] additionalArguments = null);
    }
}