using RemoteControlToolkitCore.Common.ApplicationSystem;

namespace RemoteControlToolkitCore.Common.Commandline.Parsing.CommandElements
{
    public class StringCommandElement : ICommandElement
    {
        public ICommandElement Next { get; set; }
        public ICommandElement Previous { get; set; }
        public object Value { get; }
        public override string ToString()
        {
            return Value.ToString();
        }

        public StringCommandElement(string value)
        {
            Value = value;
        }
    }
}