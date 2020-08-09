using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using RemoteControlToolkitCore.Common.Networking.NSsh.Utility;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Packets
{
    public class PacketFactory : IPacketFactory
    {
        private IDictionary<PacketType, Type> _packetTypes = new Dictionary<PacketType, Type>();

        public PacketFactory()
        {
            var types = from t in Assembly.GetExecutingAssembly().GetTypes()
                        where typeof(Packet).IsAssignableFrom(t)
                        select t;

            // Build a mapping of SSH packet type to packet classes
            foreach (PacketType packetType in Enum.GetValues(typeof(PacketType)))
            {
                if (packetType == PacketType.Invalid)
                {
                    continue;
                }

                Type type = (from t in types
                             where t.Name == packetType.ToString() + "Packet"
                             select t).FirstOrDefault();

                if (type != null)
                {
                    _packetTypes.Add(packetType, type);
                }
            }
        }

        public Packet ReadFrom(Stream stream, ICryptoTransform receiveCipher, HashAlgorithm receiveMac, uint sequenceNumber)
        {
            // Setup the context for the packet constructor
            SshPacketContext context = new SshPacketContext();
            context.ReceiveCipher = receiveCipher;
            context.ReceiveMac = receiveMac;
            context.Stream = stream;
            context.MacSequenceNumber = sequenceNumber;

            Stream readerStream = (receiveCipher == null)
                ? (Stream) new NotCloseableStream(stream)
                : (Stream) new CryptoStream(new NotCloseableStream(stream), receiveCipher, CryptoStreamMode.Read);

            context.Reader = new BinaryReader(readerStream);

            context.PacketLength = (int)context.Reader.ReadUInt32BE();
            context.PaddingLength = context.Reader.ReadByte();
            context.PacketType = (PacketType)context.Reader.ReadByte();

            // Ensure the packet is known and valid
            if (_packetTypes.ContainsKey(context.PacketType))
            {
                try
                {
                    // Create a new instance of the packet with the context
                    return (Packet)Activator.CreateInstance(_packetTypes[context.PacketType], context);
                }
                catch (TargetInvocationException e)
                {
                    Exception innerOrException = e.InnerException ?? e;
                    throw innerOrException;
                }
            }
            else
            {
                throw new ArgumentException(string.Format("Invalid packet type {0}.", context.PacketType));
            }
        }
    }
}
