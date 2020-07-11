using System;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Audio
{
    [Plugin]
    public class Startup : IApplicationStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAudio();
        }

        public void PostConfigureServices(IServiceProvider provider, IHostApplication application)
        {
            provider.GetService<AudioOutSubsystem>().InitializeSubsystem();
        }
    }
}