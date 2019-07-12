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
        #region 字段

        /// <summary>
        /// Defines the messageListenerKey
        /// </summary>
        private static readonly AttributeKey<IMessageListener> messageListenerKey = AttributeKey<IMessageListener>.ValueOf(typeof(DotNettyTransportClientFactory), nameof(IMessageListener));

        /// <summary>
        /// Defines the messageSenderKey
        /// </summary>
        private static readonly AttributeKey<IMessageSender> messageSenderKey = AttributeKey<IMessageSender>.ValueOf(typeof(DotNettyTransportClientFactory), nameof(IMessageSender));

        /// <summary>
        /// Defines the origEndPointKey
        /// </summary>
        private static readonly AttributeKey<EndPoint> origEndPointKey = AttributeKey<EndPoint>.ValueOf(typeof(DotNettyTransportClientFactory), nameof(EndPoint));

        /// <summary>
        /// Defines the _bootstrap
        /// </summary>
        private readonly Bootstrap _bootstrap;

        /// <summary>
        /// Defines the _clients
        /// </summary>
        private readonly ConcurrentDictionary<EndPoint, Lazy<Task<ITransportClient>>> _clients = new ConcurrentDictionary<EndPoint, Lazy<Task<ITransportClient>>>();

        /// <summary>
        /// Defines the _healthCheckService
        /// </summary>
        private readonly IHealthCheckService _healthCheckService;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<DotNettyTransportClientFactory> _logger;

        /// <summary>
        /// Defines the _serviceExecutor
        /// </summary>
        private readonly IServiceExecutor _serviceExecutor;

        /// <summary>
        /// Defines the _transportMessageDecoder
        /// </summary>
        private readonly ITransportMessageDecoder _transportMessageDecoder;

        /// <summary>
        /// Defines the _transportMessageEncoder
        /// </summary>
        private readonly ITransportMessageEncoder _transportMessageEncoder;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNettyTransportClientFactory"/> class.
        /// </summary>
        /// <param name="codecFactory">The codecFactory<see cref="ITransportMessageCodecFactory"/></param>
        /// <param name="healthCheckService">The healthCheckService<see cref="IHealthCheckService"/></param>
        /// <param name="logger">The logger<see cref="ILogger{DotNettyTransportClientFactory}"/></param>
        public DotNettyTransportClientFactory(ITransportMessageCodecFactory codecFactory, IHealthCheckService healthCheckService, ILogger<DotNettyTransportClientFactory> logger)
            : this(codecFactory, healthCheckService, logger, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNettyTransportClientFactory"/> class.
        /// </summary>
        /// <param name="codecFactory">The codecFactory<see cref="ITransportMessageCodecFactory"/></param>
        /// <param name="healthCheckService">The healthCheckService<see cref="IHealthCheckService"/></param>
        /// <param name="logger">The logger<see cref="ILogger{DotNettyTransportClientFactory}"/></param>
        /// <param name="serviceExecutor">The serviceExecutor<see cref="IServiceExecutor"/></param>
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

        #endregion 构造函数

        #region 方法

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
                            //客户端对象
                            var bootstrap = _bootstrap;
                            //异步连接返回channel
                            var channel = await bootstrap.ConnectAsync(k);
                            var messageListener = new MessageListener();
                            //设置监听
                            channel.GetAttribute(messageListenerKey).Set(messageListener);
                            //实例化发送者
                            var messageSender = new DotNettyMessageClientSender(_transportMessageEncoder, channel);
                            //设置channel属性
                            channel.GetAttribute(messageSenderKey).Set(messageSender);
                            channel.GetAttribute(origEndPointKey).Set(k);
                            //创建客户端
                            var client = new TransportClient(messageSender, messageListener, _logger, _serviceExecutor);
                            return client;
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

        /// <summary>
        /// The Dispose
        /// </summary>
        public void Dispose()
        {
            foreach (var client in _clients.Values.Where(i => i.IsValueCreated))
            {
                (client.Value as IDisposable)?.Dispose();
            }
        }

        /// <summary>
        /// The GetBootstrap
        /// </summary>
        /// <returns>The <see cref="Bootstrap"/></returns>
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

        #endregion 方法

        /// <summary>
        /// Defines the <see cref="DefaultChannelHandler" />
        /// </summary>
        protected class DefaultChannelHandler : ChannelHandlerAdapter
        {
            #region 字段

            /// <summary>
            /// Defines the _factory
            /// </summary>
            private readonly DotNettyTransportClientFactory _factory;

            #endregion 字段

            #region 构造函数

            /// <summary>
            /// Initializes a new instance of the <see cref="DefaultChannelHandler"/> class.
            /// </summary>
            /// <param name="factory">The factory<see cref="DotNettyTransportClientFactory"/></param>
            public DefaultChannelHandler(DotNettyTransportClientFactory factory)
            {
                this._factory = factory;
            }

            #endregion 构造函数

            #region 方法

            /// <summary>
            /// The ChannelInactive
            /// </summary>
            /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
            public override void ChannelInactive(IChannelHandlerContext context)
            {
                _factory._clients.TryRemove(context.Channel.GetAttribute(origEndPointKey).Get(), out var value);
            }

            /// <summary>
            /// The ChannelRead
            /// </summary>
            /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
            /// <param name="message">The message<see cref="object"/></param>
            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                var transportMessage = message as TransportMessage;

                var messageListener = context.Channel.GetAttribute(messageListenerKey).Get();
                var messageSender = context.Channel.GetAttribute(messageSenderKey).Get();
                messageListener.OnReceived(messageSender, transportMessage);
            }

            #endregion 方法
        }
    }
}