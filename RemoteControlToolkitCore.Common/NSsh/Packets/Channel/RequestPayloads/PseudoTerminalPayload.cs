using System.IO;
using System.Text;
using RemoteControlToolkitCore.Common.NSsh.Types;

namespace RemoteControlToolkitCore.Common.NSsh.Packets.Channel.RequestPayloads
{
    /// <summary>
    /// 
    /// string    TERM environment variable value (e.g., vt100)
    /// uint32    terminal width, characters (e.g., 80)
    /// uint32    terminal height, rows (e.g., 24)
    /// uint32    terminal width, pixels (e.g., 640)
    /// uint32    terminal height, pixels (e.g., 480)
    /// string    encoded terminal modes
    /// </summary>
    public class PseudoTerminalPayload : IByteData
    {
        public PseudoTerminalPayload() { }

        public PseudoTerminalPayload(BinaryReader reader)
        {
            TerminalType = new SshString(reader, Encoding.ASCII).Value;

            TerminalWidth = reader.ReadUInt32BE();
            TerminalHeight = reader.ReadUInt32BE();
            TerminalWidthPixels = reader.ReadUInt32BE();
            TerminalHeightPixels = reader.ReadUInt32BE();

            TerminalModes = new SshByteArray(reader).Value;
        }

        public string TerminalType { get; set; }

        public uint TerminalWidth { get; set; }
        public uint TerminalHeight { get; set; }

        public uint TerminalWidthPixels { get; set; }
        public uint TerminalHeightPixels { get; set; }

        public byte[] TerminalModes { get; set; }

        #region IByteData Members

        public byte[] ToByteArray()
        {
            MemoryStream buffer = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.Write(new SshString(TerminalType, Encoding.ASCII).ToByteArray());

            writer.WriteBE(TerminalWidth);
            writer.WriteBE(TerminalHeight);
            writer.WriteBE(TerminalWidthPixels);
            writer.WriteBE(TerminalHeightPixels);

            writer.Write(new SshByteArray(TerminalModes).ToByteArray());

            return buffer.ToArray();
        }

        #endregion
    }
}
