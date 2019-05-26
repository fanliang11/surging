using DotNetty.Buffers;
using DotNetty.Codecs.DNS.Messages;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DotNetty.Codecs.DNS
{
    public  class DnsResponseDecoder<T> where T : EndPoint
    {
        private readonly IDnsRecordDecoder recordDecoder;

        public DnsResponseDecoder(IDnsRecordDecoder recordDecoder)
        {
            this.recordDecoder = recordDecoder ?? throw new ArgumentNullException(nameof(recordDecoder));
        }

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

        protected  virtual  IDnsResponse NewResponse(T sender, T recipient, int id,
                                                   DnsOpCode opCode, DnsResponseCode responseCode) =>  new DefaultDnsResponse(id, opCode, responseCode);
       

        private void DecodeQuestions(IDnsResponse response, IByteBuffer buf, int questionCount)
        {
            for (int i = questionCount; i > 0; i--)
            {
                response.AddRecord(DnsSection.QUESTION, recordDecoder.DecodeQuestion(buf));
            }
        }

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
    }
}
