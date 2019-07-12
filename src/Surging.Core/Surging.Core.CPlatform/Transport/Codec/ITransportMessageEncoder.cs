using Surging.Core.CPlatform.Messages;

namespace Surging.Core.CPlatform.Transport.Codec
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="ITransportMessageEncoder" />
    /// </summary>
    public interface ITransportMessageEncoder
    {
        #region 方法

        /// <summary>
        /// The Encode
        /// </summary>
        /// <param name="message">The message<see cref="TransportMessage"/></param>
        /// <returns>The <see cref="byte[]"/></returns>
        byte[] Encode(TransportMessage message);

        #endregion 方法
    }

    #endregion 接口
}