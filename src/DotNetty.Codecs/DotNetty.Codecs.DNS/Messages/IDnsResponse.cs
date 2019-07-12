namespace DotNetty.Codecs.DNS.Messages
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IDnsResponse" />
    /// </summary>
    public interface IDnsResponse : IDnsMessage
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Code
        /// </summary>
        DnsResponseCode Code { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsAuthoritativeAnswer
        /// </summary>
        bool IsAuthoritativeAnswer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsRecursionAvailable
        /// </summary>
        bool IsRecursionAvailable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsTruncated
        /// </summary>
        bool IsTruncated { get; set; }

        #endregion 属性
    }

    #endregion 接口
}