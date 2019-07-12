using Surging.Core.CPlatform;

namespace Surging.Core.Codec.MessagePack
{
    /// <summary>
    /// Defines the <see cref="ContainerBuilderExtensions" />
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        #region 方法

        /// <summary>
        /// 使用messagepack编码解码方式
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IServiceBuilder UseMessagePackCodec(this IServiceBuilder builder)
        {
            return builder.UseCodec<MessagePackTransportMessageCodecFactory>();
        }

        #endregion 方法
    }
}