using DotNetty.Buffers;
using DotNetty.Common;
using System;
using System.Reflection;
using System.Text;

namespace DotNetty.Codecs.DNS.Records
{
    /// <summary>
    /// Defines the <see cref="DefaultDnsRawRecord" />
    /// </summary>
    public class DefaultDnsRawRecord : AbstractDnsRecord, IDnsRawRecord
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDnsRawRecord"/> class.
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="type">The type<see cref="DnsRecordType"/></param>
        /// <param name="dnsClass">The dnsClass<see cref="DnsRecordClass"/></param>
        /// <param name="timeToLive">The timeToLive<see cref="long"/></param>
        /// <param name="content">The content<see cref="IByteBuffer"/></param>
        public DefaultDnsRawRecord(string name, DnsRecordType type, DnsRecordClass dnsClass,
            long timeToLive, IByteBuffer content) : base(name, type, timeToLive, dnsClass)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDnsRawRecord"/> class.
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="type">The type<see cref="DnsRecordType"/></param>
        /// <param name="timeToLive">The timeToLive<see cref="long"/></param>
        /// <param name="content">The content<see cref="IByteBuffer"/></param>
        public DefaultDnsRawRecord(string name, DnsRecordType type, long timeToLive,
            IByteBuffer content) : this(name, type, DnsRecordClass.IN, timeToLive, content)
        {
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the Content
        /// </summary>
        public IByteBuffer Content { get; }

        /// <summary>
        /// Gets the ReferenceCount
        /// </summary>
        public int ReferenceCount { get; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Copy
        /// </summary>
        /// <returns>The <see cref="IByteBufferHolder"/></returns>
        public IByteBufferHolder Copy()
        {
            return Replace(Content.Copy());
        }

        /// <summary>
        /// The Duplicate
        /// </summary>
        /// <returns>The <see cref="IByteBufferHolder"/></returns>
        public IByteBufferHolder Duplicate()
        {
            return Replace(Content.Duplicate());
        }

        /// <summary>
        /// The Release
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        public bool Release()
        {
            return Content.Release();
        }

        /// <summary>
        /// The Release
        /// </summary>
        /// <param name="decrement">The decrement<see cref="int"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool Release(int decrement)
        {
            return Content.Release(decrement);
        }

        /// <summary>
        /// The Replace
        /// </summary>
        /// <param name="content">The content<see cref="IByteBuffer"/></param>
        /// <returns>The <see cref="IByteBufferHolder"/></returns>
        public virtual IByteBufferHolder Replace(IByteBuffer content) => new DefaultByteBufferHolder(content);

        /// <summary>
        /// The Retain
        /// </summary>
        /// <returns>The <see cref="IReferenceCounted"/></returns>
        public IReferenceCounted Retain()
        {
            Content.Retain();
            return this;
        }

        /// <summary>
        /// The Retain
        /// </summary>
        /// <param name="increment">The increment<see cref="int"/></param>
        /// <returns>The <see cref="IReferenceCounted"/></returns>
        public IReferenceCounted Retain(int increment)
        {
            Content.Retain(increment);
            return this;
        }

        /// <summary>
        /// The RetainedDuplicate
        /// </summary>
        /// <returns>The <see cref="IByteBufferHolder"/></returns>
        public IByteBufferHolder RetainedDuplicate() => this.Replace(this.Content.RetainedDuplicate());

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            var builder = new StringBuilder(64);
            builder.Append(GetType().GetTypeInfo().Name).Append('(');

            if (Type != DnsRecordType.OPT)
            {
                builder.Append(string.IsNullOrWhiteSpace(Name) ? "<root>" : Name)
                    .Append(' ')
                    .Append(TimeToLive)
                    .Append(' ')
                    .AppendRecordClass(DnsClass)
                    .Append(' ')
                    .Append(Type.Name);
            }
            else
            {
                builder.Append("OPT flags:")
                    .Append(TimeToLive)
                    .Append(" udp:")
                    .Append(DnsClass);
            }

            builder.Append(' ')
                .Append(Content.ReadableBytes)
                .Append("B)");

            return builder.ToString();
        }

        /// <summary>
        /// The Touch
        /// </summary>
        /// <returns>The <see cref="IReferenceCounted"/></returns>
        public IReferenceCounted Touch()
        {
            Content.Touch();
            return this;
        }

        /// <summary>
        /// The Touch
        /// </summary>
        /// <param name="hint">The hint<see cref="object"/></param>
        /// <returns>The <see cref="IReferenceCounted"/></returns>
        public IReferenceCounted Touch(object hint)
        {
            Content.Touch(hint);
            return this;
        }

        #endregion 方法
    }
}