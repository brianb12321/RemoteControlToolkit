using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.ApplicationSystem;

namespace RemoteControlToolkitCore.Common.Commandline
{
    /// <summary>
    /// Provides helper methods for reading and writing to and from the terminal.
    /// </summary>
    public interface ITerminalHandler : IExtension<RCTProcess>
    {
        List<string> History { get; }
        string ReadLine();
        (string row, string column) GetCursorPosition();
        void Clear();
        void Bell();
    }
}