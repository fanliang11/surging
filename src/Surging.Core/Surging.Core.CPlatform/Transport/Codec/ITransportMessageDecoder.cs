using Surging.Core.CPlatform.Messages;
using System;

namespace Surging.Core.CPlatform.Transport.Codec
{
    public interface ITransportMessageDecoder
    {
        TransportMessage Decode(byte[] data);

        TransportMessage Decode(Memory<byte> data);
    }
}