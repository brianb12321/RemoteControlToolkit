using System;
using System.ComponentModel;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using RemoteControlToolkitCore.Common.Networking.NSsh.Types;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Utility
{
    public static class Win32Native
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CreatePipe(
            out SafeFileHandle hReadPipe,
            out SafeFileHandle hWritePipe,            
            SecurityAttributes lpPipeAttributes,
            uint nSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DuplicateHandle(
            IntPtr hSourceProcessHandle,
            SafeHandle hSourceHandle, 
            IntPtr hTargetProcessHandle, 
            out SafeFileHandle lpTargetHandle,
            uint dwDesiredAccess, 
            bool bInheritHandle, 
            DuplicateHandleOptions Options);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool CreateProcessAsUser(
            SafeHandle hToken,
            string lpApplicationName,
            StringBuilder lpCommandLine,
            SecurityAttributes lpProcessAttributes,
            SecurityAttributes lpThreadAttributes,
            bool bInheritHandles,
            ProcessCreationFlags dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            StartupInfo lpStartupInfo,
            out ProcessInformation lpProcessInformation);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool CreateProcessWithLogon(
            string lpUsername,
            string lpDomain,
            string lpPassword,
            LogonFlags dwLogonFlags,
            string lpApplicationName,
            StringBuilder lpCommandLine,
            ProcessCreationFlags dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            StartupInfo lpStartupInfo,
            out ProcessInformation lpProcessInfo);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool CreateProcessWithToken(
            SafeHandle hToken,
            LogonFlags dwLogonFlags,
            string lpApplicationName,
            StringBuilder lpCommandLine,
            ProcessCreationFlags dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            StartupInfo lpStartupInfo,
            out ProcessInformation lpProcessInfo);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LogonUser(
          string principal,
          string authority,
          string password,
          LogonSessionType logonType,
          LogonProvider logonProvider,
          out SafeFileHandle token);

        [DllImport("userenv.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool LoadUserProfile(SafeHandle hToken, ref ProfileInfo lpProfileInfo);

        [DllImport("userenv.dll", SetLastError = true)]
        public static extern bool CreateEnvironmentBlock(out IntPtr lpEnvironment, SafeHandle hToken, bool bInherit);

        [DllImport("userenv.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyEnvironmentBlock(IntPtr lpEnvironment);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool DuplicateTokenEx(
            IntPtr hExistingToken,
            uint dwDesiredAccess,
            SecurityAttributes lpTokenAttributes,
            SecurityImpersonationLevel ImpersonationLevel,
            TokenType TokenType,
            out SafeFileHandle phNewToken);

        [DllImport("user32.dll", EntryPoint = "CreateWindowStation", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateWindowStation(
                        [MarshalAs(UnmanagedType.LPWStr)] string name,
                        [MarshalAs(UnmanagedType.U4)] int reserved,      // must be zero.
                        [MarshalAs(UnmanagedType.U4)] WindowStationAccessMask desiredAccess,
                        [MarshalAs(UnmanagedType.LPStruct)] SecurityAttributes attributes);

        [return: MarshalAs(UnmanagedType.Bool)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        [DllImport("user32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool CloseWindowStation(IntPtr hWinsta);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetProcessWindowStation(SafeHandle hWinSta);

        // ms-help://MS.VSCC.v80/MS.MSDN.v80/MS.WIN32COM.v10.en/dllproc/base/createdesktop.htm
        [DllImport("user32.dll", EntryPoint = "CreateDesktop", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateDesktop(
                        [MarshalAs(UnmanagedType.LPWStr)] string desktopName,
                        [MarshalAs(UnmanagedType.LPWStr)] string device, // must be null.
                        [MarshalAs(UnmanagedType.LPWStr)] string deviceMode, // must be null,
                        [MarshalAs(UnmanagedType.U4)] int flags,  // use 0
                        [MarshalAs(UnmanagedType.U4)] DesktopAccessMask accessMask,
                        [MarshalAs(UnmanagedType.LPStruct)] SecurityAttributes attributes);

        // ms-help://MS.VSCC.v80/MS.MSDN.v80/MS.WIN32COM.v10.en/dllproc/base/closedesktop.htm
        [DllImport("user32.dll", EntryPoint = "CloseDesktop", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseDesktop(IntPtr handle);

        [DllImport("secur32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint LsaRegisterLogonProcess(
            ref LsaString logonProcessName,
            out SafeLsaLogonProcessHandle lsaLogonHandle,
            out IntPtr securityMode);

        [DllImport("secur32.dll", SetLastError = false)]
        public static extern uint LsaDeregisterLogonProcess(IntPtr lsaLogonHandle);

        [DllImport("secur32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int LsaLookupAuthenticationPackage(
            SafeLsaLogonProcessHandle lsaLogonHandle,
            ref UnicodeString packageName,
            out uint authenticationPackage);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern uint LsaNtStatusToWinError(uint status);

        [DllImport("secur32.dll", SetLastError = false)]
        public static extern uint LsaConnectUntrusted(out SafeLsaLogonProcessHandle lsaLogonHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool AllocateLocallyUniqueId(out ulong luid);

        [DllImport("secur32.dll", SetLastError = false)]
        public static extern uint LsaLogonUser(
            SafeLsaLogonProcessHandle lsaLogonHandle,
            ref LsaString originName,
            LogonSessionType logonType,
            uint authenticationPackage,
            IntPtr authenticationInformation,
            uint authenticationInformationLength,
            IntPtr localGroups,
            ref TokenSource sourceContext,
            out IntPtr profileBuffer,
            out uint profileBufferLength,
            out ulong logonId,
            out IntPtr token,
            out QuotaLimits quotas,
            out uint subStatus);
        [DllImport("secur32.dll", SetLastError = false)]
        public static extern uint NtCreateToken(
            out IntPtr tokenHandle,
            AccessMask desiredAccess,
  //IN POBJECT_ATTRIBUTES   ObjectAttributes,
            TokenType TokenType,
            uint authenticationId,
            ref long expirationTime,
  //IN PTOKEN_USER          TokenUser,
  //IN PTOKEN_GROUPS        TokenGroups,
  //IN PTOKEN_PRIVILEGES    TokenPrivileges,
  //IN PTOKEN_OWNER         TokenOwner,
  //IN PTOKEN_PRIMARY_GROUP TokenPrimaryGroup,
  //IN PTOKEN_DEFAULT_DACL  TokenDefaultDacl,
            TokenSource tokenSource);

        public const string MicrosoftAuthenticationPackage1_0 = "MICROSOFT_AUTHENTICATION_PACKAGE_V1_0";

        public const uint StatusSuccess = 0;
    }

    [Flags]
    public enum AccessMask : uint
    {
        StandardRightsRequired = 0x000F0000,
        StandardRightsRead = 0x00020000,
        TokenAssignPrimary = 0x0001,
        TokenDuplicate = 0x0002,
        TokenImpersonate = 0x0004,
        TokenQuery = 0x0008,
        TokenQuerySource = 0x0010,
        TokenAdjustPrivileges = 0x0020,
        TokenAdjustGroups = 0x0040,
        TokenAdjustDefault = 0x0080,
        TokenAdjustSessionId = 0x0100,

        TokenRead = (StandardRightsRead | TokenQuery),

        AllAccess = (
            StandardRightsRequired | TokenAssignPrimary |
            TokenDuplicate | TokenImpersonate | TokenQuery | TokenQuerySource |
            TokenAdjustPrivileges | TokenAdjustGroups | TokenAdjustDefault |
            TokenAdjustSessionId)
    }

    [Flags]
    public enum WindowStationAccessMask : uint
    {
        None = 0,

        EnumDesktops = 0x0001,
        ReadAttributes = 0x0002,
        AcessClipboard = 0x0004,
        CreateDesktop = 0x0008,
        WriteAttributes = 0x0010,
        AccessGlobalAtoms = 0x0020,
        ExitWindows = 0x0040,
        Enumerate = 0x0100,
        ReadScreen = 0x0200,

        AllAccess = (
            EnumDesktops | ReadAttributes | AcessClipboard | CreateDesktop | 
            WriteAttributes | AccessGlobalAtoms | ExitWindows | Enumerate | 
            ReadScreen | StandardAccess.StandardRightsRequired)
    }

    [Flags]
    public enum DesktopAccessMask : uint
    {
        None = 0,
        ReadObjects = 0x0001,
        CreateWindow = 0x0002,
        CreateMenu = 0x0004,
        HookControl = 0x0008,
        JournalRecord = 0x0010,
        JournalPlayback = 0x0020,
        Enumerate = 0x0040,
        WriteObjects = 0x0080,
        SwitchDesktop = 0x0100,

        GenericAll = (
            ReadObjects | CreateWindow | CreateMenu | HookControl | 
            JournalRecord | JournalPlayback | Enumerate | WriteObjects | 
            SwitchDesktop |StandardAccess.StandardRightsRequired)
    }

    [Flags]
    public enum StandardAccess : uint
    {
        StandardRightsRequired = 0x000F
    }

    public enum LogonFlags : uint
    {
        LogonWithProfile = 1,
        LogonNetCredentialsOnly = 2
    }

    public enum DuplicateHandleOptions : uint
    {
        CloseSource = 1,
        SameAccess = 2
    }

    public enum SecurityImpersonationLevel
    {
        SecurityAnonymous,
        SecurityIdentification,
        SecurityImpersonation,
        SecurityDelegation
    }

    public enum TokenType : uint
    {
        TokenPrimary = 1,
        TokenImpersonation
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public class SecurityAttributes
    {
        uint Length = 0xc;

        public IntPtr SecurityDescriptor;

        public bool InheritHandle;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class ProfileInfo
    {
        uint Size = 32;

        public uint Flags;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string UserName;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string ProfilePath;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string DefaultPath;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string ServerName;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string PolicyPath;

        public IntPtr hProfile;

        public ProfileInfo(string userName)
        {
            UserName = userName;
        }
    }

    [Flags]
    public enum ProcessCreationFlags : uint
    {
        CreateBreakawayFromJob = 0x01000000,
        CreateDefaultErrorMode = 0x04000000,
        CreateNewConsole = 0x00000010,
        CreateNewProcessGroup = 0x00000200,
        CreateNoWindow = 0x08000000,
        CreateProtectedProcess = 0x00040000,
        CreatePreserveCodeAuthzLevel = 0x02000000,
        CreateSeparateWowVdm = 0x00001000,
        CreateSuspended = 0x00000004,
        CreateUnicodeEnvironment = 0x00000400,
        DebugOnlyThisProcess = 0x00000002,
        DebugProcess = 0x00000001,
        DetachedProcess = 0x00000008,
        ExtendedStartupinfoPresent = 0x00080000
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ProcessInformation
    {
        public IntPtr Process;
        public IntPtr Thread;
        public int ProcessId;
        public uint ThreadId;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class StartupInfo
    {
        uint Size = 0x44;
        string Reserved1;
        public string Desktop;
        public string Title;
        public uint X;
        public uint Y;
        public uint XSize;
        public uint YSize;
        public uint XCountChars;
        public uint YCountChars;
        public uint FillAttribute;
        public StartupInfoFlags Flags;
        public ushort ShowWindow;
        ushort Reserved2;
        IntPtr Reserved3 = IntPtr.Zero;
        public SafeFileHandle StdInput = new SafeFileHandle(IntPtr.Zero, false);
        public SafeFileHandle StdOutput = new SafeFileHandle(IntPtr.Zero, false);
        public SafeFileHandle StdError = new SafeFileHandle(IntPtr.Zero, false);
    }

    public enum StartupInfoFlags : uint
    {
        UseStdHandles = 0x100
    }

    public enum LogonSessionType : uint
    {
        Interactive = 2,
        Network,
        Batch,
        Service,
        Proxy,
        Unlock,
        NetworkCleartext,
        NewCredentials,
        RemoteInteractive,
        CachedInteractive,
        CachedRemoteInteractive,
        CachedUnlock
    }

    public enum LogonProvider : uint
    {
        /// <summary>
        /// The platform default.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Don't use.
        /// </summary>
        WinNT35,

        /// <summary>
        /// NTLM.
        /// </summary>
        WinNT40,

        /// <summary>
        /// Kerberos or NTLM.
        /// </summary>
        WinNT50
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct UnicodeString : IDisposable
    {
        public ushort Length;
        public ushort MaximumLength;
        private IntPtr buffer;

        public UnicodeString(string s)
        {
            Length = (ushort)(s.Length * 2);
            MaximumLength = (ushort)(Length + 2);
            buffer = Marshal.StringToHGlobalUni(s);
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(buffer);
            buffer = IntPtr.Zero;
        }

        public override string ToString()
        {
            return Marshal.PtrToStringUni(buffer);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LsaString : IDisposable
    {
        public ushort Length;
        public ushort MaximumLength;
        private IntPtr buffer;

        public LsaString(string s)
        {
            Length = (ushort)s.Length;
            MaximumLength = (ushort)(Length + 1);
            buffer = Marshal.StringToHGlobalAnsi(s);
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(buffer);
            buffer = IntPtr.Zero;
        }

        public override string ToString()
        {
            return Marshal.PtrToStringAnsi(buffer);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TokenSource
    {
        public TokenSource(string name)
        {
            SourceName = new byte[8];
            System.Text.Encoding.GetEncoding(1252).GetBytes(name, 0, name.Length, SourceName, 0);

            if (!Win32Native.AllocateLocallyUniqueId(out SourceIdentifier))
            {
                throw new Win32Exception("Error allocating locally unique Id.");
            }
        }

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] SourceName;
        public ulong SourceIdentifier;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct QuotaLimits
    {
        uint PagedPoolLimit;
        uint NonPagedPoolLimit;
        uint MinimumWorkingSetSize;
        uint MaximumWorkingSetSize;
        uint PagefileLimit;
        long TimeLimit;
    }
}
