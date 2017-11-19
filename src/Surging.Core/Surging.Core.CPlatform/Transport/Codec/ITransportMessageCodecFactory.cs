namespace Surging.Core.CPlatform.Transport.Codec
{
    /// <summary>
    /// 一个抽象的传输消息编解码器工厂。
    /// </summary>
    public interface ITransportMessageCodecFactory
    {
        /// <summary>
        /// 获取编码器。
        /// </summary>
        /// <returns>编码器实例。</returns>
        ITransportMessageEncoder GetEncoder();

        /// <summary>
        /// 获取解码器。
        /// </summary>
        /// <returns>解码器实例。</returns>
        ITransportMessageDecoder GetDecoder();
    }
}