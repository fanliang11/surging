using System;

namespace DotNetty.Codecs.DNS.Messages
{
    /// <summary>
    /// Defines the <see cref="DnsResponseCode" />
    /// </summary>
    public class DnsResponseCode
    {
        #region 字段

        /// <summary>
        /// Defines the BADALG
        /// </summary>
        public static DnsResponseCode BADALG = new DnsResponseCode(21, "BADALG");

        /// <summary>
        /// Defines the BADKEY
        /// </summary>
        public static DnsResponseCode BADKEY = new DnsResponseCode(17, "BADKEY");

        /// <summary>
        /// Defines the BADMODE
        /// </summary>
        public static DnsResponseCode BADMODE = new DnsResponseCode(19, "BADMODE");

        /// <summary>
        /// Defines the BADNAME
        /// </summary>
        public static DnsResponseCode BADNAME = new DnsResponseCode(20, "BADNAME");

        /// <summary>
        /// Defines the BADTIME
        /// </summary>
        public static DnsResponseCode BADTIME = new DnsResponseCode(18, "BADTIME");

        /// <summary>
        /// Defines the BADVERS_OR_BADSIG
        /// </summary>
        public static DnsResponseCode BADVERS_OR_BADSIG = new DnsResponseCode(16, "BADVERS_OR_BADSIG");

        /// <summary>
        /// Defines the FORMERR
        /// </summary>
        public static DnsResponseCode FORMERR = new DnsResponseCode(1, "FormErr");

        /// <summary>
        /// Defines the NOERROR
        /// </summary>
        public static DnsResponseCode NOERROR = new DnsResponseCode(0, "NoError");

        /// <summary>
        /// Defines the NOTAUTH
        /// </summary>
        public static DnsResponseCode NOTAUTH = new DnsResponseCode(9, "NotAuth");

        /// <summary>
        /// Defines the NOTIMP
        /// </summary>
        public static DnsResponseCode NOTIMP = new DnsResponseCode(4, "NotImp");

        /// <summary>
        /// Defines the NOTZONE
        /// </summary>
        public static DnsResponseCode NOTZONE = new DnsResponseCode(10, "NotZone");

        /// <summary>
        /// Defines the NXDOMAIN
        /// </summary>
        public static DnsResponseCode NXDOMAIN = new DnsResponseCode(3, "NXDomain");

        /// <summary>
        /// Defines the NXRRSET
        /// </summary>
        public static DnsResponseCode NXRRSET = new DnsResponseCode(8, "NXRRSet");

        /// <summary>
        /// Defines the REFUSED
        /// </summary>
        public static DnsResponseCode REFUSED = new DnsResponseCode(5, "Refused");

        /// <summary>
        /// Defines the SERVFAIL
        /// </summary>
        public static DnsResponseCode SERVFAIL = new DnsResponseCode(2, "ServFail");

        /// <summary>
        /// Defines the YXDOMAIN
        /// </summary>
        public static DnsResponseCode YXDOMAIN = new DnsResponseCode(6, "YXDomain");

        /// <summary>
        /// Defines the YXRRSET
        /// </summary>
        public static DnsResponseCode YXRRSET = new DnsResponseCode(7, "YXRRSet");

        /// <summary>
        /// Defines the text
        /// </summary>
        private string text;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsResponseCode"/> class.
        /// </summary>
        /// <param name="code">The code<see cref="int"/></param>
        /// <param name="name">The name<see cref="string"/></param>
        public DnsResponseCode(int code, string name)
        {
            if (code < 0 || code > 65535)
                throw new ArgumentException($"code: {code} (expected: 0 ~ 65535)");

            IntValue = code;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="DnsResponseCode"/> class from being created.
        /// </summary>
        /// <param name="code">The code<see cref="int"/></param>
        private DnsResponseCode(int code) : this(code, "UNKNOWN")
        {
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the IntValue
        /// </summary>
        public int IntValue { get; }

        /// <summary>
        /// Gets the Name
        /// </summary>
        public string Name { get; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The From
        /// </summary>
        /// <param name="responseCode">The responseCode<see cref="int"/></param>
        /// <returns>The <see cref="DnsResponseCode"/></returns>
        public static DnsResponseCode From(int responseCode)
        {
            switch (responseCode)
            {
                case 0:
                    return NOERROR;

                case 1:
                    return FORMERR;

                case 2:
                    return SERVFAIL;

                case 3:
                    return NXDOMAIN;

                case 4:
                    return NOTIMP;

                case 5:
                    return REFUSED;

                case 6:
                    return YXDOMAIN;

                case 7:
                    return YXRRSET;

                case 8:
                    return NXRRSET;

                case 9:
                    return NOTAUTH;

                case 10:
                    return NOTZONE;

                case 16:
                    return BADVERS_OR_BADSIG;

                case 17:
                    return BADKEY;

                case 18:
                    return BADTIME;

                case 19:
                    return BADMODE;

                case 20:
                    return BADNAME;

                case 21:
                    return BADALG;

                default:
                    return new DnsResponseCode(responseCode);
            }
        }

        /// <summary>
        /// The Equals
        /// </summary>
        /// <param name="obj">The obj<see cref="object"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is DnsResponseCode))
                return false;

            return IntValue == ((DnsResponseCode)obj).IntValue;
        }

        /// <summary>
        /// The GetHashCode
        /// </summary>
        /// <returns>The <see cref="int"/></returns>
        public override int GetHashCode()
        {
            return IntValue;
        }

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            string text = this.text;
            if (text == null)
                this.text = text = $"{Name} ({IntValue})";
            return text;
        }

        #endregion 方法
    }
}