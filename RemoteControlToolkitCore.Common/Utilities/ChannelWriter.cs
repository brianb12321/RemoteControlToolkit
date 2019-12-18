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
    public class ChannelTextReader : TextReader
    {
        private IChannelProducer _channel;
        private ILogger<ChannelTextReader> _logger;
        private IInstanceSession _context;

        public ChannelTextReader(IChannelProducer channel, ILogger<ChannelTextReader> logger, IInstanceSession context)
        {
            _channel = channel;
            _logger = logger;
            _context = context;
        }

        public override string ReadLine()
        {
            Packet packet;

            try
            {
                packet = _channel.GetIncomingPacket();
                //log.Debug(packet.PacketType);
            }
            catch (IOException e)
            {
                _logger.LogError("Error reading packet from channel.", e);
                return null;
            }
            catch (ObjectDisposedException e)
            {
                _logger.LogError("Error reading packet from channel.", e);
                return null;
            }
            catch (TransportDisconnectException e)
            {
                _logger.LogError($"The channel consumer had an unexpected transport layer disconnection: {e.Message}");
                _context.Close();
                return null;
            }

            try
            {
                switch (packet.PacketType)
                {
                    case PacketType.ChannelData:
                        string text = Encoding.UTF8.GetString(((ChannelDataPacket)packet).Data);
                        return text;

                    case PacketType.ChannelEof:
                        _channel.Close();
                        return null;

                    case PacketType.ChannelClose:
                        _channel.Close();
                        return null;

                    default:
                        throw new NotSupportedException("Packet type is not supported by channel: " + packet.PacketType);
                }
            }
            catch (IOException e)
            {
                _logger.LogError("Error handling packet.", e);
                return null;
            }
        }
    }
}