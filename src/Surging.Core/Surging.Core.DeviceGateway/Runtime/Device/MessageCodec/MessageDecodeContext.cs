using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Surging.Core.CPlatform.Codecs.Core;
using Surging.Core.CPlatform.Codecs.Message;
using Surging.Core.CPlatform.Protocol;
using Surging.Core.DeviceGateway.Runtime.session;
using Surging.Core.DeviceGateway.Runtime.Session;

namespace Surging.Core.DeviceGateway.Runtime.Device.MessageCodec
{
    public class MessageDecodeContext : IMessageCodecContext
    {  
        private readonly string _deviceId;
        private IDeviceSession _deviceSession;
        private readonly IEncodedMessage _encodedMessage;
        private readonly IDeviceRegistry _deviceRegistry;

        public MessageDecodeContext(string deviceId, IEncodedMessage encodedMessage, IDeviceSession deviceSession, IDeviceRegistry deviceRegistry)
        {
            _encodedMessage = encodedMessage;
            _deviceId = deviceId;
            _deviceSession = deviceSession;
            _deviceRegistry = deviceRegistry;
        }

        public IEncodedMessage GetMessage()
        {
            return _encodedMessage;
        }

        public async Task<IDeviceSession> GetSession()
        {
            return _deviceSession;
        }

        public IObservable<IDeviceOperator> GetDevice(string deviceId)
        {
            return _deviceRegistry.GetDevice(deviceId);
        }
    }
}
