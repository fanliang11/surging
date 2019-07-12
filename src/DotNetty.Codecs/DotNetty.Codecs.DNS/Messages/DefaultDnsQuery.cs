namespace DotNetty.Codecs.DNS.Messages
{
    /// <summary>
    /// Defines the <see cref="DefaultDnsQuery" />
    /// </summary>
    public class DefaultDnsQuery : AbstractDnsMessage, IDnsQuery
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDnsQuery"/> class.
        /// </summary>
        /// <param name="id">The id<see cref="int"/></param>
        public DefaultDnsQuery(int id) : base(id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDnsQuery"/> class.
        /// </summary>
        /// <param name="id">The id<see cref="int"/></param>
        /// <param name="opCode">The opCode<see cref="DnsOpCode"/></param>
        public DefaultDnsQuery(int id, DnsOpCode opCode) : base(id, opCode)
        {
        }

        #endregion 构造函数
    }
}