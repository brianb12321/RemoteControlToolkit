using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCoreLibraryWCF;

namespace RemoteControlToolkitCoreServerWCF
{
    public class WCFStream : Stream
    {
        private IRCTServiceCallback _callback;

        public WCFStream(IRCTServiceCallback callback)
        {
            _callback = callback;
        }
        public override void Flush()
        {
            
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return 0L;
        }

        public override void SetLength(long value)
        {
            
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _callback.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            string text = Encoding.UTF8.GetString(buffer, offset, count);
            _callback.Print(text);
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new InvalidOperationException();
        public override long Position
        {
            get => throw new InvalidOperationException();
            set => throw new InvalidOperationException();
        }
    }
}