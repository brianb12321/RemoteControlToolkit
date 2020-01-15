using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCore.Common.DeviceBus
{
    public class DeviceInfo
    {
        public string FileName { get; }
        public string Name { get; }
        public Dictionary<string, object> Data { get; }

        public DeviceInfo(string name, string fileName)
        {
            Name = name;
            FileName = fileName;
            Data = new Dictionary<string, object>();
        }
    }
}