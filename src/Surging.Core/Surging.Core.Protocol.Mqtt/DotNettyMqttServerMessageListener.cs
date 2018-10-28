using DotNetty.Buffers;
using DotNetty.Codecs.Mqtt;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Transport.Codec;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt
{
    public class DotNettyMqttServerMessageListener : IMessageListener, IDisposable
    {
        #region Field

        private readonly ILogger<DotNettyMqttServerMessageListener> _logger;
        private readonly ITransportMessageDecoder _transportMessageDecoder;
        private readonly ITransportMessageEncoder _transportMessageEncoder;
        private IChannel _channel;
        private readonly ISerializer<string> _serializer;

        #endregion Field

        public event ReceivedDelegate Received;

        #region Constructor
        public DotNettyMqttServerMessageListener(ILogger<DotNettyMqttServerMessageListener> logger, 
            ITransportMessageCodecFactory codecFactory,
            ISerializer<string> serializer)
        {
            _logger = logger;
            _transportMessageEncoder = codecFactory.GetEncoder();
            _transportMessageDecoder = codecFactory.GetDecoder();
            _serializer = serializer;
        }
        #endregion

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            if (Received == null)
                return;
            await Received(sender, message);
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
            .Option(ChannelOption.SoBacklog, 100)
            .ChildOption(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
            .Group(bossGroup, workerGroup)
            .Option(ChannelOption.TcpNodelay, true)
            .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
            {
                IChannelPipeline pipeline = channel.Pipeline;
                pipeline.AddLast(MqttEncoder.Instance,
                    new MqttDecoder(true, 256 * 1024), new ServerHandler(async (contenxt, message) =>
                {
                }, _logger, _serializer));
            }));
            try
            {
                _channel = await bootstrap.BindAsync(endPoint);
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"mqtt服务主机启动成功，监听地址：{endPoint}。");
            }
            catch
            {
                _logger.LogError($"mqtt服务主机启动失败，监听地址：{endPoint}。 ");
            }
        }

        private class ServerHandler : ChannelHandlerAdapter
        {
            readonly Queue<object> receivedQueue = new Queue<object>();
            readonly Queue<TaskCompletionSource<object>> readPromises = new Queue<TaskCompletionSource<object>>();
            readonly TimeSpan defaultReadTimeout;
            readonly object gate = new object();

            volatile Exception registeredException;

            public ServerHandler(Action<IChannelHandlerContext, TransportMessage> readAction, 
                ILogger logger,
                ISerializer<string> serializer) : this(readAction, logger, serializer,TimeSpan.Zero)
            {
            }

            public ServerHandler(Action<IChannelHandlerContext, TransportMessage> readAction, ILogger logger, ISerializer<string> serializer,TimeSpan defaultReadTimeout)
            {
                this.defaultReadTimeout = defaultReadTimeout;
            }

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                lock (this.gate)
                {
                    if (this.readPromises.Count > 0)
                    {
                        TaskCompletionSource<object> promise = this.readPromises.Dequeue();
                        promise.TrySetResult(message);
                    }
                    else
                    {
                        this.receivedQueue.Enqueue(message);
                    }
                }
            }

            public override void ChannelInactive(IChannelHandlerContext context)
            {
                this.SetException(new InvalidOperationException("Channel is closed."));
                base.ChannelInactive(context);
            }

            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception) => this.SetException(exception);

            void SetException(Exception exception)
            {
                this.registeredException = exception;

                lock (this.gate)
                {
                    while (this.readPromises.Count > 0)
                    {
                        TaskCompletionSource<object> promise = this.readPromises.Dequeue();
                        promise.TrySetException(exception);
                    }
                }
            }

            public async Task<object> ReceiveAsync(TimeSpan timeout = default(TimeSpan))
            {
                if (this.registeredException != null)
                {
                    throw this.registeredException;
                }

                var promise = new TaskCompletionSource<object>();

                lock (this.gate)
                {
                    if (this.receivedQueue.Count > 0)
                    {
                        return this.receivedQueue.Dequeue();
                    }

                    this.readPromises.Enqueue(promise);
                }

                timeout = timeout <= TimeSpan.Zero ? this.defaultReadTimeout : timeout;
                if (timeout > TimeSpan.Zero)
                {
                    Task task = await Task.WhenAny(promise.Task, Task.Delay(timeout));
                    if (task != promise.Task)
                    {
                        throw new TimeoutException("ReceiveAsync timed out");
                    }

                    return promise.Task.Result;
                }

                return await promise.Task;
            }
        }
    }
}
