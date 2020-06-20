namespace RemoteControlToolkitCore.DefaultShell.Parsing
{
    public enum TokenType
    {
        Semicolon,
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