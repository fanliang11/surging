using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.MessageCodec
{
    public interface IDeviceMessageDecoder
    {
        IObservable<IDeviceMessage> Decode(MessageDecodeContext context);
    }
}
