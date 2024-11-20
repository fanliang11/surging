using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;
using DotNetty.Transport.Channels.Sockets;
using Newtonsoft.Json.Linq;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Transport.Codec;
using Surging.Core.Protocol.Udp.Runtime.Implementation;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Udp
{
    /// <summary>
    /// 基于DotNetty服务端的消息发送者。
    /// </summary>
    public class DoNettyMulticastUdpMessageSender : DotNettyUdpMessageSender, IUdpMessageSender
    { 
        private readonly ConcurrentDictionary<EndPoint, IChannelHandlerContext> _keyValuePairs=new ConcurrentDictionary<EndPoint, IChannelHandlerContext>();
        private readonly IChannelHandlerContext _context;
        public DoNettyMulticastUdpMessageSender(ITransportMessageEncoder transportMessageEncoder, IChannelHandlerContext context) : base(transportMessageEncoder)
        {
            _context = context;
        }

        #region Implementation of IMessageSender


        public void AddSender(EndPoint sender, IChannelHandlerContext channel)
        {
            _keyValuePairs.GetOrAdd(sender,p=> channel);
        }
        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAsync(TransportMessage message)
        {
            var buffer = GetByteBuffer(message);
            foreach (var keyValuePair in _keyValuePairs)
            {
                await keyValuePair.Value.Channel.WriteAsync(
                              new DatagramPacket(buffer, keyValuePair.Key));
            } 
        }

        public async Task SendAsync(string value, Encoding encoding)
        {
            if (value != null)
            {
                foreach (var keyValuePair in _keyValuePairs)
                {
                    await keyValuePair.Value.Channel.WriteAsync(
                                  new DatagramPacket(Unpooled.CopiedBuffer(value, encoding), keyValuePair.Key));
                }
            }
        }
        public async Task SendAndFlushAsync(string value, Encoding encoding)
        {
            if (value != null)
            {
                foreach (var keyValuePair in _keyValuePairs)
                {
                    await keyValuePair.Value.Channel.WriteAndFlushAsync(
                                  new DatagramPacket(Unpooled.CopiedBuffer(value, encoding), keyValuePair.Key));
                }
            }
        }

        /// <summary>
        /// 发送消息并清空缓冲区。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAndFlushAsync(TransportMessage message)
        {
            var buffer = GetByteBuffer(message);
            foreach (var keyValuePair in _keyValuePairs)
            {
                await keyValuePair.Value.Channel.WriteAndFlushAsync(
                              new DatagramPacket(buffer, keyValuePair.Key));
            }
        }

        public async Task SendAsync(object message)
        {
            await SendAsync(message.ToString(), Encoding.UTF8);
        }

        public async Task SendAndFlushAsync(object message)
        {
            await SendAndFlushAsync(message.ToString(), Encoding.UTF8);
        }

        public async Task SendAndFlushAsync(IByteBuffer buffer)
        {
            foreach (var keyValuePair in _keyValuePairs)
            {
                await keyValuePair.Value.Channel.WriteAndFlushAsync(new DatagramPacket(buffer, keyValuePair.Key));
            }
        }

        public async Task SendAsync(IByteBuffer buffer)
        {
            foreach (var keyValuePair in _keyValuePairs)
            {
                await keyValuePair.Value.Channel.WriteAsync(new DatagramPacket(buffer, keyValuePair.Key));
            }
        }


        public T GetAndSet<T>(AttributeKey<T> attributeKey, T obj) where T : class =>
            _context.Channel.GetAttribute(attributeKey).GetAndSet(obj);


        public T Get<T>(AttributeKey<T> attributeKey) where T : class =>
            _context.Channel.GetAttribute(attributeKey).Get();

        public UdpClient GetClient()
        {
            return new UdpClient(_context.Channel);
        }

        #endregion Implementation of IMessageSender
    }
}
