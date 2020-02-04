using System.IO;
using System.Text;

namespace RemoteControlToolkitCore.Common.NSsh.Utility
{
    public class NotClosableStreamWriter : StreamWriter
    {
        public NotClosableStreamWriter(Stream stream) : base(stream)
        {
        }

        public NotClosableStreamWriter(Stream stream, Encoding encoding) : base(stream, encoding)
        {
        }

        public NotClosableStreamWriter(Stream stream, Encoding encoding, int bufferSize) : base(stream, encoding, bufferSize)
        {
        }

        public NotClosableStreamWriter(Stream stream, Encoding encoding, int bufferSize, bool leaveOpen) : base(stream, encoding, bufferSize, leaveOpen)
        {
        }

        public NotClosableStreamWriter(string path) : base(path)
        {
        }

        public NotClosableStreamWriter(string path, bool append) : base(path, append)
        {
        }

        public NotClosableStreamWriter(string path, bool append, Encoding encoding) : base(path, append, encoding)
        {
        }

        public NotClosableStreamWriter(string path, bool append, Encoding encoding, int bufferSize) : base(path, append, encoding, bufferSize)
        {
        }

        public override void Close()
        {
            
        }
    }
    public class NotClosableStreamReader : StreamReader
    {
        public NotClosableStreamReader(Stream stream) : base(stream)
        {
        }

        public NotClosableStreamReader(Stream stream, bool detectEncodingFromByteOrderMarks) : base(stream, detectEncodingFromByteOrderMarks)
        {
        }

        public NotClosableStreamReader(Stream stream, Encoding encoding) : base(stream, encoding)
        {
        }

        public NotClosableStreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks) : base(stream, encoding, detectEncodingFromByteOrderMarks)
        {
        }

        public NotClosableStreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize) : base(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize)
        {
        }

        public NotClosableStreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool leaveOpen) : base(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize, leaveOpen)
        {
        }

        public NotClosableStreamReader(string path) : base(path)
        {
        }

        public NotClosableStreamReader(string path, bool detectEncodingFromByteOrderMarks) : base(path, detectEncodingFromByteOrderMarks)
        {
        }

        public NotClosableStreamReader(string path, Encoding encoding) : base(path, encoding)
        {
        }

        public NotClosableStreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks) : base(path, encoding, detectEncodingFromByteOrderMarks)
        {
        }

        public NotClosableStreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize) : base(path, encoding, detectEncodingFromByteOrderMarks, bufferSize)
        {
        }

        public override void Close()
        {
            
        }
    }
    public class NotCloseableStream : Stream
    {
        private Stream _stream;

        public NotCloseableStream(Stream underlying)
        {
            _stream = underlying;
        }

        public override bool CanRead
        {
            get { return _stream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _stream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _stream.CanWrite; }
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override long Length
        {
            get { return _stream.Length; }
        }

        public override long Position
        {
            get
            {
                return _stream.Position;
            }
            set
            {
                _stream.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }

        public override void Close()
        {
            // Ignore
        }
    }
}
