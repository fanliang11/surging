using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Thrift.Protocol;

namespace Surging.Core.Thrift
{
    public class ServerHandler
    {
        private readonly Func<TProtocol, TransportMessage, Task> _readAction;
        private readonly ILogger _logger;

        public ServerHandler(Func<TProtocol, TransportMessage, Task> readAction, ILogger logger)
        {
            _readAction = readAction;
            _logger = logger;
        } 

        public async Task ChannelRead(TProtocol context, object message)
        { 
         var transportMessage = (TransportMessage)message;
         await _readAction(context, transportMessage);
          
        }
    }
}