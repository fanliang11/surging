using Surging.Core.CPlatform.Messages;

namespace Surging.Core.CPlatform.Transport.Codec
{
    public interface ITransportMessageEncoder
    {
        byte[] Encode(TransportMessage message);
    }
}