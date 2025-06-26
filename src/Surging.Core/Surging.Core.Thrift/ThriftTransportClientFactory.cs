using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Transport.Codec;
using Surging.Core.CPlatform.Transport.Implementation;
using Surging.Core.Thrift.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Thrift.Protocol;
using Thrift.Transport;
using Thrift.Transport.Client;

namespace Surging.Core.Thrift
{
   public class ThriftTransportClientFactory : ITransportClientFactory, IDisposable
    {
        #region Field

        private readonly ITransportMessageEncoder _transportMessageEncoder;
        private readonly ITransportMessageDecoder _transportMessageDecoder;
        private readonly ILogger<ThriftTransportClientFactory> _logger;
        private readonly IServiceExecutor _serviceExecutor;
        private readonly IHealthCheckService _healthCheckService;
        private readonly ConcurrentDictionary<EndPoint, Lazy<Task<ITransportClient>>> _clients = new ConcurrentDictionary<EndPoint, Lazy<Task<ITransportClient>>>();
     
        #endregion Field

        #region Constructor

        public ThriftTransportClientFactory(ITransportMessageCodecFactory codecFactory, IHealthCheckService healthCheckService, ILogger<ThriftTransportClientFactory> logger)
            : this(codecFactory, healthCheckService, logger, null)
        {
        }

        public ThriftTransportClientFactory(ITransportMessageCodecFactory codecFactory, IHealthCheckService healthCheckService, ILogger<ThriftTransportClientFactory> logger, IServiceExecutor serviceExecutor)
        {
            _transportMessageEncoder = codecFactory.GetEncoder();
            _transportMessageDecoder = codecFactory.GetDecoder();
            _logger = logger;
            _healthCheckService = healthCheckService;
            _serviceExecutor = serviceExecutor; 
    
        }

        public async Task<ITransportClient> CreateClientAsync(EndPoint endPoint)
        {
            var key = endPoint;
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"准备为服务端地址：{key}创建客户端。");
            try
            {
                return await _clients.GetOrAdd(key
                    , k => new Lazy<Task<ITransportClient>>(async () =>
                    {
                        var ipEndPoint = k as IPEndPoint;
                        var transport = new TSocketTransport(ipEndPoint.Address.ToString(), ipEndPoint.Port);
                        var tran = new TFramedTransport(transport);
                        var protocol = new TBinaryProtocol(tran);
                          var  mp = new TMultiplexedProtocol(protocol, "thrift.surging"); 
                        var messageListener = new MessageListener();
                        var messageSender = new ThriftMessageClientSender(_transportMessageEncoder, protocol);
                        var result= new TThriftClient(protocol, messageSender, messageListener, new ChannelHandler( _transportMessageDecoder, messageListener, messageSender,_logger),  _logger);
                        await result.OpenTransportAsync();
                        return result;
                    }
                    )).Value;//返回实例
            }
            catch(Exception ex)
            {
                //移除
                _clients.TryRemove(key, out var value);
                var ipEndPoint = endPoint as IPEndPoint;
                //标记这个地址是失败的请求
                if (ipEndPoint != null)
                    await _healthCheckService.MarkFailure(new IpAddressModel(ipEndPoint.Address.ToString(), ipEndPoint.Port));
                throw;
            }
        }

        public void Dispose()
        {
            foreach (var client in _clients.Values)
            {
                (client as IDisposable)?.Dispose();
            }
        }

        #endregion Constructor

    }
}
