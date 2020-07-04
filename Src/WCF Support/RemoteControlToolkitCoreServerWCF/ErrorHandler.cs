using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RemoteControlToolkitCoreServerWCF
{
    public class ErrorHandler : IErrorHandler
    {
        private readonly ILogger<ErrorHandler> _logger;

        public ErrorHandler(ILogger<ErrorHandler> logger)
        {
            _logger = logger;
        }
        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            FaultException fexp = new FaultException();
            MessageFault m = fexp.CreateMessageFault();
            fault = Message.CreateMessage(version, m, null);
        }

        public bool HandleError(Exception error)
        {
            _logger.LogError($"Fault Error: {error}");
            return true;
        }
    }
}