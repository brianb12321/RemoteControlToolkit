using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NSsh.Server.Utility
{
    public class FifoStream : Stream
    {
        private Queue<byte> _buffer = new Queue<byte>(); 

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush() {}

        public override long Length
        {
            get { return _buffer.Count; }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int i;

            for (i = 0; i < count && _buffer.Count > 0; i++)
            {
                buffer[i + offset] = _buffer.Dequeue();
            }

            return i;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                _buffer.Enqueue(buffer[i + offset]);
            }

            if (Length > 0 && DataArrived != null) DataArrived(this, EventArgs.Empty);
        }

        public event EventHandler DataArrived;
    }
}
