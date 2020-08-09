using System;
using System.IO;
using System.Text;
using RemoteControlToolkitCore.Common.Networking.NSsh.ChannelLayer;

namespace RemoteControlToolkitCore.Common.Utilities
{
    public class ChannelStreamWriter : Stream
    {
        private IChannelProducer _producer;

        public ChannelStreamWriter(IChannelProducer producer)
        {
            _producer = producer;
        }
        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            //Convert byte array into string.
            string text = Encoding.UTF8.GetString(buffer, offset, count);
            _producer.SendData(Encoding.UTF8.GetBytes(text));
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new InvalidOperationException("Cannot get length from network stream.");

        public override long Position
        {
            get => throw new InvalidOperationException("Cannot get position from network stream.");
            set => throw new InvalidOperationException("Cannot set position from network stream.");
        }
    }
}