using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCoreLibraryWCF
{
    public class NetTcpBindingFactory : IBindingFactory<NetTcpBinding>
    {
        public NetTcpBinding CreateBindingDefaults()
        {
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.TransportWithMessageCredential, true);
            binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
            binding.SendTimeout = TimeSpan.MaxValue;
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            binding.ReaderQuotas.MaxArrayLength = int.MaxValue;
            binding.ReaderQuotas.MaxBytesPerRead = int.MaxValue;
            binding.ReaderQuotas.MaxDepth = int.MaxValue;
            return binding;
        }
    }
}