using System;

namespace RemoteControlToolkitCore.Common.NSsh.Types
{
    public enum ServiceType
    {
        Unknown,

        UserAuthentication,

        Connection
    }

    public static class ServiceTypeAlgorithmHelper
    {
        public const string UserAuthentication = "ssh-userauth";

        public const string Connection = "ssh-connection";

        public static ServiceType Parse(string value)
        {
            switch (value)
            {
                case Connection:
                    return ServiceType.Connection;

                case UserAuthentication:
                    return ServiceType.UserAuthentication;

                default:
                    throw new ArgumentException("Invalid service type: " + value, "value");
            }
        }

        public static string ToString(ServiceType serviceType)
        {
            switch (serviceType)
            {
                case ServiceType.Connection:
                    return Connection;

                case ServiceType.UserAuthentication:
                    return UserAuthentication;

                default:
                    throw new ArgumentException("Invalid service type: " + serviceType, "serviceType");
            }
        }
    }
}
