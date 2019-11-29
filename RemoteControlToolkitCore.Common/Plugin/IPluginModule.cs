using System;

namespace RemoteControlToolkitCore.Common.Plugin
{
    public interface IPluginModule
    {
        void InitializeServices(IServiceProvider kernel);
    }
}