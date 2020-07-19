using Surging.Tools.Cli.Internal.Messages;
using Surging.Tools.Cli.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Surging.Tools.Cli.Internal.MessagePack
{
   public sealed class MessagePackTransportMessageEncoder : ITransportMessageEncoder
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