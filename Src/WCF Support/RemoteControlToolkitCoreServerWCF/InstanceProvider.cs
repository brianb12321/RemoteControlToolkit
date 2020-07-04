using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCoreServerWCF
{
    public class InstanceProviderServiceBehavior : IServiceBehavior
    {
        private IServiceProvider _provider;
        public InstanceProviderServiceBehavior(IServiceProvider provider)
        {
            _provider = provider;
        }
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection bindingParameters)
        {
            
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            var instanceProvider = new InstanceProvider(_provider);

            foreach (var channelDispatcherBase in serviceHostBase.ChannelDispatchers)
            {
                var dispatcher = (ChannelDispatcher) channelDispatcherBase;
                foreach (var epDispatcher in dispatcher.Endpoints)
                {
                    // this registers your custom IInstanceProvider
                    epDispatcher.DispatchRuntime.InstanceProvider = instanceProvider;
                }
            }
        }
    }
    public class InstanceProvider : IInstanceProvider
    {
        private IServiceProvider _provider;

        public InstanceProvider(IServiceProvider provider)
        {
            _provider = provider;
        }
        public object GetInstance(InstanceContext instanceContext)
        {
            return GetInstance(instanceContext, null);
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return new RCTService(_provider);
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            
        }
    }
}