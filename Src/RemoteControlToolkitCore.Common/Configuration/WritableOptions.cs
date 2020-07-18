using System;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RemoteControlToolkitCore.Common.Configuration
{
    public class WritableOptions<T> : IWritableOptions<T> where T : class, new()
    {
        private readonly string _sectionName;
        private readonly IOptionsWriter _writer;
        private readonly IOptionsMonitor<T> _options;

        public WritableOptions(string sectionName, IOptionsWriter writer, IOptionsMonitor<T> options)
        {
            _sectionName = sectionName;
            _writer = writer;
            _options = options;
        }

        public T Value => _options.CurrentValue;

        public void Update(Action<T> applyChanges)
        {
            _writer.UpdateOptions(opt =>
            {
                JToken section;
                T sectionObject = opt.TryGetValue(_sectionName, out section)
                    ? JsonConvert.DeserializeObject<T>(section.ToString())
                    : _options.CurrentValue ?? new T();
                applyChanges(sectionObject);
                string json = JsonConvert.SerializeObject(sectionObject);
                opt[_sectionName] = JObject.Parse(json);
            });
        }
    }
}