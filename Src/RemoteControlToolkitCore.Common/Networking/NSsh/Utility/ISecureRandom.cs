﻿namespace RemoteControlToolkitCore.Common.Networking.NSsh.Utility
{
    public interface ISecureRandom
    {
        int GetInt32();

        double GetDouble();

        void GetBytes(byte[] data);
    }
}
