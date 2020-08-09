using System.IO;
using System.Text;
using RemoteControlToolkitCore.Common.Networking.NSsh.Types;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Packets.Channel.RequestPayloads
{
    /// <summary>
    /// 
    /// string    command
    /// </summary>
    public class ExecuteCommandPayload : IByteData
    {
        public ExecuteCommandPayload() { }

        public ExecuteCommandPayload(BinaryReader reader)
        {
            Command = new SshString(reader, Encoding.UTF8).Value;
        }

        public string Command { get; set; }

        #region IByteData Members

        public byte[] ToByteArray()
        {
            MemoryStream buffer = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.Write(new SshString(Command, Encoding.UTF8).ToByteArray());

            return buffer.ToArray();
        }

        #endregion
    }
}
