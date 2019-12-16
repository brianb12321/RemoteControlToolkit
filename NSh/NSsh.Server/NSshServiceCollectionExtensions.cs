using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSsh.Common.Packets;
using NSsh.Common.Packets.Channel;
using NSsh.Common.Utility;
using NSsh.Server.ChannelLayer;
using NSsh.Server.Configuration;
using NSsh.Server.Services;
using NSsh.Server.TransportLayer;
using NSsh.Server.TransportLayer.State;

namespace NSsh.Server
{
    public static class NSshServiceCollectionExtensions
    {
        public static IServiceCollection AddSSH(this IServiceCollection services, NSshServiceConfiguration config)
        {
            services.AddSingleton(config);
            services.AddSingleton<ISshService, NSshService>();
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
            services.AddTransient<IChannelConsumer, PtyChannelConsumer>();
            services.AddTransient<IChannelCommandConsumer, CommandChannelConsumer>();
            //services.AddSingleton<IImpersonationProvider, ImpersonationProvider>("ImpersonationProvider");
            services.AddSingleton<IImpersonationProvider, BasicImpersonationProvider>();
            services.AddSingleton<IPacketFactory, PacketFactory>();
            return services;
        }
    }
}