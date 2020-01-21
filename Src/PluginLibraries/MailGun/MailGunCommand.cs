using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Crayon;
using Google.Apis.Auth.OAuth2;
using MailKit.Security;
using MimeKit;
using NDesk.Options;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Plugin;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace MailGun
{
    [PluginModule(Name = "mlg", ExecutingSide = NetworkSide.Server | NetworkSide.Proxy)]
    public class MailGunCommand : RCTApplication
    {
        public override string ProcessName => "Mail Gun Command.";
        public override CommandResponse Execute(CommandRequest args, RCTProcess context, CancellationToken token)
        {
            bool showHelp = false;
            string server = string.Empty;
            string username = string.Empty;
            int port = 587;
            string subject = string.Empty;
            string from = string.Empty;
            bool ssl = false;
            bool google = false;
            SecureString password = new SecureString();
            List<string> to = new List<string>();
            List<string> cc = new List<string>();
            List<string> bcc = new List<string>();
            OptionSet options = new OptionSet()
                .Add("server|s=", "The server to connect to.", v => server = v)
                .Add("port|p=", "The SMTP port number.", v => port = int.Parse(v))
                .Add("username|u=", "The username to authenticate with.", v => username = v)
                .Add("password=", "Open-text password to send to the server.", v =>
                {
                    foreach (char p in v)
                    {
                        password.AppendChar(p);
                    }
                    password.MakeReadOnly();
                })
                .Add("Google|g", "Use Google's authentication mechanism", v => google = true)
                .Add("ssl|l", "Use SSL for transport security.", v => ssl = true)
                .Add("subject|j=", "Subject of the email.", v => subject = v)
                .Add("from|f=", "The from address.", v => from = v)
                .Add("to|t=", "Specifies the list (denoted by ;) of to addresses.", v =>
                {
                    string[] values = v.Split(';');
                    to.AddRange(values);
                })
                .Add("cc|c=", "Specifies the list (denoted by ;) of cc addresses.", v =>
                {
                    string[] values = v.Split(';');
                    cc.AddRange(values);
                })
                .Add("bcc|b=", "Specifies the list (denoted by ;) of bcc addresses.", v =>
                {
                    string[] values = v.Split(';');
                    bcc.AddRange(values);
                })
                .Add("showHelp|?", "Displays the help screen.", v => showHelp = true);

            options.Parse(args.Arguments.Select(v => v.ToString()));
            if (showHelp)
            {
                options.WriteOptionDescriptions(context.Out);
            }
            else
            {
                MimeMessage message = new MimeMessage();
                message.From.Add(new MailboxAddress(from));
                foreach (string toAddress in to)
                {
                    message.To.Add(new MailboxAddress(toAddress));
                }
                foreach (string ccAddress in cc)
                {
                    message.Cc.Add(new MailboxAddress(ccAddress));
                }
                foreach (string bccAddress in bcc)
                {
                    message.Bcc.Add(new MailboxAddress(bccAddress));
                }
                message.Subject = subject;
                TextPart text = new TextPart("html");
                text.Text = context.In.ReadToEnd();
                message.Body = text;
                SmtpClient client = new SmtpClient();
                try
                {
                    client.Connect(server, port, ssl, token);
                    if (google)
                    {
                        client.Authenticate(new SaslMechanismOAuth2(username, password.ToString()), token);
                    }
                    else
                    {
                        client.Authenticate(username, password.ToString(), token);
                    }
                    client.Send(message, token);
                    client.Disconnect(true, token);
                }
                catch (Exception e)
                {
                    context.Out.WriteLine($"Unable to send email: {e.Message}".Red());
                    if (e.InnerException != null)
                    {
                        context.Out.WriteLine();
                        context.Out.WriteLine($"Inner Exception: {e.InnerException.Message}".Red());
                    }
                }
            }
            return new CommandResponse(CommandResponse.CODE_SUCCESS);
        }

        public override void InitializeServices(IServiceProvider kernel)
        {
            
        }
    }
}