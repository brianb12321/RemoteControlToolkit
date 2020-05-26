namespace RemoteControlToolkitCore.DefaultShell.Parsing
{
    public enum TokenType
    {
        EnvironmentVariable,
        Word,
        Quote,
        Script,
        OutRedirect,
        AppendOutRedirect,
        InRedirect,
        Pipe
    }
}