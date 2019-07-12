using Surging.Core.CPlatform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Codec.ProtoBuffer
{
    /// <summary>
    /// Defines the <see cref="ContainerBuilderExtensions" />
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        #region 方法

        /// <summary>
        /// The UseProtoBufferCodec
        /// </summary>
        /// <param name="builder">The builder<see cref="IServiceBuilder"/></param>
        /// <returns>The <see cref="IServiceBuilder"/></returns>
        public static IServiceBuilder UseProtoBufferCodec(this IServiceBuilder builder)
        {
            return builder.UseCodec<ProtoBufferTransportMessageCodecFactory>();
        }

        #endregion 方法
    }
}