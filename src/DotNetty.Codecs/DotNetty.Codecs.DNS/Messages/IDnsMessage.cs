using DotNetty.Codecs.DNS.Records;
using DotNetty.Common;

namespace DotNetty.Codecs.DNS.Messages
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IDnsMessage" />
    /// </summary>
    public interface IDnsMessage : IReferenceCounted
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Id
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsRecursionDesired
        /// </summary>
        bool IsRecursionDesired { get; set; }

        /// <summary>
        /// Gets or sets the OpCode
        /// </summary>
        DnsOpCode OpCode { get; set; }

        /// <summary>
        /// Gets or sets the Z
        /// </summary>
        int Z { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The AddRecord
        /// </summary>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        /// <param name="record">The record<see cref="IDnsRecord"/></param>
        void AddRecord(DnsSection section, IDnsRecord record);

        /// <summary>
        /// The AddRecord
        /// </summary>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        /// <param name="index">The index<see cref="int"/></param>
        /// <param name="record">The record<see cref="IDnsRecord"/></param>
        void AddRecord(DnsSection section, int index, IDnsRecord record);

        /// <summary>
        /// The Clear
        /// </summary>
        void Clear();

        /// <summary>
        /// The Clear
        /// </summary>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        void Clear(DnsSection section);

        /// <summary>
        /// The Count
        /// </summary>
        /// <returns>The <see cref="int"/></returns>
        int Count();

        /// <summary>
        /// The Count
        /// </summary>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        /// <returns>The <see cref="int"/></returns>
        int Count(DnsSection section);

        /// <summary>
        /// The GetRecord
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        /// <returns>The <see cref="TRecord"/></returns>
        TRecord GetRecord<TRecord>(DnsSection section) where TRecord : IDnsRecord;

        /// <summary>
        /// The GetRecord
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        /// <param name="index">The index<see cref="int"/></param>
        /// <returns>The <see cref="TRecord"/></returns>
        TRecord GetRecord<TRecord>(DnsSection section, int index) where TRecord : IDnsRecord;

        /// <summary>
        /// The RemoveRecord
        /// </summary>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        /// <param name="index">The index<see cref="int"/></param>
        void RemoveRecord(DnsSection section, int index);

        /// <summary>
        /// The SetRecord
        /// </summary>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        /// <param name="record">The record<see cref="IDnsRecord"/></param>
        void SetRecord(DnsSection section, IDnsRecord record);

        /// <summary>
        /// The SetRecord
        /// </summary>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        /// <param name="index">The index<see cref="int"/></param>
        /// <param name="record">The record<see cref="IDnsRecord"/></param>
        void SetRecord(DnsSection section, int index, IDnsRecord record);

        #endregion 方法
    }

    #endregion 接口
}