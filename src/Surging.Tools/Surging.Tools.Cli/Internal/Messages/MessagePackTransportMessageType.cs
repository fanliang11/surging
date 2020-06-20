
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Tools.Cli.Internal.Messages
{
    public class MessagePackTransportMessageType
    { 
        public static string remoteInvokeResultMessageTypeName= typeof(RemoteInvokeResultMessage).FullName;

        public static string remoteInvokeMessageTypeName = typeof(RemoteInvokeMessage).FullName;

        public static string httpMessageTypeName = typeof(HttpMessage).FullName;

        public static string httpResultMessageTypeName = typeof(HttpResultMessage<object>).FullName;
    }
}
