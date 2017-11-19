using Surging.Core.Codec.ProtoBuffer.Messages;
using Surging.Core.Codec.ProtoBuffer.Utilities;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport.Codec;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Codec.ProtoBuffer
{
   public sealed class ProtoBufferTransportMessageDecoder : ITransportMessageDecoder
    {
        #region Implementation of ITransportMessageDecoder

        public TransportMessage Decode(byte[] data)
        {
            var message = SerializerUtilitys.Deserialize<ProtoBufferTransportMessage>(data);
            return message.GetTransportMessage();
        }

        #endregion Implementation of ITransportMessageDecoder
    }
} 