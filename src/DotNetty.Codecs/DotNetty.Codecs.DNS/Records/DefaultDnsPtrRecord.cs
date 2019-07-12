using System;
using System.Reflection;
using System.Text;

namespace DotNetty.Codecs.DNS.Records
{
    /// <summary>
    /// Defines the <see cref="DefaultDnsPtrRecord" />
    /// </summary>
    public class DefaultDnsPtrRecord : AbstractDnsRecord, IDnsPtrRecord
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDnsPtrRecord"/> class.
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="dnsClass">The dnsClass<see cref="DnsRecordClass"/></param>
        /// <param name="timeToLive">The timeToLive<see cref="long"/></param>
        /// <param name="hostname">The hostname<see cref="string"/></param>
        public DefaultDnsPtrRecord(string name, DnsRecordClass dnsClass, long timeToLive, string hostname)
            : base(name, DnsRecordType.PTR, timeToLive, dnsClass)
        {
            if (string.IsNullOrWhiteSpace(hostname))
                throw new ArgumentNullException(hostname);

            HostName = hostname;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the HostName
        /// </summary>
        public string HostName { get; }

        #endregion 属性

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
                .Append(string.IsNullOrWhiteSpace(Name) ? "<root>" : Name)
                .Append(' ')
                .Append(TimeToLive)
                .Append(' ')
                .AppendRecordClass(DnsClass)
                .Append(' ')
                .Append(Type.Name)
                .Append(' ')
                .Append(HostName);

            return builder.ToString();
        }

        #endregion 方法
    }
}