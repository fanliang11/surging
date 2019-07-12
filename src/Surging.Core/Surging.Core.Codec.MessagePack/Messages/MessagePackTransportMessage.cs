using MessagePack;
using Surging.Core.Codec.MessagePack.Utilities;
using Surging.Core.CPlatform.Messages;
using System;
using System.Runtime.CompilerServices;

namespace Surging.Core.Codec.MessagePack.Messages
{
    /// <summary>
    /// Defines the <see cref="MessagePackTransportMessage" />
    /// </summary>
    [MessagePackObject]
    public class MessagePackTransportMessage
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePackTransportMessage"/> class.
        /// </summary>
        public MessagePackTransportMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePackTransportMessage"/> class.
        /// </summary>
        /// <param name="transportMessage">The transportMessage<see cref="TransportMessage"/></param>
        public MessagePackTransportMessage(TransportMessage transportMessage)
        {
            Id = transportMessage.Id;
            ContentType = transportMessage.ContentType;

            object contentObject;
            if (transportMessage.IsInvokeMessage())
            {
                contentObject = new MessagePackRemoteInvokeMessage(transportMessage.GetContent<RemoteInvokeMessage>());
            }
            else if (transportMessage.IsInvokeResultMessage())
            {
                contentObject = new MessagePackRemoteInvokeResultMessage(transportMessage.GetContent<RemoteInvokeResultMessage>());
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
        [Key(1)]
        public byte[] Content { get; set; }

        /// <summary>
        /// Gets or sets the ContentType
        /// </summary>
        [Key(2)]
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the Id
        /// </summary>
        [Key(0)]
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
                    SerializerUtilitys.Deserialize<MessagePackRemoteInvokeMessage>(Content).GetRemoteInvokeMessage();
            }
            else if (IsInvokeResultMessage())
            {
                contentObject =
                    SerializerUtilitys.Deserialize<MessagePackRemoteInvokeResultMessage>(Content)
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInvokeMessage()
        {
            return ContentType == MessagePackTransportMessageType.remoteInvokeMessageTypeName;
        }

        /// <summary>
        /// The IsInvokeResultMessage
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInvokeResultMessage()
        {
            return ContentType == MessagePackTransportMessageType.remoteInvokeResultMessageTypeName;
        }

        #endregion 方法
    }
}