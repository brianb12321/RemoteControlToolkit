using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.ApplicationSystem
{
    public class ApplicationSubsystem : BasePluginSubsystem<IApplicationSubsystem, IApplication>, IApplicationSubsystem
    {
        private readonly IServiceProvider _services;
        private readonly ProcessTable _table;
        private readonly IHostApplication _application;
        private readonly ILogger<ApplicationSubsystem> _logger;
        public RCTProcess.RCTPRocessFactory Factory { get; }
        public uint LatestProcess { get; }

        public ApplicationSubsystem(IPluginLibraryLoader loader, IServiceProvider services) : base(loader, services)
        {
            _table = new ProcessTable();
            _services = services;
            Factory = new RCTProcess.RCTPRocessFactory(_table);
            _application = services.GetService<IHostApplication>();
            _logger = services.GetService<ILogger<ApplicationSubsystem>>();
        }

        public override void Init()
        {
            
        }

        public IApplication GetApplication(string name)
        {
            bool appExists = ApplicationExists(name);
            Type applicationType = GetApplicationType(name);
            //Check if an external IApplication object can execute the command.
            if (!appExists)
            {
                throw new RctProcessException("No such application, script");
            }

            var app = (IApplication) Activator.CreateInstance(applicationType);
            app.InitializeServices(_services);
            return app;
        }

        public Type GetApplicationType(string name)
        {
            return PluginLoader.GetModuleTypes<IApplication>()
                .FirstOrDefault(t => t.GetCustomAttribute<PluginModuleAttribute>() != null && t.GetCustomAttribute<PluginModuleAttribute>().Name == name && t.GetCustomAttribute<PluginModuleAttribute>().ExecutingSide.HasFlag(_application.ExecutingSide));
        }

        public PluginModuleAttribute[] GetAllInstalledApplications()
        {
            return PluginLoader.GetModuleTypes<IApplication>()
                .Select(t => t.GetCustomAttribute<PluginModuleAttribute>())
                .Where(a => a.ExecutingSide.HasFlag(_application.ExecutingSide))
                .ToArray();
        }

        public bool ApplicationExists(string name)
        {
            return PluginLoader.GetModuleTypes<IApplication>().Count(app => app.GetCustomAttribute<PluginModuleAttribute>() != null
                                                                            && app.GetCustomAttribute<PluginModuleAttribute>().Name == name
                                                                            && app.GetCustomAttribute<PluginModuleAttribute>().ExecutingSide.HasFlag(_application.ExecutingSide)) > 0;
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