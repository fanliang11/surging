using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Codecs.Mqtt.Packets;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Surging.Core.Protocol.Mqtt.Internal.Enums;
using System.Linq;
using Surging.Core.Protocol.Mqtt.Internal.Services;
using Surging.Core.Protocol.Mqtt.Internal.Runtime;
using System.Threading.Tasks;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform;
using System.Diagnostics;
using Surging.Core.CPlatform.Diagnostics;

namespace Surging.Core.Protocol.Mqtt.Implementation
{
    public  class ServerMqttHandlerService
    {
        private readonly ILogger _logger;
        private readonly IChannelService _channelService;
        private readonly IMqttBehaviorProvider _mqttBehaviorProvider;
        public ServerMqttHandlerService(
            ILogger logger, IChannelService channelService, IMqttBehaviorProvider mqttBehaviorProvider)
        {
            _logger = logger;
            _channelService = channelService;
            _mqttBehaviorProvider = mqttBehaviorProvider;
        }

        public async Task ConnAck(IChannelHandlerContext context, ConnAckPacket packet)
        {
           await context.WriteAndFlushAsync(packet);
        }

        public async Task Login(IChannelHandlerContext context, ConnectPacket packet)
        {

            string deviceId = packet.ClientId;
            var message = TransportMessage.CreateInvokeMessage(new RemoteInvokeMessage() { ServiceId = $"Connect", Parameters = new Dictionary<string, object> { { "packet", packet} } });
            WirteDiagnosticBefore(message,context.Channel.RemoteAddress.ToString(),  deviceId, packet.PacketType);
            if (string.IsNullOrEmpty(deviceId))
            {
               await ConnAck(context, new ConnAckPacket
                {
                    ReturnCode = ConnectReturnCode.RefusedIdentifierRejected
                });
                return;
            }

            var mqttBehavior = _mqttBehaviorProvider.GetMqttBehavior();
            if (mqttBehavior != null)
            {
                if (packet.HasPassword && packet.HasUsername
                        && await mqttBehavior.Authorized(packet.Username, packet.Password))
                {
                    var mqttChannel = _channelService.GetMqttChannel(deviceId);
                    if (mqttChannel == null || !await mqttChannel.IsOnine())
                    {
                        byte[] bytes = null;
                        if (packet.WillMessage != null)
                        {
                            bytes = new byte[packet.WillMessage.ReadableBytes];
                            packet.WillMessage.ReadBytes(bytes);
                        }
                       await _channelService.Login(context.Channel, deviceId, new ConnectMessage
                        {
                            CleanSession = packet.CleanSession,
                            ClientId = packet.ClientId,
                            Duplicate = packet.Duplicate,
                            HasPassword = packet.HasPassword,
                            HasUsername = packet.HasUsername,
                            HasWill = packet.HasWill,
                            KeepAliveInSeconds = packet.KeepAliveInSeconds,
                            Password = packet.Password,
                            ProtocolLevel = packet.ProtocolLevel,
                            ProtocolName = packet.ProtocolName,
                            Qos = (int)packet.QualityOfService,
                            RetainRequested = packet.RetainRequested,
                            Username = packet.Username,
                            WillMessage = bytes,
                            WillQualityOfService = (int)packet.WillQualityOfService,
                            WillRetain = packet.WillRetain,
                            WillTopic = packet.WillTopicName

                        });
                    }
                }
                else
                {
                  await  ConnAck(context, new ConnAckPacket
                    {
                        ReturnCode = ConnectReturnCode.RefusedBadUsernameOrPassword
                    });
                }
            }
            else
            {
               await ConnAck(context, new ConnAckPacket
                {
                    ReturnCode = ConnectReturnCode.RefusedServerUnavailable
                });
            }
            WirteDiagnosticAfter(message);
        }

        public async Task Disconnect(IChannelHandlerContext context, DisconnectPacket packet)
        {
            var deviceId = await _channelService.GetDeviceId(context.Channel);
            var message = TransportMessage.CreateInvokeMessage(new RemoteInvokeMessage() { ServiceId = $"Disconnect", Parameters = new Dictionary<string, object> { { "packet", packet } } });
            WirteDiagnosticBefore(message,context.Channel.RemoteAddress.ToString(), deviceId, packet.PacketType);
            await _channelService.Close(deviceId, true);
            WirteDiagnosticAfter(message);

        }

        public async Task  PingReq(IChannelHandlerContext context, PingReqPacket packet)
        {
            var channel = context.Channel;
            if (channel.Open && channel.Active && channel.IsWritable)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("收到来自：【" + context.Channel.RemoteAddress.ToString() + "】心跳");
                await _channelService.PingReq(context.Channel);
                await PingResp(context, PingRespPacket.Instance);
            }
        }

        public async Task PingResp(IChannelHandlerContext context, PingRespPacket packet)
        {
           await context.WriteAndFlushAsync(packet);
        }

        public async Task PubAck(IChannelHandlerContext context, PubAckPacket packet)
        {
            int messageId = packet.PacketId;
            var mqttChannel = _channelService.GetMqttChannel(await _channelService.GetDeviceId(context.Channel));
            var message = mqttChannel.GetMqttMessage(messageId);
            if (message != null)
            {
                message.ConfirmStatus = ConfirmStatus.COMPLETE;
            }
            await context.WriteAndFlushAsync(packet);
        }

        public async Task PubComp(IChannelHandlerContext context, PubCompPacket packet)
        {
            int messageId = packet.PacketId;
            var mqttChannel = _channelService.GetMqttChannel(await _channelService.GetDeviceId(context.Channel));
            var message = mqttChannel.GetMqttMessage(messageId);
            if (message != null)
            {
                message.ConfirmStatus = ConfirmStatus.COMPLETE;
            }
            await context.WriteAndFlushAsync(packet);
        }

        public async Task Publish(IChannelHandlerContext context, PublishPacket packet)
        {
            await _channelService.Publish(context.Channel, packet);
        }

        public async Task PubRec(IChannelHandlerContext context, PubRecPacket packet)
        {
            int messageId = packet.PacketId;
            var mqttChannel = _channelService.GetMqttChannel(await _channelService.GetDeviceId(context.Channel));
             var message= mqttChannel.GetMqttMessage(messageId);
            if (message != null)
            {
                message.ConfirmStatus = ConfirmStatus.PUBREC;
            }
            await _channelService.Pubrec(mqttChannel, messageId);
        }

        public async Task PubRel(IChannelHandlerContext context, PubRelPacket packet)
        {
            int messageId = packet.PacketId;
            var mqttChannel = _channelService.GetMqttChannel(await _channelService.GetDeviceId(context.Channel));
            var message = mqttChannel.GetMqttMessage(messageId);
            if (message != null)
            {
                message.ConfirmStatus = ConfirmStatus.PUBREL;
            }
            await _channelService.Pubrel(context.Channel, messageId);
        }

        public async Task SubAck(IChannelHandlerContext context, SubAckPacket packet)
        {
           await  context.WriteAndFlushAsync(packet);
        }

        public async Task Subscribe(IChannelHandlerContext context, SubscribePacket packet)
        {
            if (packet != null)
            {
               var deviceId= await _channelService.GetDeviceId(context.Channel);
                var topics = packet.Requests.Select(p => p.TopicFilter).ToArray();
                var message = TransportMessage.CreateInvokeMessage(new RemoteInvokeMessage() { ServiceId = $"Subscribe", Parameters=new Dictionary<string, object> { { "packet", packet } } });
                WirteDiagnosticBefore(message,context.Channel.RemoteAddress.ToString(), deviceId, packet.PacketType); 
                await _channelService.Suscribe(deviceId, topics);
                await SubAck(context, SubAckPacket.InResponseTo(packet, QualityOfService.ExactlyOnce));
                WirteDiagnosticAfter(message);
            }
        }

        public  async Task UnsubAck(IChannelHandlerContext context, UnsubAckPacket packet)
        {
           await context.WriteAndFlushAsync(packet);
        }

        public  async Task Unsubscribe(IChannelHandlerContext context, UnsubscribePacket packet)
        {
            string [] topics = packet.TopicFilters.ToArray();
            var deviceId = await _channelService.GetDeviceId(context.Channel);
            var message = TransportMessage.CreateInvokeMessage(new RemoteInvokeMessage() { ServiceId = $"Unsubscribe", Parameters = new Dictionary<string, object> { { "packet", packet } } });
            WirteDiagnosticBefore(message, context.Channel.RemoteAddress.ToString(), deviceId, packet.PacketType);
            await _channelService.UnSubscribe(deviceId, topics);
            await  UnsubAck(context, UnsubAckPacket.InResponseTo(packet));
            WirteDiagnosticAfter(message);
        }

        private void WirteDiagnosticBefore(TransportMessage message,string address, string traceId, PacketType packetType)
        {
            if (!AppConfig.ServerOptions.DisableDiagnostic)
            {
                var diagnosticListener = new DiagnosticListener(DiagnosticListenerExtensions.DiagnosticListenerName);
                var remoteInvokeMessage = message.GetContent<RemoteInvokeMessage>();
                diagnosticListener.WriteTransportBefore(TransportType.Mqtt, new TransportEventData(new DiagnosticMessage
                {
                    Content = message.Content,
                    ContentType = message.ContentType,
                    Id = message.Id,
                    MessageName = remoteInvokeMessage.ServiceId
                }, packetType.ToString(),
                traceId,address ));
            }
        }

        private void WirteDiagnosticAfter(TransportMessage message)
        {
            if (!AppConfig.ServerOptions.DisableDiagnostic)
            {
                var diagnosticListener = new DiagnosticListener(DiagnosticListenerExtensions.DiagnosticListenerName);
                diagnosticListener.WriteTransportAfter(TransportType.Mqtt, new ReceiveEventData(new DiagnosticMessage
                {
                    Content = message.Content,
                    ContentType = message.ContentType,
                    Id = message.Id
                }));
            }
        }
    }
}
