using System;
using System.Drawing;
using System.IO;
using System.ServiceModel;
using RemoteControlToolkitCore.Common.ApplicationSystem;

namespace RemoteControlToolkitCore.Common.Networking
{
    public interface IInstanceSession : IExtensibleObject<IInstanceSession>
    {
        Guid ClientUniqueID { get; }
        string Username { get; }
        StreamReader GetClientReader();
        Stream OpenNetworkStream();
        TextWriter GetClientWriter();
        IProcessTable ProcessTable { get; }
        T GetExtension<T>() where T : IExtension<IInstanceSession>;
        void AddExtension<T>(T extension) where T : IExtension<IInstanceSession>;
        void Close();
    }
}