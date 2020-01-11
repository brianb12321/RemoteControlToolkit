/*
 * Copyright 2008 Luke Quinane
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
 * This work has been adapted from the Ganymed SSH-2 Java client.
 * 
 */

using System.IO;
using System.Text;
using RemoteControlToolkitCore.Common.NSsh.Types;

namespace RemoteControlToolkitCore.Common.NSsh.Packets
{
    /// <summary>
    /// 
    /// byte      SSH_MSG_DISCONNECT
    /// uint32    reason code
    /// string    description in ISO-10646 UTF-8 encoding [RFC3629]
    /// string    language tag [RFC3066]
    /// </summary>
    public class DisconnectPacket : Packet
    {
        public DisconnectPacket() : base(PacketType.Disconnect) { }

        public DisconnectPacket(SshPacketContext context) : base(context) { }

        public DisconnectReason DisconnectReason { get; set; }

        public string Description { get; set; }

        public string LanguageTag { get; set; }

        public override byte[] GetPayloadData()
        {
            MemoryStream buffer = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.Write((uint)DisconnectReason);
            writer.Write(new SshString(Description, Encoding.UTF8).ToByteArray());
            writer.Write(new SshString(LanguageTag, Encoding.ASCII).ToByteArray());

            return buffer.ToArray();
        }

        protected override void InitialisePayload(BinaryReader reader)
        {
            DisconnectReason = (DisconnectReason)reader.ReadUInt32();
            Description = new SshString(reader, Encoding.UTF8).Value;
            LanguageTag = new SshString(reader, Encoding.ASCII).Value;
        }
    }
}
