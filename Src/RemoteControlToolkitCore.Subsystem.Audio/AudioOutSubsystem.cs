using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Audio
{
    public class AudioOutSubsystem : PluginSubsystem
    {
        private readonly ILogger<AudioOutSubsystem> _logger;
        private readonly IServiceProvider _services;
        private readonly ConcurrentBag<IAudioProviderModule> _loadedModules;

        public AudioOutSubsystem(IPluginManager pluginManager, ILogger<AudioOutSubsystem> logger, IServiceProvider services) : base(pluginManager)
        {
            _logger = logger;
            _services = services;
            _loadedModules = new ConcurrentBag<IAudioProviderModule>();
        }

        public override void InitializeSubsystem()
        {
            _logger.LogInformation("Starting audio subsystem.");
            IAudioProviderModule[] modules = PluginManager.ActivateAllPluginModules<AudioOutSubsystem>()
                .Select(m => m as IAudioProviderModule)
                .ToArray();
            foreach (var module in modules)
            {
                if (module.BasedOnFile && module.FileExtension == "*")
                {
                    _logger.LogWarning(
                        "The subsystem detected an audio provider that has a file extension of *. This is extension is illegal, therefore the provider has been unloaded.");
                }
                else
                {
                    //Add to the dictionary.
                    _loadedModules.Add(module);
                }
            }
            //Initialize services.
            _loadedModules.ToList().ForEach(m => m.InitializeServices(_services));
        }

        public IAudioProviderModule GetAudioProvider(string name)
        {
            return _loadedModules.FirstOrDefault(m => m.GetPluginAttribute().PluginName == name);
        }

        public IAudioProviderModule[] GetAllAudioProviders()
        {
            return _loadedModules.ToArray();
        }

        public string BuildBrowserFilter()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var provider in _loadedModules.Where(m => m.BasedOnFile))
            {
                sb.Append(provider.FileDescription);
                sb.Append("|");
                sb.Append($"*.{provider.FileExtension}");
                sb.Append("|");
            }

            sb.Length--;
            return sb.ToString();
        }
    }
}