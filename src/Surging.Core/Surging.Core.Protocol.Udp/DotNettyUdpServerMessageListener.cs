using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Network;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Transport.Codec;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.Protocol.Udp.Runtime;
using Surging.Core.Protocol.Udp.Runtime.Implementation;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Udp
{
   public class DotNettyUdpServerMessageListener : IMessageListener, INetwork, IDisposable
    {
        #region Field

        private readonly ILogger<DotNettyUdpServerMessageListener> _logger;
        private readonly ITransportMessageDecoder _transportMessageDecoder;
        private readonly ITransportMessageEncoder _transportMessageEncoder;
        private IChannel _channel;
        private readonly ISerializer<string> _serializer;
        private readonly NetworkProperties _networkProperties;
        private UdpServiceEntry _udpServiceEntry;
        public string Id { get;set; }

        public event ReceivedDelegate Received;

        #endregion Field

        #region Constructor
        public DotNettyUdpServerMessageListener(ILogger<DotNettyUdpServerMessageListener> logger
           , ITransportMessageCodecFactory codecFactory, IUdpServiceEntryProvider udpServiceEntryProvider) :this(logger, codecFactory, new NetworkProperties(), udpServiceEntryProvider) { 
        
        }
        public DotNettyUdpServerMessageListener(ILogger<DotNettyUdpServerMessageListener> logger
            , ITransportMessageCodecFactory codecFactory, NetworkProperties networkProperties, IUdpServiceEntryProvider udpServiceEntryProvider)
        {
            _udpServiceEntry = udpServiceEntryProvider.GetEntry();
            Id =networkProperties?.Id;
            _logger = logger;
            _transportMessageEncoder = codecFactory.GetEncoder();
            _transportMessageDecoder = codecFactory.GetDecoder();
            _networkProperties= networkProperties; 
        }

        public async Task StartAsync(EndPoint endPoint)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"准备启动服务主机，监听地址：{endPoint}。");
            IMessageSender sender=null;
            object isMulticast=null; 
            _networkProperties.ParserConfiguration?.TryGetValue("isMulticast", out  isMulticast);
            var group = new MultithreadEventLoopGroup();
            var bootstrap = new Bootstrap();
            bootstrap
                .Group(group)
                .Channel<SocketDatagramChannel>()
                .Option(ChannelOption.SoBacklog, 1024)
                .Option(ChannelOption.SoSndbuf, 1024 * 4096 * 10)
                .Option(ChannelOption.SoRcvbuf, 1024 * 4096 * 10)
                .Handler(new ServerHandler(async (contenxt, message,endPoint) =>
                    {
                        if (isMulticast == null || !bool.Parse(isMulticast.ToString()))
                            sender = new DotNettyUdpServerMessageSender(_transportMessageEncoder, contenxt, endPoint);
                        else if (isMulticast != null && bool.Parse(isMulticast.ToString()) && sender == null)
                            sender = new DoNettyMulticastUdpMessageSender(_transportMessageEncoder,contenxt);
                        var multicastSender = sender as DoNettyMulticastUdpMessageSender;
                        if (multicastSender != null)
                        {
                            multicastSender.AddSender(endPoint,contenxt);
                            await OnReceived(multicastSender, message);
                        }
                        else
                            await OnReceived(sender, message);
                    }, _networkProperties,_udpServiceEntry, _logger, _serializer,Id)
                ).Option(ChannelOption.SoBroadcast, true);
            try
            {

                _channel = await bootstrap.BindAsync(endPoint);
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"Udp服务主机启动成功，监听地址：{endPoint}。");
            }
            catch(Exception ex)
            {
                _logger.LogError($"Udp服务主机启动失败，监听地址：{endPoint}。 ");
            }

        }

        public async Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            if (Received == null)
                return;
            await Received(sender, message);
        }


        public void CloseAsync()
        {
            Task.Run(async () =>
            {
                await _channel.CloseAsync();
            }).Wait();
        }

        public void Dispose()
        {
            Task.Run(async () =>
            {
                await _channel.DisconnectAsync();
            }).Wait();
        }

        public async Task StartAsync()
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"Udp服务主机已停止。");
            await this.StartAsync(_networkProperties.CreateSocketAddress());
        }

        NetworkType INetwork.GetType()
        {
            return NetworkType.Udp;
        }

        public void Shutdown()
        {
            Task.Run(async () =>
            {
                await _channel.CloseAsync();
            });
        }

        public bool IsAlive()
        {
            return _channel.Active;
        }

        public bool IsAutoReload()
        {
            return false;
        }

        #endregion

        private class ServerHandler : SimpleChannelInboundHandler<DatagramPacket>
        {

            private readonly Action<IChannelHandlerContext, TransportMessage,EndPoint> _readAction;
            private readonly ILogger _logger;
            private readonly ISerializer<string> _serializer;
            private readonly string _netWorkId;  
            private UdpServiceEntry _udpServiceEntry;
            private readonly NetworkProperties _udpServerProperties;
            public ServerHandler(Action<IChannelHandlerContext, TransportMessage, EndPoint> readAction, NetworkProperties udpServerProperties, UdpServiceEntry udpServiceEntry, ILogger logger, ISerializer<string> serializer)
           :this(readAction,udpServerProperties, udpServiceEntry, logger, serializer,Guid.NewGuid().ToString("N"))
            { 
            }

            public ServerHandler(Action<IChannelHandlerContext, TransportMessage,EndPoint> readAction,  NetworkProperties udpServerProperties, UdpServiceEntry udpServiceEntry, ILogger logger, ISerializer<string> serializer,string netWorkId)
            {
                _readAction = readAction;
                _logger = logger;
                _serializer = serializer;
                _netWorkId = netWorkId; 
                _udpServerProperties = udpServerProperties;
                 _udpServiceEntry = udpServiceEntry; 
                _udpServerProperties = udpServerProperties;
            }

            protected override async void ChannelRead0(IChannelHandlerContext ctx, DatagramPacket msg)
            {
                var buff = msg.Content;
                byte[] messageBytes = new byte[buff.ReadableBytes];
                buff.ReadBytes(messageBytes);
               var udpBehavior= _udpServiceEntry.Behavior();
                udpBehavior.NetworkId.OnNext(_netWorkId);
                udpBehavior.Load(new UdpClient(ctx.Channel), _udpServerProperties);
                _udpServiceEntry.BehaviorSubject.OnNext(udpBehavior);
                _udpServiceEntry.BehaviorSubject.OnCompleted();
                _readAction(ctx, new TransportMessage(_netWorkId, messageBytes), msg.Sender);

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
