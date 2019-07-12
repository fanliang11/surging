using ProtoBuf;
using Surging.Core.Codec.ProtoBuffer.Utilities;
using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Codec.ProtoBuffer.Messages
{
    /// <summary>
    /// Defines the <see cref="ProtoBufferTransportMessage" />
    /// </summary>
    [ProtoContract]
    public class ProtoBufferTransportMessage
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtoBufferTransportMessage"/> class.
        /// </summary>
        public ProtoBufferTransportMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtoBufferTransportMessage"/> class.
        /// </summary>
        /// <param name="transportMessage">The transportMessage<see cref="TransportMessage"/></param>
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

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Content
        /// </summary>
        [ProtoMember(2)]
        public byte[] Content { get; set; }

        /// <summary>
        /// Gets or sets the ContentType
        /// </summary>
        [ProtoMember(3)]
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the Id
        /// </summary>
        [ProtoMember(1)]
        public string Id { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The GetTransportMessage
        /// </summary>
        /// <returns>The <see cref="TransportMessage"/></returns>
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

        /// <summary>
        /// The IsInvokeMessage
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        public bool IsInvokeMessage()
        {
            return ContentType == MessagePackTransportMessageType.remoteInvokeMessageTypeName;
        }

        /// <summary>
        /// The IsInvokeResultMessage
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        public bool IsInvokeResultMessage()
        {
            return ContentType == MessagePackTransportMessageType.remoteInvokeResultMessageTypeName;
        }

        #endregion 方法
    }
}