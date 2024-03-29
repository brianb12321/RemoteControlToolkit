﻿using System;
using Microsoft.Win32.SafeHandles;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Types
{
    public abstract class BaseSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        protected BaseSafeHandle(IntPtr handle, bool ownsHandle)
            : base(ownsHandle)
        {
            SetHandle(handle);
        }

        /// <summary>
        /// Close the native handle. The real call is dependent on whether it is a WindowsStation or a Desktop.
        /// </summary>
        /// <param name="handle">Handle to be closed.</param>
        /// <returns>true if successful, false other wise.</returns>
        protected abstract bool CloseNativeHandle(IntPtr handle);

        protected override bool ReleaseHandle()
        {
            if (IsInvalid)
            {
                return false;
            }

            bool closed = CloseNativeHandle(this.handle);
            
            if (closed)
            {
                SetHandle(IntPtr.Zero);
            }
            
            return closed;
        }
    }
}
