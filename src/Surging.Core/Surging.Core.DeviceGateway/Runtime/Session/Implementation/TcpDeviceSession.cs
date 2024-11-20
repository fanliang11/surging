using Surging.Core.CPlatform.Codecs.Message;
using Surging.Core.CPlatform.Protocol;
using Surging.Core.DeviceGateway.Runtime.Core;
using Surging.Core.DeviceGateway.Runtime.Device;
using Surging.Core.DeviceGateway.Runtime.session;
using Surging.Core.DeviceGateway.Utilities;
using System.Net;
using System.Reactive.Linq;

namespace Surging.Core.DeviceGateway.Runtime.Session.Implementation
{
    public class TcpDeviceSession : IDeviceSession
    {
        public readonly IDeviceOperator _deviceOperator;

        private readonly IClientConnection _client;


        private readonly MessageTransport _transport=MessageTransport.Tcp;

        private readonly long _lastPingTime = Utility.CurrentTimeMillis();

        private readonly long _connectTime = Utility.CurrentTimeMillis();

        public TcpDeviceSession(IDeviceOperator deviceOperator, IClientConnection client, MessageTransport transport)
        {
            _deviceOperator = deviceOperator;
            _client = client; 
        }   

        public void Close()
        {
            _client.OnDisconnect();
        }

        public long ConnectTime()
        {
            return _connectTime;
        }

        public EndPoint GetClientAddress()
        {
            return _client.GetClientAddress();
        }

        public string GetDeviceId()
        {
            return _deviceOperator.GetDeviceId();
        }

        public string GetId()
        {
            return GetDeviceId();
        }

        public TimeSpan GetKeepAliveTimeout()
        {
           return _client.GetKeepAliveTimeout();
        }

        public IDeviceOperator GetOperator()
        {
            return _deviceOperator;
        }

        public string GetServerId()
        {
            return string.Empty;
        }

        public MessageTransport GetTransport()
        {
            return _transport;
        }

        public bool IsAlive()
        {
          return _client.IsAlive();
        }

        public IObservable<bool> IsAliveAsync()
        {
            return Observable.Return(IsAlive());
        }

        public void KeepAlive()
        {

        }

        public long LastPingTime()
        {
            return _lastPingTime;
        }

        public void OnClose(Action call)
        {
            Close();
            call();
        }

        public void Ping()
        {
        }

        public IObservable<Task> Send(EncodedMessage encodedMessage)
        {
            return  _client.SendMessage(new TcpMessage(encodedMessage.Payload));
        }

        public void SetKeepAliveTimeout(TimeSpan timeout)
        {
            _client.SetKeepAliveTimeout(timeout);
        }
    }
}
