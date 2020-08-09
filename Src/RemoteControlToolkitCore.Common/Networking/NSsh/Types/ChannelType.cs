using System;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Types
{
    public enum ChannelType
    {
        Invalid,

        Session,

        DirectTcp,

        ForwardedTcp
    }

    public static class ChannelTypeHelper
    {
        public const string Session = "session";

        public const string DirectTcp = "direct-tcpip";

        public const string ForwardedTcp = "forwarded-tcpip";  

        public static ChannelType Parse(string value)
        {
            switch (value)
            {
                case Session:
                    return ChannelType.Session;

                case DirectTcp:
                    return ChannelType.DirectTcp;

                case ForwardedTcp:
                    return ChannelType.ForwardedTcp;

                default:
                    throw new ArgumentException("Invalid channel type: " + value, "value");
            }
        }

        public static string ToString(ChannelType channelType)
        {
            switch (channelType)
            {
                case ChannelType.Session:
                    return Session;

                case ChannelType.ForwardedTcp:
                    return ForwardedTcp;

                case ChannelType.DirectTcp:
                    return DirectTcp;

                default:
                    throw new ArgumentException("Invalid channel type: " + channelType, "channelType");
            }
        }
    }
}
