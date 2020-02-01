using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.NSsh.ChannelLayer.Console;
using RemoteControlToolkitCore.Common.NSsh.Packets;
using RemoteControlToolkitCore.Common.NSsh.Packets.Channel;
using RemoteControlToolkitCore.Common.NSsh.Packets.Channel.RequestPayloads;
using RemoteControlToolkitCore.Common.NSsh.Types;

namespace RemoteControlToolkitCore.Common.NSsh.ChannelLayer
{
    public abstract class BaseConsoleChannelConsumer : IChannelConsumer
    {
        private const string THREAD_NAME_CONSUMER_CHANNEL = "consumer-channel";
        private const string THREAD_NAME_CONSUMER_STDOUT = "consumer-stdout";
        private const string THREAD_NAME_CONSUMER_STDERR = "consumer-stderr";
        private ILogger<BaseConsoleChannelConsumer> _logger;
        public List<EnvironmentPayload> InitialEnvironmentVariables { get; }

        public BaseConsoleChannelConsumer(ILogger<BaseConsoleChannelConsumer> logger)
        {
            _logger = logger;
            InitialEnvironmentVariables = new List<EnvironmentPayload>();
        }

        #region IChannelConsumer Members

        public PseudoTerminalPayload InitialTerminalConfiguration { get; set; }


        public void Initialise()
        {
            Thread processThread = new Thread(ProcessChannel);
            processThread.Start();
        }

        public void SignalWindowChange(WindowChangePayload args)
        {
            if (args != null)
            {
                console?.SignalWindowChange(args);
            }
        }

        public ChannelRequestType ChannelType { get; set; }

        private IConsole console;

        public IChannelProducer Channel { get; set; }

        public IIdentity AuthenticatedIdentity { get; set; }
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
            Thread stdnInThread = new Thread(() =>
            {
                while (!console.HasClosed)
                {
                    Packet packet = null;

                    try
                    {
                        packet = Channel.GetIncomingPacket();
                        //log.Debug(packet.PacketType);
                    }
                    catch (IOException e)
                    {
                        _logger.LogError("Error reading packet from channel.", e);
                    }
                    catch (ObjectDisposedException e)
                    {
                        _logger.LogError("Error reading packet from channel.", e);
                    }
                    catch (TransportDisconnectException e)
                    {
                        _logger.LogError($"The channel consumer had an unexpected transport layer disconnection: {e.Message}");
                        Close();
                    }

                    try
                    {
                        if (packet != null)
                        {
                            switch (packet.PacketType)
                            {
                                case PacketType.ChannelData:
                                    byte[] data = ((ChannelDataPacket) packet).Data;
                                    string debug = Encoding.UTF8.GetString(data);
                                    if (debug == "\u0003" && console.TerminalHandler.TerminalModes.SIGINT)
                                    {
                                        console.CancellationRequested();
                                    }
                                    else
                                    {
                                        console.Pipe.Write(data, 0, data.Length);
                                        console.Pipe.Flush();
                                    }
                                    break;

                                case PacketType.ChannelEof:
                                    Close();
                                    break;
                                case PacketType.ChannelClose:
                                    Close();
                                    break;
                                default:
                                    throw new NotSupportedException("Packet type is not supported by channel: " + packet.PacketType);
                            }
                        }
                    }
                    catch (IOException e)
                    {
                        _logger.LogError("Error handling packet.", e);
                    }
                }
            });
            stdnInThread.Start();
            console.Start();

            //Thread stdOutThread = new Thread(ProcessStandardOutput);
            //stdOutThread.Start();

            //Thread stdErrThread = new Thread(ProcessStandardError);
            //stdErrThread.Start();

        }
    }
}
