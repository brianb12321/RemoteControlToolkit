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
    public interface IShellExtensions : IExtension<RCTProcess>
    {
        string ReadLine(RCTProcess process, StringBuilder sb, List<string> history);
    }
}