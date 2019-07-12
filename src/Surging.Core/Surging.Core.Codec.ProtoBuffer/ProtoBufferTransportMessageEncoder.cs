using Surging.Core.Codec.ProtoBuffer.Messages;
using Surging.Core.Codec.ProtoBuffer.Utilities;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport.Codec;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Codec.ProtoBuffer
{
    /// <summary>
    /// Defines the <see cref="ProtoBufferTransportMessageEncoder" />
    /// </summary>
    public sealed class ProtoBufferTransportMessageEncoder : ITransportMessageEncoder
    {
        #region 方法

        /// <summary>
        /// The Encode
        /// </summary>
        /// <param name="message">The message<see cref="TransportMessage"/></param>
        /// <returns>The <see cref="byte[]"/></returns>
        public byte[] Encode(TransportMessage message)
        {
            var transportMessage = new ProtoBufferTransportMessage(message)
            {
                Id = message.Id,
                ContentType = message.ContentType,
            };

            return SerializerUtilitys.Serialize(transportMessage);
        }

        #endregion 方法
    }
}