using MessagePack;
using Surging.Core.Codec.MessagePack.Utilities;
using Surging.Core.CPlatform.Messages;
using System;

namespace Surging.Core.Codec.MessagePack.Messages
{
    [MessagePackObject]
    public class MessagePackTransportMessage
    {
        public MessagePackTransportMessage(TransportMessage transportMessage)
        {
            Id = transportMessage.Id;
            ContentType = transportMessage.ContentType;

            object contentObject;
            if (transportMessage.IsInvokeMessage())
            {
                contentObject = new MessagePackRemoteInvokeMessage(transportMessage.GetContent<RemoteInvokeMessage>()).ToArry();
            }
            else if (transportMessage.IsInvokeResultMessage())
            {
                contentObject = new MessagePackRemoteInvokeResultMessage(transportMessage.GetContent<RemoteInvokeResultMessage>()).ToArray();
            }
            else
            {
                throw new NotSupportedException($"无法支持的消息类型：{ContentType}！");
            }

            Content = SerializerUtilitys.Serialize(contentObject);
        }

        public MessagePackTransportMessage(object [] obj)
        {
            Id = obj[0].ToString();
            Content = obj[1] as byte[];
            ContentType = obj[2].ToString();
        }

        public MessagePackTransportMessage()
        {
        }

        [Key(0)]
        public string Id { get; set; }

        [Key(1)]
        public byte[] Content { get; set; }

        [Key(2)]
        public string ContentType { get; set; }

        public bool IsInvokeMessage()
        {
            return ContentType == MessagePackTransportMessageType.remoteInvokeMessageTypeName;
        }

        public bool IsInvokeResultMessage()
        {
            return ContentType == MessagePackTransportMessageType.remoteInvokeResultMessageTypeName;
        }

        public object[] ToArray()
        {
            var result = new object[]
            {
                Id,
                Content,
                ContentType
            };
            return result;
        }

        public TransportMessage GetTransportMessage()
        {
            var message = new TransportMessage
            {
                ContentType = ContentType,
                Id = Id,
                Content = null
            };

            object contentObject;
            if (IsInvokeMessage())
            {
                contentObject =
                   new MessagePackRemoteInvokeMessage(
                       SerializerUtilitys.Deserialize<object[]>(Content))
                       .GetRemoteInvokeMessage();
            }
            else if (IsInvokeResultMessage())
            {
                contentObject =new MessagePackRemoteInvokeResultMessage(
                    SerializerUtilitys.Deserialize<object[]>(Content))
                        .GetRemoteInvokeResultMessage();
            }
            else
            {
                throw new NotSupportedException($"无法支持的消息类型：{ContentType}！");
            }
            message.Content = contentObject;
            return message;
        }
    }
}