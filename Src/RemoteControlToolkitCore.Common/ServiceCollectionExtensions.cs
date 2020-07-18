using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RemoteControlToolkitCore.Common.Configuration;

namespace RemoteControlToolkitCore.Common
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureWritable<T>(this IServiceCollection services,
            IConfigurationRoot configuration, string sectionName, string file)
            where T : class, new()
        {
            services.Configure<T>(configuration.GetSection(sectionName));
            return services.AddTransient<IWritableOptions<T>>(provider =>
            {
                var environment = provider.GetService<IHostApplication>();
                var options = provider.GetService<IOptionsMonitor<T>>();
                IOptionsWriter writer = new OptionsWriter(environment, configuration, file);
                return new WritableOptions<T>(sectionName, writer, options);
            });
        }
        public static IServiceCollection ConfigureWritable<T>(this IServiceCollection services,
            IConfigurationRoot configuration, string sectionName, string file, Action<T> seed)
            where T : class, new()
        {
            services.Configure(seed);
            return services.AddTransient<IWritableOptions<T>>(provider =>
            {
                var environment = provider.GetService<IHostApplication>();
                var options = provider.GetService<IOptionsMonitor<T>>();
                IOptionsWriter writer = new OptionsWriter(environment, configuration, file);
                return new WritableOptions<T>(sectionName, writer, options);
            });
        }
    }
}