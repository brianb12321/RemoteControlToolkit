using System;
using Microsoft.Win32.SafeHandles;
using RemoteControlToolkitCore.Common.Networking.NSsh.Utility;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Types
{
    public class SafeLsaLogonProcessHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeLsaLogonProcessHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            return Win32Native.LsaDeregisterLogonProcess(base.handle) == Win32Native.StatusSuccess;
        }
    }
}
