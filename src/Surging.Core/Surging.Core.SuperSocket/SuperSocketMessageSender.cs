using DotNetty.Buffers;
using SuperSocket;
using SuperSocket.Client;
using SuperSocket.Server.Abstractions.Session;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Transport.Codec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using static Surging.Core.SuperSocket.SuperSocketServerMessageListener;

namespace Surging.Core.SuperSocket
{
    public abstract class SuperSocketMessageSender
    {
        private readonly ITransportMessageEncoder _transportMessageEncoder;

        protected SuperSocketMessageSender(ITransportMessageEncoder transportMessageEncoder)
        {
            _transportMessageEncoder = transportMessageEncoder;
        }

        protected byte[] GetByteBuffer(TransportMessage message)
        {
            var data = _transportMessageEncoder.Encode(message).ToList();
            data.AddRange(Encoding.UTF8.GetBytes("!!!"));
            //var buffer = PooledByteBufferAllocator.Default.Buffer();
            return data.ToArray();
        }
    }

    public class SuperSocketMessageClientSender : SuperSocketMessageSender, IMessageSender
    {
        private readonly IEasyClient<TransportMessage> _client; 

        public SuperSocketMessageClientSender(ITransportMessageEncoder transportMessageEncoder, IEasyClient<TransportMessage> client) : base(transportMessageEncoder)
        {
            _client = client; 
        }

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns> 
        public async Task SendAsync(TransportMessage message)
        {
            var buffer = GetByteBuffer(message);
           await _client.SendAsync(buffer); 
        }

        /// <summary>
        /// 发送消息并清空缓冲区。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns> 
        public async Task SendAndFlushAsync(TransportMessage message)
        {
            var buffer = GetByteBuffer(message);
            await _client.SendAsync(buffer); 
            // _client.StartReceive();
            //var p=  await _client.ReceiveAsync();
        }
    }

    #region Implementation of IMessageSender
    public class SuperSocketServerMessageSender : SuperSocketMessageSender, IMessageSender
    {
        private readonly IAppSession _session;

        public SuperSocketServerMessageSender(ITransportMessageEncoder transportMessageEncoder, IAppSession session) : base(transportMessageEncoder)
        {
            _session = session;
        }


        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns> 
        public async Task SendAsync(TransportMessage message)
        {
            var buffer = GetByteBuffer(message);
           await _session.SendAsync(buffer);
        }

        /// <summary>
        /// 发送消息并清空缓冲区。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns> 
        public async Task SendAndFlushAsync(TransportMessage message)
        {
            var buffer = GetByteBuffer(message);
           await _session.SendAsync(buffer);
        }

    }
    #endregion
}
