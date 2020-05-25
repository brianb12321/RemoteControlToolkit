using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCore.Common.Plugin
{
    public abstract class PluginModule<TSubsystem> : IPluginModule<TSubsystem> where TSubsystem : PluginSubsystem
    {
        public TSubsystem ParentSubsystem { get; set; }
        public PluginAttribute GetPluginAttribute()
        {
            return GetType().GetCustomAttribute<PluginAttribute>();
        }

        public virtual void InitializeServices(IServiceProvider provider)
        {
            
        }
    }
}