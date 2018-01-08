using ProtoBuf;
using Surging.Core.Codec.ProtoBuffer.Utilities;
using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Codec.ProtoBuffer.Messages
{
    [ProtoContract]
    public class ProtoBufferTransportMessage
    {
        public ProtoBufferTransportMessage(TransportMessage transportMessage)
        {
            Id = transportMessage.Id;
            ContentType = transportMessage.ContentType;

            object contentObject;
            if (transportMessage.IsInvokeMessage())
            {
                contentObject = new ProtoBufferRemoteInvokeMessage(transportMessage.GetContent<RemoteInvokeMessage>());
            }
            else if (transportMessage.IsInvokeResultMessage())
            {
                contentObject = new ProtoBufferRemoteInvokeResultMessage(transportMessage.GetContent<RemoteInvokeResultMessage>());
            }
            else
            {
                throw new NotSupportedException($"无法支持的消息类型：{ContentType}！");
            }

            Content = SerializerUtilitys.Serialize(contentObject);
        }

        public ProtoBufferTransportMessage()
        {
        }
        
        [ProtoMember(1)]
        public string Id { get; set; }
        
        [ProtoMember(2)]
        public byte[] Content { get; set; }

        [ProtoMember(3)]
        public string ContentType { get; set; }

    
        public bool IsInvokeMessage()
        {
            return ContentType == MessagePackTransportMessageType.remoteInvokeMessageTypeName;
        }

        public bool IsInvokeResultMessage()
        {
            return ContentType == MessagePackTransportMessageType.remoteInvokeResultMessageTypeName;
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
                    SerializerUtilitys.Deserialize<ProtoBufferRemoteInvokeMessage>(Content).GetRemoteInvokeMessage();
            }
            else if (IsInvokeResultMessage())
            {
                contentObject =
                    SerializerUtilitys.Deserialize<ProtoBufferRemoteInvokeResultMessage>(Content)
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