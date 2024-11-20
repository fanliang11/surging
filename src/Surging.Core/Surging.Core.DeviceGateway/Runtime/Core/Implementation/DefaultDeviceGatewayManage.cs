using Surging.Core.CPlatform.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Implementation
{
    public class DefaultDeviceGatewayManage : IDeviceGatewayManage
    {
        private readonly ConcurrentDictionary<string, DeviceGatewayProperties> _deviceGatewayProperties = new ConcurrentDictionary<string, DeviceGatewayProperties>();
        private readonly ConcurrentDictionary<string, IDeviceGateway> _deviceGateway = new ConcurrentDictionary<string, IDeviceGateway>();

        private readonly ConcurrentDictionary<string, IDeviceGatewayProvider> _deviceGatewayProvider = new ConcurrentDictionary<string, IDeviceGatewayProvider>();

        public IObservable<IDeviceGateway> CreateOrUpdate(IDeviceGatewayProvider provider, DeviceGatewayProperties properties)
        {
            _deviceGatewayProperties.AddOrUpdate(properties.Id, properties, (key, value) => properties);
            _deviceGatewayProvider.AddOrUpdate(properties.Id, provider, (key, value) => provider);
            var result = provider.CreateDeviceGateway(properties);
            result.Subscribe(p =>
            {
                _deviceGateway.AddOrUpdate(properties.Id, p, (key, value) => p);
            });
            return result;
        }

        public IObservable<IDeviceGateway?> GetGateway(string id)
        {
            return Observable.Return(_deviceGateway.GetValueOrDefault(id));
        }

        public List<DeviceGatewayProperties> GetGatewayProperties()
        {
            return _deviceGatewayProperties.Values.ToList();
        }

        public IDeviceGatewayProvider? GetProvider(string id)
        {
            return _deviceGatewayProvider.GetValueOrDefault(id);
        }

        public List<IDeviceGatewayProvider> GetProviders()
        {
            return _deviceGatewayProvider.Values.ToList();
        }

        public IObservable<IDeviceGateway> Reload(string id)
        {
            var result = Observable.Empty<IDeviceGateway>();
            var provider = _deviceGatewayProvider.GetValueOrDefault(id, null);
            var property = _deviceGatewayProperties.GetValueOrDefault(id, null);
            if (provider != null)
            {
                provider.CreateDeviceGateway(property).Subscribe(p =>
                {
                    result.Publish(p);
                    _deviceGateway.AddOrUpdate(property.Id, p, (key, value) => p);

                });
            }
            return result;
        }

        public IObservable<Task> Shutdown(string gatewayId)
        {
            var deviceGateway = _deviceGateway.GetValueOrDefault(gatewayId, null);
            if (deviceGateway != null)
            {
                return deviceGateway.ShutdownAsync();
            }
            return Observable.Empty<Task>();
        }

        public IObservable<Task> Start(string gatewayId)
        {
            var deviceGateway = _deviceGateway.GetValueOrDefault(gatewayId, null);
            if (deviceGateway != null)
            {
                return Observable.Return(deviceGateway.Startup());
            }
            return Observable.Empty<Task>();
        }
    }
}
