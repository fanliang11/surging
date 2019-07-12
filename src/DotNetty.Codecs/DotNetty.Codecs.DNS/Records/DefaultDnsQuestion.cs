using System.Reflection;
using System.Text;

namespace DotNetty.Codecs.DNS.Records
{
    /// <summary>
    /// Defines the <see cref="DefaultDnsQuestion" />
    /// </summary>
    public class DefaultDnsQuestion : AbstractDnsRecord, IDnsQuestion
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDnsQuestion"/> class.
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="type">The type<see cref="DnsRecordType"/></param>
        public DefaultDnsQuestion(string name, DnsRecordType type) : base(name, type, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDnsQuestion"/> class.
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="type">The type<see cref="DnsRecordType"/></param>
        /// <param name="dnsClass">The dnsClass<see cref="DnsRecordClass"/></param>
        public DefaultDnsQuestion(string name, DnsRecordType type, DnsRecordClass dnsClass) :
            base(name, type, 0, dnsClass)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDnsQuestion"/> class.
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="type">The type<see cref="DnsRecordType"/></param>
        /// <param name="timeToLive">The timeToLive<see cref="long"/></param>
        /// <param name="dnsClass">The dnsClass<see cref="DnsRecordClass"/></param>
        public DefaultDnsQuestion(string name,
            DnsRecordType type, long timeToLive,
            DnsRecordClass dnsClass = DnsRecordClass.IN)
            : base(name, type, timeToLive, dnsClass)
        {
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            var builder = new StringBuilder(64);

            builder.Append(GetType().GetTypeInfo().Name)
                .Append('(')
                .Append(Name)
                .Append(' ')
                .AppendRecordClass(DnsClass)
                .Append(' ')
                .Append(Type.Name)
                .Append(')');

            return builder.ToString();
        }

        #endregion 方法
    }
}