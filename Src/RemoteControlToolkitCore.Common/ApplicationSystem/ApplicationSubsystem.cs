using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.ApplicationSystem
{
    public class ApplicationSubsystem : PluginSubsystem
    {
        private readonly ProcessTable _table;
        private readonly IHostApplication _application;
        private readonly ILogger<ApplicationSubsystem> _logger;
        private readonly IServiceProvider _provider;
        public RCTProcess.RCTPRocessFactory Factory { get; }
        public uint LatestProcess { get; }

        public ApplicationSubsystem(IPluginManager pluginManager, IServiceProvider provider) : base(pluginManager)
        {
            _provider = provider;
            _table = new ProcessTable(provider);
            Factory = new RCTProcess.RCTPRocessFactory(_table, provider);
            _application = provider.GetService<IHostApplication>();
            _logger = provider.GetService<ILogger<ApplicationSubsystem>>();
        }

        public IApplication GetApplication(string name)
        {
            IApplication app = (IApplication)PluginManager.ActivatePluginModule<ApplicationSubsystem>(name);
            if (app == null)
            {
                throw new RctProcessException("No such application, script.");
            }
            app.InitializeServices(_provider);
            return app;
        }

        

        public PluginAttribute[] GetAllInstalledApplications()
        {
            return PluginManager.GetAllPluginModuleInformation<IApplication>();
        }

        public IEnumerable<Type> GetAllApplicationType()
        {
            return PluginManager.ActivateGenericTypes<IApplication>().Select(t => t.GetType());
        }

        public bool ApplicationExists(string name)
        {
            return PluginManager.GetAllPluginModuleInformation<IApplication>().Any(a => a.PluginName == name);
        }

        public void AddProcess(RCTProcess process)
        {
            _table.AddProcess(process);
        }

        public void CancelProcess(uint pid)
        {
            _table.CancelProcess(pid);
        }

        public void AbortProcess(uint pid)
        {
            _table.AbortProcess(pid);
        }

        public void RemoveProcess(uint pid)
        {
            _table.RemoveProcess(pid);
        }

        public bool ProcessExists(uint pid)
        {
            return _table.ProcessExists(pid);
        }

        public void SendControlC(uint pid)
        {
            _table.SendControlC(pid);
        }

        public void CloseAll()
        {
            _table.CloseAll();;
        }
    }
}