using Surging.Core.DeviceGateway.Runtime.device;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device
{
    public interface IDeviceRegistry
    {
        IObservable<IDeviceOperator> GetDevice(string deviceId);

        IObservable<IDeviceProductOperator> GetProduct(string productId);

        ISubject<DeviceStateInfo> CheckDeviceState(List<string> id);

        IObservable<IDeviceOperator> Register(DeviceInfo deviceInfo);

        IObservable<IDeviceProductOperator> Register(ProductInfo productInfo);

        void UnregisterDevice(string deviceId);

        void UnregisterProduct(string productId);
    }
}
