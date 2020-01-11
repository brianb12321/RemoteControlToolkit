using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.NSsh.ChannelLayer;
using RemoteControlToolkitCore.Common.NSsh.Configuration;
using RemoteControlToolkitCore.Common.NSsh.Packets;
using RemoteControlToolkitCore.Common.NSsh.Services;
using RemoteControlToolkitCore.Common.NSsh.TransportLayer;
using RemoteControlToolkitCore.Common.NSsh.Utility;

namespace RemoteControlToolkitCore.Common.NSsh
{
    public static class NSshServiceCollectionExtensions
    {
        public static IServiceCollection AddSSH(this IServiceCollection services, NSshServiceConfiguration config)
        {
            services.AddSingleton(config);
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
            return services;
        }
    }
}