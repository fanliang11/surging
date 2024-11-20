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

namespace Surging.Core.DotNettyWSServer
{
   public class DotNettyWSMessageListener : IMessageListener, IDisposable
    {
        #region Field

        private readonly ILogger<DotNettyWSMessageListener> _logger; 
        private IChannel _channel;
        private List<WSServiceEntry> _wSServiceEntries;
        public event ReceivedDelegate Received;

        #endregion Field

        #region Constructor
        public DotNettyWSMessageListener(ILogger<DotNettyWSMessageListener> logger
            ,  IWSServiceEntryProvider wsServiceEntryProvider)
        {
            _logger = logger; 
            _wSServiceEntries = wsServiceEntryProvider.GetEntries().ToList();
        }

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
                    pipeline.AddLast("WsProtocolHandler",
                    new WebSocketServerProtocolHandler(p.Path, p.Behavior.Protocol, true));
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

            protected override void Decode(IChannelHandlerContext ctx, WebSocketFrame msg, List<Object> output)
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

            protected override void Decode(IChannelHandlerContext ctx, IByteBuffer msg, List<Object> output)
            {
                WebSocketFrame webSocketFrame = new BinaryWebSocketFrame(msg);
                output.Add(webSocketFrame);
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
