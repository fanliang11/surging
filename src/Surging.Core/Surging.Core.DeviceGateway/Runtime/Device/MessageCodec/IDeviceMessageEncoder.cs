using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Surging.Core.CPlatform.Codecs.Core;
using Surging.Core.CPlatform.Codecs.Message;

namespace Surging.Core.DeviceGateway.Runtime.Device.MessageCodec
{
    public interface IDeviceMessageEncoder
    {
        IObservable<IEncodedMessage> Encode(MessageEncodeContext context);
    }
}
