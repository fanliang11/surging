using MessagePack;
using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Surging.Core.Codec.MessagePack.Messages
{
    /// <summary>
    /// Defines the <see cref="MessagePackRemoteInvokeMessage" />
    /// </summary>
    [MessagePackObject]
    public class MessagePackRemoteInvokeMessage
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePackRemoteInvokeMessage"/> class.
        /// </summary>
        public MessagePackRemoteInvokeMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePackRemoteInvokeMessage"/> class.
        /// </summary>
        /// <param name="message">The message<see cref="RemoteInvokeMessage"/></param>
        public MessagePackRemoteInvokeMessage(RemoteInvokeMessage message)
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
        [Key(5)]
        public ParameterItem[] Attachments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether DecodeJOject
        /// </summary>
        [Key(2)]
        public bool DecodeJOject { get; set; }

        /// <summary>
        /// Gets or sets the Parameters
        /// </summary>
        [Key(4)]
        public ParameterItem[] Parameters { get; set; }

        /// <summary>
        /// Gets or sets the ServiceId
        /// </summary>
        [Key(0)]
        public string ServiceId { get; set; }

        /// <summary>
        /// Gets or sets the ServiceKey
        /// </summary>
        [Key(3)]
        public string ServiceKey { get; set; }

        /// <summary>
        /// Gets or sets the Token
        /// </summary>
        [Key(1)]
        public string Token { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The GetRemoteInvokeMessage
        /// </summary>
        /// <returns>The <see cref="RemoteInvokeMessage"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    /// <summary>
    /// Defines the <see cref="ParameterItem" />
    /// </summary>
    [MessagePackObject]
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
        [Key(0)]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the Value
        /// </summary>
        [Key(1)]
        public DynamicItem Value { get; set; }

        #endregion 属性
    }
}