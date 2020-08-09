using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.Plugin;
using Microsoft.Extensions.FileProviders;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.ApplicationSystem.Factory;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Configuration;
using RemoteControlToolkitCore.Common.Networking.NSsh;

[assembly: PluginLibrary("CommonPlugin","Common Plugin")]
namespace RemoteControlToolkitCore.Common
{
    public class Application : IHostApplication
    {
        private readonly ILogger<Application> _logger;
        private ProcessFactorySubsystem _factorySubsystem;
        private readonly IServiceProvider _provider;
        private RctProcess _sshProcess;
        private ApplicationOptions _applicationOptions;
        public NetworkSide ExecutingSide { get; }
        public IProcessTable GlobalSystemProcessTable { get; }
        public IAppBuilder Builder { get; }
        public IPluginManager PluginManager { get; }
        public IFileProvider RootFileProvider { get; }

        public Application(ILogger<Application> logger,
            NetworkSide side,
            IAppBuilder builder,
            IPluginManager pluginManager,
            IServiceProvider provider)
        {
            _logger = logger;
            ExecutingSide = side;
            Builder = builder;
            PluginManager = pluginManager;
            RootFileProvider = new PhysicalFileProvider(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            GlobalSystemProcessTable = new ProcessTable();
            _provider = provider;
        }

        public void UnRegisterSession(ISshSession session)
        {
            _sshProcess.EventBus.Publish(new UnRegisterSessionEvent(this, session));
        }

        public void Run(string[] args)
        {
            _factorySubsystem = _provider.GetService<ProcessFactorySubsystem>();
            _applicationOptions = _provider.GetService<IWritableOptions<ApplicationOptions>>().Value;
            _sshProcess = GlobalSystemProcessTable.CreateProcessBuilder()
                .SetProcessName(name => "BootLoader")
                .SetAction((innerArgs, context, token) =>
                {
                    List<RctProcess> processesToStart = new List<RctProcess>();
                    foreach (var process in _applicationOptions.BootLoader.StartupPrograms)
                    {
                        var newProcess =
                            _factorySubsystem.CreateProcess("Application", context, GlobalSystemProcessTable);
                        newProcess.CommandLineName = process.Name;
                        newProcess.Arguments = process.Arguments;
                        newProcess.ThreadError += (sender, e) =>
                            _logger.LogError($"An error occurred with a startup process: {e.Message}");
                        processesToStart.Add(newProcess);
                    }

                    foreach (var process in processesToStart)
                    {
                        process.Start();
                    }
                    //Enter loop until cancelled.
                    while(!token.IsCancellationRequested) {}
                    return new CommandResponse(CommandResponse.CODE_SUCCESS);
                })
                .Build();
            //_sshProcess = _factorySubsystem.CreateProcess("Application", null, GlobalSystemProcessTable);
            _sshProcess.CommandLineName = "system-boot";
            _sshProcess.Start();
            _sshProcess.WaitForExit();
        }

        public void RegisterSession(ISshSession session, Thread sessionThread)
        {

        }

        public void Dispose()
        {
            _sshProcess.Close();
        }
    }
}