using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Transport.Codec;
using Surging.Core.CPlatform.Transport.Implementation;
using Surging.Core.DotNetty.Adapter;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DotNetty
{
    /// <summary>
    /// 基于DotNetty的传输客户端工厂。
    /// </summary>
    public class DotNettyTransportClientFactory : ITransportClientFactory, IDisposable
    {
        #region Field

        private readonly ITransportMessageEncoder _transportMessageEncoder;
        private readonly ITransportMessageDecoder _transportMessageDecoder;
        private readonly ILogger<DotNettyTransportClientFactory> _logger;
        private readonly IServiceExecutor _serviceExecutor;
        private readonly IHealthCheckService _healthCheckService;
        private readonly ConcurrentDictionary<EndPoint, Lazy<Task<ITransportClient>>> _clients = new ConcurrentDictionary<EndPoint, Lazy<Task<ITransportClient>>>();
        private readonly Bootstrap _bootstrap;

        private static readonly AttributeKey<IMessageSender> messageSenderKey = AttributeKey<IMessageSender>.ValueOf(typeof(DotNettyTransportClientFactory), nameof(IMessageSender));
        private static readonly AttributeKey<IMessageListener> messageListenerKey = AttributeKey<IMessageListener>.ValueOf(typeof(DotNettyTransportClientFactory), nameof(IMessageListener));
        private static readonly AttributeKey<EndPoint> origEndPointKey = AttributeKey<EndPoint>.ValueOf(typeof(DotNettyTransportClientFactory), nameof(EndPoint));

        #endregion Field

        #region Constructor

        public DotNettyTransportClientFactory(ITransportMessageCodecFactory codecFactory, IHealthCheckService healthCheckService, ILogger<DotNettyTransportClientFactory> logger)
            : this(codecFactory, healthCheckService, logger, null)
        {
        }

        public DotNettyTransportClientFactory(ITransportMessageCodecFactory codecFactory, IHealthCheckService healthCheckService, ILogger<DotNettyTransportClientFactory> logger, IServiceExecutor serviceExecutor)
        {
            _transportMessageEncoder = codecFactory.GetEncoder();
            _transportMessageDecoder = codecFactory.GetDecoder();
            _logger = logger;
            _healthCheckService = healthCheckService;
            _serviceExecutor = serviceExecutor;
            _bootstrap = GetBootstrap();
            _bootstrap.Handler(new ActionChannelInitializer<ISocketChannel>(c =>
            {
                var pipeline = c.Pipeline;
                pipeline.AddLast(new LengthFieldPrepender(4));
                pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                pipeline.AddLast(new TransportMessageChannelHandlerAdapter(_transportMessageDecoder));
                pipeline.AddLast(new DefaultChannelHandler(this));
            }));
        }

        #endregion Constructor

        #region Implementation of ITransportClientFactory

        /// <summary>
        /// 创建客户端。
        /// </summary>
        /// <param name="endPoint">终结点。</param>
        /// <returns>传输客户端实例。</returns>
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
                        var bootstrap = _bootstrap;
                        var channel = await bootstrap.ConnectAsync(k);
                        var messageListener = new MessageListener();
                        channel.GetAttribute(messageListenerKey).Set(messageListener);
                        var messageSender = new DotNettyMessageClientSender(_transportMessageEncoder, channel);
                        channel.GetAttribute(messageSenderKey).Set(messageSender);
                        channel.GetAttribute(origEndPointKey).Set(k);
                        var client = new TransportClient(messageSender, messageListener, _logger, _serviceExecutor);
                        return client;
                    }
                    )).Value;
            }
            catch
            {
                _clients.TryRemove(key, out var value);
                var ipEndPoint = endPoint as IPEndPoint;
                if (ipEndPoint != null)
                    await _healthCheckService.MarkFailure(new IpAddressModel(ipEndPoint.Address.ToString(), ipEndPoint.Port));
                throw;
            }
        }

        #endregion Implementation of ITransportClientFactory

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            foreach (var client in _clients.Values.Where(i => i.IsValueCreated))
            {
                (client.Value as IDisposable)?.Dispose();
            }
        }

        #endregion Implementation of IDisposable

        private static Bootstrap GetBootstrap()
        {
            IEventLoopGroup group;
            
            var bootstrap = new Bootstrap();
            if (AppConfig.ServerOptions.Libuv)
            {
                group = new EventLoopGroup();
                bootstrap.Channel<TcpServerChannel>();
            }
            else
            {
                group = new MultithreadEventLoopGroup();
                bootstrap.Channel<TcpServerSocketChannel>();
            }
            bootstrap
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                .Group(group);

            return bootstrap;
        }

        protected class DefaultChannelHandler : ChannelHandlerAdapter
        {
            private readonly DotNettyTransportClientFactory _factory;

            public DefaultChannelHandler(DotNettyTransportClientFactory factory)
            {
                this._factory = factory;
            }

            #region Overrides of ChannelHandlerAdapter

            public override void ChannelInactive(IChannelHandlerContext context)
            {
                _factory._clients.TryRemove(context.Channel.GetAttribute(origEndPointKey).Get(), out var value);
            }

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                var transportMessage = message as TransportMessage;

                var messageListener = context.Channel.GetAttribute(messageListenerKey).Get();
                var messageSender = context.Channel.GetAttribute(messageSenderKey).Get();
                messageListener.OnReceived(messageSender, transportMessage);
            }

            #endregion Overrides of ChannelHandlerAdapter
        }
    }
}