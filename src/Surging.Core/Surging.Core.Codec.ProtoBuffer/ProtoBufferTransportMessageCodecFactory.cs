using Surging.Core.CPlatform.Transport.Codec;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Codec.ProtoBuffer
{
    /// <summary>
    /// Defines the <see cref="ProtoBufferTransportMessageCodecFactory" />
    /// </summary>
    public sealed class ProtoBufferTransportMessageCodecFactory : ITransportMessageCodecFactory
    {
        #region 字段

        /// <summary>
        /// Defines the _transportMessageDecoder
        /// </summary>
        private readonly ITransportMessageDecoder _transportMessageDecoder = new ProtoBufferTransportMessageDecoder();

        /// <summary>
        /// Defines the _transportMessageEncoder
        /// </summary>
        private readonly ITransportMessageEncoder _transportMessageEncoder = new ProtoBufferTransportMessageEncoder();

        #endregion 字段

        #region 方法

        /// <summary>
        /// The GetDecoder
        /// </summary>
        /// <returns>The <see cref="ITransportMessageDecoder"/></returns>
        public ITransportMessageDecoder GetDecoder()
        {
            return _transportMessageDecoder;
        }

        /// <summary>
        /// The GetEncoder
        /// </summary>
        /// <returns>The <see cref="ITransportMessageEncoder"/></returns>
        public ITransportMessageEncoder GetEncoder()
        {
            return _transportMessageEncoder;
        }

        #endregion 方法
    }
}