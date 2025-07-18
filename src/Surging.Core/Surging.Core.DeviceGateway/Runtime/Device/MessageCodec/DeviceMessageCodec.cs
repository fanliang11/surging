using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Surging.Core.CPlatform.Codecs.Core;
using Surging.Core.CPlatform.Codecs.Message;
using Surging.Core.DeviceGateway.Runtime.Core;

namespace Surging.Core.DeviceGateway.Runtime.Device.MessageCodec
{
    public abstract class DeviceMessageCodec : IDeviceMessageDecoder, IDeviceMessageEncoder
    {
        public virtual MessageTransport SupportTransport { get; set; }
            =MessageTransport.Tcp;
    
        public abstract IObservable<IDeviceMessage> Decode(MessageDecodeContext context);

        public abstract IObservable<IEncodedMessage> Encode(MessageEncodeContext context);
    }
}
