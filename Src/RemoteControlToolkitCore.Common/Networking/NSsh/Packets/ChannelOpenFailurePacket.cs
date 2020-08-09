using System.IO;
using System.Text;
using RemoteControlToolkitCore.Common.Networking.NSsh.Packets.Channel;
using RemoteControlToolkitCore.Common.Networking.NSsh.Types;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Packets
{
    /// <summary>
    ///
    /// byte      SSH_MSG_CHANNEL_OPEN_FAILURE
    /// uint32    recipient channel
    /// uint32    reason code
    /// string    description in ISO-10646 UTF-8 encoding [RFC3629]
    /// string    language tag [RFC3066]
    /// </summary>
    public class ChannelOpenFailurePacket : Packet
    {
        public ChannelOpenFailurePacket() : base(PacketType.ChannelOpenFailure) { }

        public ChannelOpenFailurePacket(SshPacketContext context) : base(context) { }

        public uint RecipientChannel { get; set; }

        public ChannelOpenFailureReason FailureReason { get; set; }

        public string Description { get; set; }

        public string LanguageTag { get; set; }

        protected override void InitialisePayload(BinaryReader reader)
        {
            RecipientChannel = reader.ReadUInt32BE();
            FailureReason = (ChannelOpenFailureReason) reader.ReadUInt32BE();
            Description = new SshString(reader, Encoding.UTF8).Value;
            LanguageTag = new SshString(reader, Encoding.ASCII).Value;
        }

        public override byte[] GetPayloadData()
        {
            MemoryStream buffer = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.WriteBE(RecipientChannel);
            writer.WriteBE((uint)FailureReason);
            writer.Write(new SshString(Description, Encoding.UTF8).ToByteArray());
            writer.Write(new SshString(LanguageTag, Encoding.ASCII).ToByteArray());

            return buffer.ToArray();
        }
    }
}
