using Surging.Core.CPlatform.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device
{
    public interface IDeviceProductOperator
    {
        string GetId(); 
 
        IObservable<IProtocolSupport> GetProtocol();
 
        ISubject<IDeviceOperator> GetDevices();
    }
}
