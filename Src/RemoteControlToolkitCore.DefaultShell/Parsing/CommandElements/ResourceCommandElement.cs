using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;

namespace RemoteControlToolkitCore.DefaultShell.Parsing.CommandElements
{
    public class ResourceCommandElement : BaseCommandElement
    {
        private readonly IFileSystem _system;

        public ResourceCommandElement(string value, IFileSystem system)
        {
            _system = system;
            Value = value;
        }

        public override string ToStringImpl()
        {
            return _system.ReadAllText(Value.ToString());
        }
    }
}