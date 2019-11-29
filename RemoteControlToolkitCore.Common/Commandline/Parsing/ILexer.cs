using System.Collections.Generic;

namespace RemoteControlToolkitCore.Common.Commandline.Parsing
{
    public interface ILexer
    {
        IReadOnlyList<CommandToken> Lex(string input);
    }
}