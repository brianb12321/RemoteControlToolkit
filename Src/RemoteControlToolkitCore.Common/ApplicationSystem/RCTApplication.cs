using System;
using System.ServiceModel;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.ApplicationSystem
{
    public abstract class RCTApplication : PluginModule<ApplicationSubsystem>, IApplication
    {

        protected RCTApplication()
        {
        }


        public abstract string ProcessName { get; }
        public abstract CommandResponse Execute(CommandRequest args, RctProcess context, CancellationToken token);


        public virtual void Dispose()
        {
           
        }
    }
}