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
    /// Defines the <see cref="ProtoBufferTransportMessageDecoder" />
    /// </summary>
    public sealed class ProtoBufferTransportMessageDecoder : ITransportMessageDecoder
    {
        #region 方法

        /// <summary>
        /// The Decode
        /// </summary>
        /// <param name="data">The data<see cref="byte[]"/></param>
        /// <returns>The <see cref="TransportMessage"/></returns>
        public TransportMessage Decode(byte[] data)
        {
            var message = SerializerUtilitys.Deserialize<ProtoBufferTransportMessage>(data);
            return message.GetTransportMessage();
        }

        #endregion 方法
    }
}