using System;
using System.ServiceModel;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.ApplicationSystem
{
    [ModuleInstance(TransientMode = true)]
    public abstract class RCTApplication : IApplication
    {

        protected RCTApplication()
        {
        }


        public abstract string ProcessName { get; }
        public abstract CommandResponse Execute(CommandRequest args, RCTProcess context, CancellationToken token);

        public abstract void InitializeServices(IServiceProvider kernel);

        public virtual void Dispose()
        {
           
        }
    }
}