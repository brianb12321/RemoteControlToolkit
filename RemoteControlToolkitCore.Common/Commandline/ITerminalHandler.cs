using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.NSsh.Packets.Channel.RequestPayloads;

namespace RemoteControlToolkitCore.Common.Commandline
{
    /// <summary>
    /// Provides helper methods for reading and writing to and from the terminal.
    /// </summary>
    public interface ITerminalHandler : IExtension<IInstanceSession>
    {
        event EventHandler TerminalDimensionsChanged;
        List<string> History { get; }
        (string row, string column) GetCursorPosition();
        uint TerminalRows { get; set; }
        uint TerminalColumns { get; set; }
        PseudoTerminalPayload InitialTerminalConfig { get; }
        PseudoTerminalMode TerminalModes { get; }
        void Clear();
        void Bell();
        string ReadLine();
        string ReadToEnd();
        char Read();
        void UpdateHomePosition(int col, int row);
        void UpdateCursorPosition(int col, int row);
    }
}