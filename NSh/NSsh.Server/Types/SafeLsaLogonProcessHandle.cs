using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32.SafeHandles;
using NSsh.Server.Utility;

namespace NSsh.Server.Types
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
