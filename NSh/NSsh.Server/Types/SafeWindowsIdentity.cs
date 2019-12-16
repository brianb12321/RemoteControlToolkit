using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using NSsh.Server.Utility;
using System.Runtime.InteropServices;

namespace NSsh.Server.Types
{
    public class SafeWindowsIdentity : WindowsIdentity
    {
        private SafeHandle _token;

        public SafeWindowsIdentity(SafeHandle token)
            : base(token.DangerousGetHandle())
        {
            _token = token;
        }

        ~SafeWindowsIdentity()
        {
            Dispose(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _token.Dispose();
            }
        }
    }
}
