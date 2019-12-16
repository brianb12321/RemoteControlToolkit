using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using NSsh.Server.Services;
using NSsh.Common.Utility;
using NSsh.Common.Packets;
using NSsh.Common.Types;
using System.Security.Principal;
using Microsoft.Extensions.Logging;
using NSsh.Common.Packets.Channel;
using NSsh.Server.ChannelLayer.Console;

namespace NSsh.Server.ChannelLayer
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

        protected abstract IConsole CreateConsole(IInstance);

        public void ProcessChannel(object unused)
        {
            Thread.CurrentThread.Name = THREAD_NAME_CONSUMER_CHANNEL;

            console = CreateConsole();
            console.Closed += delegate { Channel.Close(); };

            Thread stdOutThread = new Thread(ProcessStandardOutput);
            stdOutThread.Start();

            Thread stdErrThread = new Thread(ProcessStandardError);
            stdErrThread.Start();

            while (!console.HasClosed)
            {
                Packet packet;

                try
                {
                    packet = Channel.GetIncomingPacket();
                    //log.Debug(packet.PacketType);
                }
                catch (IOException e)
                {
                    _logger.LogError("Error reading packet from channel.", e);
                    return;
                }
                catch (ObjectDisposedException e)
                {
                    _logger.LogError("Error reading packet from channel.", e);
                    return;
                }
                catch (TransportDisconnectException e)
                {
                    _logger.LogError("The channel consumer had an unexpected transport layer disconnection.");
                    return;
                }

                try
                {
                    switch (packet.PacketType)
                    {
                        case PacketType.ChannelData:
                            console.StandardInput.Write(Encoding.UTF8.GetString(((ChannelDataPacket)packet).Data));
                            console.StandardInput.Flush();
                            break;

                        case PacketType.ChannelEof:
                            console.StandardInput.Close();
                            break;

                        case PacketType.ChannelClose:
                            console.Close();
                            break;

                        default:
                            throw new NotSupportedException("Packet type is not supported by channel: " + packet.PacketType);
                    }
                }
                catch (IOException e)
                {
                    _logger.LogError("Error handling packet.", e);
                    return;
                }
            }
        }

        public void ProcessStandardOutput()
        {
            char[] buffer = new char[0x1000];

            try
            {
                Thread.CurrentThread.Name = THREAD_NAME_CONSUMER_STDOUT;

                while (!console.HasClosed)
                {
                    int readBytes = console.StandardOutput.Read(buffer, 0, buffer.Length);
                    Channel.SendData(Encoding.UTF8.GetBytes(buffer, 0, readBytes));
                }
            }
            catch (IOException e)
            {
                _logger.LogError("Error reading standard output.", e);
            }
        }

        public void ProcessStandardError()
        {
            char[] buffer = new char[0x1000];

            try
            {
                Thread.CurrentThread.Name = THREAD_NAME_CONSUMER_STDERR;

                while (!console.HasClosed)
                {
                    int readBytes = console.StandardError.Read(buffer, 0, buffer.Length);
                    Channel.SendErrorData(Encoding.UTF8.GetBytes(buffer, 0, readBytes));
                }
            }
            catch (IOException e)
            {
                _logger.LogError("Error reading standard error.", e);
            }
        }
    }
}
