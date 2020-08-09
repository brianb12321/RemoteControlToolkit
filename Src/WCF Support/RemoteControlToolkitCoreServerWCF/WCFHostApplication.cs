using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Networking.NSsh;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCoreLibraryWCF;

namespace RemoteControlToolkitCoreServerWCF
{
    public class WCFHostApplication : IHostApplication
    {
        public NetworkSide ExecutingSide => NetworkSide.Server;
        public IAppBuilder Builder { get; }
        public IPluginManager PluginManager { get; }
        private ServiceHost _serviceHost;
        private readonly IServiceProvider _provider;
        private readonly ILogger<WCFHostApplication> _logger;
        private readonly ILogger<ErrorHandler> _errorLogger;
        private readonly IBindingFactory<NetTcpBinding> _tcpBindingFactory;
        public IProcessTable GlobalSystemProcessTable { get; }

        public WCFHostApplication(IAppBuilder builder, IPluginManager manager,
            IServiceProvider provider)
        {
            _logger = provider.GetService<ILogger<WCFHostApplication>>();
            Builder = builder;
            PluginManager = manager;
            _provider = provider;
            _errorLogger = provider.GetService<ILogger<ErrorHandler>>();
            _tcpBindingFactory = provider.GetService<IBindingFactory<NetTcpBinding>>();
            RootFileProvider = new PhysicalFileProvider(Assembly.GetEntryAssembly().Location);
            GlobalSystemProcessTable = new ProcessTable();
            
        }
        public void Dispose()
        {
            if(_serviceHost.State != CommunicationState.Closed)
                _serviceHost.Close();

            System.Windows.Forms.Application.Exit();
        }

        public IFileProvider RootFileProvider { get; }

        public void UnRegisterSession(ISshSession session)
        {
            
        }

        public void Run(string[] args)
        {
            _logger.LogInformation("Starting and configuring service host.");
            _serviceHost = new ServiceHost(typeof(RCTService));
            var binding = _tcpBindingFactory.CreateBindingDefaults();
            var endpoint = _serviceHost.AddServiceEndpoint(typeof(IRCTService), binding, "net.tcp://0.0.0.0:9000/Remote");
            _serviceHost.Description.Behaviors.Add(new InstanceProviderServiceBehavior(_provider));
            _serviceHost.Description.Behaviors.Add(new ErrorBehavior(_errorLogger));
            _serviceHost.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new LoginValidator();
            _serviceHost.Credentials.ServiceCertificate.Certificate = new X509Certificate2("RCTWCFCert.pfx", "password123");
            _serviceHost.Credentials.UserNameAuthentication.UserNamePasswordValidationMode =
                UserNamePasswordValidationMode.Custom;
            _serviceHost.Opened += (sender, e) => _logger.LogInformation("Host opened.");
            _serviceHost.Opening += (sender, e) => _logger.LogInformation("Host opening.");
            _serviceHost.Closed += (sender, e) =>
            {
                _logger.LogInformation("Host closed.");
                Dispose();
            };
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            System.Windows.Forms.Application.Run(new ServerControl(_serviceHost));
        }
    }
}