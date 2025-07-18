using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport.Codec;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Thrift;
using Thrift.Processor;
using Thrift.Protocol;
using Thrift.Protocol.Entities;
using Thrift.Transport;

namespace Surging.Core.Thrift.Extensions
{
    public class TMultiplexedServiceProcessor : ITAsyncProcessor
    {
        private const int WAND = 0xcdc;
        private ILogger _logger;
        private readonly Dictionary<string, ITAsyncProcessor> _serviceProcessorMap =
       new Dictionary<string, ITAsyncProcessor>();
        private TProtocolFactory _protocolFactory = new TBinaryProtocol.Factory();
        private readonly ITransportMessageDecoder _transportMessageDecoder;
        private readonly ITransportMessageEncoder _transportMessageEncoder;
        private readonly ServerHandler _serverHandler;

        public TMultiplexedServiceProcessor(ILogger logger, ITransportMessageDecoder transportMessageDecoder, ITransportMessageEncoder transportMessageEncoder, ServerHandler serverHandler)
        {
            _logger = logger;
            _transportMessageDecoder = transportMessageDecoder;
            _transportMessageEncoder = transportMessageEncoder;
            _serverHandler = serverHandler;
        }

        public async Task<bool> ProcessAsync(TProtocol iprot, TProtocol oprot)
        {
            return await ProcessAsync(iprot, oprot, CancellationToken.None);
        }

        public async Task<bool> ProcessAsync(TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return await Task.FromCanceled<bool>(cancellationToken);
            }

            try
            {  
                var message = await iprot.ReadMessageBeginAsync(cancellationToken);
                if (message.Name=="thrift.surging")
                { 
                      var TransportMessage = await iprot.ReadBinaryAsync(cancellationToken); 
                    await iprot.ReadMessageEndAsync(cancellationToken);
                    await _serverHandler.ChannelRead(oprot, _transportMessageDecoder.Decode(TransportMessage));
                    return true;
                }
                if ((message.Type != TMessageType.Call) && (message.Type != TMessageType.Oneway))
                {
                    await FailAsync(oprot, message, TApplicationException.ExceptionType.InvalidMessageType,
                        "Message exType CALL or ONEWAY expected", cancellationToken);
                    return false;
                }

                // Extract the service name
                var index = message.Name.IndexOf(TMultiplexedProtocol.Separator, StringComparison.Ordinal);
                if (index < 0)
                {
                    await FailAsync(oprot, message, TApplicationException.ExceptionType.InvalidProtocol,
                        $"Service name not found in message name: {message.Name}. Did you forget to use a TMultiplexProtocol in your client?",
                        cancellationToken);
                    return false;
                }

                // Create a new TMessage, something that can be consumed by any TProtocol
                var serviceName = message.Name.Substring(0, index);
                ITAsyncProcessor actualProcessor;
                if (!_serviceProcessorMap.TryGetValue(serviceName, out actualProcessor))
                {
                    await FailAsync(oprot, message, TApplicationException.ExceptionType.InternalError,
                        $"Service name not found: {serviceName}. Did you forget to call RegisterProcessor()?",
                        cancellationToken);
                    return false;
                }

                // Create a new TMessage, removing the service name
                var newMessage = new TMessage(
                    message.Name.Substring(serviceName.Length + TMultiplexedProtocol.Separator.Length),
                    message.Type,
                    message.SeqID);

                // Dispatch processing to the stored processor
                return
                    await
                        actualProcessor.ProcessAsync(new StoredMessageProtocol(iprot, newMessage), oprot,
                            cancellationToken);
            }
            catch (IOException)
            {
                return false; // similar to all other processors
            }
        }

        public void RegisterProcessor(string serviceName, ITAsyncProcessor processor)
        {
            if (_serviceProcessorMap.ContainsKey(serviceName))
            {
                throw new InvalidOperationException(
                    $"Processor map already contains processor with name: '{serviceName}'");
            }

            _serviceProcessorMap.Add(serviceName, processor);
        }

        private async Task FailAsync(TProtocol oprot, TMessage message, TApplicationException.ExceptionType extype,
            string etxt, CancellationToken cancellationToken)
        {
            var appex = new TApplicationException(extype, etxt);

            var newMessage = new TMessage(message.Name, TMessageType.Exception, message.SeqID);

            await oprot.WriteMessageBeginAsync(newMessage, cancellationToken);
            await appex.WriteAsync(oprot, cancellationToken);
            await oprot.WriteMessageEndAsync(cancellationToken);
            await oprot.Transport.FlushAsync(cancellationToken);
        }

        private class StoredMessageProtocol : TProtocolDecorator
        {
            readonly TMessage _msgBegin;

            public StoredMessageProtocol(TProtocol protocol, TMessage messageBegin)
                : base(protocol)
            {
                _msgBegin = messageBegin;
            }

            public override async ValueTask<TMessage> ReadMessageBeginAsync(CancellationToken cancellationToken)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return await Task.FromCanceled<TMessage>(cancellationToken);
                }

                return _msgBegin;
            }

   
        }
    }
}

