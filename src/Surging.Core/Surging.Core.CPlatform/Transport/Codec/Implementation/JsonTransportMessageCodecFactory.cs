using Surging.Core.CPlatform.Runtime.Server;

namespace Surging.Core.CPlatform.Transport.Codec.Implementation
{
    /// <summary>
    /// Defines the <see cref="JsonTransportMessageCodecFactory" />
    /// </summary>
    public class JsonTransportMessageCodecFactory : ITransportMessageCodecFactory
    {
        #region 字段

        /// <summary>
        /// Defines the _transportMessageDecoder
        /// </summary>
        private readonly ITransportMessageDecoder _transportMessageDecoder = new JsonTransportMessageDecoder();

        /// <summary>
        /// Defines the _transportMessageEncoder
        /// </summary>
        private readonly ITransportMessageEncoder _transportMessageEncoder = new JsonTransportMessageEncoder();

        #endregion 字段

        #region 方法

        /// <summary>
        /// 获取解码器。
        /// </summary>
        /// <returns>解码器实例。</returns>
        public ITransportMessageDecoder GetDecoder()
        {
            return _transportMessageDecoder;
        }

        /// <summary>
        /// 获取编码器。
        /// </summary>
        /// <returns>编码器实例。</returns>
        public ITransportMessageEncoder GetEncoder()
        {
            return _transportMessageEncoder;
        }

        #endregion 方法
    }
}