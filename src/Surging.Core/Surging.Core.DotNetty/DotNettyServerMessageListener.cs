using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Transport.Codec;
using Surging.Core.DotNetty.Adapter;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Surging.Core.DotNetty
{
    /// <summary>
    /// Defines the <see cref="DotNettyServerMessageListener" />
    /// </summary>
    public class DotNettyServerMessageListener : IMessageListener, IDisposable
    {
        #region 字段

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<DotNettyServerMessageListener> _logger;

        /// <summary>
        /// Defines the _transportMessageDecoder
        /// </summary>
        private readonly ITransportMessageDecoder _transportMessageDecoder;

        /// <summary>
        /// Defines the _transportMessageEncoder
        /// </summary>
        private readonly ITransportMessageEncoder _transportMessageEncoder;

        /// <summary>
        /// Defines the _channel
        /// </summary>
        private IChannel _channel;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNettyServerMessageListener"/> class.
        /// </summary>
        /// <param name="logger">The logger<see cref="ILogger{DotNettyServerMessageListener}"/></param>
        /// <param name="codecFactory">The codecFactory<see cref="ITransportMessageCodecFactory"/></param>
        public DotNettyServerMessageListener(ILogger<DotNettyServerMessageListener> logger, ITransportMessageCodecFactory codecFactory)
        {
            _logger = logger;
            _transportMessageEncoder = codecFactory.GetEncoder();
            _transportMessageDecoder = codecFactory.GetDecoder();
        }

        #endregion 构造函数

        #region 事件

        /// <summary>
        /// Defines the Received
        /// </summary>
        public event ReceivedDelegate Received;

        #endregion 事件

        #region 方法

        /// <summary>
        /// The CloseAsync
        /// </summary>
        public void CloseAsync()
        {
            Task.Run(async () =>
            {
                await _channel.EventLoop.ShutdownGracefullyAsync();
                await _channel.CloseAsync();
            }).Wait();
        }

        /// <summary>
        /// The Dispose
        /// </summary>
        public void Dispose()
        {
            Task.Run(async () =>
            {
                await _channel.DisconnectAsync();
            }).Wait();
        }

        /// <summary>
        /// 触发接收到消息事件。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="message">接收到的消息。</param>
        /// <returns>一个任务。</returns>
        public async Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            if (Received == null)
                return;
            await Received(sender, message);
        }

        /// <summary>
        /// The StartAsync
        /// </summary>
        /// <param name="endPoint">The endPoint<see cref="EndPoint"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task StartAsync(EndPoint endPoint)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"准备启动服务主机，监听地址：{endPoint}。");

            IEventLoopGroup bossGroup = new MultithreadEventLoopGroup(1);
            IEventLoopGroup workerGroup = new MultithreadEventLoopGroup();//Default eventLoopCount is Environment.ProcessorCount * 2
            var bootstrap = new ServerBootstrap();

            if (AppConfig.ServerOptions.Libuv)
            {
                var dispatcher = new DispatcherEventLoopGroup();
                bossGroup = dispatcher;
                workerGroup = new WorkerEventLoopGroup(dispatcher);
                bootstrap.Channel<TcpServerChannel>();
            }
            else
            {
                bossGroup = new MultithreadEventLoopGroup(1);
                workerGroup = new MultithreadEventLoopGroup();
                bootstrap.Channel<TcpServerSocketChannel>();
            }
            bootstrap
            .Option(ChannelOption.SoBacklog, AppConfig.ServerOptions.SoBacklog)
            .ChildOption(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
            .Group(bossGroup, workerGroup)
            .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
            {
                var pipeline = channel.Pipeline;
                pipeline.AddLast(new LengthFieldPrepender(4));
                pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                pipeline.AddLast(new TransportMessageChannelHandlerAdapter(_transportMessageDecoder));
                pipeline.AddLast(new ServerHandler(async (contenxt, message) =>
                {
                    var sender = new DotNettyServerMessageSender(_transportMessageEncoder, contenxt);
                    await OnReceived(sender, message);
                }, _logger));
            }));
            try
            {
                _channel = await bootstrap.BindAsync(endPoint);
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"服务主机启动成功，监听地址：{endPoint}。");
            }
            catch
            {
                _logger.LogError($"服务主机启动失败，监听地址：{endPoint}。 ");
            }
        }

        #endregion 方法

        /// <summary>
        /// Defines the <see cref="ServerHandler" />
        /// </summary>
        private class ServerHandler : ChannelHandlerAdapter
        {
            #region 字段

            /// <summary>
            /// Defines the _logger
            /// </summary>
            private readonly ILogger _logger;

            /// <summary>
            /// Defines the _readAction
            /// </summary>
            private readonly Action<IChannelHandlerContext, TransportMessage> _readAction;

            #endregion 字段

            #region 构造函数

            /// <summary>
            /// Initializes a new instance of the <see cref="ServerHandler"/> class.
            /// </summary>
            /// <param name="readAction">The readAction<see cref="Action{IChannelHandlerContext, TransportMessage}"/></param>
            /// <param name="logger">The logger<see cref="ILogger"/></param>
            public ServerHandler(Action<IChannelHandlerContext, TransportMessage> readAction, ILogger logger)
            {
                _readAction = readAction;
                _logger = logger;
            }

            #endregion 构造函数

            #region 方法

            /// <summary>
            /// The ChannelRead
            /// </summary>
            /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
            /// <param name="message">The message<see cref="object"/></param>
            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                Task.Run(() =>
                {
                    var transportMessage = (TransportMessage)message;
                    _readAction(context, transportMessage);
                });
            }

            /// <summary>
            /// The ChannelReadComplete
            /// </summary>
            /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
            public override void ChannelReadComplete(IChannelHandlerContext context)
            {
                context.Flush();
            }

            /// <summary>
            /// The ExceptionCaught
            /// </summary>
            /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
            /// <param name="exception">The exception<see cref="Exception"/></param>
            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
            {
                context.CloseAsync();//客户端主动断开需要应答，否则socket变成CLOSE_WAIT状态导致socket资源耗尽
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(exception, $"与服务器：{context.Channel.RemoteAddress}通信时发送了错误。");
            }

            #endregion 方法
        }
    }
}