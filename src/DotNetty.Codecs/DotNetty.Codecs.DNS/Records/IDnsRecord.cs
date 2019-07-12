namespace DotNetty.Codecs.DNS.Records
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IDnsRecord" />
    /// </summary>
    public interface IDnsRecord
    {
        #region 属性

        /// <summary>
        /// Gets the DnsClass
        /// </summary>
        DnsRecordClass DnsClass { get; }

        /// <summary>
        /// Gets the Name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets the TimeToLive
        /// </summary>
        long TimeToLive { get; set; }

        /// <summary>
        /// Gets the Type
        /// </summary>
        DnsRecordType Type { get; }

        #endregion 属性
    }

    #endregion 接口
}