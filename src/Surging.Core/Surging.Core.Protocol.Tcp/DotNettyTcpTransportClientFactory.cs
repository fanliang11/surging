using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using Surging.Core.CPlatform;
using System.Collections.Concurrent;
using DotNetty.Common.Utilities;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Transport.Implementation;
using Surging.Core.Protocol.Tcp.Runtime.Implementation;
using Surging.Core.CPlatform.Network;
using System.Net.Http.Headers;

namespace Surging.Core.Protocol.Tcp
{
    internal class DotNettyTcpTransportClientFactory : ITransportClientFactory,INetwork, IDisposable
    {
        #region Field
         
        private readonly ILogger<DotNettyTcpTransportClientFactory> _logger;
        private readonly IServiceExecutor _serviceExecutor; 
        private readonly ConcurrentDictionary<EndPoint, Lazy<Task<ITransportClient>>> _clients = new ConcurrentDictionary<EndPoint, Lazy<Task<ITransportClient>>>();
        private readonly Bootstrap _bootstrap;
        private readonly NetworkProperties _networkProperties;
        private static readonly AttributeKey<IMessageSender> messageSenderKey = AttributeKey<IMessageSender>.ValueOf(typeof(DotNettyTcpTransportClientFactory), nameof(IMessageSender));
        private static readonly AttributeKey<IMessageListener> messageListenerKey = AttributeKey<IMessageListener>.ValueOf(typeof(DotNettyTcpTransportClientFactory), nameof(IMessageListener));
        private static readonly AttributeKey<EndPoint> origEndPointKey = AttributeKey<EndPoint>.ValueOf(typeof(DotNettyTcpTransportClientFactory), nameof(EndPoint));

        public string Id { get; set; }

        #endregion Field

        #region Constructor

        public DotNettyTcpTransportClientFactory(ILogger<DotNettyTcpTransportClientFactory> logger)
            : this(logger,new NetworkProperties())
        {
        }

        public DotNettyTcpTransportClientFactory(ILogger<DotNettyTcpTransportClientFactory> logger, NetworkProperties networkProperties)
        {
            _networkProperties = networkProperties;
            _logger = logger;
            _bootstrap = GetBootstrap();
            _bootstrap.Handler(new ActionChannelInitializer<ISocketChannel>(c =>
            {
                var pipeline = c.Pipeline;
                pipeline.AddLast(new LengthFieldPrepender(4));
                pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4)); 
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
                        var messageSender = new DotNettyTcpServerMessageSender( channel);
                        //设置channel属性
                        channel.GetAttribute(messageSenderKey).Set(messageSender);
                        channel.GetAttribute(origEndPointKey).Set(k);
                        //创建客户端
                        var client = new TcpTransportClient(messageSender, messageListener, _logger);
                        return client;
                    }
                    )).Value;//返回实例
            }
            catch
            {
                //移除
                _clients.TryRemove(key, out var value);
                var ipEndPoint = endPoint as IPEndPoint;
                
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
                .Option(ChannelOption.Allocator, new UnpooledByteBufferAllocator(false, false))
                .Group(group);

            return bootstrap;
        }

        public async Task StartAsync()
        {
            await CreateClientAsync(_networkProperties.CreateSocketAddress());
        }

        NetworkType INetwork.GetType()
        {
            return NetworkType.TcpClient;
        }

        public async void Shutdown()
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"Tcp客户端主机已停止。");
            _clients.Clear();
        }

        public bool IsAlive()
        {
            return false;
        }

        public bool IsAutoReload()
        {
            return false;
        }
         
        protected class DefaultChannelHandler : ChannelHandlerAdapter
        {
            private readonly DotNettyTcpTransportClientFactory _factory;
            private readonly string _clintId;
            public DefaultChannelHandler(DotNettyTcpTransportClientFactory factory,string clintId)
            {
                this._factory = factory;
                _clintId= clintId;
            }

            #region Overrides of ChannelHandlerAdapter
            [MethodImpl(MethodImplOptions.NoInlining)]
            public override void ChannelInactive(IChannelHandlerContext context)
            {
                _factory._clients.TryRemove(context.Channel.GetAttribute(origEndPointKey).Get(), out var value);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                var transportMessage = message as TransportMessage;
                TransportMessage.CreateInvokeResultMessage(_clintId, new RemoteInvokeResultMessage() { Result = message });
                var messageListener = context.Channel.GetAttribute(messageListenerKey).Get();
                var messageSender = context.Channel.GetAttribute(messageSenderKey).Get();
                messageListener.OnReceived(messageSender, transportMessage);
            }

            #endregion Overrides of ChannelHandlerAdapter
        }
    }
}