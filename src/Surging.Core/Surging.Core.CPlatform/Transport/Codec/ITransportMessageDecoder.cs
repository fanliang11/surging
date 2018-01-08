using Surging.Core.CPlatform.Messages;

namespace Surging.Core.CPlatform.Transport.Codec
{
    public interface ITransportMessageDecoder
    {
        TransportMessage Decode(byte[] data);
    }
}