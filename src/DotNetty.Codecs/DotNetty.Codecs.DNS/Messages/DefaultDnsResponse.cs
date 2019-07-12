using System;
using System.Text;

namespace DotNetty.Codecs.DNS.Messages
{
    /// <summary>
    /// Defines the <see cref="DefaultDnsResponse" />
    /// </summary>
    public class DefaultDnsResponse : AbstractDnsMessage, IDnsResponse
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDnsResponse"/> class.
        /// </summary>
        /// <param name="id">The id<see cref="int"/></param>
        public DefaultDnsResponse(int id)
            : this(id, DnsOpCode.QUERY, DnsResponseCode.NOERROR)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDnsResponse"/> class.
        /// </summary>
        /// <param name="id">The id<see cref="int"/></param>
        /// <param name="opCode">The opCode<see cref="DnsOpCode"/></param>
        public DefaultDnsResponse(int id, DnsOpCode opCode)
            : this(id, opCode, DnsResponseCode.NOERROR)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDnsResponse"/> class.
        /// </summary>
        /// <param name="id">The id<see cref="int"/></param>
        /// <param name="opCode">The opCode<see cref="DnsOpCode"/></param>
        /// <param name="code">The code<see cref="DnsResponseCode"/></param>
        public DefaultDnsResponse(int id, DnsOpCode opCode, DnsResponseCode code)
            : base(id, opCode)
        {
            Code = code ?? throw new ArgumentNullException(nameof(code));
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Code
        /// </summary>
        public DnsResponseCode Code { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsAuthoritativeAnswer
        /// </summary>
        public bool IsAuthoritativeAnswer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsRecursionAvailable
        /// </summary>
        public bool IsRecursionAvailable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsTruncated
        /// </summary>
        public bool IsTruncated { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            return new StringBuilder(128).AppendResponse(this).ToString();
        }

        #endregion 方法
    }
}