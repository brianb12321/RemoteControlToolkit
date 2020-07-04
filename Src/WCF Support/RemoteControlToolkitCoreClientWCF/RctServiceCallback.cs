using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCoreLibraryWCF;

namespace RemoteControlToolkitCoreClientWCF
{
    [CallbackBehavior(IncludeExceptionDetailInFaults = true,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        UseSynchronizationContext = true)]
    public class RctServiceCallback : IRCTServiceCallback
    {
        public void SetTitle(string title)
        {
            Console.Title = title;
        }

        public void Print(string message)
        {
            Console.Write(message);
        }

        public string ReadLine()
        {
            return Console.ReadLine();
        }

        public string ReadToEnd()
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                var text = ReadLine();
                if (text == null) break;
                sb.AppendLine(text);
            }

            return sb.ToString();
        }

        public void ClearScreen()
        {
            Console.Clear();
        }

        public char Read()
        {
            return (char)Console.Read();
        }

        public int Read(byte[] data, int offset, int count)
        {
            return Console.OpenStandardInput().Read(data, offset, count);
        }

        public (uint columns, uint rows) GetTerminalDimensions()
        {
            return ((uint)Console.WindowWidth, (uint)Console.WindowHeight);
        }

        public (string row, string column) GetCursorPosition()
        {
            return (Console.CursorTop.ToString(), Console.CursorLeft.ToString());
        }

        public void UpdateCursorPosition(int col, int row)
        {
            Console.CursorLeft = col;
            Console.CursorTop = row;
        }
    }
}