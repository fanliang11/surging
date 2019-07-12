using ProtoBuf;
using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surging.Core.Codec.ProtoBuffer.Messages
{
    /// <summary>
    /// Defines the <see cref="ParameterItem" />
    /// </summary>
    [ProtoContract]
    public class ParameterItem
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterItem"/> class.
        /// </summary>
        public ParameterItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterItem"/> class.
        /// </summary>
        /// <param name="item">The item<see cref="KeyValuePair{string, object}"/></param>
        public ParameterItem(KeyValuePair<string, object> item)
        {
            Key = item.Key;
            Value = item.Value == null ? null : new DynamicItem(item.Value);
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Key
        /// </summary>
        [ProtoMember(1)]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the Value
        /// </summary>
        [ProtoMember(2)]
        public DynamicItem Value { get; set; }

        #endregion 属性
    }

    /// <summary>
    /// Defines the <see cref="ProtoBufferRemoteInvokeMessage" />
    /// </summary>
    [ProtoContract]
    public class ProtoBufferRemoteInvokeMessage
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtoBufferRemoteInvokeMessage"/> class.
        /// </summary>
        public ProtoBufferRemoteInvokeMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtoBufferRemoteInvokeMessage"/> class.
        /// </summary>
        /// <param name="message">The message<see cref="RemoteInvokeMessage"/></param>
        public ProtoBufferRemoteInvokeMessage(RemoteInvokeMessage message)
        {
            ServiceId = message.ServiceId;
            DecodeJOject = message.DecodeJOject;
            ServiceKey = message.ServiceKey;
            Parameters = message.Parameters?.Select(i => new ParameterItem(i)).ToArray();
            Attachments = message.Attachments?.Select(i => new ParameterItem(i)).ToArray();
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Attachments
        /// </summary>
        [ProtoMember(6)]
        public ParameterItem[] Attachments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether DecodeJOject
        /// </summary>
        [ProtoMember(3)]
        public bool DecodeJOject { get; set; }

        /// <summary>
        /// Gets or sets the Parameters
        /// </summary>
        [ProtoMember(5)]
        public ParameterItem[] Parameters { get; set; }

        /// <summary>
        /// Gets or sets the ServiceId
        /// </summary>
        [ProtoMember(1)]
        public string ServiceId { get; set; }

        /// <summary>
        /// Gets or sets the ServiceKey
        /// </summary>
        [ProtoMember(4)]
        public string ServiceKey { get; set; }

        /// <summary>
        /// Gets or sets the Token
        /// </summary>
        [ProtoMember(2)]
        public string Token { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The GetRemoteInvokeMessage
        /// </summary>
        /// <returns>The <see cref="RemoteInvokeMessage"/></returns>
        public RemoteInvokeMessage GetRemoteInvokeMessage()
        {
            return new RemoteInvokeMessage
            {
                Parameters = Parameters?.ToDictionary(i => i.Key, i => i.Value?.Get()),
                Attachments = Attachments?.ToDictionary(i => i.Key, i => i.Value?.Get()),
                ServiceId = ServiceId,
                DecodeJOject = DecodeJOject,
                ServiceKey = ServiceKey,
            };
        }

        #endregion 方法
    }
}