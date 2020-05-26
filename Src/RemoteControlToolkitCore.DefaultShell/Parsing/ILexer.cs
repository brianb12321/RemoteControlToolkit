using System.Collections.Generic;

namespace RemoteControlToolkitCore.DefaultShell.Parsing
{
    public interface ILexer
    {
        IReadOnlyList<CommandToken> Lex(string input);
    }
}