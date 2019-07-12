using DotNetty.Buffers;
using DotNetty.Codecs.DNS.Messages;
using DotNetty.Codecs.DNS.Records;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Net;

namespace DotNetty.Codecs.DNS
{
    /// <summary>
    /// Defines the <see cref="DatagramDnsResponseEncoder" />
    /// </summary>
    public class DatagramDnsResponseEncoder : MessageToMessageEncoder<IAddressedEnvelope<IDnsResponse>>
    {
        #region 字段

        /// <summary>
        /// Defines the recordEncoder
        /// </summary>
        private IDnsRecordEncoder recordEncoder;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DatagramDnsResponseEncoder"/> class.
        /// </summary>
        public DatagramDnsResponseEncoder() : this(new DefaultDnsRecordEncoder())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatagramDnsResponseEncoder"/> class.
        /// </summary>
        /// <param name="recordEncoder">The recordEncoder<see cref="IDnsRecordEncoder"/></param>
        public DatagramDnsResponseEncoder(IDnsRecordEncoder recordEncoder)
        {
            this.recordEncoder = recordEncoder ?? throw new ArgumentNullException(nameof(recordEncoder));
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The AllocateBuffer
        /// </summary>
        /// <param name="ctx">The ctx<see cref="IChannelHandlerContext"/></param>
        /// <param name="message">The message<see cref="IAddressedEnvelope{IDnsResponse}"/></param>
        /// <returns>The <see cref="IByteBuffer"/></returns>
        protected IByteBuffer AllocateBuffer(IChannelHandlerContext ctx,
            IAddressedEnvelope<IDnsResponse> message) => ctx.Allocator.Buffer(1024);

        /// <summary>
        /// The Encode
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="message">The message<see cref="IAddressedEnvelope{IDnsResponse}"/></param>
        /// <param name="output">The output<see cref="List{object}"/></param>
        protected override void Encode(IChannelHandlerContext context, IAddressedEnvelope<IDnsResponse> message, List<object> output)
        {
            EndPoint recipient = message.Recipient;
            IDnsResponse response = message.Content;
            if (response != null)
            {
                IByteBuffer buffer = AllocateBuffer(context, message);

                bool success = false;
                try
                {
                    EncodeHeader(response, buffer);
                    EncodeQuestions(response, buffer);
                    EncodeRecords(response, DnsSection.ANSWER, buffer);
                    EncodeRecords(response, DnsSection.AUTHORITY, buffer);
                    EncodeRecords(response, DnsSection.ADDITIONAL, buffer);
                    success = true;
                }
                finally
                {
                    if (!success)
                        buffer.Release();
                }

                output.Add(new DatagramPacket(buffer, null, recipient));
            }
        }

        /// <summary>
        /// The EncodeHeader
        /// </summary>
        /// <param name="response">The response<see cref="IDnsResponse"/></param>
        /// <param name="buffer">The buffer<see cref="IByteBuffer"/></param>
        private static void EncodeHeader(IDnsResponse response, IByteBuffer buffer)
        {
            buffer.WriteShort(response.Id);
            int flags = 32768;
            flags |= (response.OpCode.ByteValue & 0xFF) << 11;
            if (response.IsAuthoritativeAnswer)
                flags |= 1 << 10;

            if (response.IsTruncated)
                flags |= 1 << 9;

            if (response.IsRecursionDesired)
                flags |= 1 << 8;

            if (response.IsRecursionAvailable)
                flags |= 1 << 7;

            flags |= response.Z << 4;
            flags |= response.Code.IntValue;
            buffer.WriteShort(flags);
            buffer.WriteShort(response.Count(DnsSection.QUESTION));
            buffer.WriteShort(response.Count(DnsSection.ANSWER));
            buffer.WriteShort(response.Count(DnsSection.AUTHORITY));
            buffer.WriteShort(response.Count(DnsSection.ADDITIONAL));
        }

        /// <summary>
        /// The EncodeQuestions
        /// </summary>
        /// <param name="response">The response<see cref="IDnsResponse"/></param>
        /// <param name="buffer">The buffer<see cref="IByteBuffer"/></param>
        private void EncodeQuestions(IDnsResponse response, IByteBuffer buffer)
        {
            int count = response.Count(DnsSection.QUESTION);
            for (int i = 0; i < count; i++)
            {
                recordEncoder.EncodeQuestion(response.GetRecord<IDnsQuestion>(DnsSection.QUESTION, i), buffer);
            }
        }

        /// <summary>
        /// The EncodeRecords
        /// </summary>
        /// <param name="response">The response<see cref="IDnsResponse"/></param>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        /// <param name="buffer">The buffer<see cref="IByteBuffer"/></param>
        private void EncodeRecords(IDnsResponse response, DnsSection section, IByteBuffer buffer)
        {
            int count = response.Count(section);
            for (int i = 0; i < count; i++)
            {
                recordEncoder.EncodeRecord(response.GetRecord<IDnsRecord>(section, i), buffer);
            }
        }

        #endregion 方法
    }
}