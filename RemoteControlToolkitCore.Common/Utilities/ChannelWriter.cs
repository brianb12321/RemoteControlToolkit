using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.NSsh.ChannelLayer;
using RemoteControlToolkitCore.Common.NSsh.Packets;
using RemoteControlToolkitCore.Common.NSsh.Types;

namespace RemoteControlToolkitCore.Common.Utilities
{
    public class ChannelWriter : Stream
    {
        private IChannelProducer _channel;
        private ILogger<ChannelWriter> _logger;
        public ChannelWriter(IChannelProducer channel, ILogger<ChannelWriter> logger)
        {
            _channel = channel;
            _logger = logger;
        }
        public override void Flush()
        {
            
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return 0;
        }

        public override void SetLength(long value)
        {
            
        }
        public override int Read(byte[] buffer, int offset, int count)
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
                return -1;
            }
            catch (ObjectDisposedException e)
            {
                _logger.LogError("Error reading packet from channel.", e);
                return -1;
            }
            catch (TransportDisconnectException e)
            {
                _logger.LogError("The channel consumer had an unexpected transport layer disconnection.", e);
                return -1;
            }

            try
            {
                switch (packet.PacketType)
                {
                    case PacketType.ChannelData:
                        string debug = Encoding.UTF8.GetString(((ChannelDataPacket) packet).Data);
                        Encoding.UTF8.GetBytes(debug).CopyTo(buffer, 0);
                        return 0;

                    case PacketType.ChannelEof:
                        _channel.Close();
                        return 0;

                    case PacketType.ChannelClose:
                        _channel.Close();
                        return 0;

                    default:
                        throw new NotSupportedException("Packet type is not supported by channel: " + packet.PacketType);
                }
            }
            catch (IOException e)
            {
                _logger.LogError("Error handling packet.", e);
                return -1;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _channel.SendData(buffer);
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length { get; }
        public override long Position { get; set; }
    }

    public class ChannelTextWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
        private ChannelWriter _writer;

        public ChannelTextWriter(ChannelWriter writer)
        {
            _writer = writer;
        }

        public override void Write(string value)
        {
            _writer.Write(Encoding.GetBytes(value), 0, 0);
        }
        public override void WriteLine(string value)
        {
            _writer.Write(Encoding.GetBytes(value + "\r\n"), 0, 0);
        }

        public override void WriteLine()
        {
            _writer.Write(Encoding.GetBytes("\r\n"), 0, 0);
        }
    }
    public class ChannelTextReader : TextReader
    {
        public ChannelWriter _writer;
        public ChannelTextReader(ChannelWriter writer)
        {
            _writer = writer;
        }

        public override int Read(char[] buffer, int index, int count)
        {
            byte[] tempBuffer = new byte[count];
            int num = _writer.Read(tempBuffer, index, tempBuffer.Length);
            Encoding.UTF8.GetChars(tempBuffer).CopyTo(buffer, index);
            if (num != -1)
            {
                return buffer[0];
            }

            return -1;
        }

        public override int Read()
        {
            throw new NotImplementedException();
        }
    }
}