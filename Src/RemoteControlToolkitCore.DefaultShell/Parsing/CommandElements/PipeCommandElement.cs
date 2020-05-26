using RemoteControlToolkitCore.Common.ApplicationSystem;

namespace RemoteControlToolkitCore.DefaultShell.Parsing.CommandElements
{
    public class PipeCommandElement : ICommandElement
    {
        public ICommandElement Next { get; set; }
        public ICommandElement Previous { get; set; }
        public object Value => "|";

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}