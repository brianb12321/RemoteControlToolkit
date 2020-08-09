using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Principal;
using RemoteControlToolkitCore.Common.Networking.NSsh.Packets;
using RemoteControlToolkitCore.Common.Networking.NSsh.TransportLayer.State;
using RemoteControlToolkitCore.Common.Networking.NSsh.Types;
using RemoteControlToolkitCore.Common.Networking.NSsh.Utility;
using BigInteger = Mono.Math.BigInteger;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.TransportLayer
{
    public interface ITransportLayerManager : IDisposable
    {
        event EventHandler OnIdleTimeout;

        void StartIdleTimeout(TimeSpan idleTimeout);

        void Process(Stream stream);

        void Reject(Stream stream);

        bool Connected { get; }

        void Disconnect(DisconnectReason reason);

        void DisconnectReceived(DisconnectPacket packet);

        event EventHandler Disconnected;

        string ReadLine();

        void Write(byte[] data);

        void WritePacket(Packet packet);

        Packet ReadPacket();

        Socket ClientSocket { get; set; }

        string ClientVersion { get; set; }

        string ServerVersion { get; set; }

        void ChangeState(TransportLayerState newState);

        TransportLayerParameters Parameters { get; set; }

        KexInitPacket ClientKexInit { get; set; }

        KexInitPacket ServerKexInit { get; set; }

        BigInteger E { get; set; }

        BigInteger X { get; set; }

        BigInteger Key { get; set; }

        byte[] Hash { get; set; }

        byte[] SessionId { get; set; }

        /// <summary>
        /// An object used to synchronise access to the transport layer manager communications.
        /// </summary>
        object CommunicationLock { get; set; }

        uint TransmitSequenceNumber { get; set; }

        uint ReceiveSequenceNumber { get; set; }

        SymmetricAlgorithm TransmitCipher { get; set; }

        SymmetricAlgorithm ReceiveCipher { get; set; }

        HashAlgorithmCreator TransmitMac { get; set; }

        HashAlgorithmCreator ReceiveMac { get; set; }

        IIdentity AuthenticatedIdentity { get; set; }

        AbstractTransportState State { get; }
    }
}
