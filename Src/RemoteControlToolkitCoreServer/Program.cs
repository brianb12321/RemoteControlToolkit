using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.ApplicationSystem.Factory;
using RemoteControlToolkitCore.Common.VirtualFileSystem;
using RemoteControlToolkitCore.Common.Scripting;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Commandline.Commands;
using RemoteControlToolkitCore.Common.DeviceBus;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.NSsh;
using RemoteControlToolkitCore.Common.NSsh.Configuration;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.Proxy;
using RemoteControlToolkitCore.DefaultShell;
using RemoteControlToolkitCore.Subsystem.Audio;
using RemoteControlToolkitCore.Subsystem.Serial;
using RemoteControlToolkitCore.Subsystem.Workflow;
using RemoteControlToolkitCore.Subsystem.Roslyn;

namespace RemoteControlToolkitCoreServer
{
    class Program
    {
        static void Main(string[] args)
        {
            IHostApplication app = new AppBuilder()
                .AddStartup<Startup>()
                .AddConfiguration(c =>
                {
                    c.Sources.Clear();
                    c.AddJsonFile("Configurations/Server/server.config", true, false);
                })
                .ConfigureLogging(factory =>
                    factory.AddConsole())
                .AddStartup<RemoteControlToolkitCore.Subsystem.Workflow.Startup>()
                .AddStartup<RemoteControlToolkitCore.Subsystem.Audio.Startup>()
                .UsePluginManager<PluginManager>()
                .LoadFromPluginsFolder()
                .Build();

            app.Run(args);
        }
    }

    public class Startup : IApplicationStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDeviceBus();
            services.AddVFS();
            services.AddScriptingEngine();
            services.AddCommandLine();
            services.AddSingleton<IServerPool, ServerPool>();
            services.AddPipeService();
            services.AddSSH(config =>
            {
                config.ListenEndPoints = new List<IPEndPoint>
                    { new IPEndPoint(IPAddress.Any, 8081)};
                config.IdleTimeout = TimeSpan.FromDays(25);
                config.MaximumClientConnections = 10;
                config.UserAuthenticationBanner =
                    "You are about to connect to a RemoteControlToolkit server. Any damages caused by the use of this software will be held against the user. Please refer to the user manual before proceeding.";
                config.ReceiveMaximumPacketSize = uint.MaxValue;
            });
        }

        public void PostConfigureServices(IServiceProvider provider, IHostApplication application)
        {
            application.PluginManager.LoadFromType(typeof(DefaultShell));
            application.PluginManager.LoadFromType(typeof(HelpCommand));
            application.PluginManager.LoadFromType(typeof(AudioCommand));
            application.PluginManager.LoadFromType(typeof(WorkflowCommand));
            application.PluginManager.LoadFromType(typeof(RCTSerialDevice));
            application.PluginManager.LoadFromType(typeof(AsmGen));
            provider.GetService<ApplicationSubsystem>().InitializeSubsystem();
            provider.GetService<ProcessFactorySubsystem>().InitializeSubsystem();;
            provider.GetService<FileSystemSubsystem>().InitializeSubsystem();;
            provider.GetService<ScriptingSubsystem>().InitializeSubsystem();
            provider.GetService<DeviceBusSubsystem>().InitializeSubsystem();
        }
    }
}