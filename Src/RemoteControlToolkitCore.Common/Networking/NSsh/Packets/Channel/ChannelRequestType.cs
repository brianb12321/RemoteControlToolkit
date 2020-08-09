using System;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Packets.Channel
{
    public enum ChannelRequestType : uint
    {
        Invalid,

        PseudoTerminal,

        Shell,

        WindowChange,

        Environment,

        X11Forwarding,

        AuthenticationAgent,

        ExecuteCommand,

        /// <summary>
        /// See Putty Manual Appendix F. The server must respond with SSH_MSG_CHANNEL_FAILURE.
        /// </summary>
        PuttyWinAdj
    }

    public static class ChannelRequestTypeHelper
    {
        public const string PseudoTerminal = "pty-req";

        public const string Shell = "shell";

        public const string WindowChange = "window-change";

        public const string Environment = "env";

        public const string X11Forwarding = "x11-req";

        public const string AuthenticationAgent = "auth-agent-req";

        public const string ExecuteCommand = "exec";

    	public const string PuttyWinAdj = "winadj@putty.projects.tartarus.org";
        
        public static ChannelRequestType Parse(string value)
        {
            switch (value)
            {
                case PseudoTerminal:
                    return ChannelRequestType.PseudoTerminal;

                case Shell:
                    return ChannelRequestType.Shell;

                case WindowChange:
                    return ChannelRequestType.WindowChange;

                case Environment:
                    return ChannelRequestType.Environment;

                case X11Forwarding:
                    return ChannelRequestType.X11Forwarding;

                case AuthenticationAgent:
                    return ChannelRequestType.AuthenticationAgent;

                case ExecuteCommand:
                    return ChannelRequestType.ExecuteCommand;

                case PuttyWinAdj:
                    return ChannelRequestType.PuttyWinAdj;

                default:
                    throw new ArgumentException("Invalid channel request type: " + value, "value");
            }
        }

        public static string ToString(ChannelRequestType requestType)
        {
            switch (requestType)
            {
                case ChannelRequestType.PseudoTerminal:
                    return PseudoTerminal;

                case ChannelRequestType.Shell:
                    return Shell;

                case ChannelRequestType.WindowChange:
                    return WindowChange;

                case ChannelRequestType.Environment:
                    return Environment;

                case ChannelRequestType.X11Forwarding:
                    return X11Forwarding;

                case ChannelRequestType.AuthenticationAgent:
                    return AuthenticationAgent;

                case ChannelRequestType.ExecuteCommand:
                    return ExecuteCommand;

                case ChannelRequestType.PuttyWinAdj:
                    return PuttyWinAdj;

                default:
                    throw new ArgumentException("Invalid channel request type: " + requestType, "requestType");
            }
        }
    }
}
