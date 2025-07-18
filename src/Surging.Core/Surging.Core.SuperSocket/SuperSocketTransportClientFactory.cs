using Microsoft.Extensions.Logging;
using SuperSocket.Client;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Transport.Codec;
using Surging.Core.CPlatform.Transport.Implementation;
using Surging.Core.SuperSocket.Adapter;
using System.Collections.Concurrent;
using System.Net;

namespace Surging.Core.SuperSocket
{
    internal class SuperSocketTransportClientFactory : ITransportClientFactory, IDisposable
    {
        private readonly ITransportMessageEncoder _transportMessageEncoder;
        private readonly ITransportMessageDecoder _transportMessageDecoder;
        private readonly ILogger<SuperSocketTransportClientFactory> _logger;
        private readonly IServiceExecutor _serviceExecutor;
        private readonly IHealthCheckService _healthCheckService;
        private readonly ConcurrentDictionary<EndPoint, Lazy<Task<ITransportClient>>> _clients = new ConcurrentDictionary<EndPoint, Lazy<Task<ITransportClient>>>();

        public SuperSocketTransportClientFactory(ITransportMessageCodecFactory codecFactory, IHealthCheckService healthCheckService, ILogger<SuperSocketTransportClientFactory> logger)
    : this(codecFactory, healthCheckService, logger, null)
        {
        }

        public SuperSocketTransportClientFactory(ITransportMessageCodecFactory codecFactory, IHealthCheckService healthCheckService, ILogger<SuperSocketTransportClientFactory> logger, IServiceExecutor serviceExecutor)
        {
            _transportMessageEncoder = codecFactory.GetEncoder();
            _transportMessageDecoder = codecFactory.GetDecoder();
            _logger = logger;
            _serviceExecutor = serviceExecutor;
            _healthCheckService = healthCheckService;
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
                 //客户端对象 
                 var client = new EasyClient<TransportMessage>(new TransportMessagePipelineFilter()).AsClient();
                 var messageListener = new MessageListener();
                 var messageSender = new SuperSocketMessageClientSender(_transportMessageEncoder, client);
                 await client.ConnectAsync(endPoint);
                 client.PackageHandler += async (sender, package) =>
                 {
                     await messageListener.OnReceived(messageSender, package);
                 };
                 client.StartReceive();
                 //创建客户端
                 var transportClient = new TransportClient(messageSender, messageListener, _logger, _serviceExecutor);
                 return transportClient;
             }
                 )).Value;//返回实例
            }
            catch
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
    }
}
