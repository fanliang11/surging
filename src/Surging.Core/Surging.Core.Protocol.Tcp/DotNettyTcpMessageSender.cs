using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport;
using Surging.Core.Protocol.Tcp.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Tcp
{
    public abstract class DotNettyTcpMessageSender
    { 

        protected DotNettyTcpMessageSender()
        { 
        }

        protected IByteBuffer GetByteBuffer(TransportMessage message)
        {
            var data = message.GetContent<byte[]>();
            return Unpooled.WrappedBuffer(data);
        }
    }

        #region Implementation of IMessageSender

    /// <summary>
    /// 基于DotNetty服务端的消息发送者。
    /// </summary>
    public class DotNettyTcpServerMessageSender : DotNettyTcpMessageSender, IMessageSender
    {
        private readonly IChannel _channel;

        public DotNettyTcpServerMessageSender(IChannel channel) : base()
        {
            _channel = channel;
        }

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAsync(TransportMessage message)
        {
            var buffer = GetByteBuffer(message);
            await _channel.WriteAsync(buffer);
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

        
    }
    #endregion Implementation of IMessageSender

    public class TcpServerMessageSender : ITcpMessageSender
    {
        private readonly IChannelHandlerContext _context;

        public TcpServerMessageSender(IChannelHandlerContext context) : base()
        {
            _context = context;
        }

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAsync(object message, Encoding encoding)
        {
            if (message != null)
            {
                var buffer = Unpooled.WrappedBuffer(encoding.GetBytes(message.ToString()));
                await SendAsync(buffer);
            }
        }

        /// <summary>
        /// 发送消息并清空缓冲区。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAndFlushAsync(object message, Encoding encoding)
        {
            if (message != null)
            {
                var buffer = Unpooled.WrappedBuffer(encoding.GetBytes(message.ToString())); 
                await SendAndFlushAsync(buffer);
            }
        }

        public async Task SendAsync(object message)
        {
            await SendAsync(message, Encoding.UTF8);
        }

        public async Task SendAndFlushAsync(object message)
        {
            await SendAndFlushAsync(message, Encoding.UTF8);
        }

        public async Task SendAndFlushAsync(IByteBuffer buffer)
        {
            if (_context.Channel.RemoteAddress != null)
                await _context.WriteAndFlushAsync(buffer);
        }

        public async Task SendAsync(IByteBuffer buffer)
        {
            if (_context.Channel.RemoteAddress != null)
                await _context.WriteAsync(buffer);
        }
    }
}