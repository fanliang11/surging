using Microsoft.Win32;
using Surging.Core.DeviceGateway.Runtime.Core.Implementation;
using Surging.Core.DeviceGateway.Runtime.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core
{
    public interface IAuthenticator
    { 
        IObservable<AuthenticationResult> Authenticate(IAuthenticationRequest request,
                                                   IDeviceOperator device);
 
        IObservable<AuthenticationResult> Authenticate(IAuthenticationRequest request,
                                                      IDeviceRegistry registry);
    }
}
