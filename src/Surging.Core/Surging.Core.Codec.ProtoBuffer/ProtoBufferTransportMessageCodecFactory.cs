using Surging.Core.CPlatform.Transport.Codec;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Codec.ProtoBuffer
{
   public sealed  class ProtoBufferTransportMessageCodecFactory : ITransportMessageCodecFactory
    {
        #region Field

        private readonly ITransportMessageEncoder _transportMessageEncoder = new ProtoBufferTransportMessageEncoder();
        private readonly ITransportMessageDecoder _transportMessageDecoder = new ProtoBufferTransportMessageDecoder();

        #endregion Field

        #region Implementation of ITransportMessageCodecFactory
        
        public ITransportMessageEncoder GetEncoder()
        {
            return _transportMessageEncoder;
        }
        
        public ITransportMessageDecoder GetDecoder()
        {
            return _transportMessageDecoder;
        }

        #endregion Implementation of ITransportMessageCodecFactory
    }
}

