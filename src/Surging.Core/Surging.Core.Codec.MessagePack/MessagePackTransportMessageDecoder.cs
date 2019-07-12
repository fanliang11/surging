using Surging.Core.Codec.MessagePack.Messages;
using Surging.Core.Codec.MessagePack.Utilities;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport.Codec;
using System.Runtime.CompilerServices;

namespace Surging.Core.Codec.MessagePack
{
    /// <summary>
    /// Defines the <see cref="MessagePackTransportMessageDecoder" />
    /// </summary>
    public sealed class MessagePackTransportMessageDecoder : ITransportMessageDecoder
    {
        #region 方法

        /// <summary>
        /// The Decode
        /// </summary>
        /// <param name="data">The data<see cref="byte[]"/></param>
        /// <returns>The <see cref="TransportMessage"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TransportMessage Decode(byte[] data)
        {
            var message = SerializerUtilitys.Deserialize<MessagePackTransportMessage>(data);
            return message.GetTransportMessage();
        }

        #endregion 方法
    }
}