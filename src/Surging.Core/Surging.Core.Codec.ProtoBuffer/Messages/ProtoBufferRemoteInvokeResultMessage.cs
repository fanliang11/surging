using ProtoBuf;
using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Codec.ProtoBuffer.Messages
{
    [ProtoContract]
   public class ProtoBufferRemoteInvokeResultMessage
    {
        #region Constructor

        public ProtoBufferRemoteInvokeResultMessage(RemoteInvokeResultMessage message)
        {
            ExceptionMessage = message.ExceptionMessage;
            Result = message.Result == null ? null : new DynamicItem(message.Result);
        }

        public ProtoBufferRemoteInvokeResultMessage()
        {
        }

        #endregion Constructor
        
        [ProtoMember(1)]
        public string ExceptionMessage { get; set; }

        
        [ProtoMember(2)]
        public DynamicItem Result { get; set; }

        public RemoteInvokeResultMessage GetRemoteInvokeResultMessage()
        {
            return new RemoteInvokeResultMessage
            {
                ExceptionMessage = ExceptionMessage,
                Result = Result?.Get()
            };
        }
    }
}

