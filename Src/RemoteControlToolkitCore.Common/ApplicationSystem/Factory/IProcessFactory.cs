using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.ApplicationSystem.Factory
{
    /// <summary>
    /// Creates a new <see cref="RctProcess"/>.
    /// </summary>
    public interface IProcessFactory : IPluginModule<ProcessFactorySubsystem>
    {
        IProcessBuilder CreateProcessBuilder(CommandRequest request, RctProcess parentProcess, IProcessTable table);
    }
}