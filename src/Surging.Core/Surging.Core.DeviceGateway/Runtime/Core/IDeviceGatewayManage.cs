using Microsoft.CodeAnalysis;
using Surging.Core.CPlatform.Network;
using Surging.Core.DeviceGateway.Runtime.Core.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core
{
    public interface IDeviceGatewayManage
    {
        IObservable<Task> Shutdown(String gatewayId);
        IObservable<Task> Start(String gatewayId);

        IObservable<IDeviceGateway?> GetGateway(String id);

        public List<DeviceGatewayProperties> GetGatewayProperties();
        public IDeviceGatewayProvider? GetProvider(string provider);

        public List<IDeviceGatewayProvider> GetProviders();

        IObservable<IDeviceGateway> Reload(string id);
        IObservable<IDeviceGateway> CreateOrUpdate(IDeviceGatewayProvider provider,  DeviceGatewayProperties properties);
    }
}
