using Surging.Core.CPlatform.Messages;

namespace Surging.Tools.Cli.Internal
{
    public interface ITransportMessageEncoder
    {
        byte[] Encode(TransportMessage message);
    }
}
