using Surging.Core.CPlatform.Protocol;
using Surging.Core.DeviceGateway.Runtime.Core;
using Surging.Core.DeviceGateway.Runtime.Device.MessageCodec;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Implementation
{
    public class ComplexProtocolSupport : ProtocolSupport
    { 
        private readonly ConcurrentDictionary<string, IObservable<DeviceMessageCodec>> _messageCodecSupports = new ConcurrentDictionary<string, IObservable<DeviceMessageCodec>>();

        public override void AddMessageCodecSupport(MessageTransport transportType, Func<IObservable<DeviceMessageCodec>> messageCodec)
        {
            _messageCodecSupports.GetOrAdd(transportType.ToString(), messageCodec.Invoke());
        }

        public override IObservable<DeviceMessageCodec> GetMessageCodecSupport(string transportType)
        {
            if (!string.IsNullOrEmpty(transportType))
            {
                return _messageCodecSupports.GetValueOrDefault(transportType);
            }
            return Observable.Return(default(DeviceMessageCodec));
        }
         
    }
}
