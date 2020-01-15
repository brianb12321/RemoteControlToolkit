using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.DeviceBus;

namespace RemoteControlToolkitCore.Subsystem.Serial
{
    public class RCTSerialDevice : IDevice
    {
        private readonly string _port;
        private readonly DeviceInfo _info;

        public RCTSerialDevice(DeviceInfo info)
        {
            _info = info;
            _port = info.FileName;
        }
        public Stream OpenDevice()
        {
            SerialPort sp = new SerialPort(_port, (int)_info.Data["BaudRate"], (Parity)_info.Data["Parity"]);
            sp.Open();
            return sp.BaseStream;
        }

        public DeviceInfo GetDeviceInfo()
        {
            return _info;
        }
    }
}