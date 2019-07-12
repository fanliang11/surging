using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.DNS.Records
{
    /// <summary>
    /// Represents a DNS record type.
    /// </summary>
    public class DnsRecordType
    {
        #region 字段

        /// <summary>
        /// Defines the A
        /// </summary>
        public static readonly DnsRecordType A = new DnsRecordType(0x0001, "A");

        /// <summary>
        /// Defines the AAAA
        /// </summary>
        public static readonly DnsRecordType AAAA = new DnsRecordType(0x001c, "AAAA");

        /// <summary>
        /// Defines the AFSDB
        /// </summary>
        public static readonly DnsRecordType AFSDB = new DnsRecordType(0x0012, "AFSDB");

        /// <summary>
        /// Defines the ANY
        /// </summary>
        public static readonly DnsRecordType ANY = new DnsRecordType(0x00ff, "ANY");

        /// <summary>
        /// Defines the APL
        /// </summary>
        public static readonly DnsRecordType APL = new DnsRecordType(0x002a, "APL");

        /// <summary>
        /// Defines the AXFR
        /// </summary>
        public static readonly DnsRecordType AXFR = new DnsRecordType(0x00fc, "AXFR");

        /// <summary>
        /// Defines the CAA
        /// </summary>
        public static readonly DnsRecordType CAA = new DnsRecordType(0x0101, "CAA");

        /// <summary>
        /// Defines the CERT
        /// </summary>
        public static readonly DnsRecordType CERT = new DnsRecordType(0x0025, "CERT");

        /// <summary>
        /// Defines the CNAME
        /// </summary>
        public static readonly DnsRecordType CNAME = new DnsRecordType(0x0005, "CNAME");

        /// <summary>
        /// Defines the DHCID
        /// </summary>
        public static readonly DnsRecordType DHCID = new DnsRecordType(0x0031, "DHCID");

        /// <summary>
        /// Defines the DLV
        /// </summary>
        public static readonly DnsRecordType DLV = new DnsRecordType(0x8001, "DLV");

        /// <summary>
        /// Defines the DNAME
        /// </summary>
        public static readonly DnsRecordType DNAME = new DnsRecordType(0x0027, "DNAME");

        /// <summary>
        /// Defines the DNSKEY
        /// </summary>
        public static readonly DnsRecordType DNSKEY = new DnsRecordType(0x0030, "DNSKEY");

        /// <summary>
        /// Defines the DS
        /// </summary>
        public static readonly DnsRecordType DS = new DnsRecordType(0x002b, "DS");

        /// <summary>
        /// Defines the HIP
        /// </summary>
        public static readonly DnsRecordType HIP = new DnsRecordType(0x0037, "HIP");

        /// <summary>
        /// Defines the IPSECKEY
        /// </summary>
        public static readonly DnsRecordType IPSECKEY = new DnsRecordType(0x002d, "IPSECKEY");

        /// <summary>
        /// Defines the IXFR
        /// </summary>
        public static readonly DnsRecordType IXFR = new DnsRecordType(0x00fb, "IXFR");

        /// <summary>
        /// Defines the KEY
        /// </summary>
        public static readonly DnsRecordType KEY = new DnsRecordType(0x0019, "KEY");

        /// <summary>
        /// Defines the KX
        /// </summary>
        public static readonly DnsRecordType KX = new DnsRecordType(0x0024, "KX");

        /// <summary>
        /// Defines the LOC
        /// </summary>
        public static readonly DnsRecordType LOC = new DnsRecordType(0x001d, "LOC");

        /// <summary>
        /// Defines the MX
        /// </summary>
        public static readonly DnsRecordType MX = new DnsRecordType(0x000f, "MX");

        /// <summary>
        /// Defines the NAPTR
        /// </summary>
        public static readonly DnsRecordType NAPTR = new DnsRecordType(0x0023, "NAPTR");

        /// <summary>
        /// Defines the NS
        /// </summary>
        public static readonly DnsRecordType NS = new DnsRecordType(0x0002, "NS");

        /// <summary>
        /// Defines the NSEC
        /// </summary>
        public static readonly DnsRecordType NSEC = new DnsRecordType(0x002f, "NSEC");

        /// <summary>
        /// Defines the NSEC3
        /// </summary>
        public static readonly DnsRecordType NSEC3 = new DnsRecordType(0x0032, "NSEC3");

        /// <summary>
        /// Defines the NSEC3PARAM
        /// </summary>
        public static readonly DnsRecordType NSEC3PARAM = new DnsRecordType(0x0033, "NSEC3PARAM");

        /// <summary>
        /// Defines the OPT
        /// </summary>
        public static readonly DnsRecordType OPT = new DnsRecordType(0x0029, "OPT");

        /// <summary>
        /// Defines the PTR
        /// </summary>
        public static readonly DnsRecordType PTR = new DnsRecordType(0x000c, "PTR");

        /// <summary>
        /// Defines the RP
        /// </summary>
        public static readonly DnsRecordType RP = new DnsRecordType(0x0011, "RP");

        /// <summary>
        /// Defines the RRSIG
        /// </summary>
        public static readonly DnsRecordType RRSIG = new DnsRecordType(0x002e, "RRSIG");

        /// <summary>
        /// Defines the SIG
        /// </summary>
        public static readonly DnsRecordType SIG = new DnsRecordType(0x0018, "SIG");

        /// <summary>
        /// Defines the SOA
        /// </summary>
        public static readonly DnsRecordType SOA = new DnsRecordType(0x0006, "SOA");

        /// <summary>
        /// Defines the SPF
        /// </summary>
        public static readonly DnsRecordType SPF = new DnsRecordType(0x0063, "SPF");

        /// <summary>
        /// Defines the SRV
        /// </summary>
        public static readonly DnsRecordType SRV = new DnsRecordType(0x0021, "SRV");

        /// <summary>
        /// Defines the SSHFP
        /// </summary>
        public static readonly DnsRecordType SSHFP = new DnsRecordType(0x002c, "SSHFP");

        /// <summary>
        /// Defines the TA
        /// </summary>
        public static readonly DnsRecordType TA = new DnsRecordType(0x8000, "TA");

        /// <summary>
        /// Defines the TKEY
        /// </summary>
        public static readonly DnsRecordType TKEY = new DnsRecordType(0x00f9, "TKEY");

        /// <summary>
        /// Defines the TLSA
        /// </summary>
        public static readonly DnsRecordType TLSA = new DnsRecordType(0x0034, "TLSA");

        /// <summary>
        /// Defines the TSIG
        /// </summary>
        public static readonly DnsRecordType TSIG = new DnsRecordType(0x00fa, "TSIG");

        /// <summary>
        /// Defines the TXT
        /// </summary>
        public static readonly DnsRecordType TXT = new DnsRecordType(0x0010, "TXT");

        /// <summary>
        /// Defines the byName
        /// </summary>
        private static readonly Dictionary<string, DnsRecordType> byName = new Dictionary<string, DnsRecordType>();

        /// <summary>
        /// Defines the byType
        /// </summary>
        private static readonly Dictionary<int, DnsRecordType> byType = new Dictionary<int, DnsRecordType>();

        /// <summary>
        /// Defines the EXPECTED
        /// </summary>
        private static readonly string EXPECTED;

        /// <summary>
        /// Defines the text
        /// </summary>
        private string text = string.Empty;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsRecordType"/> class.
        /// </summary>
        /// <param name="intValue">The intValue<see cref="int"/></param>
        /// <param name="name">The name<see cref="string"/></param>
        public DnsRecordType(int intValue, string name)
        {
            if ((intValue & 0xffff) != intValue)
            {
                throw new ArgumentException("intValue: " + intValue + " (expected: 0 ~ 65535)");
            }
            IntValue = intValue;
            Name = name;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="DnsRecordType"/> class from being created.
        /// </summary>
        /// <param name="intValue">The intValue<see cref="int"/></param>
        private DnsRecordType(int intValue) : this(intValue, "UNKNOWN")
        {
        }

        /// <summary>
        /// Initializes static members of the <see cref="DnsRecordType"/> class.
        /// </summary>
        static DnsRecordType()
        {
            DnsRecordType[] all = {
                A, NS, CNAME, SOA, PTR, MX, TXT, RP, AFSDB, SIG, KEY, AAAA, LOC, SRV, NAPTR, KX, CERT, DNAME, OPT, APL,
                DS, SSHFP, IPSECKEY, RRSIG, NSEC, DNSKEY, DHCID, NSEC3, NSEC3PARAM, TLSA, HIP, SPF, TKEY, TSIG, IXFR,
                AXFR, ANY, CAA, TA, DLV
            };

            var expected = new StringBuilder(512);

            expected.Append(" (expected: ");

            foreach (var type in all)
            {
                byName.Add(type.Name, type);
                byType.Add(type.IntValue, type);

                expected.Append(type.Name)
                    .Append('(')
                    .Append(type.IntValue)
                    .Append("), ");
            }

            expected.Length = expected.Length - 2;
            expected.Append(')');
            EXPECTED = expected.ToString();
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
        /// <param name="intValue">The intValue<see cref="int"/></param>
        /// <returns>The <see cref="DnsRecordType"/></returns>
        public static DnsRecordType From(int intValue)
        {
            if (byType.ContainsKey(intValue))
                return byType[intValue];

            return new DnsRecordType(intValue);
        }

        /// <summary>
        /// The From
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <returns>The <see cref="DnsRecordType"/></returns>
        public static DnsRecordType From(string name)
        {
            if (byName.ContainsKey(name))
                return byName[name];

            throw new ArgumentException($"name: {name} {EXPECTED}");
        }

        /// <summary>
        /// The Equals
        /// </summary>
        /// <param name="obj">The obj<see cref="object"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public override bool Equals(object obj)
        {
            return obj is DnsRecordType && ((DnsRecordType)obj).IntValue == IntValue;
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
                this.text = text = Name + '(' + IntValue + ')';

            return text;
        }

        #endregion 方法
    }
}