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
    /// Defines the <see cref="DatagramDnsResponseDecoder" />
    /// </summary>
    public class DatagramDnsResponseDecoder : MessageToMessageDecoder<DatagramPacket>
    {
        #region 字段

        /// <summary>
        /// Defines the recordDecoder
        /// </summary>
        private readonly IDnsRecordDecoder recordDecoder;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DatagramDnsResponseDecoder"/> class.
        /// </summary>
        public DatagramDnsResponseDecoder() : this(new DefaultDnsRecordDecoder())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatagramDnsResponseDecoder"/> class.
        /// </summary>
        /// <param name="recordDecoder">The recordDecoder<see cref="IDnsRecordDecoder"/></param>
        public DatagramDnsResponseDecoder(IDnsRecordDecoder recordDecoder)
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
            IDnsResponse response = NewResponse(message, buffer);
            bool success = false;

            try
            {
                int questionCount = buffer.ReadUnsignedShort();
                int answerCount = buffer.ReadUnsignedShort();
                int authorityRecordCount = buffer.ReadUnsignedShort();
                int additionalRecordCount = buffer.ReadUnsignedShort();

                DecodeQuestions(response, buffer, questionCount);
                DecodeRecords(response, DnsSection.ANSWER, buffer, answerCount);
                DecodeRecords(response, DnsSection.AUTHORITY, buffer, authorityRecordCount);
                DecodeRecords(response, DnsSection.ADDITIONAL, buffer, additionalRecordCount);

                output.Add(response);
                success = true;
            }
            finally
            {
                if (!success)
                    response.Release();
            }
        }

        /// <summary>
        /// The NewResponse
        /// </summary>
        /// <param name="packet">The packet<see cref="DatagramPacket"/></param>
        /// <param name="buffer">The buffer<see cref="IByteBuffer"/></param>
        /// <returns>The <see cref="IDnsResponse"/></returns>
        private static IDnsResponse NewResponse(DatagramPacket packet, IByteBuffer buffer)
        {
            int id = buffer.ReadUnsignedShort();
            int flags = buffer.ReadUnsignedShort();
            if (flags >> 15 == 0) throw new CorruptedFrameException("not a response");

            IDnsResponse response = new DatagramDnsResponse(
                packet.Sender,
                packet.Recipient,
                id,
                DnsOpCode.From((byte)(flags >> 1 & 0xf)),
                DnsResponseCode.From((byte)(flags & 0xf)));
            return response;
        }

        /// <summary>
        /// The DecodeQuestions
        /// </summary>
        /// <param name="response">The response<see cref="IDnsResponse"/></param>
        /// <param name="buffer">The buffer<see cref="IByteBuffer"/></param>
        /// <param name="questionCount">The questionCount<see cref="int"/></param>
        private void DecodeQuestions(IDnsResponse response, IByteBuffer buffer, int questionCount)
        {
            for (int i = questionCount - 1; i > 0; i--)
            {
                response.AddRecord(DnsSection.QUESTION, recordDecoder.DecodeQuestion(buffer));
            }
        }

        /// <summary>
        /// The DecodeRecords
        /// </summary>
        /// <param name="response">The response<see cref="IDnsResponse"/></param>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        /// <param name="buffer">The buffer<see cref="IByteBuffer"/></param>
        /// <param name="count">The count<see cref="int"/></param>
        private void DecodeRecords(IDnsResponse response, DnsSection section, IByteBuffer buffer, int count)
        {
            for (int i = count - 1; i > 0; i--)
            {
                IDnsRecord r = recordDecoder.DecodeRecord(buffer);
                if (r == null)
                    break;

                response.AddRecord(section, r);
            }
        }

        #endregion 方法
    }
}