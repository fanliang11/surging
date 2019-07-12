namespace DotNetty.Codecs.DNS.Records
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IDnsOptPseudoRecord" />
    /// </summary>
    public interface IDnsOptPseudoRecord : IDnsRecord
    {
        #region 属性

        /// <summary>
        /// Gets the ExtendedRcode
        /// </summary>
        int ExtendedRcode { get; }

        /// <summary>
        /// Gets the Flags
        /// </summary>
        int Flags { get; }

        /// <summary>
        /// Gets the Version
        /// </summary>
        int Version { get; }

        #endregion 属性
    }

    #endregion 接口
}