using Surging.Core.Codec.ProtoBuffer.Messages;
using Surging.Core.Codec.ProtoBuffer.Utilities;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport.Codec;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Codec.ProtoBuffer
{
    public sealed class ProtoBufferTransportMessageEncoder : ITransportMessageEncoder
    {
        #region Implementation of ITransportMessageEncoder

        public byte[] Encode(TransportMessage message)
        {
            var transportMessage = new ProtoBufferTransportMessage(message)
            {
                Id = message.Id,
                ContentType = message.ContentType,
            };

            return SerializerUtilitys.Serialize(transportMessage);
        }

        #endregion Implementation of ITransportMessageEncoder
    }
}