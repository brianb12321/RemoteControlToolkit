namespace RemoteControlToolkitCore.Common.Commandline.Readline
{
    public interface IAutoCompleteHandler
    {
        char[] Separators { get; set; }
        string[] GetSuggestions(string text, int index);
    }
}