using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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

        public ChannelTextWriter(IChannelProducer channel)
        {
            _channel = channel;
        }

        public override void Write(string value)
        {
            _channel.SendData(Encoding.GetBytes(value));
        }
        public override void WriteLine(string value)
        {
            _channel.SendData(Encoding.GetBytes(value + "\r\n"));
        }

        public override void WriteLine()
        {
            _channel.SendData(Encoding.GetBytes("\r\n"));
        }
    }
}