using Surging.Core.CPlatform.Messages;

namespace Surging.Core.CPlatform.Runtime.Client
{
    /// <summary>
    /// 远程调用上下文。
    /// </summary>
    public class RemoteInvokeContext
    {
        #region 属性

        /// <summary>
        /// Gets or sets the InvokeMessage
        /// 远程调用消息。
        /// </summary>
        public RemoteInvokeMessage InvokeMessage { get; set; }

        /// <summary>
        /// Gets or sets the Item
        /// </summary>
        public string Item { get; set; }

        #endregion 属性
    }
}