using Surging.Core.CPlatform.Messages;

namespace Surging.Core.CPlatform.Transport.Codec
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="ITransportMessageDecoder" />
    /// </summary>
    public interface ITransportMessageDecoder
    {
        #region 方法

        /// <summary>
        /// The Decode
        /// </summary>
        /// <param name="data">The data<see cref="byte[]"/></param>
        /// <returns>The <see cref="TransportMessage"/></returns>
        TransportMessage Decode(byte[] data);

        #endregion 方法
    }

    #endregion 接口
}