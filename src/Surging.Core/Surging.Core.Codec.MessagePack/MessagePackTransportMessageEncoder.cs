using Surging.Core.Codec.MessagePack.Messages;
using Surging.Core.Codec.MessagePack.Utilities;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport.Codec;
using System.Runtime.CompilerServices;

namespace Surging.Core.Codec.MessagePack
{
   public sealed class MessagePackTransportMessageEncoder:ITransportMessageEncoder
    {
        #region Implementation of ITransportMessageEncoder

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
        #endregion Implementation of ITransportMessageEncoder
    }
}
