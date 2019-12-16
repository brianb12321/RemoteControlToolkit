using System.Security.Principal;
using System.Threading;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.NSsh.ChannelLayer.Console;
using RemoteControlToolkitCore.Common.NSsh.Packets.Channel;

namespace RemoteControlToolkitCore.Common.NSsh.ChannelLayer
{
    public abstract class BaseConsoleChannelConsumer : IChannelConsumer
    {
        private const string THREAD_NAME_CONSUMER_CHANNEL = "consumer-channel";
        private const string THREAD_NAME_CONSUMER_STDOUT = "consumer-stdout";
        private const string THREAD_NAME_CONSUMER_STDERR = "consumer-stderr";
        private ILogger<BaseConsoleChannelConsumer> _logger;

        public BaseConsoleChannelConsumer(ILogger<BaseConsoleChannelConsumer> logger)
        {
            _logger = logger;
        }

        #region IChannelConsumer Members

        public void Initialise()
        {
            Thread processThread = new Thread(ProcessChannel);
            processThread.Start();
        }

        public ChannelRequestType ChannelType { get; set; }

        private IConsole console;

        public IChannelProducer Channel { get; set; }

        public IIdentity AuthenticatedIdentity { get; set; }

        public string Password { get; set; }

        public void Close()
        {
            if (console != null)
            {
                console.Close();
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Close();
        }

        #endregion

        protected abstract IConsole CreateConsole();

        public void ProcessChannel()
        {
            console = CreateConsole();
            console.Closed += delegate { Channel.Close(); };
            Thread.CurrentThread.Name = THREAD_NAME_CONSUMER_CHANNEL;
            //Thread stdnInThread = new Thread(() =>
            //{
            //    while (!console.HasClosed)
            //    {
                    
            //    }
            //});
            //stdnInThread.Start();
            console.Start();

            //Thread stdOutThread = new Thread(ProcessStandardOutput);
            //stdOutThread.Start();

            //Thread stdErrThread = new Thread(ProcessStandardError);
            //stdErrThread.Start();

        }
    }
}
