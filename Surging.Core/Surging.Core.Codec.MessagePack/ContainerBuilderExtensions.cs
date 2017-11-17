using Surging.Core.CPlatform;

namespace Surging.Core.Codec.MessagePack
{
    public static class ContainerBuilderExtensions
    {
        public static IServiceBuilder UseMessagePackCodec(this IServiceBuilder builder)
        {
            return builder.UseCodec<MessagePackTransportMessageCodecFactory>();
        }
    }
}