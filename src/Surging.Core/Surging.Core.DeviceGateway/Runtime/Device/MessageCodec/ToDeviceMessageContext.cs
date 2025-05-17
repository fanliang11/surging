using DotNetty.Common.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.Win32;
using Surging.Core.DeviceGateway.Runtime.session;
using Surging.Core.DeviceGateway.Runtime.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.MessageCodec
{
    public class ToDeviceMessageContext: MessageEncodeContext
    {
        public readonly IDeviceOperator _device;
        private readonly IDeviceMessage _message;
        private readonly IDeviceSession _session;
        private readonly IDeviceRegistry _registry;
        private readonly IDeviceSessionManager _sessionManager;
        private Action<IDeviceMessage> Reply { get; set; }
        public ToDeviceMessageContext(IDeviceOperator device,
            IDeviceMessage message, 
            IDeviceSession session, 
            IDeviceRegistry deviceRegistry,
            IDeviceSessionManager  deviceSessionManager, Action<IDeviceMessage> reply) :base(message, reply)
        {
            _device = device;
            _message = message;
            _session = session;
            _registry = deviceRegistry;
            _sessionManager = deviceSessionManager;
        }

        public IDeviceOperator GetDevice()
        {
            return _device;
        }
         
        public IObservable<IDeviceOperator> GetDevice(String deviceId)
        {
            return _registry.GetDevice(deviceId);
        }
        public IObservable<bool> Disconnect()
        {
            return _sessionManager
                .Remove(_device.GetDeviceId(), true); 
        }
         
        public IDeviceSession GetSession()
        {
            return _session;
        } 
        public IObservable<IDeviceSession> getSession(String deviceId)
        {
            return _sessionManager.GetSession(deviceId);
        }
         
        public IObservable<Boolean> SessionIsAlive(String deviceId)
        {
            return _sessionManager.IsAlive(deviceId);
        }

        //public Dictionary<String, Object> getConfiguration()
        //{
        //    return ToDeviceMessageContext.super.getConfiguration();
        //}

    
        public IDeviceMessage GetMessage()
        {
            return _message;
        }
    }
}
