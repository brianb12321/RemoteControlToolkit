using System;
using System.Reflection;
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

        public IProcessBuilder CreateProcessBuilder(RctProcess parentProcess, IProcessTable table)
        {
            IProcessBuilder builder = table.CreateProcessBuilder()
                .SetAction((args, current, token) =>
                {
                    IFileSystem fileSystem = current.Extensions.Find<IExtensionFileSystem>().GetFileSystem();
                    Assembly assembly = Assembly.Load(fileSystem.ReadAllBytes(args.Arguments[0]));
                    string[] arguments = args.Arguments;
                    // ReSharper disable once CoVariantArrayConversion
                    object[] parameters = new[] {arguments};
                    assembly.EntryPoint.Invoke(null, parameters);
                    return new CommandResponse(CommandResponse.CODE_SUCCESS);
                })
                .AddProcessExtensions(_provider.GetServices<IExtensionProvider<RctProcess>>())
                .SetProcessName(name => "External .NET Assembly")
                .SetParent(parentProcess);

            return builder;
        }
    }
}