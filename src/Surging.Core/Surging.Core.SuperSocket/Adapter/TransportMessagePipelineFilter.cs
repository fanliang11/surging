using SuperSocket.ProtoBase;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport.Codec;
using Surging.Core.CPlatform.Utilities;
using System.Buffers;
using System.Text;

namespace Surging.Core.SuperSocket.Adapter
{
    public class TransportMessagePipelineFilter : TerminatorPipelineFilter<TransportMessage>
    {
        private readonly ITransportMessageDecoder _transportMessageDecoder;
        public TransportMessagePipelineFilter() : base(new[] { (byte)'\r', (byte)'\n' })
        {
            _transportMessageDecoder = ServiceLocator.GetService<ITransportMessageCodecFactory>().GetDecoder();
        }


        public override TransportMessage Filter(ref SequenceReader<byte> bufferStream)
        {
            try
            {
                var bytes = bufferStream.Sequence.Slice(0, bufferStream.Length - 2).ToArray();
                var transportMessage = _transportMessageDecoder.Decode(bytes);
                return transportMessage;
            }
            finally
            {
                bufferStream.Advance(bufferStream.Length);
            }
        }
    }
}
