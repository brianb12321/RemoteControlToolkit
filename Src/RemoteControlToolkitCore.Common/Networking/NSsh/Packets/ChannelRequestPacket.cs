using System;
using System.IO;
using System.Text;
using RemoteControlToolkitCore.Common.Networking.NSsh.Packets.Channel;
using RemoteControlToolkitCore.Common.Networking.NSsh.Packets.Channel.RequestPayloads;
using RemoteControlToolkitCore.Common.Networking.NSsh.Types;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Packets
{
    /// <summary>
    ///
    /// byte      SSH_MSG_CHANNEL_REQUEST
    /// uint32    recipient channel
    /// string    request type in US-ASCII characters only
    /// boolean   want reply
    /// ....      type-specific data follows
    /// </summary>
    public class ChannelRequestPacket : Packet
    {
        public ChannelRequestPacket() : base(PacketType.ChannelRequest) { }

        public ChannelRequestPacket(SshPacketContext context) : base(context) { }

        public uint RecipientChannel { get; set; }

        public ChannelRequestType RequestType { get; set; }

        public bool WantReply { get; set; }

        public IByteData ChannelRequestPayload { get; set; }

        protected override void InitialisePayload(BinaryReader reader)
        {
            RecipientChannel = reader.ReadUInt32BE();
            string channelRequestType = new SshString(reader, Encoding.ASCII).Value;
            RequestType = ChannelRequestTypeHelper.Parse(channelRequestType);
            
            WantReply = (reader.ReadByte() == 1);

            switch (RequestType)
            {
                case ChannelRequestType.PseudoTerminal:
                    ChannelRequestPayload = new PseudoTerminalPayload(reader);
                    break;

                case ChannelRequestType.Shell:
                    // Nothing extra
                    break;

                case ChannelRequestType.AuthenticationAgent:
                    // Nothing extra
                    break;

                case ChannelRequestType.WindowChange:
                    ChannelRequestPayload = new WindowChangePayload(reader);
                    break;

                case ChannelRequestType.Environment:
                    ChannelRequestPayload = new EnvironmentPayload(reader);
                    break;

                case ChannelRequestType.X11Forwarding:
                    ChannelRequestPayload = new X11ForwardingPayload(reader);
                    break;

                case ChannelRequestType.ExecuteCommand:
                    ChannelRequestPayload = new ExecuteCommandPayload(reader);
                    break;

                case ChannelRequestType.PuttyWinAdj:
                    // Nothing extra
                    break;

                default:
                    throw new NotSupportedException("Request not supported.");
            }
        }

        public override byte[] GetPayloadData()
        {
            MemoryStream buffer = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.WriteBE(RecipientChannel);
            writer.Write(new SshString(ChannelRequestTypeHelper.ToString(RequestType), Encoding.ASCII).ToByteArray());
            writer.Write((byte)(WantReply ? 1 : 0));

            if (ChannelRequestPayload != null)
            {
                writer.Write(ChannelRequestPayload.ToByteArray());
            }

            return buffer.ToArray();
        }

        public override string ToString()
        {
            return base.ToString() + " " + RequestType;
        }
    }
}
