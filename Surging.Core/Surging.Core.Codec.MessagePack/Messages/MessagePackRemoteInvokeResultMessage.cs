using Surging.Core.CPlatform.Messages;

namespace Surging.Core.Codec.MessagePack.Messages
{
    public class MessagePackRemoteInvokeResultMessage
    {
        #region Constructor

        public MessagePackRemoteInvokeResultMessage(RemoteInvokeResultMessage message)
        {
            ExceptionMessage = message.ExceptionMessage;
            Result = message.Result == null ? null : new DynamicItem(message.Result);
        }

        public MessagePackRemoteInvokeResultMessage()
        { }

        #endregion Constructor

        public string ExceptionMessage { get; set; }

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