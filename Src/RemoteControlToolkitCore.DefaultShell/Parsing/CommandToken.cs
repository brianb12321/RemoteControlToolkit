namespace RemoteControlToolkitCore.DefaultShell.Parsing
{
    public class CommandToken
    {
        public TokenType Type { get; }

        public string Value { get; private set; }

        public CommandToken(string value, TokenType type)
        {
            Value = value;
            Type = type;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}