using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Codecs.DNS;
using DotNetty.Codecs.DNS.Messages;
using DotNetty.Codecs.DNS.Records;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Transport.Codec;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DNS
{
    /// <summary>
    /// Defines the <see cref="DotNettyDnsServerMessageListener" />
    /// </summary>
    internal class DotNettyDnsServerMessageListener : IMessageListener, IDisposable
    {
        #region 字段

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<DotNettyDnsServerMessageListener> _logger;

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
        /// Initializes a new instance of the <see cref="DotNettyDnsServerMessageListener"/> class.
        /// </summary>
        /// <param name="logger">The logger<see cref="ILogger{DotNettyDnsServerMessageListener}"/></param>
        /// <param name="codecFactory">The codecFactory<see cref="ITransportMessageCodecFactory"/></param>
        public DotNettyDnsServerMessageListener(ILogger<DotNettyDnsServerMessageListener> logger, ITransportMessageCodecFactory codecFactory)
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
        /// The OnReceived
        /// </summary>
        /// <param name="sender">The sender<see cref="IMessageSender"/></param>
        /// <param name="message">The message<see cref="TransportMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
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

            var group = new MultithreadEventLoopGroup();
            var bootstrap = new Bootstrap();
            bootstrap
                .Group(group)
                .Channel<SocketDatagramChannel>()
                .Handler(new ActionChannelInitializer<IDatagramChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    pipeline.AddLast(new DatagramDnsQueryDecoder());
                    pipeline.AddLast(new DatagramDnsResponseEncoder());
                    pipeline.AddLast(new ServerHandler(async (contenxt, message) =>
                    {
                        var sender = new DotNettyDnsServerMessageSender(_transportMessageEncoder, contenxt);
                        await OnReceived(sender, message);
                    }, _logger));
                })).Option(ChannelOption.SoBroadcast, true);
            try
            {
                _channel = await bootstrap.BindAsync(endPoint);
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"DNS服务主机启动成功，监听地址：{endPoint}。");
            }
            catch
            {
                _logger.LogError($"DNS服务主机启动失败，监听地址：{endPoint}。 ");
            }
        }

        #endregion 方法

        /// <summary>
        /// Defines the <see cref="ServerHandler" />
        /// </summary>
        private class ServerHandler : SimpleChannelInboundHandler<DatagramDnsQuery>
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

            /// <summary>
            /// Defines the _serializer
            /// </summary>
            private readonly ISerializer<string> _serializer;

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
            /// The ExceptionCaught
            /// </summary>
            /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
            /// <param name="exception">The exception<see cref="Exception"/></param>
            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
            {
                context.CloseAsync();
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(exception, $"与服务器：{context.Channel.RemoteAddress}通信时发送了错误。");
            }

            /// <summary>
            /// The ChannelRead0
            /// </summary>
            /// <param name="ctx">The ctx<see cref="IChannelHandlerContext"/></param>
            /// <param name="query">The query<see cref="DatagramDnsQuery"/></param>
            protected override void ChannelRead0(IChannelHandlerContext ctx, DatagramDnsQuery query)
            {
                DatagramDnsResponse response = new DatagramDnsResponse(query.Recipient, query.Sender, query.Id);
                DefaultDnsQuestion dnsQuestion = query.GetRecord<DefaultDnsQuestion>(DnsSection.QUESTION);
                response.AddRecord(DnsSection.QUESTION, dnsQuestion);
                _readAction(ctx, new TransportMessage(new DnsTransportMessage
                {
                    DnsResponse = response,
                    DnsQuestion = dnsQuestion
                }));
            }

            #endregion 方法
        }
    }
}