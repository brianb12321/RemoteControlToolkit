using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Audio
{
    public static class AudioServiceCollectionExtensions 
    {
        public static IServiceCollection AddAudio(this IServiceCollection services)
        {
            services.AddSingleton<IPluginSubsystem<IAudioOutDeviceModule>, AudioOutSubsystem>();
            return services.AddSingleton<IInstanceExtensionProvider, AudioQueueExtensionProvider>();
        }
    }
}