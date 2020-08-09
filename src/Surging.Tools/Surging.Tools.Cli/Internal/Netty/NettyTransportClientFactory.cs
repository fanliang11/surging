using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using McMaster.Extensions.CommandLineUtils;
using Surging.Core.CPlatform.Messages;
using Surging.Tools.Cli.Internal.Implementation;
using System.Net;
using System.Threading.Tasks;

namespace Surging.Tools.Cli.Internal.Netty
{
    public class NettyTransportClientFactory : ITransportClientFactory
    {
        private readonly CommandLineApplication _app;
        private readonly IConsole _console;
        private readonly Bootstrap _bootstrap;
        private readonly ITransportMessageEncoder _transportMessageEncoder;
        private readonly ITransportMessageDecoder _transportMessageDecoder;
        private static readonly AttributeKey<IMessageSender> messageSenderKey = AttributeKey<IMessageSender>.ValueOf(typeof(NettyTransportClientFactory), nameof(IMessageSender));
        private static readonly AttributeKey<IMessageListener> messageListenerKey = AttributeKey<IMessageListener>.ValueOf(typeof(NettyTransportClientFactory), nameof(IMessageListener));
        private static readonly AttributeKey<EndPoint> origEndPointKey = AttributeKey<EndPoint>.ValueOf(typeof(NettyTransportClientFactory), nameof(EndPoint));

        public NettyTransportClientFactory(ITransportMessageCodecFactory codecFactory, CommandLineApplication app, IConsole console)
        {
            _transportMessageEncoder = codecFactory.GetEncoder();
            _transportMessageDecoder = codecFactory.GetDecoder();
            _app = app;
            _console = console;
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

        public async Task<ITransportClient> CreateClientAsync(EndPoint endPoint)
        {
            try
            {
                //客户端对象
                var bootstrap = _bootstrap;
                //异步连接返回channel
                var channel = await bootstrap.ConnectAsync(endPoint);
                var messageListener = new MessageListener();
                //设置监听
                channel.GetAttribute(messageListenerKey).Set(messageListener);
                //实例化发送者
                var messageSender = new DotNettyMessageClientSender(_transportMessageEncoder, channel);
                //设置channel属性
                channel.GetAttribute(messageSenderKey).Set(messageSender);
                channel.GetAttribute(origEndPointKey).Set(endPoint);
                //创建客户端
                var client = new DotnettyTransportClient(messageSender, messageListener,_app);
                return client;

            }
            catch
            {
                throw;
            }
        }

        private static Bootstrap GetBootstrap()
        {
            IEventLoopGroup group;

            var bootstrap = new Bootstrap();
            group = new EventLoopGroup();
            bootstrap.Channel<TcpServerChannel>();

            bootstrap
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                .Group(group);

            return bootstrap;
        }

        protected class DefaultChannelHandler : ChannelHandlerAdapter
        {
            private readonly NettyTransportClientFactory _factory;

            public DefaultChannelHandler(NettyTransportClientFactory factory)
            {
                this._factory = factory;
            }

            #region Overrides of ChannelHandlerAdapter

            public override void ChannelInactive(IChannelHandlerContext context)
            {
                
            }

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                var transportMessage = message as TransportMessage;

                var messageListener = context.Channel.GetAttribute(messageListenerKey).Get();
                var messageSender = context.Channel.GetAttribute(messageSenderKey).Get();
                messageListener.OnReceived(messageSender, transportMessage);
            }

            #endregion Overrides of ChannelHandlerAdapter
        }
    }
}
