using ProtoBuf;
using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Codec.ProtoBuffer.Messages
{
    /// <summary>
    /// Defines the <see cref="ProtoBufferRemoteInvokeResultMessage" />
    /// </summary>
    [ProtoContract]
    public class ProtoBufferRemoteInvokeResultMessage
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtoBufferRemoteInvokeResultMessage"/> class.
        /// </summary>
        public ProtoBufferRemoteInvokeResultMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtoBufferRemoteInvokeResultMessage"/> class.
        /// </summary>
        /// <param name="message">The message<see cref="RemoteInvokeResultMessage"/></param>
        public ProtoBufferRemoteInvokeResultMessage(RemoteInvokeResultMessage message)
        {
            ExceptionMessage = message.ExceptionMessage;
            Result = message.Result == null ? null : new DynamicItem(message.Result);
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the ExceptionMessage
        /// </summary>
        [ProtoMember(1)]
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// Gets or sets the Result
        /// </summary>
        [ProtoMember(2)]
        public DynamicItem Result { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The GetRemoteInvokeResultMessage
        /// </summary>
        /// <returns>The <see cref="RemoteInvokeResultMessage"/></returns>
        public RemoteInvokeResultMessage GetRemoteInvokeResultMessage()
        {
            return new RemoteInvokeResultMessage
            {
                ExceptionMessage = ExceptionMessage,
                Result = Result?.Get()
            };
        }

        #endregion 方法
    }
}