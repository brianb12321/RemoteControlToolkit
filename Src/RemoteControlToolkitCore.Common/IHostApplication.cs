using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.FileProviders;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Networking.NSsh;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common
{
    public interface IHostApplication : IDisposable
    {
        IProcessTable GlobalSystemProcessTable { get; }
        IFileProvider RootFileProvider { get; }
        void UnRegisterSession(ISshSession session);
        void Run(string[] args);
        NetworkSide ExecutingSide { get; }
        IAppBuilder Builder { get; }
        IPluginManager PluginManager { get; }
    }
}