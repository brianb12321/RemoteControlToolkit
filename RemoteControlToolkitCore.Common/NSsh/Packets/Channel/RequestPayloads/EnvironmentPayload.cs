using System.IO;
using System.Text;
using RemoteControlToolkitCore.Common.NSsh.Types;

namespace RemoteControlToolkitCore.Common.NSsh.Packets.Channel.RequestPayloads
{
    /// <summary>
    /// 
    /// string    variable name
    /// string    variable value
    /// </summary>
    public class EnvironmentPayload : IByteData
    {
        public EnvironmentPayload() { }

        public EnvironmentPayload(BinaryReader reader)
        {
            VariableName = new SshString(reader, Encoding.UTF8).Value;
            VariableValue = new SshString(reader, Encoding.UTF8).Value;
        }

        public string VariableName { get; set; }
        public string VariableValue { get; set; }

        #region IByteData Members

        public byte[] ToByteArray()
        {
            MemoryStream buffer = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.Write(new SshString(VariableName, Encoding.UTF8).ToByteArray());
            writer.Write(new SshString(VariableValue, Encoding.UTF8).ToByteArray());

            return buffer.ToArray();
        }

        #endregion
    }
}
