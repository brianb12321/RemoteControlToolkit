using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Audio
{
    public class AudioOutSubsystem : BasePluginSubsystem<IAudioOutSubsystem, IAudioProviderModule>, IAudioOutSubsystem
    {
        private ILogger<AudioOutSubsystem> _logger;
        private IServiceProvider _services;

        public AudioOutSubsystem(IPluginLibraryLoader loader, IServiceProvider services, ILogger<AudioOutSubsystem> logger) : base(loader, services)
        {
            _logger = logger;
            _services = services;
        }

        public override void Init()
        {
            _logger.LogInformation("Starting audio subsystem.");
            IAudioProviderModule[] modules = GetAllAudioProviders();
            foreach (var module in modules)
            {
                if (module.BasedOnFile && module.FileExtension == "*")
                {
                    _logger.LogWarning(
                        "The subsystem detected an audio provider that has a file extension of *. This is extension is illegal, therefore the provider has been unloaded.");
                    PluginLoader.UnloadModule<IAudioProviderModule>(module);
                }
            }
            base.Init();
            GetAllAudioProviders().ToList().ForEach(m => m.InitializeServices(_services));
        }

        public IAudioProviderModule GetAudioProvider(string name)
        {
            return PluginLoader.GetAllModules<IAudioProviderModule>()
                .FirstOrDefault(m => m.ProviderName == name);
        }

        public IAudioProviderModule[] GetAllAudioProviders()
        {
            return PluginLoader.GetAllModules<IAudioProviderModule>();
        }

        public string BuildBrowserFilter()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var provider in GetAllAudioProviders().Where(m => m.BasedOnFile))
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