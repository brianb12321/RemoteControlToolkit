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

namespace NSsh.Server.Services
{
    public class ImpersonationProvider : IImpersonationProvider
    {
        private ILogger<ImpersonationProvider> _logger;

        private SafeWindowStationHandle _windowStation;

        private SafeDesktopHandle _desktop;

        public ImpersonationProvider(ILogger<ImpersonationProvider> logger)
        {
            _logger = logger;
            if (@"NT AUTHORITY\SYSTEM".Equals(WindowsIdentity.GetCurrent().Name, StringComparison.InvariantCultureIgnoreCase))
            {
                _windowStation = SafeWindowStationHandle.CreateWindowStation("nssh");
                Win32Native.SetProcessWindowStation(_windowStation);

                _desktop = SafeDesktopHandle.CreateDesktop("default");
            }
        }

        ~ImpersonationProvider()
        {
            Dispose(false);
        }

        public ProcessDetails CreateProcess(IIdentity identity, string password, ProcessStartInfo processStartInfo)
        {
            string userName = identity.Name.Substring(identity.Name.LastIndexOf('\\') + 1);
            string domain = identity.Name.Substring(0, identity.Name.LastIndexOf('\\'));

            var windowsIdentity = identity as WindowsIdentity;
            if (windowsIdentity == null)
            {
                throw new ArgumentException("Identity must be a windows identity.", "identity");
            }

            SafeFileHandle primaryUserToken;
            var result = Win32Native.DuplicateTokenEx(
                windowsIdentity.Token,
                0,
                new SecurityAttributes(),
                SecurityImpersonationLevel.SecurityImpersonation,
                TokenType.TokenPrimary,
                out primaryUserToken);

            if (!result)
            {
                _logger.LogError("DuplicateTokenEx failed");
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return CreateProcessFromPrimaryToken(processStartInfo, primaryUserToken);
        }

        private ProcessDetails CreateProcessFromPrimaryToken(ProcessStartInfo processStartInfo, SafeHandle primaryUserToken)
        {
            ExtendedProcessDetails processDetails = new ExtendedProcessDetails();

            SafeFileHandle childWriteOutput;
            SafeFileHandle childWriteError;
            SafeFileHandle childReadInput;

            CreatePipe(out processDetails.ParentReadOutput, out childWriteOutput, false);
            CreatePipe(out processDetails.ParentReadError, out childWriteError, false);
            CreatePipe(out processDetails.ParentWriteInput, out childReadInput, true);

            processDetails.StandardInput = new StreamWriter(new FileStream(processDetails.ParentWriteInput, FileAccess.Write));
            processDetails.StandardOutput = new StreamReader(new FileStream(processDetails.ParentReadOutput, FileAccess.Read));
            processDetails.StandardError = new StreamReader(new FileStream(processDetails.ParentReadError, FileAccess.Read));

            StartupInfo startupInfo = new StartupInfo();
            startupInfo.Flags = StartupInfoFlags.UseStdHandles;
            startupInfo.StdOutput = childWriteOutput;
            startupInfo.StdError = childWriteError;
            startupInfo.StdInput = childReadInput;
            startupInfo.Desktop = _desktop == null ? null : "nssh\\default";
            
            IntPtr environmentBlock = IntPtr.Zero;

            try
            {
                if (!Win32Native.CreateEnvironmentBlock(out environmentBlock, primaryUserToken, false))
                {
                    _logger.LogError("CreateEnvironmentBlock failed");
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                StringBuilder commandLine = new StringBuilder(processStartInfo.Arguments);

                SecurityAttributes processAttributes = new SecurityAttributes();
                SecurityAttributes threadAttributes = new SecurityAttributes();
                ProcessInformation processInformation = new ProcessInformation();

                if (!Win32Native.CreateProcessWithToken(
                    primaryUserToken,
                    LogonFlags.LogonWithProfile,
                    processStartInfo.FileName,
                    commandLine,
                    ProcessCreationFlags.CreateNewConsole | ProcessCreationFlags.CreateNoWindow | ProcessCreationFlags.CreateUnicodeEnvironment,
                    environmentBlock,
                    processStartInfo.WorkingDirectory,
                    startupInfo,
                    out processInformation))
                {
                    _logger.LogError("CreateProcessWithToken failed");
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                processDetails.ProcessId = processInformation.ProcessId;
                processDetails.Process = Process.GetProcessById(processInformation.ProcessId);
            }
            finally
            {
                if (environmentBlock != IntPtr.Zero)
                {
                    Win32Native.DestroyEnvironmentBlock(environmentBlock);
                }
            }

            childReadInput.Dispose();
            childWriteOutput.Dispose();
            childWriteError.Dispose();

            return processDetails;
        }

        private void CreatePipe(out SafeFileHandle parentHandle, out SafeFileHandle childHandle, bool parentReading)
        {
            SecurityAttributes pipeAttributes = new SecurityAttributes();
            pipeAttributes.InheritHandle = true;

            SafeFileHandle writePipeHandle = null;

            try
            {
                if (parentReading)
                {
                    Win32Native.CreatePipe(out childHandle, out writePipeHandle, pipeAttributes, 0);
                }
                else
                {
                    Win32Native.CreatePipe(out writePipeHandle, out childHandle, pipeAttributes, 0);
                }

                if (!Win32Native.DuplicateHandle(
                    Process.GetCurrentProcess().Handle,
                    writePipeHandle,
                    Process.GetCurrentProcess().Handle,
                    out parentHandle,
                    0,
                    false,
                    DuplicateHandleOptions.SameAccess))
                {
                    _logger.LogError("DuplicateHandle failed");
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                if (writePipeHandle != null)
                {
                    writePipeHandle.Close();
                }
            }
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
                if (_desktop != null) _desktop.Dispose();
                if (_windowStation != null) _windowStation.Dispose();
            }
        }

        #endregion
    }

    public class ExtendedProcessDetails : ProcessDetails
    {
        public SafeFileHandle ParentReadOutput;
        public SafeFileHandle ParentReadError;
        public SafeFileHandle ParentWriteInput;

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (ParentReadOutput != null) ParentReadOutput.Dispose();
                    if (ParentReadError != null) ParentReadError.Dispose();
                    if (ParentWriteInput != null) ParentWriteInput.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}