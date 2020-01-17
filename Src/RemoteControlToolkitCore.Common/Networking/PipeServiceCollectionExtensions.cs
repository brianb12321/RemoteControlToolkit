using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace RemoteControlToolkitCore.Common.Networking
{
    public static class PipeServiceCollectionExtensions
    {
        public static IServiceCollection AddPipeService(this IServiceCollection services)
        {
            return services.AddSingleton<IPipeService, PipeService>();
        }
    }
}
