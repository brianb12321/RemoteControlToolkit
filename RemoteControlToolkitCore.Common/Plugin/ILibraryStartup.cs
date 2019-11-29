using Microsoft.Extensions.DependencyInjection;

namespace RemoteControlToolkitCore.Common.Plugin
{
    public interface ILibraryStartup
    {
        void Init(IServiceCollection services);
        void PostInit();
    }
}