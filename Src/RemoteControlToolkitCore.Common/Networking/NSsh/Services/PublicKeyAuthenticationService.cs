using System;
using System.ComponentModel;
using System.Security.Principal;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.Networking.NSsh.Packets;
using RemoteControlToolkitCore.Common.Networking.NSsh.Types;
using RemoteControlToolkitCore.Common.Networking.NSsh.Utility;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Services
{
    public class PublicKeyAuthenticationService : IPublicKeyAuthenticationService, IDisposable
    {
        private ILogger<PublicKeyAuthenticationService> _logger;

        SafeLsaLogonProcessHandle _lsaLogonHandle;
        LsaString _logonProcessName;

        public PublicKeyAuthenticationService(ILogger<PublicKeyAuthenticationService> logger)
        {
            _logger = logger;
            //var privilege = new Privilege("SeTcbPrivilege");
            //privilege.Enable();

            _logonProcessName = new LsaString("NSsh");
            IntPtr securityMode; // MSDN says to ignore this
            var result = Win32Native.LsaRegisterLogonProcess(ref _logonProcessName, out _lsaLogonHandle, out securityMode);

            if (result == Win32Native.StatusSuccess)
            {
                return;
            }
            else if (Win32Native.LsaNtStatusToWinError(result) == ResultWin32.ERROR_ACCESS_DENIED)
            {
                _logger.LogInformation("Access denied registering logon process");
                result = Win32Native.LsaConnectUntrusted(out _lsaLogonHandle);
            }

            if (result != Win32Native.StatusSuccess)
            {
                _logger.LogInformation("Failed to register logon process: " + result);
                throw new Win32Exception("Error registering LSA logon process: " + result);
            }
        }

        ~PublicKeyAuthenticationService()
        {
            Dispose(false);
        }

        public IIdentity CreateIdentity(string userName, UserAuthPublicKeyPayload publicKeyPayload)
        {
            // TODO: if userName contains domain in the form domain\user or user@domain then retain that domain
            // TODO: check key!! omg!!

            UnicodeString packageName = new UnicodeString(Win32Native.MicrosoftAuthenticationPackage1_0);
            uint authenticationPackage;

            var result = Win32Native.LsaLookupAuthenticationPackage(
                _lsaLogonHandle,
                ref packageName,
                out authenticationPackage);

            if (result != Win32Native.StatusSuccess)
            {
                _logger.LogInformation("Failed to lookup LSA authentication package: " + result);
                throw new Win32Exception("Failed to lookup LSA authentication package: " + result);
            }



            throw new NotImplementedException();
        }

        #region IDisposable Members

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _lsaLogonHandle.Dispose();
                _logonProcessName.Dispose();
            }
        }

        #endregion
    }
}
