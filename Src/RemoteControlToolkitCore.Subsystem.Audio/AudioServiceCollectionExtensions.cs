using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Audio
{
    public static class AudioServiceCollectionExtensions 
    {
        public static IServiceCollection AddAudio(this IServiceCollection services)
        {
            services.AddSingleton(provider =>
                new AudioOutSubsystem(provider.GetRequiredService<IHostApplication>().PluginManager,
                    provider.GetService<ILogger<AudioOutSubsystem>>(), provider));
            return services.AddSingleton<IExtensionProvider<IInstanceSession>, AudioQueueExtensionProvider>();
        }
    }
}