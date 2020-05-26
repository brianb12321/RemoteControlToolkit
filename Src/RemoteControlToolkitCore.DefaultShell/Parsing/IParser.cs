using System.Collections.Generic;
using RemoteControlToolkitCore.Common.ApplicationSystem;

namespace RemoteControlToolkitCore.DefaultShell.Parsing
{
    public enum RedirectionMode
    {
        File,
        VFS,
        None
    }
    public interface IParser
    {
        //IReadOnlyList<IReadOnlyList<ICommandElement>> Parse(IReadOnlyList<CommandToken> tokens, ICommandShell shell);
        RedirectionMode OutputRedirected { get; }
        bool OutputAppendMode { get; }
        RedirectionMode ErrorRedirected { get; }
        bool ErrorAppendMode { get; }
        RedirectionMode InputRedirected { get; }
        string Input { get; }
        string Output { get; }
        string Error { get; }
        IReadOnlyList<IReadOnlyList<ICommandElement>> Parse(IReadOnlyList<CommandToken> tokens);
    }
}