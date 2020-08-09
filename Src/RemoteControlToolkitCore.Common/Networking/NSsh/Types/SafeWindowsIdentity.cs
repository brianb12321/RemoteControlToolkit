using System.Runtime.InteropServices;
using System.Security.Principal;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Types
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
