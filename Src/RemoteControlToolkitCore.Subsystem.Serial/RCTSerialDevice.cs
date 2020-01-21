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

        public RCTSerialDevice(string port)
        {
            _port = port;
            _info = setupDevice();
        }

        private DeviceInfo setupDevice()
        {
            return new DeviceInfo(_port, _port);
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

        public TType Query<TType>(string key)
        {
            return (TType) _info.Data[key];
        }

        public void SetProperty(string propertyName, object value)
        {
            _info.Data[propertyName] = value;
        }
    }
}