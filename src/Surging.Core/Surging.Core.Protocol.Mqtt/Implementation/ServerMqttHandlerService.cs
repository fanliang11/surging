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
                    if (mqttChannel == null || !mqttChannel.IsOnine())
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
        }

        public async Task Disconnect(IChannelHandlerContext context, DisconnectPacket packet)
        {
            await _channelService.Close(await _channelService.GetDeviceId(context.Channel), true);
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
                var topics = packet.Requests.Select(p => p.TopicFilter).ToArray();
                await _channelService.Suscribe(await _channelService.GetDeviceId(context.Channel), topics);
                await SubAck(context, SubAckPacket.InResponseTo(packet, QualityOfService.ExactlyOnce
                 ));
            }
        }

        public  async Task UnsubAck(IChannelHandlerContext context, UnsubAckPacket packet)
        {
           await context.WriteAndFlushAsync(packet);
        }

        public  async Task Unsubscribe(IChannelHandlerContext context, UnsubscribePacket packet)
        {
            string [] topics = packet.TopicFilters.ToArray();
            await _channelService.UnSubscribe(await _channelService.GetDeviceId(context.Channel), topics);
            await  UnsubAck(context, UnsubAckPacket.InResponseTo(packet));
        }
    }
}
