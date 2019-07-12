using DotNetty.Buffers;
using DotNetty.Codecs.DNS.Messages;
using DotNetty.Codecs.DNS.Records;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.DNS
{
    /// <summary>
    /// Defines the <see cref="DnsQueryEncoder" />
    /// </summary>
    internal sealed class DnsQueryEncoder
    {
        #region 字段

        /// <summary>
        /// Defines the recordEncoder
        /// </summary>
        private readonly IDnsRecordEncoder recordEncoder;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsQueryEncoder"/> class.
        /// </summary>
        public DnsQueryEncoder() : this(new DefaultDnsRecordEncoder())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsQueryEncoder"/> class.
        /// </summary>
        /// <param name="recordEncoder">The recordEncoder<see cref="IDnsRecordEncoder"/></param>
        public DnsQueryEncoder(IDnsRecordEncoder recordEncoder)
        {
            this.recordEncoder = recordEncoder ?? throw new ArgumentNullException(nameof(recordEncoder));
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Encode
        /// </summary>
        /// <param name="query">The query<see cref="IDnsQuery"/></param>
        /// <param name="buffer">The buffer<see cref="IByteBuffer"/></param>
        public void Encode(IDnsQuery query, IByteBuffer buffer)
        {
            EncodeHeader(query, buffer);
            EncodeQuestions(query, buffer);
            EncodeRecords(query, DnsSection.ADDITIONAL, buffer);
        }

        /// <summary>
        /// The EncodeHeader
        /// </summary>
        /// <param name="query">The query<see cref="IDnsQuery"/></param>
        /// <param name="buffer">The buffer<see cref="IByteBuffer"/></param>
        private void EncodeHeader(IDnsQuery query, IByteBuffer buffer)
        {
            buffer.WriteShort(query.Id);
            int flags = 0;
            flags |= (query.OpCode.ByteValue & 0xFF) << 14;
            if (query.IsRecursionDesired)
                flags |= 1 << 8;

            buffer.WriteShort(flags);
            buffer.WriteShort(query.Count(DnsSection.QUESTION));
            buffer.WriteShort(0);
            buffer.WriteShort(0);
            buffer.WriteShort(query.Count(DnsSection.ADDITIONAL));
        }

        /// <summary>
        /// The EncodeQuestions
        /// </summary>
        /// <param name="query">The query<see cref="IDnsQuery"/></param>
        /// <param name="buffer">The buffer<see cref="IByteBuffer"/></param>
        private void EncodeQuestions(IDnsQuery query, IByteBuffer buffer)
        {
            int count = query.Count(DnsSection.QUESTION);
            for (int i = 0; i < count; i++)
            {
                recordEncoder.EncodeQuestion(query.GetRecord<IDnsQuestion>(DnsSection.QUESTION, i), buffer);
            }
        }

        /// <summary>
        /// The EncodeRecords
        /// </summary>
        /// <param name="query">The query<see cref="IDnsQuery"/></param>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        /// <param name="buffer">The buffer<see cref="IByteBuffer"/></param>
        private void EncodeRecords(IDnsQuery query, DnsSection section, IByteBuffer buffer)
        {
            int count = query.Count(section);
            for (int i = 0; i < count; i++)
            {
                recordEncoder.EncodeRecord(query.GetRecord<IDnsRecord>(section, i), buffer);
            }
        }

        #endregion 方法
    }
}