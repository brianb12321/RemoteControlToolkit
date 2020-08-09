using System.IO;
using RemoteControlToolkitCore.Common.Networking.NSsh.Types;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Packets.Channel.RequestPayloads
{
    /// <summary>
    /// 
    ///  uint32    terminal width, columns
    ///  uint32    terminal height, rows
    ///  uint32    terminal width, pixels
    ///  uint32    terminal height, pixels
    /// </summary>
    public class WindowChangePayload : IByteData
    {
        public WindowChangePayload() { }

        public WindowChangePayload(BinaryReader reader)
        {
            TerminalWidth = reader.ReadUInt32BE();
            TerminalHeight = reader.ReadUInt32BE();
            TerminalWidthPixels = reader.ReadUInt32BE();
            TerminalHeightPixels = reader.ReadUInt32BE();
        }

        public uint TerminalWidth { get; set; }
        public uint TerminalHeight { get; set; }

        public uint TerminalWidthPixels { get; set; }
        public uint TerminalHeightPixels { get; set; }

        #region IByteData Members

        public byte[] ToByteArray()
        {
            MemoryStream buffer = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.WriteBE(TerminalWidth);
            writer.WriteBE(TerminalHeight);
            writer.WriteBE(TerminalWidthPixels);
            writer.WriteBE(TerminalHeightPixels);

            return buffer.ToArray();
        }

        #endregion
    }
}
