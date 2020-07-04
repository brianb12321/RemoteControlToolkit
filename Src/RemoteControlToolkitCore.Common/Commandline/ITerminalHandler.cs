using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.NSsh.Packets.Channel.RequestPayloads;

namespace RemoteControlToolkitCore.Common.Commandline
{
    public delegate bool KeyBindingDelegate(StringBuilder buffer, StringBuilder renderBuffer, ref int cursorPosition);
    /// <summary>
    /// Represents an allocated pseudo-terminal control device.
    /// </summary>
    public interface ITerminalHandler : IExtension<IInstanceSession>, IExtensibleObject<ITerminalHandler>
    {
        TextWriter TerminalOut { get; }
        TextReader TerminalIn { get; }
        Stream RawTerminalIn { get; }
        event EventHandler TerminalDimensionsChanged;
        event EventHandler ReadLineInvoked;
        event EventHandler<string> ReadLineCompleted;
        string TerminalName { get; }
        (string row, string column) GetCursorPosition();
        uint TerminalRows { get; set; }
        uint TerminalColumns { get; set; }
        PseudoTerminalMode TerminalModes { get; }
        void Clear();
        string ClearScreenCursorDown(bool writeCode = false);
        void ClearRow();
        void Bell();
        string ReadLine();
        string ReadToEnd();
        char Read();
        void BindKey(string key, KeyBindingDelegate function);
        int ReadFromPipe(char[] buffer, int offset, int count);
        void UpdateHomePosition(int col, int row);
        string UpdateCursorPosition(int col, int row, bool writeCode = false);
        void MoveCursorLeft(int count = 1);
        void MoveCursorRight(int count = 1);
        void MoveCursorUp(int count = 1);
        void MoveCursorDown(int count = 1);
        void SetDisplayMode(int code);
        void Reset();
        void ScrollDown();
        void SetTitle(string title);
        void SetForeground(ConsoleColor color);
        void SetBackground(ConsoleColor color);
        void HideCursor();
        void ShowCursor();
        void ResizeWindow(int column, int row);
    }
}