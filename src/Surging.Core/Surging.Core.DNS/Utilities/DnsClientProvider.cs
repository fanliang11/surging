using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;
using DotNetty.Codecs.DNS.Records;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Surging.Core.DNS.Utilities
{
    public class DnsClientProvider
    {
        public  DnsMessage Resolve(string name, DnsRecordType recordType , DnsRecordClass recordClass = DnsRecordClass.IN)
        {
            var dnsMessage = GetDnsClient().Resolve(DomainName.Parse(name), (RecordType)recordType.IntValue,(RecordClass)(int)recordClass);
            return dnsMessage;
        }

        public  DnsClient GetDnsClient()
        {
            var dnsOption = AppConfig.DnsOption;
            DnsClient dnsClient = new DnsClient(IPAddress.Parse(dnsOption.RootDnsAddress), dnsOption.QueryTimeout);
            return dnsClient;
        }

        public static DnsClientProvider Instance()
        {
            return new DnsClientProvider();
        }
    }
}
