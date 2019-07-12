using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.DNS
{
    /// <summary>
    /// Defines the <see cref="DnsOpCode" />
    /// </summary>
    public class DnsOpCode
    {
        #region 字段

        /// <summary>
        /// Defines the IQUERY
        /// </summary>
        public static readonly DnsOpCode IQUERY = new DnsOpCode(0x01, "IQUERY");

        /// <summary>
        /// Defines the NOTIFY
        /// </summary>
        public static readonly DnsOpCode NOTIFY = new DnsOpCode(0x04, "NOTIFY");

        /// <summary>
        /// Defines the QUERY
        /// </summary>
        public static readonly DnsOpCode QUERY = new DnsOpCode(0x00, "QUERY");

        /// <summary>
        /// Defines the STATUS
        /// </summary>
        public static readonly DnsOpCode STATUS = new DnsOpCode(0x02, "STATUS");

        /// <summary>
        /// Defines the UPDATE
        /// </summary>
        public static readonly DnsOpCode UPDATE = new DnsOpCode(0x05, "UPDATE");

        /// <summary>
        /// Defines the text
        /// </summary>
        private string text;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsOpCode"/> class.
        /// </summary>
        /// <param name="byteValue">The byteValue<see cref="int"/></param>
        /// <param name="name">The name<see cref="string"/></param>
        public DnsOpCode(int byteValue, string name = "UNKNOWN")
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ByteValue = (byte)byteValue;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the ByteValue
        /// </summary>
        public byte ByteValue { get; }

        /// <summary>
        /// Gets the Name
        /// </summary>
        public string Name { get; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The From
        /// </summary>
        /// <param name="byteValue">The byteValue<see cref="int"/></param>
        /// <returns>The <see cref="DnsOpCode"/></returns>
        public static DnsOpCode From(int byteValue)
        {
            switch (byteValue)
            {
                case 0x00:
                    return QUERY;

                case 0x01:
                    return IQUERY;

                case 0x02:
                    return STATUS;

                case 0x04:
                    return NOTIFY;

                case 0x05:
                    return UPDATE;

                default:
                    return new DnsOpCode(byteValue);
            }
        }

        /// <summary>
        /// The Equals
        /// </summary>
        /// <param name="obj">The obj<see cref="object"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            if (!(obj is DnsOpCode))
                return false;

            return ByteValue == ((DnsOpCode)obj).ByteValue;
        }

        /// <summary>
        /// The GetHashCode
        /// </summary>
        /// <returns>The <see cref="int"/></returns>
        public override int GetHashCode()
        {
            return ByteValue;
        }

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            string text = this.text;
            if (string.IsNullOrWhiteSpace(text))
                this.text = text = $"{Name}({ByteValue & 0xFF})";

            return text;
        }

        #endregion 方法
    }
}