using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting.Hosting.Shell;
using RemoteControlToolkitCore.Common.NSsh.ChannelLayer;
using RemoteControlToolkitCore.Common.NSsh.Packets.Channel.RequestPayloads;
using RemoteControlToolkitCore.Common.NSsh.Utility;
using IConsole = RemoteControlToolkitCore.Common.NSsh.ChannelLayer.Console.IConsole;

namespace RemoteControlToolkitCore.Common.Commandline
{
    public class EchoConsole : IConsole
    {
        private EchoStream m_echoStream = new EchoStream();
        private EchoStream m_errorStream = new EchoStream();

        public void SignalWindowChange(WindowChangePayload args)
        {
            
        }

        public IChannelProducer Producer { get; }
        public AnonymousPipeServerStream Pipe { get; }
        public TextWriter StandardInput { get; private set; }
        public TextReader StandardOutput { get; private set; }
        public TextReader StandardError { get; private set; }
        public void Start()
        {
            throw new NotImplementedException();
        }

        public bool HasClosed { get; private set; }

        public event EventHandler Closed;

        public EchoConsole()
        {
            StandardInput = new StreamWriter(m_echoStream);
            StandardOutput = new StreamReader(m_echoStream);
            StandardError = new StreamReader(m_errorStream);
        }

        public void Close()
        {
            m_echoStream.Close();
            m_errorStream.Close();
        }
    }
}
