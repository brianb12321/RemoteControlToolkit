using System;
using System.Diagnostics;
using System.Security;
using System.Security.Principal;
using RemoteControlToolkitCore.Common.NSsh.Types;

namespace RemoteControlToolkitCore.Common.NSsh.Services
{
    public class BasicImpersonationProvider : IImpersonationProvider
    {

        ~BasicImpersonationProvider()
        {
            Dispose(false);
        }

        public ProcessDetails CreateProcess(IIdentity identity, string password, ProcessStartInfo processStartInfo)
        {
            string userName = identity.Name.Substring(identity.Name.LastIndexOf('\\') + 1);
            string domain = identity.Name.Substring(0, identity.Name.LastIndexOf('\\'));

            processStartInfo.UserName = userName;
            processStartInfo.Domain = domain;
            processStartInfo.Password = new SecureString();
            processStartInfo.Password.Append(password);

            Process process = Process.Start(processStartInfo);

            return new ProcessDetails()
            {
                Process = process,
                ProcessId = process.Id,
                StandardError = process.StandardError,
                StandardOutput = process.StandardOutput,
                StandardInput = process.StandardInput
            };
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
            }
        }

        #endregion
    }
}