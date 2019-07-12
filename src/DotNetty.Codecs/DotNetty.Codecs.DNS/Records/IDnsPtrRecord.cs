namespace DotNetty.Codecs.DNS.Records
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IDnsPtrRecord" />
    /// </summary>
    public interface IDnsPtrRecord : IDnsRecord
    {
        #region 属性

        /// <summary>
        /// Gets the HostName
        /// </summary>
        string HostName { get; }

        #endregion 属性
    }

    #endregion 接口
}