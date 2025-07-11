

using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.EventExecutor;
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
using System.Runtime.CompilerServices;
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
        private readonly IEventExecutorProvider _eventExecutorProvider;
        private static readonly AttributeKey<IMessageSender> messageSenderKey = AttributeKey<IMessageSender>.ValueOf(typeof(DotNettyTransportClientFactory), nameof(IMessageSender));
        private static readonly AttributeKey<IMessageListener> messageListenerKey = AttributeKey<IMessageListener>.ValueOf(typeof(DotNettyTransportClientFactory), nameof(IMessageListener));
        private static readonly AttributeKey<EndPoint> origEndPointKey = AttributeKey<EndPoint>.ValueOf(typeof(DotNettyTransportClientFactory), nameof(EndPoint));

        #endregion Field

        #region Constructor

        public DotNettyTransportClientFactory(ITransportMessageCodecFactory codecFactory,IEventExecutorProvider eventExecutorProvider, IHealthCheckService healthCheckService, ILogger<DotNettyTransportClientFactory> logger)
            : this(codecFactory, eventExecutorProvider,  healthCheckService, logger, null)
        {
        }

        public DotNettyTransportClientFactory(ITransportMessageCodecFactory codecFactory, IEventExecutorProvider eventExecutorProvider, IHealthCheckService healthCheckService, ILogger<DotNettyTransportClientFactory> logger, IServiceExecutor serviceExecutor)
        {
            _eventExecutorProvider = eventExecutorProvider;
            _transportMessageEncoder = codecFactory.GetEncoder();
            _transportMessageDecoder = codecFactory.GetDecoder();
            _logger = logger;
            _healthCheckService = healthCheckService;
            var eventExecutor = _eventExecutorProvider.GetWorkEventExecutor();
            _serviceExecutor = serviceExecutor;
            _bootstrap = GetBootstrap(eventExecutorProvider.GetWorkEventExecutor());
            _bootstrap.Handler(new ActionChannelInitializer<ISocketChannel>(c =>
            {
                var pipeline = c.Pipeline;
                pipeline.AddLast(new LengthFieldPrepender2(2));
                pipeline.AddLast(new LengthFieldBasedFrameDecoder2(int.MaxValue, 0, 2, 0, 2));
                pipeline.AddLast(eventExecutor, "transportMessageHandler", new TransportMessageHandlerEncoder(_transportMessageEncoder));//Time-consuming execution, add eventExecutor, otherwise memory leak
                pipeline.AddLast(eventExecutor, "clientTransportMessageHandler", new ClientTransportMessageChannelHandler(_transportMessageDecoder));//Time-consuming execution, add eventExecutor, otherwise memory leak
                pipeline.AddLast(eventExecutor, "clientHandler", new DefaultChannelHandler(this,_logger));//Time-consuming execution, add eventExecutor, otherwise memory leak
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

        #endregion Implementation of ITransportClientFactory

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            foreach (var client in _clients.Values)
            {
                (client as IDisposable)?.Dispose();
            }
        }

        #endregion Implementation of IDisposable

        private static Bootstrap GetBootstrap(IEventLoopGroup group)
        {  
            var bootstrap = new Bootstrap(); 
            bootstrap
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Option(ChannelOption.SoKeepalive, true)
                .Option(ChannelOption.Allocator, UnpooledByteBufferAllocator.Default)
                .Group(group);

            return bootstrap;
        }

        protected class DefaultChannelHandler :  ChannelHandlerAdapter
        {
            private readonly DotNettyTransportClientFactory _factory;
            private readonly ILogger _logger;
            public DefaultChannelHandler(DotNettyTransportClientFactory factory, ILogger logger)
            {
                this._factory = factory;
                this._logger = logger;
            }

            #region Overrides of ChannelHandlerAdapter
            [MethodImpl(MethodImplOptions.NoInlining)]
            public override void ChannelInactive(IChannelHandlerContext context)
            {
                _factory._clients.TryRemove(context.Channel.GetAttribute(origEndPointKey).Get(), out var value);
                context.CloseAsync();
            }

             [MethodImpl(MethodImplOptions.NoInlining)]
             public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
            {
                // Close the connection when an exception is raised.
                _factory._clients.TryRemove(context.Channel.GetAttribute(origEndPointKey).Get(), out var value);
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(exception, $"与服务器：{context.Channel.RemoteAddress}通信时发送了错误。");
                context.CloseAsync();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                try
                {
                    var transportMessage = message as TransportMessage;
                    var messageListener = context.Channel.GetAttribute(messageListenerKey).Get();
                    var messageSender = context.Channel.GetAttribute(messageSenderKey).Get();
                    messageListener.OnReceived(messageSender, transportMessage);
                }
                finally  
                {
                    message = null;
                }
            }

            #endregion Overrides of ChannelHandlerAdapter
        }
    }
}