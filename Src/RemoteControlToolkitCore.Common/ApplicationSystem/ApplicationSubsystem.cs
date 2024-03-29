﻿using System;
using System.Collections.Generic;
using System.Linq;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.ApplicationSystem
{
    public class ApplicationSubsystem : PluginSubsystem
    {
        private readonly IServiceProvider _provider;

        public ApplicationSubsystem(IPluginManager pluginManager, IServiceProvider provider) : base(pluginManager)
        {
            _provider = provider;
        }

        public IApplication GetApplicationWithoutInit(string name)
        {
            IApplication app = (IApplication)PluginManager.ActivatePluginModule<ApplicationSubsystem>(name);
            if (app == null)
            {
                throw new RctProcessException($"The application \"{name}\" does not exist.");
            }

            return app;
        }
        public IApplication GetApplication(string name)
        {
            var app = GetApplicationWithoutInit(name);
            app.InitializeServices(_provider);
            return app;
        }

        public bool ApplicationExists(string name)
        {
            return GetAllInstalledApplications().Any(a => a.PluginName == name);
        }

        public PluginAttribute[] GetAllInstalledApplications()
        {
            return PluginManager.GetAllPluginModuleInformation<IApplication>();
        }

        public IEnumerable<Type> GetAllApplicationType()
        {
            return PluginManager.ActivateGenericTypes<IApplication>().Select(t => t.GetType());
        }
    }
}