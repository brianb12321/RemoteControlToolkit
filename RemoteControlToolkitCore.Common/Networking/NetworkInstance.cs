using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading;
using Crayon;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Commandline.Parsing.CommandElements;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Networking
{
    public class NetworkInstance : IInstanceSession
    {
        private TcpClient _client;
        private NetworkStream _networkStream;
        private readonly RCTProcess _workingThread;
        private readonly ILogger<NetworkInstance> _logger;
        private StreamReader _streamReader;
        private StreamWriter _streamWriter;
        private RCTProcess _commandShell;

        public IExtensionCollection<IInstanceSession> Extensions { get; }
        public Guid ClientUniqueID { get; }
        public string Username { get; }
        public IProcessTable ProcessTable { get; }
        public NetworkInstance(TcpClient client, ILogger<NetworkInstance> logger, IApplicationSubsystem appSubsystem, IInstanceExtensionProvider[] providers)
        {
            Extensions = new ExtensionCollection<IInstanceSession>(this);
            _client = client;
            _logger = logger;
            _networkStream = _client.GetStream();
            //X509Certificate2 certificate = new X509Certificate2(File.ReadAllBytes("bcscert.pfx"), "abc123");
            //_sslStream = new SslStream(_networkStream, false);
            //_sslStream.AuthenticateAsServer(certificate, false, true);
            ProcessTable = new ProcessTable();
            foreach (IInstanceExtensionProvider provider in providers)
            {
                provider.GetExtension(this);
            }

            _workingThread = ProcessTable.Factory.Create(this, "Network Process", (proc, token) =>
            {
                _streamReader = new StreamReader(_networkStream);
                _streamWriter = new StreamWriter(_networkStream);
                _streamWriter.AutoFlush = true;
                try
                {
                    _commandShell = ProcessTable.Factory.CreateOnApplication(this, appSubsystem.GetApplication("shell"),
                        proc, new CommandRequest(new ICommandElement[] { new StringCommandElement("shell"), }));
                    _commandShell.Extensions.Add(new DefaultShell());
                    _commandShell.SetOut(GetClientWriter());
                    _commandShell.SetIn(GetClientReader());
                    _commandShell.SetError(GetClientWriter());
                    _commandShell.Start();
                    _commandShell.WaitForExit();
                    return new CommandResponse(CommandResponse.CODE_SUCCESS);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "A communication error occurred. The connection will be terminated.");
                    return new CommandResponse(CommandResponse.CODE_FAILURE);
                }
                finally
                {
                    _streamWriter.Close();
                    _streamReader.Close();
                    //_sslStream.Close();
                    _networkStream.Close();
                    foreach (IInstanceExtensionProvider provider in providers)
                    {
                        provider.RemoveExtension(this);
                    }
                    _logger.LogInformation("Channel closed.");
                }
            }, null);
            initializeEnvironmentVariables(_workingThread);
        }
        private void initializeEnvironmentVariables(RCTProcess process)
        {
            _logger.LogInformation("Initializing environment variables.");
            process.EnvironmentVariables.Add("TERMINAL_ROWS", "36");
            process.EnvironmentVariables.Add("TERMINAL_COLUMNS", "130");
            process.EnvironmentVariables.Add(".", "0");
        }

        public void Start()
        {
            _workingThread.Start();
        }

        public void Stop()
        {
            _workingThread.Close();
        }
        public StreamReader GetClientReader()
        {
            return _streamReader;
        }

        public StreamWriter GetClientWriter()
        {
            return _streamWriter;
        }

        public T GetExtension<T>() where T : IExtension<IInstanceSession>
        {
            return Extensions.Find<T>();
        }

        public void AddExtension<T>(T extension) where T : IExtension<IInstanceSession>
        {
            Extensions.Add(extension);
        }
    }
}