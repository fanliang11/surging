using Surging.Core.CPlatform.Messages;

namespace Surging.Tools.Cli.Internal
{
    public  interface ITransportMessageDecoder
    {
        TransportMessage Decode(byte[] data);
    }
}
