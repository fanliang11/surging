using System;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace DotNetty.Codecs.DNS.Records
{
    #region 枚举

    /// <summary>
    /// Defines the DnsRecordClass
    /// </summary>
    public enum DnsRecordClass : int
    {
        /// <summary>
        /// Defines the IN
        /// </summary>
        IN = 0x0001,

        /// <summary>
        /// Defines the CSNET
        /// </summary>
        CSNET = 0x0002,

        /// <summary>
        /// Defines the CHAOS
        /// </summary>
        CHAOS = 0x0003,

        /// <summary>
        /// Defines the HESIOD
        /// </summary>
        HESIOD = 0x0004,

        /// <summary>
        /// Defines the NONE
        /// </summary>
        NONE = 0x00fe,

        /// <summary>
        /// Defines the ANY
        /// </summary>
        ANY = 0x00ff
    }

    #endregion 枚举

    /// <summary>
    /// Defines the <see cref="AbstractDnsRecord" />
    /// </summary>
    public abstract class AbstractDnsRecord : IDnsRecord
    {
        #region 字段

        /// <summary>
        /// Defines the idn
        /// </summary>
        private readonly IdnMapping idn = new IdnMapping();

        /// <summary>
        /// Defines the hashCode
        /// </summary>
        private int hashCode;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractDnsRecord"/> class.
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="type">The type<see cref="DnsRecordType"/></param>
        /// <param name="timeToLive">The timeToLive<see cref="long"/></param>
        /// <param name="dnsClass">The dnsClass<see cref="DnsRecordClass"/></param>
        protected AbstractDnsRecord(string name, DnsRecordType type,
            long timeToLive, DnsRecordClass dnsClass = DnsRecordClass.IN)
        {
            if (TimeToLive < 0)
                throw new ArgumentException($"timeToLive: {timeToLive} (expected: >= 0)");

            TimeToLive = timeToLive;

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            Name = AppendTrailingDot(idn.GetAscii(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            DnsClass = dnsClass;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the DnsClass
        /// </summary>
        public DnsRecordClass DnsClass { get; }

        /// <summary>
        /// Gets the Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the TimeToLive
        /// </summary>
        public long TimeToLive { get; set; }

        /// <summary>
        /// Gets the Type
        /// </summary>
        public DnsRecordType Type { get; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Equals
        /// </summary>
        /// <param name="obj">The obj<see cref="object"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            if (!(obj is AbstractDnsRecord))
                return false;

            var that = (AbstractDnsRecord)obj;
            int hashCode = GetHashCode();
            if (hashCode != 0 && hashCode != that.GetHashCode())
                return false;

            return Type.IntValue == that.Type.IntValue &&
                DnsClass == that.DnsClass &&
                Name.Equals(that.Name);
        }

        /// <summary>
        /// The GetHashCode
        /// </summary>
        /// <returns>The <see cref="int"/></returns>
        public override int GetHashCode()
        {
            int hashCode = this.hashCode;
            if (hashCode != 0)
                return hashCode;

            return this.hashCode = Name.GetHashCode() * 31 +
                Type.IntValue * 31 + (int)DnsClass;
        }

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
                .Append(TimeToLive)
                .Append(' ')
                .AppendRecordClass(DnsClass)
                .Append(' ')
                .Append(Type.Name)
                .Append(')');

            return builder.ToString();
        }

        /// <summary>
        /// The AppendTrailingDot
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        private static string AppendTrailingDot(string name)
        {
            if (name.Length > 0 && !name.EndsWith("."))
                return name + ".";

            return name;
        }

        #endregion 方法
    }
}