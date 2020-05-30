using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.NSsh.Packets;
using RemoteControlToolkitCore.Common.NSsh.Types;

namespace RemoteControlToolkitCore.Common.Utilities
{
    public class ChannelTextWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
        private ChannelStreamWriter _channel;
        public ITerminalHandler TerminalHandler { get; set; }
        private bool opost = true;

        public ChannelTextWriter(ChannelStreamWriter channel)
        {
            _channel = channel;
            if (TerminalHandler != null)
            {
                opost = TerminalHandler.TerminalModes.OPOST;
            }
        }

        public override void Write(char value)
        {
            _channel.Write(new[] {(byte)value}, 0, 0);
        }

        public override void Write(string value)
        {
            _channel.Write(Encoding.GetBytes(value), 0, 0);
        }

        static IEnumerable<string> Split(string str, int maxChunkSize)
        {
            for (int i = 0; i < str.Length; i += maxChunkSize)
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
        }
        public override void WriteLine(string value)
        {
            _channel.Write(Encoding.GetBytes(value + "\n" + (opost ? "\r" : string.Empty)), 0, 0);
        }

        public override void WriteLine()
        {
            _channel.Write(Encoding.GetBytes("\n" + (opost ? "\r" : string.Empty)), 0, 0);
        }
    }
}