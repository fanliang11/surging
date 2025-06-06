using Surging.Core.CPlatform.Protocol;
using Surging.Core.DeviceGateway.Runtime.device;
using Surging.Core.DeviceGateway.Runtime.Enums;
using Surging.Core.DeviceGateway.Runtime.Session;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Implementation
{
    public class DefaultDeviceRegistry : IDeviceRegistry
    {
        private readonly ConcurrentDictionary<string, DeviceInfo> _deviceList = new ConcurrentDictionary<string, DeviceInfo>();
        private readonly ConcurrentDictionary<string, ProductInfo> _productList = new ConcurrentDictionary<string, ProductInfo>();
        
        private IDeviceSessionManager _deviceSessionManager;
        private IProtocolSupports _protocolSupports;
        public DefaultDeviceRegistry(IDeviceSessionManager deviceSessionManager,IProtocolSupports protocolSupports) {
            _deviceSessionManager= deviceSessionManager;
            _protocolSupports= protocolSupports;
        }
        public ISubject<DeviceStateInfo> CheckDeviceState(List<string> ids)
        {
            var result = new AsyncSubject<DeviceStateInfo>();
            ids.ForEach(async id =>
            {
                var isAlive = await _deviceSessionManager.CheckAlive(id, true);
                var state = isAlive ? DeviceState.Online : DeviceState.Offline;
                result.OnNext(new DeviceStateInfo
                {
                    DeviceId = id,
                    State = (sbyte)state
                });
            });
            return result;
        }

        public IObservable<IDeviceOperator> GetDevice(string deviceId)
        {
            var result = Observable.Return<IDeviceOperator>(default);
            _deviceList.TryGetValue(deviceId, out DeviceInfo deviceInfo);
            if (deviceInfo != null)
            {
                _deviceSessionManager.GetSession(deviceInfo.Id).Subscribe(async p =>
                {
                    ProtocolSupport? protocolSupport = await _protocolSupports.GetProtocol(deviceInfo.Protocol) as ProtocolSupport;
                    result=  result.Publish(p?.GetOperator());
                });
            }
            return result;
        }

        public IObservable<IDeviceProductOperator> GetProduct(string productId)
        {
            var result = Observable.Empty<IDeviceProductOperator>();
            _productList.TryGetValue(productId, out ProductInfo productInfo);
            _productList.AddOrUpdate(productInfo.Id, productInfo, (key, value) => productInfo);
            var deviceInfos = _deviceList.Values.Where(p => p.ProductId == productInfo.Id).ToList();
            _protocolSupports.GetProtocol(productInfo.Id).Subscribe(async protocolSupport =>
         await result.Publish(new DefaultDeviceProductOperator(_deviceSessionManager, deviceInfos,(ProtocolSupport)protocolSupport))
        );
            return result;
        }

       

        public IObservable<IDeviceOperator> Register(DeviceInfo deviceInfo)
        {
            var result = Observable.Return<IDeviceOperator>(null);
            _deviceList.AddOrUpdate(deviceInfo.Id, deviceInfo, (key, value) => deviceInfo);

            _deviceSessionManager.GetSession(deviceInfo.Id).Subscribe(async p =>
          {
              ProtocolSupport protocolSupport = (ProtocolSupport)await _protocolSupports.GetProtocol(deviceInfo.Protocol);
              result=  result.Publish(new DefaultDeviceOperator(p, deviceInfo, protocolSupport));
          });

            return result;
        }

        public IObservable<IDeviceProductOperator> Register(ProductInfo productInfo)
        {
            var result = Observable.Empty<IDeviceProductOperator>();
            _productList.AddOrUpdate(productInfo.Id, productInfo, (key, value) => productInfo);
            var deviceInfos = _deviceList.Values.Where(p => p.ProductId == productInfo.Id).ToList();
            _protocolSupports.GetProtocol(productInfo.Id).Subscribe(async protocolSupport =>
         await result.Publish(new DefaultDeviceProductOperator(_deviceSessionManager, deviceInfos, (ProtocolSupport)protocolSupport))
        
        );
            return result;
        }

        public void UnregisterDevice(string deviceId)
        {
            _deviceList.TryRemove(deviceId, out _);
        }

        public void UnregisterProduct(string productId)
        {
            _productList.TryRemove(productId, out _);
        }
    }
}
