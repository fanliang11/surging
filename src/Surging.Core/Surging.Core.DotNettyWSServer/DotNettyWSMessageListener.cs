using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Codecs.Http;
using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Handlers.Streams;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.EventExecutor;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Transport.Codec;
using Surging.Core.DotNettyWSServer.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Surging.Core.DotNettyWSServer
{
   public class DotNettyWSMessageListener : IMessageListener, IDisposable
    {
        #region Field

        private readonly ILogger<DotNettyWSMessageListener> _logger; 
        private IChannel _channel;
        private List<WSServiceEntry> _wSServiceEntries;
        private readonly IEventExecutorProvider _eventExecutorProvider;
        public event ReceivedDelegate Received;

        #endregion Field

        #region Constructor
        public DotNettyWSMessageListener(ILogger<DotNettyWSMessageListener> logger,
            IEventExecutorProvider eventExecutorProvider,
             IWSServiceEntryProvider wsServiceEntryProvider)
        {
            _eventExecutorProvider= eventExecutorProvider;
            _logger = logger; 
            _wSServiceEntries = wsServiceEntryProvider.GetEntries().ToList();
        }

        public async Task StartAsync(EndPoint endPoint)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"准备启动服务主机，监听地址：{endPoint}。");

            var ipEndPoint = endPoint as IPEndPoint;
            var host = $"ws://{ipEndPoint.Address}:{ipEndPoint.Port}";
            IEventLoopGroup bossGroup = _eventExecutorProvider.GetBossEventExecutor();
            IEventLoopGroup workerGroup = _eventExecutorProvider.GetWorkEventExecutor();//Default eventLoopCount is Environment.ProcessorCount * 2
            var bootstrap = new ServerBootstrap();

          
            bootstrap
            .Option(ChannelOption.SoBacklog, AppConfig.ServerOptions.SoBacklog)
            .ChildOption(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
            .ChildOption(ChannelOption.WriteBufferHighWaterMark,1024*1024*8)
            .Group(bossGroup, workerGroup)
            .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
            {
                var pipeline = channel.Pipeline;
                pipeline.AddLast("readTimeout", new ReadTimeoutHandler(45));
                pipeline.AddLast("HttpServerCodec", new HttpServerCodec());
                pipeline.AddLast("ChunkedWriter", new ChunkedWriteHandler<byte[]>());
                pipeline.AddLast("HttpAggregator", new HttpObjectAggregator(65535));
                _wSServiceEntries.ForEach(p =>
                {
                    pipeline.AddLast($"WsProtocolHandler{p.Path}",
                    new WebSocketServerProtocolHandler(p.Path, p.Behavior.Protocol, true));
                    pipeline.AddLast("WsProtocolHandler",
                  new WebSocketServerHandler(_logger,new WebSocketServerHandshakerFactory($"host{p.Path}",null,false)));
                });
            
                pipeline.AddLast("WSBinaryDecoder", new WebSocketFrameDecoder());
                pipeline.AddLast("WSEncoder", new WebSocketFramePrepender()); 
                pipeline.AddLast(new ServerHandler( _logger));
            })).Option(ChannelOption.SoBroadcast, true);
            try
            {

                _channel = await bootstrap.BindAsync(endPoint);
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"WS服务主机启动成功，监听地址：{endPoint}。");
            }
            catch (Exception ex)
            {
                _logger.LogError($"WS服务主机启动失败，监听地址：{endPoint}。 ");
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
                await _channel.EventLoop.ShutdownGracefullyAsync();
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

        #endregion

        public class WebSocketFrameDecoder : MessageToMessageDecoder<WebSocketFrame> {

            protected override void Decode(IChannelHandlerContext ctx, WebSocketFrame msg, List<System.Object> output)
            {
                var buff = msg.Content;
                byte[] messageBytes = new byte[buff.ReadableBytes];
                buff.ReadBytes(messageBytes);
                var bytebuf = PooledByteBufferAllocator.Default.Buffer(); 
                bytebuf.WriteBytes(messageBytes);
                output.Add(bytebuf.Retain());
            }
        }

        public class WebSocketFramePrepender : MessageToMessageDecoder<IByteBuffer>
        {

            protected override void Decode(IChannelHandlerContext ctx, IByteBuffer msg, List<System.Object> output)
            {
                WebSocketFrame webSocketFrame = new BinaryWebSocketFrame(msg);
                output.Add(webSocketFrame);
            }
        }
 

        public class WebSocketServerHandler : SimpleChannelInboundHandler<System.Object>
        {
            private WebSocketServerHandshaker handshaker;
            private readonly ILogger _logger;
            private readonly WebSocketServerHandshakerFactory _wsFactory;

            public WebSocketServerHandler(ILogger logger, WebSocketServerHandshakerFactory wsFactory)
            {
                _logger = logger;
                //ws://localhost:81/websocket
                _wsFactory = wsFactory;
            }

            protected override void ChannelRead0(IChannelHandlerContext ctx, System.Object msg)
            {
                if (msg is IFullHttpRequest) {
                    HandleHttpRequest(ctx, (IFullHttpRequest)msg);
                } else if (msg is WebSocketFrame) {
                    HandleWebSocketFrame(ctx, (WebSocketFrame)msg);
                }
            }

            private void HandleHttpRequest(IChannelHandlerContext ctx, IFullHttpRequest req)
            {
                if (!req.Result.IsSuccess)
                {
                    SendHttpResponse(ctx, req, new DefaultFullHttpResponse(DotNetty.Codecs.Http.HttpVersion.Http11, HttpResponseStatus.BadRequest));
                    return;
                }
                 
                handshaker = _wsFactory.NewHandshaker(req);
                if (handshaker == null)
                {
                    WebSocketServerHandshakerFactory.SendUnsupportedVersionResponse(ctx.Channel);
                }
                else
                {
                    handshaker.Handshake(ctx.Channel, req, req.Headers, ctx.NewPromise());
                }
            }

                private void HandleWebSocketFrame(IChannelHandlerContext ctx, WebSocketFrame frame)
                {
                    if (frame is TextWebSocketFrame) {
                        var request = ((TextWebSocketFrame)frame).Text;
                        ctx.Channel.WriteAndFlushAsync(new TextWebSocketFrame("Server received: " + request));
                    } else if (frame is CloseWebSocketFrame) {
                        handshaker.CloseAsync(ctx.Channel, (CloseWebSocketFrame)frame.Retain());
                    } else if (frame is PingWebSocketFrame) {
                        ctx.Channel.WriteAsync(new PongWebSocketFrame((IByteBuffer)frame.Content.Retain()));
                    }
                }

                private static void SendHttpResponse(IChannelHandlerContext ctx, IFullHttpRequest req, DefaultFullHttpResponse res)
                {
                    if (res.Status.Code != 200)
                    {
                        var  buf = res.Content;
                        buf.WriteBytes(Encoding.UTF8.GetBytes( $"Failure:{res.Status.ToString()}"));
                    }
                     ctx.Channel.WriteAndFlushAsync(res);
                     
                }

                public override void  ExceptionCaught(IChannelHandlerContext context, Exception exception)
                {
                    context.CloseAsync();//客户端主动断开需要应答，否则socket变成CLOSE_WAIT状态导致socket资源耗尽
                    if (_logger.IsEnabled(LogLevel.Error))
                        _logger.LogError(exception, $"与服务器：{context.Channel.RemoteAddress}通信时发送了错误。");
                }
              


        }

        private class ServerHandler : SimpleChannelInboundHandler<TextWebSocketFrame>
        {
            private readonly ILogger _logger; 

            public ServerHandler(ILogger logger)
            {
                _logger = logger; 
            }

            public override void ChannelActive(IChannelHandlerContext ctx)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("ws 连接 ctx:" + ctx);
                if (PlayerGroup.ChannelGroup == null) PlayerGroup.ChannelGroup = new DefaultChannelGroup(ctx.Executor);
                PlayerGroup.AddChannel(ctx.Channel);
            }

            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
            {
                context.CloseAsync();
                PlayerGroup.RemoveChannel(context.Channel);
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(exception, $"与服务器：{context.Channel.RemoteAddress}通信时发送了错误。");
            }

            protected override void ChannelRead0(IChannelHandlerContext ctx, TextWebSocketFrame msg)
            {
               
            }
        }
    }
}
