using DotNetty.Common.Utilities;
using Surging.Core.CPlatform.Codecs.Message;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport;
using Surging.Core.DeviceGateway.Runtime.Core;
using Surging.Core.DeviceGateway.Runtime.Device;
using Surging.Core.DeviceGateway.Runtime.Device.Implementation.Http;
using Surging.Core.DeviceGateway.Runtime.session;
using Surging.Core.DeviceGateway.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using HttpRequestMessage = Surging.Core.DeviceGateway.Runtime.Device.Implementation.Http.HttpRequestMessage;

namespace Surging.Core.DeviceGateway.Runtime.Session.Implementation
{
    public class HttpDeviceSession : IDeviceSession
    {
        public readonly IDeviceOperator _deviceOperator;

        private readonly IHttpMessageSender _sender;

        private readonly MessageTransport _transport = MessageTransport.Http;

        private readonly EndPoint? _address;

        private long _lastPingTime = Utility.CurrentTimeMillis();

        private long _connectTime = Utility.CurrentTimeMillis();
        private TimeSpan _keepAliveTimeOutMs = TimeSpan.Zero;

        public HttpDeviceSession(IHttpMessageSender sender) :this(null,null, sender)
        {
         
        }

        public HttpDeviceSession(IDeviceOperator deviceOperator,EndPoint? endPoint,IHttpMessageSender sender)
        {
            _deviceOperator = deviceOperator;
            _address = endPoint;
            _sender = sender;
        }

        public void Close()
        { 
        }

        public long ConnectTime()
        {
            return _connectTime;
        }

        public EndPoint? GetClientAddress()
        {
            return _address;
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
            throw new NotImplementedException();
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
           return MessageTransport.Http;
        }

        public bool IsAlive()
        {
            return _keepAliveTimeOutMs == TimeSpan.Zero
              || Utility.CurrentTimeMillis() - _lastPingTime < _keepAliveTimeOutMs.TotalMilliseconds;
        }

        public IObservable<bool> IsAliveAsync()
        {
            var result = _keepAliveTimeOutMs == TimeSpan.Zero
  || Utility.CurrentTimeMillis() - _lastPingTime < _keepAliveTimeOutMs.TotalMilliseconds;
            return Observable.Return(result);
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
            
        }

        public void Ping()
        {
            _lastPingTime = Utility.CurrentTimeMillis();
        }

        public IObservable<Task> Send(EncodedMessage encodedMessage)
        {
            var responseMessage = encodedMessage as HttpRequestMessage;
             if (responseMessage != null)
                return Observable.Return(_sender.SendAndFlushAsync(responseMessage.PayloadAsString(),responseMessage.Headers.ToDictionary(p=>p.Name,p=>string.Join(";", p.Value))));
            return Observable.Return(Task.CompletedTask);
        }

        public void SetKeepAliveTimeout(TimeSpan timeout)
        {
            _keepAliveTimeOutMs = timeout;
        }
    }
}
