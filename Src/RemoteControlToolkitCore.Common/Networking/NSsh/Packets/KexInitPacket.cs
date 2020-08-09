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
using RemoteControlToolkitCore.Common.Networking.NSsh.Types;
using RemoteControlToolkitCore.Common.Networking.NSsh.Utility;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Packets
{
    /// <summary>
    /// 
    /// Key exchange begins by each side sending the following packet:
    ///   byte         SSH_MSG_KEXINIT
    ///   byte[16]     cookie (random bytes)
    ///   name-list    kex_algorithms
    ///   name-list    server_host_key_algorithms
    ///   name-list    encryption_algorithms_client_to_server
    ///   name-list    encryption_algorithms_server_to_client
    ///   name-list    mac_algorithms_client_to_server
    ///   name-list    mac_algorithms_server_to_client
    ///   name-list    compression_algorithms_client_to_server
    ///   name-list    compression_algorithms_server_to_client
    ///   name-list    languages_client_to_server
    ///   name-list    languages_server_to_client
    ///   boolean      first_kex_packet_follows
    ///   uint32       0 (reserved for future extension)
    /// </summary>
    public class KexInitPacket : Packet
    {
        public KexInitPacket(ISecureRandom secureRandom) : base(PacketType.KexInit)
        {

            Cookie = new byte[16];
            secureRandom.GetBytes(Cookie);

            KexAlgorithms = new NameList();
            ServerHostKeyAlgorithms = new NameList();
            EncryptionAlgorithmsClientToServer = new NameList();
            EncryptionAlgorithmsServerToClient = new NameList();
            MacAlgorithmsClientToServer = new NameList();
            MacAlgorithmsServerToClient = new NameList();
            CompressionAlgorithmsClientToServer = new NameList();
            CompressionAlgorithmsServerToClient = new NameList();
            LanguagesClientToServer = new NameList();
            LanguagesServerToClient = new NameList();
        }

        public KexInitPacket(SshPacketContext context) : base(context) { }
        
        /// <summary>
        /// The cookie. Must be 16 bytes.
        /// </summary>
        public byte[] Cookie { get; set; }

        public NameList KexAlgorithms { get; set; }

        public NameList ServerHostKeyAlgorithms { get; set; }

        public NameList EncryptionAlgorithmsClientToServer { get; set; }

        public NameList EncryptionAlgorithmsServerToClient { get; set; }

        public NameList MacAlgorithmsClientToServer { get; set; }

        public NameList MacAlgorithmsServerToClient { get; set; }

        public NameList CompressionAlgorithmsClientToServer { get; set; }

        public NameList CompressionAlgorithmsServerToClient { get; set; }

        public NameList LanguagesClientToServer { get; set; }

        public NameList LanguagesServerToClient { get; set; }

        public bool FirstKexPacketFollows { get; set; }

        public uint Reserved { get; set; }

        protected override void InitialisePayload(BinaryReader reader)
        {
            Cookie = reader.ReadBytes(16);

            KexAlgorithms = new NameList(reader);
            ServerHostKeyAlgorithms = new NameList(reader);
            EncryptionAlgorithmsClientToServer = new NameList(reader);
            EncryptionAlgorithmsServerToClient = new NameList(reader);
            MacAlgorithmsClientToServer = new NameList(reader);
            MacAlgorithmsServerToClient = new NameList(reader);
            CompressionAlgorithmsClientToServer = new NameList(reader);
            CompressionAlgorithmsServerToClient = new NameList(reader);
            LanguagesClientToServer = new NameList(reader);
            LanguagesServerToClient = new NameList(reader);

            FirstKexPacketFollows = (reader.ReadByte() == 1);
            Reserved = reader.ReadUInt32();
        }

        public override byte[] GetPayloadData()
        {
            MemoryStream buffer = new MemoryStream(Length);
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.Write(Cookie);
            writer.Write(KexAlgorithms.ToByteArray());
            writer.Write(ServerHostKeyAlgorithms.ToByteArray());
            writer.Write(EncryptionAlgorithmsClientToServer.ToByteArray());
            writer.Write(EncryptionAlgorithmsServerToClient.ToByteArray());
            writer.Write(MacAlgorithmsClientToServer.ToByteArray());
            writer.Write(MacAlgorithmsServerToClient.ToByteArray());
            writer.Write(CompressionAlgorithmsClientToServer.ToByteArray());
            writer.Write(CompressionAlgorithmsServerToClient.ToByteArray());
            writer.Write(LanguagesClientToServer.ToByteArray());
            writer.Write(LanguagesServerToClient.ToByteArray());
            writer.Write((byte)(FirstKexPacketFollows ? 1 : 0));
            writer.Write(Reserved);

            return buffer.ToArray();
        }
    }
}
