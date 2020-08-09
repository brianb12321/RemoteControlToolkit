using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Networking.NSsh.ChannelLayer;
using RemoteControlToolkitCore.Common.Networking.NSsh.Configuration;
using RemoteControlToolkitCore.Common.Networking.NSsh.Packets;
using RemoteControlToolkitCore.Common.Networking.NSsh.Services;
using RemoteControlToolkitCore.Common.Networking.NSsh.TransportLayer;
using RemoteControlToolkitCore.Common.Networking.NSsh.Utility;

namespace RemoteControlToolkitCore.Common.Networking.NSsh
{
    public static class NSshServiceCollectionExtensions
    {
        private static void configureServices(IServiceCollection services)
        {
            services.AddSingleton(typeof(ITerminalHandlerFactory), typeof(StandardTerminalHandlerFactory));
            services.AddTransient<ISshSession, SshSession>();
            services.AddTransient<ITransportLayerManager, TransportLayerManager>();
            services.AddSingleton<StateManager>();
            services.AddSingleton<IKeySetupService, KeySetupService>();
            services.AddSingleton<ISecureRandom, SecureRandom>();
            services.AddSingleton<ICipherFactory, CipherFactory>();
            services.AddSingleton<IMacFactory, MacFactory>();
            services.AddSingleton<IPasswordAuthenticationService, PasswordAuthenticationService>();
            services.AddSingleton<IPublicKeyAuthenticationService, PublicKeyAuthenticationService>();
            services.AddTransient<IChannel, Channel>();
            services.AddTransient<IChannelConsumer, CommandChannelConsumer>();
            services.AddTransient<IChannelCommandConsumer, CommandChannelConsumer>();
            //services.AddSingleton<IImpersonationProvider, ImpersonationProvider>("ImpersonationProvider");
            services.AddSingleton<IImpersonationProvider, BasicImpersonationProvider>();
            services.AddSingleton<IPacketFactory, PacketFactory>();
        }
        public static IServiceCollection AddSSH(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.ConfigureWritable<NSshServiceConfiguration>(configuration, "SSH", "appsettings.json");
            configureServices(services);
            return services;
        }
        public static IServiceCollection AddSSH(this IServiceCollection services, IConfigurationRoot configRoot, Action<NSshServiceConfiguration> config)
        {
            services.ConfigureWritable<NSshServiceConfiguration>(configRoot, "SSH", "appsettings.json", config);
            configureServices(services);
            return services;
        }
    }
}