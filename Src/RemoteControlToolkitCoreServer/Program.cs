using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.ApplicationSystem.Factory;
using RemoteControlToolkitCore.Common.VirtualFileSystem;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Commandline.Commands;
using RemoteControlToolkitCore.Common.DeviceBus;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.Networking.NSsh;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.Proxy;
using RemoteControlToolkitCore.DefaultShell;

namespace RemoteControlToolkitCoreServer
{
    class Program
    {
        static void Main(string[] args)
        {
            IHostApplication app = new AppBuilder()
                .AddConfiguration(c =>
                {
                    c.Sources.Clear();
                    c.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
                    c.AddJsonFile("appsettings.json", true, false);
                })
                .AddStartup<Startup>()
                .ConfigureLogging(factory =>
                    factory.AddConsole())
                .UsePluginManager<PluginManager>()
                .LoadFromPluginsFolder()
                .Build();

            app.Run(args);
        }
    }

    public class Startup : IApplicationStartup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDeviceBus();
            services.AddVFS();
            services.AddCommandLine();
            services.AddSingleton<IServerPool, ServerPool>();
            services.AddPipeService();
            services.AddSSH((IConfigurationRoot)_configuration);
            //services.AddSSH(config =>
            //{
            //    config.ListenEndPoints = new List<IPEndPoint>
            //        { new IPEndPoint(IPAddress.Any, 8081)};
            //    config.IdleTimeout = TimeSpan.FromDays(25);
            //    config.MaximumClientConnections = 10;
            //    config.UserAuthenticationBanner =
            //        "You are about to connect to a RemoteControlToolkit server. Any damages caused by the use of this software will be held against the user. Please refer to the user manual before proceeding.";
            //    config.ReceiveMaximumPacketSize = uint.MaxValue;
            //});
        }

        public void PostConfigureServices(IServiceProvider provider, IHostApplication application)
        {
            application.PluginManager.LoadFromType(typeof(DefaultShell));
            application.PluginManager.LoadFromType(typeof(HelpCommand));
            provider.GetService<ApplicationSubsystem>().InitializeSubsystem();
            provider.GetService<ProcessFactorySubsystem>().InitializeSubsystem();;
            provider.GetService<FileSystemSubsystem>().InitializeSubsystem();;
            provider.GetService<DeviceBusSubsystem>().InitializeSubsystem();
        }
    }
}