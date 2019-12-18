﻿using System.IO;

namespace RemoteControlToolkitCore.Common.NSsh.Packets
{
    /// <summary>
    ///
    /// byte      SSH_MSG_CHANNEL_CLOSE
    /// uint32    recipient channel
    /// </summary>
    public class ChannelClosePacket : Packet
    {
        public ChannelClosePacket() : base(PacketType.ChannelClose) { }

        public ChannelClosePacket(SshPacketContext context) : base(context) { }

        public uint RecipientChannel { get; set; }

        protected override void InitialisePayload(BinaryReader reader)
        {
            RecipientChannel = reader.ReadUInt32BE();
        }

        public override byte[] GetPayloadData()
        {
            MemoryStream buffer = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.WriteBE(RecipientChannel);

            return buffer.ToArray();
        }
    }
}