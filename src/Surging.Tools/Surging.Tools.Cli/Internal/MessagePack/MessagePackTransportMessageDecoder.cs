using Surging.Tools.Cli.Internal.Messages;
using Surging.Tools.Cli.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Surging.Tools.Cli.Internal.MessagePack
{
    public sealed class MessagePackTransportMessageDecoder : ITransportMessageDecoder
    {
        #region Implementation of ITransportMessageDecoder

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TransportMessage Decode(byte[] data)
        {
            var message = SerializerUtilitys.Deserialize<MessagePackTransportMessage>(data);
            return message.GetTransportMessage();
        }

        #endregion Implementation of ITransportMessageDecoder
    }
}
