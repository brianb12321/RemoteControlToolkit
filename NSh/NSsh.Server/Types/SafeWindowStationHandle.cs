using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using NSsh.Server.Utility;

namespace NSsh.Server.Types
{
    public class SafeWindowStationHandle : BaseSafeHandle
    {
        public SafeWindowStationHandle(IntPtr handle, bool ownsHandle)
            : base(handle, ownsHandle)
        { }

        protected override bool CloseNativeHandle(IntPtr handle)
        {
            return Win32Native.CloseWindowStation(handle);
        }

        public static SafeWindowStationHandle CreateWindowStation(string name)
        {
            IntPtr handle = Win32Native.CreateWindowStation(
                name,
                0,
                WindowStationAccessMask.AllAccess,
                null);

            if (handle == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return new SafeWindowStationHandle(handle, true);
        }
    }
}
