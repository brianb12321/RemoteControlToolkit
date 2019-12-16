using System;

namespace RemoteControlToolkitCore.Common.NSsh
{
    [AttributeUsage(AttributeTargets.All)]
    public sealed class CoverageExcludeAttribute : Attribute
    {
        public CoverageExcludeAttribute() { }

        public CoverageExcludeAttribute(string reason)
        {
            _reason = reason;
        }

        private string _reason;

        public string Reason { get { return _reason; } }
    }
}
