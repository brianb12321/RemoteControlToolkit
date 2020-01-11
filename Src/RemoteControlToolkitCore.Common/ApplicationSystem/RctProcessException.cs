using System;

namespace RemoteControlToolkitCore.Common.ApplicationSystem
{
    [Serializable]
    public class RctProcessException : Exception
    {
        public RctProcessException(string name) : base(name)
        {
            
        }
    }
}