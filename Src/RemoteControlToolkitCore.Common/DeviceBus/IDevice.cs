using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCore.Common.DeviceBus
{
    public interface IDevice
    {
        Stream OpenDevice();
        DeviceInfo GetDeviceInfo();
        TType Query<TType>(string key);
        void SetProperty(string propertyName, object value);
    }
}