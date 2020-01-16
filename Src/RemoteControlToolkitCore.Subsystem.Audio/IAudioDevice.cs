﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using RemoteControlToolkitCore.Common.DeviceBus;

namespace RemoteControlToolkitCore.Subsystem.Audio
{
    public interface IAudioDevice : IDevice
    {
        IWavePlayer Init(IWaveProvider provider);
    }
}