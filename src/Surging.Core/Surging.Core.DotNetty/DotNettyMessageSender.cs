using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Transport.Codec;
using System;
using System.Threading.Tasks;

namespace Surging.Core.DotNetty
{
    /// <summary>
    /// 基于DotNetty客户端的消息发送者。
    /// </summary>
    public class DotNettyMessageClientSender : DotNettyMessageSender, IMessageSender, IDisposable
    {
        #region 字段

        /// <summary>
        /// Defines the _channel
        /// </summary>
        private readonly IChannel _channel;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNettyMessageClientSender"/> class.
        /// </summary>
        /// <param name="transportMessageEncoder">The transportMessageEncoder<see cref="ITransportMessageEncoder"/></param>
        /// <param name="channel">The channel<see cref="IChannel"/></param>
        public DotNettyMessageClientSender(ITransportMessageEncoder transportMessageEncoder, IChannel channel) : base(transportMessageEncoder)
        {
            _channel = channel;
        }

        #endregion 构造函数

        #region 方法

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
        /// 发送消息并清空缓冲区。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAndFlushAsync(TransportMessage message)
        {
            var buffer = GetByteBuffer(message);
            await _channel.WriteAndFlushAsync(buffer);
        }

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAsync(TransportMessage message)
        {
            var buffer = GetByteBuffer(message);
            await _channel.WriteAndFlushAsync(buffer);
        }

        #endregion 方法
    }

    /// <summary>
    /// 基于DotNetty的消息发送者基类。
    /// </summary>
    public abstract class DotNettyMessageSender
    {
        #region 字段

        /// <summary>
        /// Defines the _transportMessageEncoder
        /// </summary>
        private readonly ITransportMessageEncoder _transportMessageEncoder;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNettyMessageSender"/> class.
        /// </summary>
        /// <param name="transportMessageEncoder">The transportMessageEncoder<see cref="ITransportMessageEncoder"/></param>
        protected DotNettyMessageSender(ITransportMessageEncoder transportMessageEncoder)
        {
            _transportMessageEncoder = transportMessageEncoder;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The GetByteBuffer
        /// </summary>
        /// <param name="message">The message<see cref="TransportMessage"/></param>
        /// <returns>The <see cref="IByteBuffer"/></returns>
        protected IByteBuffer GetByteBuffer(TransportMessage message)
        {
            var data = _transportMessageEncoder.Encode(message);
            //var buffer = PooledByteBufferAllocator.Default.Buffer();
            return Unpooled.WrappedBuffer(data);
        }

        #endregion 方法
    }

    /// <summary>
    /// 基于DotNetty服务端的消息发送者。
    /// </summary>
    public class DotNettyServerMessageSender : DotNettyMessageSender, IMessageSender
    {
        #region 字段

        /// <summary>
        /// Defines the _context
        /// </summary>
        private readonly IChannelHandlerContext _context;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNettyServerMessageSender"/> class.
        /// </summary>
        /// <param name="transportMessageEncoder">The transportMessageEncoder<see cref="ITransportMessageEncoder"/></param>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        public DotNettyServerMessageSender(ITransportMessageEncoder transportMessageEncoder, IChannelHandlerContext context) : base(transportMessageEncoder)
        {
            _context = context;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// 发送消息并清空缓冲区。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAndFlushAsync(TransportMessage message)
        {
            var buffer = GetByteBuffer(message);
            await _context.WriteAndFlushAsync(buffer);
        }

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAsync(TransportMessage message)
        {
            var buffer = GetByteBuffer(message);
            await _context.WriteAsync(buffer);
        }

        #endregion 方法
    }
}