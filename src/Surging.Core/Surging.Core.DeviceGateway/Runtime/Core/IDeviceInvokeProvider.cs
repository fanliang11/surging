using Surging.Core.DeviceGateway.Runtime.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core
{
    public interface IDeviceInvokeProvider
    {
        Task<IDeviceMessage> Invoke(string messageId, object deviceMessage, CancellationToken cancellationToken);
    }
}
