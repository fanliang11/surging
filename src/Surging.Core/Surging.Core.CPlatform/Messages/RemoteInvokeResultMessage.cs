namespace Surging.Core.CPlatform.Messages
{
    /// <summary>
    /// 远程调用结果消息。
    /// </summary>
    public class RemoteInvokeResultMessage
    {
        #region 属性

        /// <summary>
        /// Gets or sets the ExceptionMessage
        /// 异常消息。
        /// </summary>
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// Gets or sets the Result
        /// 结果内容。
        /// </summary>
        public object Result { get; set; }

        /// <summary>
        /// Gets or sets the StatusCode
        /// 状态码
        /// </summary>
        public int StatusCode { get; set; } = 200;

        #endregion 属性
    }
}