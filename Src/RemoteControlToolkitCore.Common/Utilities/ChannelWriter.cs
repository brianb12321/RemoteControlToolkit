using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.NSsh.ChannelLayer;
using RemoteControlToolkitCore.Common.NSsh.Packets;
using RemoteControlToolkitCore.Common.NSsh.Types;

namespace RemoteControlToolkitCore.Common.Utilities
{
    public class ChannelTextWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
        private IChannelProducer _channel;
        public ITerminalHandler TerminalHandler { get; set; }
        private bool opost = true;

        public ChannelTextWriter(IChannelProducer channel)
        {
            _channel = channel;
            if (TerminalHandler != null)
            {
                opost = TerminalHandler.TerminalModes.OPOST;
            }
        }

        public override void Write(char value)
        {
            _channel.SendData(new[] {(byte)value});
        }

        public override void Write(string value)
        {
            _channel.SendData(Encoding.GetBytes(value));
        }

        static IEnumerable<string> Split(string str, int maxChunkSize)
        {
            for (int i = 0; i < str.Length; i += maxChunkSize)
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
        }
        public override void WriteLine(string value)
        {
            _channel.SendData(Encoding.GetBytes(value + "\n" + (opost ? "\r" : string.Empty)));
        }

        public override void WriteLine()
        {
            _channel.SendData(Encoding.GetBytes("\n" + (opost ? "\r" : string.Empty)));
        }
    }
}