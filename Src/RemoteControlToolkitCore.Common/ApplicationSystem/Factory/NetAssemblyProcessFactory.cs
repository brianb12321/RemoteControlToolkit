using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.VirtualFileSystem;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;

namespace RemoteControlToolkitCore.Common.ApplicationSystem.Factory
{
    [Plugin(PluginName = "NetAssembly")]
    public class NetAssemblyProcessFactory : PluginModule<ProcessFactorySubsystem>, IProcessFactory
    {
        private IServiceProvider _provider;
        public override void InitializeServices(IServiceProvider provider)
        {
            _provider = provider;
        }

        public IProcessBuilder CreateProcessBuilder(CommandRequest request, RctProcess parentProcess, IProcessTable table)
        {
            IProcessBuilder builder = table.CreateProcessBuilder()
                .SetAction((current, token) =>
                {
                    IFileSystem fileSystem = current.Extensions.Find<IExtensionFileSystem>().GetFileSystem();
                    Assembly assembly = Assembly.Load(fileSystem.ReadAllBytes(request.Arguments[0]));
                    string[] arguments = request.Arguments;
                    // ReSharper disable once CoVariantArrayConversion
                    object[] parameters = new[] {arguments};
                    assembly.EntryPoint.Invoke(null, parameters);
                    return new CommandResponse(CommandResponse.CODE_SUCCESS);
                })
                .AddProcessExtensions(_provider.GetServices<IExtensionProvider<RctProcess>>())
                .SetProcessName("External .NET Assembly")
                .SetParent(parentProcess);

            return builder;
        }
    }
}