using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using NSsh.Server.Utility;

namespace NSsh.Server.Types
{
    public class SafeDesktopHandle : BaseSafeHandle
    {
        public SafeDesktopHandle(IntPtr handle, bool ownsHandle)
            : base(handle, ownsHandle)
        { }

        protected override bool CloseNativeHandle(IntPtr handle)
        {
            return Win32Native.CloseDesktop(handle);
        }

        public static SafeDesktopHandle CreateDesktop(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name", "Desktop name cannot be null");
            }

            IntPtr handle = Win32Native.CreateDesktop(
                name,
                null,
                null,
                0,
                DesktopAccessMask.GenericAll,
                null);

            if (handle == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return new SafeDesktopHandle(handle, true);
        }
    }
}
