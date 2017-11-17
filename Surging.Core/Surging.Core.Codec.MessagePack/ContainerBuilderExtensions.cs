using Surging.Core.CPlatform;
using System;
using System.Collections.Generic;
using System.Text;

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
