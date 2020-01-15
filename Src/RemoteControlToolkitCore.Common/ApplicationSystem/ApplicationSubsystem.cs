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
        private readonly ProcessTable _table;
        private readonly IHostApplication _application;
        private readonly ILogger<ApplicationSubsystem> _logger;
        public RCTProcess.RCTPRocessFactory Factory { get; }
        public uint LatestProcess { get; }

        public ApplicationSubsystem(IPluginLibraryLoader loader, IServiceProvider services) : base(loader, services)
        {
            _table = new ProcessTable(services);
            Factory = new RCTProcess.RCTPRocessFactory(_table, services);
            _application = services.GetService<IHostApplication>();
            _logger = services.GetService<ILogger<ApplicationSubsystem>>();
        }

        public override void Init()
        {
            
        }

        public IApplication GetApplication(string name)
        {
            IApplication app = PluginLoader.ActivateModuleByName<IApplication>(name);
            if(app == null) throw new RctProcessException("No such application, script");
            return app;
        }

        public Type GetApplicationType(string name)
        {
            return PluginLoader.GetModuleTypeByName<IApplication>(name);
        }

        public PluginModuleAttribute[] GetAllInstalledApplications()
        {
            return PluginLoader.GetAllModuleAttribute<IApplication>();
        }

        public bool ApplicationExists(string name)
        {
            return PluginLoader.HasPluginModule<IApplication>(name);
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