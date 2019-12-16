using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NSsh.Server.ChannelLayer.Console
{
    public interface IConsole
    {
        TextReader StandardInput { get; }
        TextWriter StandardOutput { get; }
        TextWriter StandardError { get; }
        void Close();
        bool HasClosed { get; }
        event EventHandler Closed;
    }
}
