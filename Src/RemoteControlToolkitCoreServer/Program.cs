using System;
using System.Net;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.VirtualFileSystem;
using RemoteControlToolkitCore.Common.Scripting;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.DeviceBus;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.NSsh;
using RemoteControlToolkitCore.Common.NSsh.Configuration;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.Proxy;
using RemoteControlToolkitCore.Subsystem.Audio;
using RemoteControlToolkitCore.Subsystem.Serial;
using RemoteControlToolkitCore.Subsystem.Workflow;

namespace RemoteControlToolkitCoreServer
{
    class Program
    {
        static void Main(string[] args)
        {
            IHostApplication app = new AppBuilder()
                .AddStartup<Startup>()
                .AddStartup<RemoteControlToolkitCore.Subsystem.Workflow.Startup>()
                .ScanForAppStartup("Extensions")
                .Build();

            app.Run(args);
        }
    }

    public class Startup : IApplicationStartup
    {
        public void ConfigureServices(IServiceCollection services, IAppBuilder builder)
        {
            services.AddLogging(logBuilder =>
            {
                logBuilder.AddConsole();
            });
            services.AddPluginSystem<DefaultPluginLoader>();
            services.AddDeviceBus();
            services.AddVFS();
            services.AddScriptingEngine<ScriptingSubsystem>();
            services.AddAudio();
            services.AddCommandLine();
            services.AddSingleton<IServerPool, ServerPool>();
            services.AddPipeService();
            services.AddSSH(new NSshServiceConfiguration()
            {
                ListenEndPoints = { new IPEndPoint(IPAddress.Any, 8081)},
                IdleTimeout = TimeSpan.FromHours(2),
                MaximumClientConnections = 10,
                UserAuthenticationBanner = "You are about to connect to a RemoteControlToolkit server. Any damages caused by the use of this software will be held against the user. Please refer to the user manual before proceeding.",
                ReceiveMaximumPacketSize = uint.MaxValue
            });
        }

        public void PostConfigureServices(IServiceProvider provider, IHostApplication application)
        {
            provider.GetService<IPluginLibraryLoader>().LoadFromAssembly(Assembly.GetAssembly(typeof(DefaultShell)),
                application.ExecutingSide);
            provider.GetService<IPluginLibraryLoader>()
                .LoadFromAssembly(Assembly.GetAssembly(typeof(AudioCommand)), application.ExecutingSide);
            provider.GetService<IPluginLibraryLoader>()
                .LoadFromFolder("Extensions", application.ExecutingSide);
            provider.GetService<IPluginLibraryLoader>()
                .LoadFromAssembly(Assembly.GetAssembly(typeof(WorkflowCommand)), application.ExecutingSide);
            provider.GetService<IPluginLibraryLoader>()
                .LoadFromAssembly(Assembly.GetAssembly(typeof(RCTSerialDevice)), application.ExecutingSide);
            provider.GetService<IApplicationSubsystem>().Init();
            provider.GetService<IFileSystemSubsystem>().Init();
            provider.GetService<IDeviceBus>().Init();
            provider.GetService<IScriptingSubsystem>().Init();
        }
    }
}