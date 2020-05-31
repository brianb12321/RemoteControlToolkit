using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.ApplicationSystem.Factory
{
    public class ProcessFactorySubsystem : PluginSubsystem
    {
        private readonly IServiceProvider _provider;
        public ProcessFactorySubsystem(IPluginManager pluginManager, IServiceProvider provider) : base(pluginManager)
        {
            _provider = provider;
        }

        public override void InitializeSubsystem()
        {
            
        }
        public IProcessBuilder GetProcessBuilder(string factoryName, CommandRequest request, RctProcess parent, IProcessTable table)
        {
            IProcessFactory factory =
                (IProcessFactory)PluginManager.ActivatePluginModule<ProcessFactorySubsystem>(factoryName);

            factory.InitializeServices(_provider);
            return factory.CreateProcessBuilder(request, parent, table);
        }
        public RctProcess CreateProcess(string factoryName, CommandRequest request, RctProcess parent, IProcessTable table)
        {
            return GetProcessBuilder(factoryName, request, parent, table).Build();
        }
    }
}