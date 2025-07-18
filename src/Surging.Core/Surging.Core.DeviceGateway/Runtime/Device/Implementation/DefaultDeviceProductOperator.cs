using Surging.Core.CPlatform.Protocol;
using Surging.Core.DeviceGateway.Runtime.device;
using Surging.Core.DeviceGateway.Runtime.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Implementation
{
    public class DefaultDeviceProductOperator : IDeviceProductOperator
    {
        private IDeviceSessionManager _deviceSessionManager;
        private readonly List<DeviceInfo> _deviceInfos;
        private readonly ProtocolSupport _protocolSupport;
        
        public DefaultDeviceProductOperator(IDeviceSessionManager deviceSessionManager,
            List<DeviceInfo> deviceInfos,
            ProtocolSupport protocolSupport)
        {
            _deviceSessionManager = deviceSessionManager;
            _deviceInfos = deviceInfos;
            _protocolSupport = protocolSupport;
        }

        public ISubject<IDeviceOperator> GetDevices()
        {
            var result=new AsyncSubject<IDeviceOperator>();
            _deviceInfos.ForEach(async info =>
            {
                var deviceSession= await _deviceSessionManager.GetSession(info.Id);
                result.OnNext(new DefaultDeviceOperator(deviceSession, info, _protocolSupport));
            });
            result.OnCompleted();
            return result;
        }

        public string GetId()
        {
            throw new NotImplementedException();
        }

        public IObservable<IProtocolSupport> GetProtocol()
        {
            return Observable.Return( _protocolSupport);
           
        }
    }
}
