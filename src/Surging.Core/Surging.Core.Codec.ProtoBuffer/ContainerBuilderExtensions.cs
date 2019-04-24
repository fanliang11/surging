using Surging.Core.CPlatform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Codec.ProtoBuffer
{
   public static class ContainerBuilderExtensions
    {
        public static IServiceBuilder UseProtoBufferCodec(this IServiceBuilder builder)
        {
            return builder.UseCodec<ProtoBufferTransportMessageCodecFactory>();
        }
    }
}
