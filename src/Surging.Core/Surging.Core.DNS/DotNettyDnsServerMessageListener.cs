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
    class DotNettyDnsServerMessageListener : IMessageListener, IDisposable
    {
        #region Field

        private readonly ILogger<DotNettyDnsServerMessageListener> _logger;
        private readonly ITransportMessageDecoder _transportMessageDecoder;
        private readonly ITransportMessageEncoder _transportMessageEncoder;
        private IChannel _channel;

        public event ReceivedDelegate Received;

        #endregion Field

        #region Constructor

        public DotNettyDnsServerMessageListener(ILogger<DotNettyDnsServerMessageListener> logger, ITransportMessageCodecFactory codecFactory)
        {
            _logger = logger;
            _transportMessageEncoder = codecFactory.GetEncoder();
            _transportMessageDecoder = codecFactory.GetDecoder();
        }

        #endregion Constructor

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

        public void CloseAsync()
        {
            Task.Run(async () =>
            {
                await _channel.EventLoop.ShutdownGracefullyAsync();
                await _channel.CloseAsync();
            }).Wait();
        }

        #region Implementation of IDisposable


        public void Dispose()
        {
            Task.Run(async () =>
            {
                await _channel.DisconnectAsync();
            }).Wait();
        }

        public async Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            if (Received == null)
                return;
            await Received(sender, message);
        }

        #endregion Implementation of IDisposable

        private class ServerHandler : SimpleChannelInboundHandler<DatagramDnsQuery>
        {

            private readonly Action<IChannelHandlerContext, TransportMessage> _readAction;
            private readonly ILogger _logger;
            private readonly ISerializer<string> _serializer;

            public ServerHandler(Action<IChannelHandlerContext, TransportMessage> readAction, ILogger logger)
            {
                _readAction = readAction;
                _logger = logger; 
            }

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

            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
            {
                 context.CloseAsync();
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(exception, $"与服务器：{context.Channel.RemoteAddress}通信时发送了错误。");
            }
        }
    }
}
