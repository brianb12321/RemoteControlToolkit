using System;

namespace RemoteControlToolkitCore.Common.ApplicationSystem
{
    public class ControlCEventArgs : EventArgs
    {
        public bool CloseProcess { get; set; } = true;
    }
}