﻿using Zio;

namespace RemoteControlToolkitCore.Common.Commandline.Parsing.CommandElements
{
    public class ResourceCommandElement : BaseCommandElement
    {
        private IFileSystem _system;

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