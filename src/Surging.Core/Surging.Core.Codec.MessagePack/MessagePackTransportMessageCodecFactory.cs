using Surging.Core.CPlatform.Transport.Codec;
using System.Runtime.CompilerServices;

namespace Surging.Core.Codec.MessagePack
{
    /// <summary>
    /// Defines the <see cref="MessagePackTransportMessageCodecFactory" />
    /// </summary>
    public sealed class MessagePackTransportMessageCodecFactory : ITransportMessageCodecFactory
    {
        #region 字段

        /// <summary>
        /// Defines the _transportMessageDecoder
        /// </summary>
        private readonly ITransportMessageDecoder _transportMessageDecoder = new MessagePackTransportMessageDecoder();

        /// <summary>
        /// Defines the _transportMessageEncoder
        /// </summary>
        private readonly ITransportMessageEncoder _transportMessageEncoder = new MessagePackTransportMessageEncoder();

        #endregion 字段

        #region 方法

        /// <inheritdoc />
        /// <summary>
        /// 获取解码器
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITransportMessageDecoder GetDecoder()
        {
            return _transportMessageDecoder;
        }

        /// <inheritdoc />
        /// <summary>
        /// 获取编码器
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITransportMessageEncoder GetEncoder()
        {
            return _transportMessageEncoder;
        }

        #endregion 方法
    }
}