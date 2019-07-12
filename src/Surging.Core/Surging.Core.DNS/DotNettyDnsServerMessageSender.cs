using ARSoft.Tools.Net.Dns;
using DotNetty.Buffers;
using DotNetty.Codecs.DNS;
using DotNetty.Codecs.DNS.Messages;
using DotNetty.Codecs.DNS.Records;
using DotNetty.Transport.Channels;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Transport.Codec;
using Surging.Core.DNS.Extensions;
using Surging.Core.DNS.Utilities;
using Surging.Core.DotNetty;
using System.Net;
using System.Threading.Tasks;

namespace Surging.Core.DNS
{
    /// <summary>
    /// Defines the <see cref="DotNettyDnsServerMessageSender" />
    /// </summary>
    internal class DotNettyDnsServerMessageSender : DotNettyMessageSender, IMessageSender
    {
        #region 字段

        /// <summary>
        /// Defines the _context
        /// </summary>
        private readonly IChannelHandlerContext _context;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNettyDnsServerMessageSender"/> class.
        /// </summary>
        /// <param name="transportMessageEncoder">The transportMessageEncoder<see cref="ITransportMessageEncoder"/></param>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        public DotNettyDnsServerMessageSender(ITransportMessageEncoder transportMessageEncoder, IChannelHandlerContext context) : base(transportMessageEncoder)
        {
            _context = context;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The GetDnsMessage
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="recordType">The recordType<see cref="DnsRecordType"/></param>
        /// <returns>The <see cref="Task{DnsMessage}"/></returns>
        public async Task<DnsMessage> GetDnsMessage(string name, DnsRecordType recordType)
        {
            return await DnsClientProvider.Instance().Resolve(name, recordType);
        }

        /// <summary>
        /// The SendAndFlushAsync
        /// </summary>
        /// <param name="message">The message<see cref="TransportMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task SendAndFlushAsync(TransportMessage message)
        {
            var response = await WriteResponse(message);
            await _context.WriteAndFlushAsync(response);
        }

        /// <summary>
        /// The SendAsync
        /// </summary>
        /// <param name="message">The message<see cref="TransportMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task SendAsync(TransportMessage message)
        {
            var response = await WriteResponse(message);
            await _context.WriteAsync(response);
        }

        /// <summary>
        /// The GetDnsQuestion
        /// </summary>
        /// <param name="message">The message<see cref="TransportMessage"/></param>
        /// <returns>The <see cref="IDnsQuestion"/></returns>
        private IDnsQuestion GetDnsQuestion(TransportMessage message)
        {
            if (message.Content != null && !message.IsDnsResultMessage())
                return null;

            var transportMessage = message.GetContent<DnsTransportMessage>();
            return transportMessage.DnsQuestion;
        }

        /// <summary>
        /// The GetDnsResponse
        /// </summary>
        /// <param name="message">The message<see cref="TransportMessage"/></param>
        /// <returns>The <see cref="IDnsResponse"/></returns>
        private IDnsResponse GetDnsResponse(TransportMessage message)
        {
            if (message.Content != null && !message.IsDnsResultMessage())
                return null;

            var transportMessage = message.GetContent<DnsTransportMessage>();
            return transportMessage.DnsResponse;
        }

        /// <summary>
        /// The GetIpAddr
        /// </summary>
        /// <param name="message">The message<see cref="TransportMessage"/></param>
        /// <returns>The <see cref="IPAddress"/></returns>
        private IPAddress GetIpAddr(TransportMessage message)
        {
            if (message.Content != null && !message.IsDnsResultMessage())
                return null;

            var transportMessage = message.GetContent<DnsTransportMessage>();
            return transportMessage.Address;
        }

        /// <summary>
        /// The WriteResponse
        /// </summary>
        /// <param name="message">The message<see cref="TransportMessage"/></param>
        /// <returns>The <see cref="Task{IDnsResponse}"/></returns>
        private async Task<IDnsResponse> WriteResponse(TransportMessage message)
        {
            var response = GetDnsResponse(message);
            var dnsQuestion = GetDnsQuestion(message);
            var ipAddr = GetIpAddr(message);
            if (response != null && dnsQuestion != null)
            {
                if (ipAddr != null)
                {
                    var buf = Unpooled.WrappedBuffer(ipAddr.GetAddressBytes());
                    response.AddRecord(DnsSection.ANSWER, new DefaultDnsRawRecord(dnsQuestion.Name, DnsRecordType.A, 100, buf));
                }
                else
                {
                    var dnsMessage = await GetDnsMessage(dnsQuestion.Name, dnsQuestion.Type);
                    if (dnsMessage != null)
                    {
                        foreach (DnsRecordBase dnsRecord in dnsMessage.AnswerRecords)
                        {
                            var aRecord = dnsRecord as ARecord;
                            var buf = Unpooled.Buffer();
                            if (dnsRecord.RecordType == RecordType.Ptr)
                            {
                                var ptrRecord = dnsRecord as PtrRecord;
                                response.AddRecord(DnsSection.ANSWER, new DefaultDnsPtrRecord(ptrRecord.Name.ToString(), (DnsRecordClass)(int)ptrRecord.RecordClass, ptrRecord.TimeToLive, ptrRecord.PointerDomainName.ToString()));
                            }
                            if (aRecord != null)
                            {
                                buf = Unpooled.WrappedBuffer(aRecord.Address.GetAddressBytes());
                                response.AddRecord(DnsSection.ANSWER, new DefaultDnsRawRecord(dnsQuestion.Name, DnsRecordType.From((int)dnsRecord.RecordType), (DnsRecordClass)(int)aRecord.RecordClass, dnsRecord.TimeToLive, buf));
                            }
                        }
                    }
                }
            }
            return response;
        }

        #endregion 方法
    }
}