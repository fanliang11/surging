﻿using Surging.Core.CPlatform.Messages;
using System;
using System.Threading.Tasks;
using Thrift.Protocol;
using Thrift.Protocol.Entities;

namespace Surging.Tools.Cli.Internal.Thrift
{
    public abstract class ThriftMessageSender
    {
        private readonly ITransportMessageEncoder _transportMessageEncoder;

        protected ThriftMessageSender(ITransportMessageEncoder transportMessageEncoder)
        {
            _transportMessageEncoder = transportMessageEncoder;
        }

        protected byte[] GetBinary(TransportMessage message)
        {
            return _transportMessageEncoder.Encode(message);
        }
    }

    public class ThriftMessageClientSender : ThriftMessageSender, IMessageSender, IDisposable
    {
        private readonly TProtocol _protocol;

        public ThriftMessageClientSender(ITransportMessageEncoder transportMessageEncoder, TProtocol context) : base(transportMessageEncoder)
        {
            _protocol = context;
        }


        #region Implementation of IDisposable

        public void Dispose()
        {
            _protocol.Dispose();
        }

        #endregion Implementation of IDisposable

        #region Implementation of IMessageSender

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAsync(TransportMessage message)
        {
            await _protocol.WriteMessageBeginAsync(new TMessage("thrift.surging", TMessageType.Call, 0));
            var binary = GetBinary(message);
            await _protocol.WriteMessageEndAsync();
            await _protocol.WriteBinaryAsync(binary);
        }

        /// <summary>
        /// 发送消息并清空缓冲区。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAndFlushAsync(TransportMessage message)
        {
            await _protocol.WriteMessageBeginAsync(new TMessage("thrift.surging", TMessageType.Call, 0));
            var binary = GetBinary(message);
            await _protocol.WriteBinaryAsync(binary);
            await _protocol.WriteMessageEndAsync();
            await _protocol.Transport.FlushAsync();
        }

        #endregion Implementation of IMessageSender
    }
}

