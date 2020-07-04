using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCore.Common.Commandline.TerminalExtensions
{
    /// <summary>
    /// Allows users to use their up and down arrows to scroll through their entered history.
    /// </summary>
    public interface ITerminalHistory : IExtension<ITerminalHandler>
    {
        List<string> History { get; }
        int HistoryPosition { get; }
    }
    public class TerminalHistory : ITerminalHistory
    {
        public List<string> History { get; }
        public int HistoryPosition { get; set; }

        public TerminalHistory()
        {
            History = new List<string>();
            HistoryPosition = 0;
        }
        public void Attach(ITerminalHandler owner)
        {
            owner.ReadLineInvoked += (sender, e) => HistoryPosition = History.Count;
            owner.ReadLineCompleted += (sender, input) =>
            {
                if(owner.TerminalModes.ECHO) History.Add(input);
            };
            //Cursor Up
            owner.BindKey("\u001b[A", (StringBuilder sb, StringBuilder renderBuffer, ref int cursorPosition) =>
            {
                if (History.Count > 0 && HistoryPosition > 0)
                {
                    HistoryPosition--;
                    sb.Clear();
                    cursorPosition = 0;
                    string historyCommand = History[HistoryPosition];
                    byte[] historyCommandBytes = Encoding.UTF8.GetBytes(historyCommand);
                    renderBuffer.Clear();
                    owner.RawTerminalIn.Write(historyCommandBytes, 0, historyCommandBytes.Length);
                }

                return true;
            });
            //Cursor Down
            owner.BindKey("\u001b[B", (StringBuilder buffer, StringBuilder renderBuffer, ref int cursorPosition) =>
            {
                if (History.Count > 0 && HistoryPosition < History.Count - 1)
                {
                    HistoryPosition++;
                    buffer.Clear();
                    cursorPosition = 0;
                    string historyCommand = History[HistoryPosition];
                    renderBuffer.Clear();
                    byte[] historyCommandBytes = Encoding.UTF8.GetBytes(historyCommand);
                    owner.RawTerminalIn.Write(historyCommandBytes, 0, historyCommandBytes.Length);
                }

                return true;
            });
        }

        public void Detach(ITerminalHandler owner)
        {
            
        }
    }
}