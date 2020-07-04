using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCoreLibraryWCF
{
    
    /// <summary>
    /// Provides a central way to create WCF bindings.
    /// </summary>
    /// <typeparam name="TBinding"></typeparam>
    public interface IBindingFactory<out TBinding> where TBinding : Binding
    {
        TBinding CreateBindingDefaults();
    }
}