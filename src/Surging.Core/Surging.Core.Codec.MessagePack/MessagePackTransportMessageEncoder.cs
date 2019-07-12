using Surging.Core.Codec.MessagePack.Messages;
using Surging.Core.Codec.MessagePack.Utilities;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport.Codec;
using System.Runtime.CompilerServices;

namespace Surging.Core.Codec.MessagePack
{
    /// <summary>
    /// Defines the <see cref="MessagePackTransportMessageEncoder" />
    /// </summary>
    public sealed class MessagePackTransportMessageEncoder : ITransportMessageEncoder
    {
        #region 方法

        /// <summary>
        /// The Encode
        /// </summary>
        /// <param name="message">The message<see cref="TransportMessage"/></param>
        /// <returns>The <see cref="byte[]"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] Encode(TransportMessage message)
        {
            var transportMessage = new MessagePackTransportMessage(message)
            {
                Id = message.Id,
                ContentType = message.ContentType,
            };
            return SerializerUtilitys.Serialize(transportMessage);
        }

        #endregion 方法
    }
}