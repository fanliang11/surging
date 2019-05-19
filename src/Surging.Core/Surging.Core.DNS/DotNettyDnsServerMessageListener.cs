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
        private readonly ISerializer<string> _serializer;

        public event ReceivedDelegate Received;

        #endregion Field

        #region Constructor

        public DotNettyDnsServerMessageListener(ILogger<DotNettyDnsServerMessageListener> logger, ITransportMessageCodecFactory codecFactory, ISerializer<string> serializer)
        {
            _logger = logger;
            _transportMessageEncoder = codecFactory.GetEncoder();
            _transportMessageDecoder = codecFactory.GetDecoder();
            _serializer = serializer;
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
                    pipeline.AddLast(new DatagramDnsResponseEncoder()); ;
                    pipeline.AddLast(new ServerHandler(async (contenxt, message) =>
                    {
                        
                    }, _logger, _serializer));
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

            public ServerHandler(Action<IChannelHandlerContext, TransportMessage> readAction, ILogger logger, ISerializer<string> serializer)
            {
                _readAction = readAction;
                _logger = logger;
                _serializer = serializer;
            }

            protected override void ChannelRead0(IChannelHandlerContext ctx, DatagramDnsQuery query)
            {
              //Test Code
              //  DatagramDnsResponse response = new DatagramDnsResponse(query.Recipient, query.Sender, query.Id);
              //  DefaultDnsQuestion dnsQuestion = query.GetRecord<DefaultDnsQuestion>(DnsSection.QUESTION);
              ////  IDnsRecord additionalRecord = query.GetRecord<IDnsRecord>(DnsSection.ADDITIONAL);
              //  response.AddRecord(DnsSection.QUESTION, dnsQuestion);
      
              //    //additionalRecord = query.GetRecord<IDnsRecord>(DnsSection.ADDITIONAL);
              //  DnsClient dnsClient = new DnsClient(IPAddress.Parse("192.168.249.1"), 100);

              //  var dnsMessage = dnsClient.Resolve(DomainName.Parse(dnsQuestion.Name),(RecordType)dnsQuestion.Type.IntValue);
              //  if (dnsMessage != null)
              //  {
              //      foreach (DnsRecordBase dnsRecord in dnsMessage.AnswerRecords)
              //      {
              //          var aRecord = dnsRecord as ARecord;
              //          var buf = Unpooled.Buffer();
              //          if (dnsRecord.RecordType == RecordType.Ptr)
              //          {
              //              var ptrRecord = dnsRecord as PtrRecord;
              //              response.AddRecord(DnsSection.ANSWER, new DefaultDnsPtrRecord(ptrRecord.Name.ToString(), (DnsRecordClass)(int)ptrRecord.RecordClass, ptrRecord.TimeToLive, ptrRecord.PointerDomainName.ToString()));
              //          }
              //          if (aRecord != null)
              //          {
              //              buf = Unpooled.WrappedBuffer(aRecord.Address.GetAddressBytes());
              //              response.AddRecord(DnsSection.ANSWER, new DefaultDnsRawRecord(dnsQuestion.Name, DnsRecordType.From((int)dnsRecord.RecordType), (DnsRecordClass)(int)aRecord.RecordClass, dnsRecord.TimeToLive, buf));
              //          }

              //      }
              //      ctx.WriteAndFlushAsync(response);
              //  }
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
