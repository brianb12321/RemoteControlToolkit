using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteControlToolkitCore.Common
{
    public interface IAppBuilder
    {
        IHostApplication Build();
        IAppBuilder UseStartup<TStartup>() where TStartup : IApplicationStartup;
    }
}