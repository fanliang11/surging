using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;
using DotNetty.Codecs.DNS.Records;
using System.Net;
using System.Threading.Tasks;

namespace Surging.Core.DNS.Utilities
{
    /// <summary>
    /// Defines the <see cref="DnsClientProvider" />
    /// </summary>
    public class DnsClientProvider
    {
        #region 方法

        /// <summary>
        /// The Instance
        /// </summary>
        /// <returns>The <see cref="DnsClientProvider"/></returns>
        public static DnsClientProvider Instance()
        {
            return new DnsClientProvider();
        }

        /// <summary>
        /// The GetDnsClient
        /// </summary>
        /// <returns>The <see cref="DnsClient"/></returns>
        public DnsClient GetDnsClient()
        {
            var dnsOption = AppConfig.DnsOption;
            DnsClient dnsClient = new DnsClient(IPAddress.Parse(dnsOption.RootDnsAddress), dnsOption.QueryTimeout);
            return dnsClient;
        }

        /// <summary>
        /// The Resolve
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="recordType">The recordType<see cref="DnsRecordType"/></param>
        /// <param name="recordClass">The recordClass<see cref="DnsRecordClass"/></param>
        /// <returns>The <see cref="Task{DnsMessage}"/></returns>
        public async Task<DnsMessage> Resolve(string name, DnsRecordType recordType, DnsRecordClass recordClass = DnsRecordClass.IN)
        {
            var dnsMessage = await GetDnsClient().ResolveAsync(DomainName.Parse(name), (RecordType)recordType.IntValue, (RecordClass)(int)recordClass);
            return dnsMessage;
        }

        #endregion 方法
    }
}