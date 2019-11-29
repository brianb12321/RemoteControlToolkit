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
        public IExtensionCollection<IApplication> Extensions { get; }

        protected RCTApplication()
        {
            Extensions = new ExtensionCollection<IApplication>(this);
        }


        public abstract string ProcessName { get; }
        public abstract CommandResponse Execute(CommandRequest args, RCTProcess context, CancellationToken token);

        public abstract void InitializeServices(IServiceProvider kernel);

        public virtual void Dispose()
        {
           
        }
    }
}