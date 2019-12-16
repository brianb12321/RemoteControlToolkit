using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Networking;

namespace RemoteControlToolkitCore.Common.Commandline
{
    /// <summary>
    /// Provides helper methods for reading and writing to and from the terminal.
    /// </summary>
    public interface ITerminalHandler : IExtension<IInstanceSession>
    {
        List<string> History { get; }
        (string row, string column) GetCursorPosition();
        int TerminalRows { get; set; }
        int TerminalColumns { get; set; }
        void Clear();
        void Bell();
    }
}