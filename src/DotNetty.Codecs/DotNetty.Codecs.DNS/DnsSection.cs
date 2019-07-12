namespace DotNetty.Codecs.DNS
{
    #region 枚举

    /// <summary>
    /// Defines the DnsSection
    /// </summary>
    public enum DnsSection
    {
        /// <summary>
        /// Defines the QUESTION
        /// </summary>
        QUESTION,

        /// <summary>
        /// Defines the ANSWER
        /// </summary>
        ANSWER,

        /// <summary>
        /// Defines the AUTHORITY
        /// </summary>
        AUTHORITY,

        /// <summary>
        /// Defines the ADDITIONAL
        /// </summary>
        ADDITIONAL
    }

    #endregion 枚举
}