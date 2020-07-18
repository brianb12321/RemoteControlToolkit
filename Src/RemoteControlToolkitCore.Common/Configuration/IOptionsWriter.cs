using System;
using Newtonsoft.Json.Linq;

namespace RemoteControlToolkitCore.Common.Configuration
{
    public interface IOptionsWriter
    {
        void UpdateOptions(Action<JObject> callback, bool reload = true);
    }
}
