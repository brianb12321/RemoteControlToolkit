using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCoreServerWCF
{
    public class WCFAppBuilder : AppBuilder
    {
        protected override IHostApplication InjectHostApplication(IServiceProvider provider)
        {
            return new WCFHostApplication(this, _pluginManager, provider);
        }
    }
}