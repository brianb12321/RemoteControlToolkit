using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCore.Common.DeviceBus
{
    public class InlineDevice : IDevice
    {
        private readonly Func<Stream> _streamFunction;
        public DeviceInfo Info { get; set; }

        public InlineDevice(Func<Stream> streamFunction)
        {
            _streamFunction = streamFunction;
        }
        public Stream OpenDevice()
        {
            return _streamFunction();
        }

        public DeviceInfo GetDeviceInfo()
        {
            return Info;
        }
    }
}