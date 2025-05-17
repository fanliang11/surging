using DotNetty.Buffers;
using Surging.Core.CPlatform.Transport;
using Surging.Core.Protocol.Mqtt.Internal.Messages;
using Surging.Core.Protocol.Mqtt.Internal.Services;
using System.Text;
using System.Threading.Tasks;


namespace Surging.Core.Protocol.Mqtt.Internal.Runtime.Implementation
{
    public class MqttMessageSender : IMqttMessageSender
    {
        private readonly IChannelService _channelService;
        public MqttMessageSender(IChannelService channelService)
        {
            _channelService = channelService;
        }

        public async Task SendAndFlushAsync(CPlatform.Codecs.Message.MqttMessage mqttMessage)
        {
            await _channelService.Publish(mqttMessage.ClientId, new MqttWillMessage
            {
                Qos = mqttMessage.QosLevel,
                Topic = mqttMessage.Topic,
                WillMessage = mqttMessage.Payload.ToString(Encoding.UTF8),
                WillRetain = mqttMessage.Retain,
            });
        }

        public async Task SendAndFlushAsync(object message)
        {
            var mqttMessage= message  as CPlatform.Codecs.Message.MqttMessage;
            if (mqttMessage != null)
            {
               await SendAndFlushAsync(mqttMessage);
            }
        }
    }
}
