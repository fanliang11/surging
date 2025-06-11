using Surging.Core.CPlatform.Protocol;
using Surging.Core.CPlatform.Transport;
using Surging.Core.DeviceGateway.Runtime.Core;
using Surging.Core.DeviceGateway.Runtime.Core.Implementation;
using Surging.Core.DeviceGateway.Runtime.device;
using Surging.Core.DeviceGateway.Runtime.Enums;
using Surging.Core.DeviceGateway.Runtime.session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Implementation
{
    public class DefaultDeviceOperator : AbstractConfigurable,IDeviceOperator
    {
        private readonly IDeviceSession _deviceSession;
        private readonly DeviceInfo _deviceInfo; 
        private ProtocolSupport _protocolSupport;
        private readonly TimeSpan DEVICE_OFFLINE_TIME_0UT = TimeSpan.FromMinutes(10);

        public DefaultDeviceOperator(IDeviceSession deviceSession, DeviceInfo deviceInfo, ProtocolSupport protocolSupport)
        {
            _deviceSession = deviceSession;
            _deviceInfo = deviceInfo;
            _protocolSupport = protocolSupport;
        }

        public IObservable<AuthenticationResult> Authenticate(IAuthenticationRequest request)
        {
            var result = Observable.Return<AuthenticationResult>(default);
            _protocolSupport.GetAuthenticator(request.GetTransport()).Subscribe( p =>
            {
                p.Authenticate(request, this).Subscribe(authResult =>
                {
                    result = result.Publish(authResult);
                });

            });
            return result;
        }

        public sbyte CheckState()
        {
            return (sbyte)DeviceState.Online;
        }

        public bool Disconnect()
        {
               _deviceSession.Close();
            return true;
        }

        public string GetAddress()
        { 
            return _deviceInfo.Configuration.GetValueOrDefault("address")?.ToString();
        }

        public string GetDeviceId()
        {
            return _deviceInfo.Id;
        }

        public string GetNetworkId()
        {
            return _deviceInfo.Configuration.GetValueOrDefault("networkId")?.ToString();
        }

        public long GetOfflineTime()
        {
            return _deviceSession.LastPingTime();
        }

        public long GetOnlineTime()
        {
            return _deviceSession.ConnectTime();
        }

        public IProtocolSupport GetProtocol()
        {
           return _protocolSupport;
        }

        public string GetSessionId()
        {
            return _deviceSession.GetId();
        }

        public sbyte GetState()
        {
            if(TimeSpan.FromMilliseconds(GetOfflineTime()-GetOnlineTime())> DEVICE_OFFLINE_TIME_0UT)
            {
                return (sbyte) DeviceState.Offline;
            }
            if (_deviceSession.IsAlive())
                return (sbyte)DeviceState.Online;
            return (sbyte) DeviceState.Offline;
        }

        public bool IsOnline()
        {
            return _deviceSession.IsAlive();
        }

        public IMessageSender MessageSender()
        {
            return null;
        }

        public bool Offline()
        {
            return !_deviceSession.IsAlive();
        }

        public IObservable<bool> Online(string serverId, string sessionId)
        {
            return Observable.Return(true);
        }

        public IObservable<bool> Online(string serverId, string sessionId, string address)
        {
            return Observable.Return(true);
        }

        public IObservable<bool> Online(string serverId, string address, long onlineTime)
        {
            return Observable.Return(true);
        }

        public bool PutState(sbyte state)
        {
            return true;
        }

        public void SetAddress(string address)
        { 
        }
    }
}
