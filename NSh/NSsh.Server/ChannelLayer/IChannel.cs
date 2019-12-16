using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSsh.Common.Packets;
using NSsh.Common.Types;
using System.Security.Principal;
using System.Threading;
using NSsh.Server.TransportLayer;

namespace NSsh.Server.ChannelLayer
{
    public interface IChannel : IDisposable
    {
        ITransportLayerManager TransportLayerManager { get; set; }

        uint ChannelId { get; set; }

        void SetChannelType(ChannelType channelType);

        void SetTransmitWindowSize(uint windowSize);

        void SetMaximumTransmitPacketSize(uint maximumPacketSize);

        void Initialise();

        uint ReceiveWindowSize { get; }

        uint ReceiveMaximumPacketSize { get; }

        void ProcessChannelData(ChannelDataPacket dataPacket);

        void ProcessChannelExtendedData(ChannelExtendedDataPacket packet);

        void AdjustTransmitWindow(ChannelWindowAdjustPacket packet);

        void EofReceived(ChannelEofPacket packet);

        void CloseReceived(ChannelClosePacket packet);

        void RequestReceived(ChannelRequestPacket packet);

        event EventHandler Closed;
    }
}
