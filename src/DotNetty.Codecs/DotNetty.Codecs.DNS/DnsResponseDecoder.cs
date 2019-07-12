using DotNetty.Buffers;
using DotNetty.Codecs.DNS.Messages;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DotNetty.Codecs.DNS
{
    /// <summary>
    /// Defines the <see cref="DnsResponseDecoder{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DnsResponseDecoder<T> where T : EndPoint
    {
        #region 字段

        /// <summary>
        /// Defines the recordDecoder
        /// </summary>
        private readonly IDnsRecordDecoder recordDecoder;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsResponseDecoder{T}"/> class.
        /// </summary>
        /// <param name="recordDecoder">The recordDecoder<see cref="IDnsRecordDecoder"/></param>
        public DnsResponseDecoder(IDnsRecordDecoder recordDecoder)
        {
            this.recordDecoder = recordDecoder ?? throw new ArgumentNullException(nameof(recordDecoder));
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Decode
        /// </summary>
        /// <param name="sender">The sender<see cref="T"/></param>
        /// <param name="recipient">The recipient<see cref="T"/></param>
        /// <param name="buffer">The buffer<see cref="IByteBuffer"/></param>
        /// <returns>The <see cref="IDnsResponse"/></returns>
        public IDnsResponse Decode(T sender, T recipient, IByteBuffer buffer)
        {
            int id = buffer.ReadUnsignedShort();
            int flags = buffer.ReadUnsignedShort();
            if (flags >> 15 == 0)
            {
                throw new CorruptedFrameException("not a response");
            }

            IDnsResponse response = NewResponse(
                  sender,
                  recipient,
                  id,
                 new DnsOpCode((byte)(flags >> 11 & 0xf)), DnsResponseCode.From((flags & 0xf)));

            response.IsRecursionDesired = (flags >> 8 & 1) == 1;
            response.IsAuthoritativeAnswer = (flags >> 10 & 1) == 1;
            response.IsTruncated = (flags >> 9 & 1) == 1;
            response.IsRecursionAvailable = (flags >> 7 & 1) == 1;
            response.Z = flags >> 4 & 0x7;

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
                success = true;
                return response;
            }
            finally
            {
                if (!success)
                {
                    response.Release();
                }
            }
        }

        /// <summary>
        /// The NewResponse
        /// </summary>
        /// <param name="sender">The sender<see cref="T"/></param>
        /// <param name="recipient">The recipient<see cref="T"/></param>
        /// <param name="id">The id<see cref="int"/></param>
        /// <param name="opCode">The opCode<see cref="DnsOpCode"/></param>
        /// <param name="responseCode">The responseCode<see cref="DnsResponseCode"/></param>
        /// <returns>The <see cref="IDnsResponse"/></returns>
        protected virtual IDnsResponse NewResponse(T sender, T recipient, int id,
                                                   DnsOpCode opCode, DnsResponseCode responseCode) => new DefaultDnsResponse(id, opCode, responseCode);

        /// <summary>
        /// The DecodeQuestions
        /// </summary>
        /// <param name="response">The response<see cref="IDnsResponse"/></param>
        /// <param name="buf">The buf<see cref="IByteBuffer"/></param>
        /// <param name="questionCount">The questionCount<see cref="int"/></param>
        private void DecodeQuestions(IDnsResponse response, IByteBuffer buf, int questionCount)
        {
            for (int i = questionCount; i > 0; i--)
            {
                response.AddRecord(DnsSection.QUESTION, recordDecoder.DecodeQuestion(buf));
            }
        }

        /// <summary>
        /// The DecodeRecords
        /// </summary>
        /// <param name="response">The response<see cref="IDnsResponse"/></param>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        /// <param name="buf">The buf<see cref="IByteBuffer"/></param>
        /// <param name="count">The count<see cref="int"/></param>
        private void DecodeRecords(IDnsResponse response, DnsSection section, IByteBuffer buf, int count)
        {
            for (int i = count; i > 0; i--)
            {
                var r = recordDecoder.DecodeRecord(buf);
                if (r == null)
                {
                    break;
                }

                response.AddRecord(section, r);
            }
        }

        #endregion 方法
    }
}