using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using NSsh.Server.Utility;
using Microsoft.Win32.SafeHandles;
using NSsh.Server.Types;
using NSsh.Common;

namespace NSsh.Server.Services
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