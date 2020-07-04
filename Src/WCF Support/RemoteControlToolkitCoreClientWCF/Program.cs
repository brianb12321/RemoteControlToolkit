using System;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Security;
using RemoteControlToolkitCoreLibraryWCF;

namespace RemoteControlToolkitCoreClientWCF
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.CursorSize = 100;
            Console.Write("Enter address of remote server: ");
            string address = Console.ReadLine();
            Uri addressUri = new Uri(address);
            Console.Write("Enter username: ");
            string username = Console.ReadLine();
            Console.Write("Enter Password: ");
            string password = Console.ReadLine();
            var binding = new NetTcpBindingFactory().CreateBindingDefaults();
            RctServiceCallback callback = new RctServiceCallback();

            //Use a certificate as DNS identify because later on, the user will be able to provide the certificate file on the command-line.
            DuplexChannelFactory<IRCTService> channelFactory = new DuplexChannelFactory<IRCTService>(callback, binding,
                new EndpointAddress(addressUri,
                    EndpointIdentity.CreateX509CertificateIdentity(new X509Certificate2("RCTWCFCert.pfx", "password123"))));
            channelFactory.Credentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;
            channelFactory.Credentials.UserName.UserName = username;
            channelFactory.Credentials.UserName.Password = password;
            var channel = channelFactory.CreateChannel();
            Console.CancelKeyPress += (sender, e) =>
            {
                channel.SendControlC();
            };
            channel.StartShell();
            channelFactory.Close();
        }
    }
}