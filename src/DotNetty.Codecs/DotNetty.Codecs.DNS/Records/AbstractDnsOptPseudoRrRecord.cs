using System.Reflection;
using System.Text;

namespace DotNetty.Codecs.DNS.Records
{
    /// <summary>
    /// Defines the <see cref="AbstractDnsOptPseudoRrRecord" />
    /// </summary>
    public abstract class AbstractDnsOptPseudoRrRecord : AbstractDnsRecord, IDnsOptPseudoRecord
    {
        #region 常量

        /// <summary>
        /// Defines the EMPTY_STRING
        /// </summary>
        private const string EMPTY_STRING = "";

        #endregion 常量

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractDnsOptPseudoRrRecord"/> class.
        /// </summary>
        /// <param name="maxPayloadSize">The maxPayloadSize<see cref="int"/></param>
        protected AbstractDnsOptPseudoRrRecord(int maxPayloadSize)
            : base(EMPTY_STRING, DnsRecordType.OPT, 0, (DnsRecordClass)maxPayloadSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractDnsOptPseudoRrRecord"/> class.
        /// </summary>
        /// <param name="maxPayloadSize">The maxPayloadSize<see cref="int"/></param>
        /// <param name="extendedRcode">The extendedRcode<see cref="int"/></param>
        /// <param name="version">The version<see cref="int"/></param>
        protected AbstractDnsOptPseudoRrRecord(int maxPayloadSize, int extendedRcode, int version)
            : base(EMPTY_STRING, DnsRecordType.OPT, PackIntoLong(extendedRcode, version), (DnsRecordClass)maxPayloadSize)
        {
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the ExtendedRcode
        /// </summary>
        public int ExtendedRcode => (short)(((int)TimeToLive >> 16) & 0xff);

        /// <summary>
        /// Gets the Flags
        /// </summary>
        public int Flags => (short)((short)TimeToLive & 0xff);

        /// <summary>
        /// Gets the Version
        /// </summary>
        public int Version => (short)(((int)TimeToLive >> 16) & 0xff);

        #endregion 属性

        #region 方法

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            return GetBuilder().ToString();
        }

        /// <summary>
        /// The GetBuilder
        /// </summary>
        /// <returns>The <see cref="StringBuilder"/></returns>
        protected StringBuilder GetBuilder()
        {
            return new StringBuilder(64)
                .Append(GetType().GetTypeInfo().Name)
                .Append('(')
                .Append("OPT flags:")
                .Append(Flags)
                .Append(" version:")
                .Append(Version)
                .Append(" extendedRecode:")
                .Append(ExtendedRcode)
                .Append(" udp:")
                .Append(DnsClass)
                .Append(')');
        }

        /// <summary>
        /// The PackIntoLong
        /// </summary>
        /// <param name="val">The val<see cref="int"/></param>
        /// <param name="val2">The val2<see cref="int"/></param>
        /// <returns>The <see cref="long"/></returns>
        private static long PackIntoLong(int val, int val2)
        {
            return ((val & 0xff) << 24 | (val2 & 0xff) << 16 | (0 & 0xff) << 8 | 0 & 0xff) & 0xFFFFFFFFL;
        }

        #endregion 方法
    }
}