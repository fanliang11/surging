using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Messages
{
    /// <summary>
    /// Defines the <see cref="MessagePackTransportMessageType" />
    /// </summary>
    public class MessagePackTransportMessageType
    {
        #region 字段

        /// <summary>
        /// Defines the httpMessageTypeName
        /// </summary>
        public static string httpMessageTypeName = typeof(HttpMessage).FullName;

        /// <summary>
        /// Defines the httpResultMessageTypeName
        /// </summary>
        public static string httpResultMessageTypeName = typeof(HttpResultMessage<object>).FullName;

        /// <summary>
        /// Defines the remoteInvokeMessageTypeName
        /// </summary>
        public static string remoteInvokeMessageTypeName = typeof(RemoteInvokeMessage).FullName;

        /// <summary>
        /// Defines the remoteInvokeResultMessageTypeName
        /// </summary>
        public static string remoteInvokeResultMessageTypeName = typeof(RemoteInvokeResultMessage).FullName;

        #endregion 字段
    }
}