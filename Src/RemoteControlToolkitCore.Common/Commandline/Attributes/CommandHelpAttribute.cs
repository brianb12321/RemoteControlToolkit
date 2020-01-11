using System;

namespace RemoteControlToolkitCore.Common.Commandline.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CommandHelpAttribute : Attribute
    {
        public string Help { get; }

        public CommandHelpAttribute(string help)
        {
            Help = help;
        }
    }
}