using Surging.Core.CPlatform.Transport.Codec;
namespace Surging.Core.Codec.MessagePack
{
    public sealed class MessagePackTransportMessageCodecFactory : ITransportMessageCodecFactory
    {
        #region Field
        private readonly ITransportMessageEncoder _transportMessageEncoder = new MessagePackTransportMessageEncoder();
        private readonly ITransportMessageDecoder _transportMessageDecoder = new MessagePackTransportMessageDecoder();
        #endregion Field

        #region Implementation of ITransportMessageCodecFactory

        public ITransportMessageEncoder GetEncoder()
        {
            return _transportMessageEncoder;
        }

        public ITransportMessageDecoder GetDecoder()
        {
            return _transportMessageDecoder;
        }
        #endregion Implementation of ITransportMessageCodecFactory
    }
}
