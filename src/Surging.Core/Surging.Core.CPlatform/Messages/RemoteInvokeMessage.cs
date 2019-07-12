using System.Collections.Generic;

namespace Surging.Core.CPlatform.Messages
{
    /// <summary>
    /// 远程调用消息。
    /// </summary>
    public class RemoteInvokeMessage
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Attachments
        /// </summary>
        public IDictionary<string, object> Attachments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether DecodeJOject
        /// </summary>
        public bool DecodeJOject { get; set; }

        /// <summary>
        /// Gets or sets the Parameters
        /// 服务参数。
        /// </summary>
        public IDictionary<string, object> Parameters { get; set; }

        /// <summary>
        /// Gets or sets the ServiceId
        /// 服务Id。
        /// </summary>
        public string ServiceId { get; set; }

        /// <summary>
        /// Gets or sets the ServiceKey
        /// </summary>
        public string ServiceKey { get; set; }

        #endregion 属性
    }
}