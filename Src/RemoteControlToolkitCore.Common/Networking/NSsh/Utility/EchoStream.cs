/*
 * Copyright 2009 Aaron Clauson (aaron@sipsorcery.com)
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Utility {

    /// <summary>
    /// An in memory stream that echoes any data written to it. The stream can be used
    /// simultaneously in a stream reader and writer with any data written by the writer
    /// being echoed back to the reader.
    /// </summary>
    public class EchoStream : MemoryStream {

        private ManualResetEvent m_dataReady = new ManualResetEvent(false);
        private List<byte> m_buffer = new List<byte>();

        public void Write(byte[] buffer) {
            m_buffer.AddRange(buffer);
            m_dataReady.Set();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            m_buffer.AddRange(buffer.ToList().Skip(offset).Take(count));
            m_dataReady.Set();
        }

        public override void WriteByte(byte value) {
            m_buffer.Add(value);
            m_dataReady.Set();
        }

        public override int ReadByte() {
            if (m_buffer.Count == 0) {
                // Block until the stream has some more data.
                m_dataReady.Reset();
                m_dataReady.WaitOne();
            }

            byte firstByte = m_buffer[0];
            m_buffer = m_buffer.Skip(1).ToList();
            return firstByte;
        }

        public override int Read(byte[] buffer, int offset, int count) {
            if (m_buffer.Count == 0) {
                // Block until the stream has some more data.
                m_dataReady.Reset();
                m_dataReady.WaitOne();
            }

            if (m_buffer.Count >= count) {
                // More bytes available than were requested.
                Array.Copy(m_buffer.ToArray(), 0, buffer, offset, count);
                m_buffer = m_buffer.Skip(count).ToList();
                return count;
            }
            else {
                int length = m_buffer.Count;
                Array.Copy(m_buffer.ToArray(), 0, buffer, offset, length);
                m_buffer.Clear();
                return length;
            }
        }
    }
}
