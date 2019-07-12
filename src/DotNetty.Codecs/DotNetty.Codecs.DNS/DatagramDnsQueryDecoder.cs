using DotNetty.Buffers;
using DotNetty.Codecs.DNS.Messages;
using DotNetty.Codecs.DNS.Records;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;

namespace DotNetty.Codecs.DNS
{
    /// <summary>
    /// Defines the <see cref="DatagramDnsQueryDecoder" />
    /// </summary>
    public class DatagramDnsQueryDecoder : MessageToMessageDecoder<DatagramPacket>
    {
        #region 字段

        /// <summary>
        /// Defines the recordDecoder
        /// </summary>
        private readonly IDnsRecordDecoder recordDecoder;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DatagramDnsQueryDecoder"/> class.
        /// </summary>
        public DatagramDnsQueryDecoder() : this(new DefaultDnsRecordDecoder())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatagramDnsQueryDecoder"/> class.
        /// </summary>
        /// <param name="recordDecoder">The recordDecoder<see cref="IDnsRecordDecoder"/></param>
        public DatagramDnsQueryDecoder(IDnsRecordDecoder recordDecoder)
        {
            this.recordDecoder = recordDecoder ?? throw new ArgumentNullException(nameof(recordDecoder));
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Decode
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="message">The message<see cref="DatagramPacket"/></param>
        /// <param name="output">The output<see cref="List{object}"/></param>
        protected override void Decode(IChannelHandlerContext context, DatagramPacket message, List<object> output)
        {
            IByteBuffer buffer = message.Content;
            IDnsQuery query = NewQuery(message, buffer);
            bool success = false;

            try
            {
                int questionCount = buffer.ReadUnsignedShort();
                int answerCount = buffer.ReadUnsignedShort();
                int authorityRecordCount = buffer.ReadUnsignedShort();
                int additionalRecordCount = buffer.ReadUnsignedShort();

                DecodeQuestions(query, buffer, questionCount);
                DecodeRecords(query, DnsSection.ANSWER, buffer, answerCount);
                DecodeRecords(query, DnsSection.AUTHORITY, buffer, authorityRecordCount);
                DecodeRecords(query, DnsSection.ADDITIONAL, buffer, additionalRecordCount);

                output.Add(query);
                success = true;
            }
            finally
            {
                if (!success)
                    query.Release();
            }
        }

        /// <summary>
        /// The NewQuery
        /// </summary>
        /// <param name="packet">The packet<see cref="DatagramPacket"/></param>
        /// <param name="buffer">The buffer<see cref="IByteBuffer"/></param>
        /// <returns>The <see cref="IDnsQuery"/></returns>
        private static IDnsQuery NewQuery(DatagramPacket packet, IByteBuffer buffer)
        {
            int id = buffer.ReadUnsignedShort();
            int flags = buffer.ReadUnsignedShort();
            if (flags >> 15 == 1)
                throw new CorruptedFrameException("not a query");

            IDnsQuery query = new DatagramDnsQuery(
                packet.Sender, packet.Recipient, id,
                DnsOpCode.From((byte)(flags >> 11 & 0xf)));

            query.IsRecursionDesired = (flags >> 8 & 1) == 1;
            query.Z = flags >> 4 & 0x7;
            return query;
        }

        /// <summary>
        /// The DecodeQuestions
        /// </summary>
        /// <param name="query">The query<see cref="IDnsQuery"/></param>
        /// <param name="buffer">The buffer<see cref="IByteBuffer"/></param>
        /// <param name="questionCount">The questionCount<see cref="int"/></param>
        private void DecodeQuestions(IDnsQuery query, IByteBuffer buffer, int questionCount)
        {
            for (int i = questionCount; i > 0; i--)
            {
                query.AddRecord(DnsSection.QUESTION, recordDecoder.DecodeQuestion(buffer));
            }
        }

        /// <summary>
        /// The DecodeRecords
        /// </summary>
        /// <param name="query">The query<see cref="IDnsQuery"/></param>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        /// <param name="buffer">The buffer<see cref="IByteBuffer"/></param>
        /// <param name="count">The count<see cref="int"/></param>
        private void DecodeRecords(IDnsQuery query, DnsSection section, IByteBuffer buffer, int count)
        {
            for (int i = count; i > 0; i--)
            {
                IDnsRecord r = recordDecoder.DecodeRecord(buffer);
                if (r == null)
                    break;

                query.AddRecord(section, r);
            }
        }

        #endregion 方法
    }
}