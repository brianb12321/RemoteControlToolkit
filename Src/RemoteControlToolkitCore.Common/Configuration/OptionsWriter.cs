using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RemoteControlToolkitCore.Common.Configuration
{
    public class OptionsWriter : IOptionsWriter
    {
        private readonly IHostApplication _application;
        private readonly IConfigurationRoot _configuration;
        private readonly string _file;

        public OptionsWriter(IHostApplication environment, IConfigurationRoot configuration, string file)
        {
            _application = environment;
            _configuration = configuration;
            _file = file;
        }

        public void UpdateOptions(Action<JObject> callback, bool reload = true)
        {
            IFileProvider fileProvider = _application.RootFileProvider;
            IFileInfo fi = fileProvider.GetFileInfo(_file);
            JObject config = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(fi.PhysicalPath));
            callback(config);
            using(var stream = File.OpenWrite(fi.PhysicalPath))
            using(var streamWriter = new StreamWriter(stream))
            using(var jsonWriter = new JsonTextWriter(streamWriter))
            {
                jsonWriter.Formatting = Formatting.Indented;
                stream.SetLength(0);
                config.WriteTo(jsonWriter);
            }

            _configuration.Reload();
        }
    }
}