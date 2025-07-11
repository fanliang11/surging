using System.Net;
using System.Threading.Tasks;
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

namespace Surging.Core.DNS
{
    class DotNettyDnsServerMessageSender : DotNettyMessageSender, IMessageSender
    {
        private readonly IChannelHandlerContext _context;

        public DotNettyDnsServerMessageSender(ITransportMessageEncoder transportMessageEncoder, IChannelHandlerContext context) : base(transportMessageEncoder)
        {
            _context = context;
        }

        public async Task SendAndFlushAsync(TransportMessage message)
        {
            var response=await  WriteResponse(message);
            await _context.WriteAndFlushAsync(response);
        }

        public async Task SendAsync(TransportMessage message)
        {
            var response = await WriteResponse(message);
            await _context.WriteAsync(response);
        }

        private IDnsResponse GetDnsResponse(TransportMessage message)
        { 
            if (message.Content !=null && !message.IsDnsResultMessage())
                return null;

            var transportMessage = message.GetContent<DnsTransportMessage>(); 
            return transportMessage.DnsResponse;
        }

        private IDnsQuestion GetDnsQuestion(TransportMessage message)
        {
            if (message.Content != null && !message.IsDnsResultMessage())
                return null;

            var transportMessage = message.GetContent<DnsTransportMessage>();
            return transportMessage.DnsQuestion;
        }

        private IPAddress GetIpAddr(TransportMessage message)
        {
            if (message.Content != null && !message.IsDnsResultMessage())
                return null;

            var transportMessage = message.GetContent<DnsTransportMessage>();
            return transportMessage.Address;
        }

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
                    var dnsMessage =await GetDnsMessage(dnsQuestion.Name, dnsQuestion.Type);
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

        public async Task<DnsMessage> GetDnsMessage(string name, DnsRecordType recordType)
        {
           return await DnsClientProvider.Instance().Resolve(name, recordType);
        }
    }
}
