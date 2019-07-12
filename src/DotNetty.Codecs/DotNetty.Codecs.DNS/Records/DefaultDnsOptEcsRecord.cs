using System;
using System.Net;
using System.Text;

namespace DotNetty.Codecs.DNS.Records
{
    /// <summary>
    /// Defines the <see cref="DefaultDnsOptEcsRecord" />
    /// </summary>
    public class DefaultDnsOptEcsRecord : AbstractDnsOptPseudoRrRecord, IDnsOptEcsRecord
    {
        #region 字段

        /// <summary>
        /// Defines the address
        /// </summary>
        private readonly byte[] address;

        /// <summary>
        /// Defines the srcPrefixLength
        /// </summary>
        private readonly int srcPrefixLength;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDnsOptEcsRecord"/> class.
        /// </summary>
        /// <param name="maxPayloadSize">The maxPayloadSize<see cref="int"/></param>
        /// <param name="extendedRcode">The extendedRcode<see cref="int"/></param>
        /// <param name="version">The version<see cref="int"/></param>
        /// <param name="srcPrefixLength">The srcPrefixLength<see cref="int"/></param>
        /// <param name="address">The address<see cref="byte[]"/></param>
        public DefaultDnsOptEcsRecord(int maxPayloadSize, int extendedRcode, int version,
            int srcPrefixLength, byte[] address) : base(maxPayloadSize, extendedRcode, version)
        {
            SourcePrefixLength = srcPrefixLength;
            address = VerifyAddress(address);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDnsOptEcsRecord"/> class.
        /// </summary>
        /// <param name="maxPayloadSize">The maxPayloadSize<see cref="int"/></param>
        /// <param name="srcPrefixLength">The srcPrefixLength<see cref="int"/></param>
        /// <param name="address">The address<see cref="byte[]"/></param>
        public DefaultDnsOptEcsRecord(int maxPayloadSize, int srcPrefixLength, byte[] address)
            : this(maxPayloadSize, 0, 0, srcPrefixLength, address)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDnsOptEcsRecord"/> class.
        /// </summary>
        /// <param name="maxPayloadSize">The maxPayloadSize<see cref="int"/></param>
        /// <param name="address">The address<see cref="IPAddress"/></param>
        public DefaultDnsOptEcsRecord(int maxPayloadSize, IPAddress address)
            : this(maxPayloadSize, 0, 0, 0, address.GetAddressBytes())
        {
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the Address
        /// </summary>
        public byte[] Address => (byte[])address.Clone();

        /// <summary>
        /// Gets the ScopePrefixLength
        /// </summary>
        public int ScopePrefixLength => 0;

        /// <summary>
        /// Gets the SourcePrefixLength
        /// </summary>
        public int SourcePrefixLength { get; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            StringBuilder builder = GetBuilder();
            builder.Length = builder.Length - 1;
            return builder.Append(" address:")
                .Append(string.Join(".", address, 0, address.Length))
                .Append(" sourcePrefixLength:")
                .Append(SourcePrefixLength)
                .Append(" scopePrefixLength:")
                .Append(ScopePrefixLength)
                .Append(')').ToString();
        }

        /// <summary>
        /// The VerifyAddress
        /// </summary>
        /// <param name="bytes">The bytes<see cref="byte[]"/></param>
        /// <returns>The <see cref="byte[]"/></returns>
        private static byte[] VerifyAddress(byte[] bytes)
        {
            if (bytes.Length == 4 || bytes.Length == 16)
                return bytes;

            throw new ArgumentException("bytes.length must either 4 or 16");
        }

        #endregion 方法
    }
}