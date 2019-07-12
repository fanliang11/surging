namespace DotNetty.Codecs.DNS.Records
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IDnsOptEcsRecord" />
    /// </summary>
    public interface IDnsOptEcsRecord : IDnsOptPseudoRecord
    {
        #region 属性

        /// <summary>
        /// Gets the Address
        /// </summary>
        byte[] Address { get; }

        /// <summary>
        /// Gets the ScopePrefixLength
        /// </summary>
        int ScopePrefixLength { get; }

        /// <summary>
        /// Gets the SourcePrefixLength
        /// </summary>
        int SourcePrefixLength { get; }

        #endregion 属性
    }

    #endregion 接口
}