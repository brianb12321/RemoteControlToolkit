using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.VirtualFileSystem;
using RemoteControlToolkitCore.Common.Scripting;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.Proxy;
using RemoteControlToolkitCore.Subsystem.Audio;

namespace RemoteControlToolkitCoreServer
{
    class Program
    {
        static void Main(string[] args)
        {
            IHostApplication app = new AppBuilder()
                .UseStartup<Startup>()
                .Build();
            app.Run(args);
        }
    }

    public class Startup : IApplicationStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder => builder.AddConsole());
            services.AddPluginSystem<DefaultPluginLoader>();
            services.AddVFS();
            services.AddScriptingEngine<IronPythonScriptingEngine>();
            services.AddAudio();
            services.AddCommandLine();
            services.AddSingleton<IServerPool, ServerPool>();
        }

        public void PostConfigureServices(IServiceProvider provider, IHostApplication application)
        {
            provider.GetService<IPluginLibraryLoader>().LoadFromAssembly(Assembly.GetAssembly(typeof(DefaultShell)),
                application.ExecutingSide);
            provider.GetService<IPluginLibraryLoader>()
                .LoadFromAssembly(Assembly.GetAssembly(typeof(AudioCommand)), application.ExecutingSide);
            provider.GetService<IPluginSubsystem<IApplication>>().Init();
            provider.GetService<IPluginSubsystem<IFileSystemPluginModule>>().Init();
        }
    }
}