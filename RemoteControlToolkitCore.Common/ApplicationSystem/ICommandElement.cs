namespace RemoteControlToolkitCore.Common.ApplicationSystem
{
    /// <summary>
    /// Represents a command argument that can be expanded when needed without the need of a parser.
    /// </summary>
    public interface ICommandElement
    {
        ICommandElement Next { get; set; }
        ICommandElement Previous { get; set; }
        object Value { get; }
        string ToString();
    }
}