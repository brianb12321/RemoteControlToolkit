﻿using System;
using RemoteControlToolkitCore.Common.Networking.NSsh.Packets;
using RemoteControlToolkitCore.Common.Networking.NSsh.TransportLayer;
using RemoteControlToolkitCore.Common.Networking.NSsh.Types;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.ChannelLayer
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
