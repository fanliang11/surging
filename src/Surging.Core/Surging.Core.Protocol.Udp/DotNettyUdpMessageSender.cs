using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Transport.Codec;
using Surging.Core.Protocol.Udp.Runtime.Implementation;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Udp
{
   public abstract class DotNettyUdpMessageSender
    {
        private readonly ITransportMessageEncoder _transportMessageEncoder;

        protected DotNettyUdpMessageSender(ITransportMessageEncoder transportMessageEncoder)
        {
            _transportMessageEncoder = transportMessageEncoder;
        }

        protected IByteBuffer GetByteBuffer(TransportMessage message)
        {
            var data =  message.GetContent<byte []>(); 
            return Unpooled.WrappedBuffer(data);
        }
    }

    /// <summary>
    /// 基于DotNetty服务端的消息发送者。
    /// </summary>
    public class DotNettyUdpServerMessageSender : DotNettyUdpMessageSender, IUdpMessageSender
    {
        private readonly IChannelHandlerContext _context;
        private readonly EndPoint _endPoint;    
        public DotNettyUdpServerMessageSender(ITransportMessageEncoder transportMessageEncoder, IChannelHandlerContext context,EndPoint endPoint) : base(transportMessageEncoder)
        {
            _context = context;
            _endPoint = endPoint;
        }

        #region Implementation of IMessageSender

        public UdpClient GetClient()
        {
            return new UdpClient(_context.Channel);
        }

        public T GetAndSet<T>(AttributeKey<T> attributeKey, T obj) where T : class =>
            _context.Channel.GetAttribute(attributeKey).GetAndSet(obj);


        public T Get<T>(AttributeKey<T> attributeKey) where T : class =>
            _context.Channel.GetAttribute(attributeKey).Get();

        public async Task SendAsync(string value, Encoding encoding)
        {
            if (value != null)
            {
                await _context.Channel.WriteAsync(
                new DatagramPacket(Unpooled.CopiedBuffer(value, encoding), _endPoint));
            }
        }


        public async Task SendAndFlushAsync(string value, Encoding encoding)
        {
            if (value != null)
            {
                await _context.Channel.WriteAndFlushAsync(
               new DatagramPacket(Unpooled.CopiedBuffer(value, encoding), _endPoint));
            }
            
        }
        

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAsync(TransportMessage message)
        {
            var buffer = GetByteBuffer(message);
            await _context.Channel.WriteAsync(
                new DatagramPacket(buffer, _endPoint));
        }

        /// <summary>
        /// 发送消息并清空缓冲区。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAndFlushAsync(TransportMessage message)
        {
            var buffer = GetByteBuffer(message);
            await _context.Channel.WriteAndFlushAsync(
                         new DatagramPacket(buffer, _endPoint));
        }

        public async Task SendAsync(object message)
        {
           await SendAsync(message.ToString(),Encoding.UTF8);
        }

        public async Task SendAndFlushAsync(object message)
        {
            await SendAndFlushAsync(message.ToString(), Encoding.UTF8);
        }

        public async Task SendAndFlushAsync(IByteBuffer buffer)
        {
            await _context.Channel.WriteAndFlushAsync(new DatagramPacket(buffer, _endPoint));
        }

        public async Task SendAsync(IByteBuffer buffer)
        {
            await _context.Channel.WriteAsync(new DatagramPacket(buffer, _endPoint));
        }

        #endregion Implementation of IMessageSender
    }
}
