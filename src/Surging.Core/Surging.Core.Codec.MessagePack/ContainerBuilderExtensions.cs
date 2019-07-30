using Surging.Core.CPlatform;

namespace Surging.Core.Codec.MessagePack
{
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// 使用messagepack编码解码方式 
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IServiceBuilder UseMessagePackCodec(this IServiceBuilder builder)
        {
            return builder.UseCodec<MessagePackTransportMessageCodecFactory>();
        }
    }
}
