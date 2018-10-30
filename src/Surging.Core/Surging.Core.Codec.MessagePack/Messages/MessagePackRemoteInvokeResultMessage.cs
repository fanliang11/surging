using MessagePack;
using Surging.Core.CPlatform.Messages;

namespace Surging.Core.Codec.MessagePack.Messages
{
    [MessagePackObject]
    public class MessagePackRemoteInvokeResultMessage
    {
        #region Constructor

        public MessagePackRemoteInvokeResultMessage(RemoteInvokeResultMessage message)
        {
            ExceptionMessage = message.ExceptionMessage;
            Result = message.Result == null ? null : new DynamicItem(message.Result);
        }

        public MessagePackRemoteInvokeResultMessage()
        {
        }

        public MessagePackRemoteInvokeResultMessage(object [] objs)
        {
            ExceptionMessage = objs[0]?.ToString();
            Result =new DynamicItem(objs[1] as  object []);
        }

        #endregion Constructor

        [Key(0)]
        public string ExceptionMessage { get; set; }

        [Key(1)]
        public DynamicItem Result { get; set; }

        public RemoteInvokeResultMessage GetRemoteInvokeResultMessage()
        {
            return new RemoteInvokeResultMessage
            {
                ExceptionMessage = ExceptionMessage,
                Result = Result?.Get()
            };
        }

        public object [] ToArray()
        {
            var result = new object[]
            {
               ExceptionMessage,
               Result,
            };
            return result;
        }
    }
}

