using Surging.Core.CPlatform.Codecs.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Transport
{
    public interface IMqttMessageSender:IDeviceMessageSender
    {
        Task SendAndFlushAsync(MqttMessage  mqttMessage);
    }
}
