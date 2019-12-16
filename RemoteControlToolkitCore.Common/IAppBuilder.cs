using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace RemoteControlToolkitCore.Common
{
    public interface IAppBuilder
    {
        NetworkSide ExecutingSide { get; }
        IHostApplication Build();
        IAppBuilder AddStartup<TStartup>() where TStartup : IApplicationStartup;
        IAppBuilder ScanForAppStartup(string folder);
    }
}