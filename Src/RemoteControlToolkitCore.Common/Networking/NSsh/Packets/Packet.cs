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

using System;
using System.IO;
using System.Security.Cryptography;
using RemoteControlToolkitCore.Common.Networking.NSsh.Utility;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Packets
{
    /// <summary>
    /// An SSH packet.
    /// 
    /// The format of each packet is as follows:
    /// 
    /// uint32    packet_length
    /// byte      padding_length
    /// byte[n1]  payload; n1 = packet_length - padding_length - 1
    /// byte[n2]  random padding; n2 = padding_length
    /// byte[m]   mac (Message Authentication Code - MAC); m = mac_length
    /// </summary>
    public abstract class Packet
    {
        public Packet(PacketType packetType)
        {
            this.PacketType = packetType;
        }

        public Packet(SshPacketContext context)
        {
            PacketType = context.PacketType;
            Length = context.PacketLength;

            InitialisePayload(context.Reader);

            RandomPadding = context.Reader.ReadBytes(context.PaddingLength);

            if (context.ReceiveMac != null)
            {
                // Read the packet MAC
                byte[] macData = new byte[context.ReceiveMac.HashSize / 8];
                int count = context.Stream.Read(macData, 0, macData.Length);

                if (count != macData.Length)
                {
                    throw new IOException("Error reading from stream.");
                }
                
                // Generate the expected MAC
                MemoryStream macBuffer = new MemoryStream();
                BinaryWriter macWriter = new BinaryWriter(macBuffer);
                macWriter.WriteBE(context.MacSequenceNumber);
                macWriter.WriteBE((uint)Length);
                macWriter.Write((byte)RandomPadding.Length);
                macWriter.Write((byte) PacketType);
                macWriter.Write(GetPayloadData());
                macWriter.Write(RandomPadding);

                context.ReceiveMac.Initialize();
                byte[] expectedMacData;
                expectedMacData = context.ReceiveMac.ComputeHash(macBuffer.ToArray());

                for (int i = 0; i < expectedMacData.Length; i++)
                {
                    if (expectedMacData[i] != macData[i])
                    {
                        throw new InvalidOperationException("MAC doesn't validate for packet.");
                    }
                }
            }
        }

        public PacketType PacketType { get; set; }

        public byte[] RandomPadding { get; set; }

        public int Length { get; set; }

        /// <summary>
        /// Initialises the payload data for the packet from the given binary reader.
        /// </summary>
        /// <param name="reader">The reader to initialise from.</param>
        protected abstract void InitialisePayload(BinaryReader reader);

        public abstract byte[] GetPayloadData();

        public byte[] ToByteArray(ISecureRandom random)
        {
            return ToByteArray(null, null, 0, random);
        }

        public byte[] ToByteArray(ICryptoTransform transmitCipher, HashAlgorithm transmitMac, uint sequenceNumber, ISecureRandom secureRandom)
        {
            byte[] payloadData = GetPayloadData();

            // Determine the length of the padding
            int blockSize = (transmitCipher != null) ? transmitCipher.OutputBlockSize : 8;
            int paddingLength = Math.Abs(secureRandom.GetInt32()) % Byte.MaxValue;
            paddingLength = paddingLength - (4 + 1 + 1 + payloadData.Length + paddingLength) % blockSize;

            // Round up -ve lengths
            if (paddingLength < 0)
            {
                paddingLength += blockSize;
            }
            
            // Minimum of 4 bytes padding
            if (paddingLength < 4)
            {
                paddingLength += blockSize;
            }

            // Generate some random padding
            this.RandomPadding = new byte[paddingLength];
            secureRandom.GetBytes(RandomPadding);

            // Calculate payload length
            Length = 1 + 1 + payloadData.Length + RandomPadding.Length;
            
            MemoryStream unencryptedBuffer = new MemoryStream(Length);
            BinaryWriter writer = new BinaryWriter(unencryptedBuffer);

            MemoryStream resultBuffer = unencryptedBuffer;
            BinaryWriter resultWriter = writer;

            // Generate the unencrypted packet
            writer.WriteBE((uint)Length);
            writer.Write((byte)RandomPadding.Length);
            writer.Write((byte) PacketType);
            writer.Write(payloadData);
            writer.Write(RandomPadding);

            byte[] macData = null;

            // Compute message MAC if required
            if (transmitMac != null)
            {
                MemoryStream macBuffer = new MemoryStream();
                BinaryWriter macWriter = new BinaryWriter(macBuffer);
                macWriter.WriteBE(sequenceNumber);
                macWriter.Write(unencryptedBuffer.ToArray());

                transmitMac.Initialize();
                macData = transmitMac.ComputeHash(macBuffer.ToArray());
            }

            // Encrypted the packet if required
            if (transmitCipher != null)
            {
                MemoryStream encryptedBuffer = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(encryptedBuffer, transmitCipher, CryptoStreamMode.Write);
                cryptoStream.Write(unencryptedBuffer.ToArray());
                cryptoStream.Flush();

                resultBuffer = encryptedBuffer;
                resultWriter = new BinaryWriter(encryptedBuffer);
            }

            // Write out the unencrypted MAC if present
            if (macData != null)
            {
                resultWriter.Write(macData);
            }

            // Return the resulting buffer: (unencrypted or encrypted) + optional MAC
            return resultBuffer.ToArray();
        }        

        public override string ToString()
        {
            return PacketType.ToString();
        }
    }
}
