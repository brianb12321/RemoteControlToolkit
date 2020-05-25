using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCore.Common.Plugin
{
    /// <summary>
    /// Extends extendable objects.
    /// </summary>
    public interface IExtensionProvider<in TExtendableObject> where TExtendableObject : IExtensibleObject<TExtendableObject>
    {
        void GetExtension(TExtendableObject extendableObject);
        void RemoveExtension(TExtendableObject extendableObject);
    }
}