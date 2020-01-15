using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace RemoteControlToolkitCore.Common.DeviceBus
{
    public static class DeviceBusServiceCollectionExtensions
    {
        public static IServiceCollection AddDeviceBus(this IServiceCollection services)
        {
            return services.AddSingleton<IDeviceBus, DeviceBus>();
        }
    }
}