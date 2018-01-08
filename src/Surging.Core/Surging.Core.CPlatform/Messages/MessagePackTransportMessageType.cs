using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Messages
{
   public class MessagePackTransportMessageType
    {
        public static string remoteInvokeResultMessageTypeName= typeof(RemoteInvokeResultMessage).FullName;

        public static string remoteInvokeMessageTypeName = typeof(RemoteInvokeMessage).FullName;
    }
}
