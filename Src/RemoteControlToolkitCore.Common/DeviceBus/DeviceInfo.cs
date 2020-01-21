using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCore.Common.DeviceBus
{
    public class DeviceInfo
    {
        public string FileName => (string)Data["FileName"];
        public string Name => (string)Data["Name"];
        public Dictionary<string, object> Data { get; }

        public DeviceInfo(string name, string fileName)
        {
            Data = new Dictionary<string, object>();
            Data.Add("Name", name);
            Data.Add("FileName", fileName);
        }
    }
}