using System;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline;

namespace RemoteControlToolkitCore.DefaultShell.Parsing.CommandElements
{
    public abstract class BaseCommandElement : ICommandElement
    {
        public ICommandElement Next { get; set; }
        public ICommandElement Previous { get; set; }
        public object Value { get; protected set; }
        public abstract string ToStringImpl();
        public override string ToString()
        {
            try
            {
                return ToStringImpl();
            }
            catch (Exception e)
            {
                throw new CommandElementException(e);
            }
        }
    }
}