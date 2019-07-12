using DotNetty.Transport.Channels;
using System;
using System.Net;

namespace DotNetty.Codecs.DNS.Messages
{
    /// <summary>
    /// Defines the <see cref="DatagramDnsResponse" />
    /// </summary>
    public class DatagramDnsResponse : DefaultDnsResponse, IAddressedEnvelope<DatagramDnsResponse>
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DatagramDnsResponse"/> class.
        /// </summary>
        /// <param name="sender">The sender<see cref="EndPoint"/></param>
        /// <param name="recipient">The recipient<see cref="EndPoint"/></param>
        /// <param name="id">The id<see cref="int"/></param>
        public DatagramDnsResponse(EndPoint sender, EndPoint recipient, int id)
            : this(sender, recipient, id, DnsOpCode.QUERY, DnsResponseCode.NOERROR)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatagramDnsResponse"/> class.
        /// </summary>
        /// <param name="sender">The sender<see cref="EndPoint"/></param>
        /// <param name="recipient">The recipient<see cref="EndPoint"/></param>
        /// <param name="id">The id<see cref="int"/></param>
        /// <param name="opCode">The opCode<see cref="DnsOpCode"/></param>
        public DatagramDnsResponse(EndPoint sender, EndPoint recipient, int id, DnsOpCode opCode)
            : this(sender, recipient, id, opCode, DnsResponseCode.NOERROR)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatagramDnsResponse"/> class.
        /// </summary>
        /// <param name="sender">The sender<see cref="EndPoint"/></param>
        /// <param name="recipient">The recipient<see cref="EndPoint"/></param>
        /// <param name="id">The id<see cref="int"/></param>
        /// <param name="opCode">The opCode<see cref="DnsOpCode"/></param>
        /// <param name="responseCode">The responseCode<see cref="DnsResponseCode"/></param>
        public DatagramDnsResponse(EndPoint sender, EndPoint recipient, int id, DnsOpCode opCode, DnsResponseCode responseCode)
            : base(id, opCode, responseCode)
        {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            Recipient = recipient ?? throw new ArgumentNullException(nameof(recipient));
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the Content
        /// </summary>
        public DatagramDnsResponse Content => this;

        /// <summary>
        /// Gets the Recipient
        /// </summary>
        public EndPoint Recipient { get; }

        /// <summary>
        /// Gets the Sender
        /// </summary>
        public EndPoint Sender { get; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Equals
        /// </summary>
        /// <param name="obj">The obj<see cref="object"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public override bool Equals(object obj)
        {
            if (this == obj) return true;

            if (!base.Equals(obj)) return false;

            if (!(obj is IAddressedEnvelope<DatagramDnsResponse>)) return false;

            var that = (IAddressedEnvelope<DatagramDnsResponse>)obj;

            if (Sender == null)
            {
                if (that.Sender != null)
                    return true;
            }
            else if (!Sender.Equals(that.Sender))
            {
                return false;
            }

            if (Recipient == null)
            {
                if (that.Recipient != null)
                    return false;
            }
            else if (!Recipient.Equals(that.Recipient))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// The GetHashCode
        /// </summary>
        /// <returns>The <see cref="int"/></returns>
        public override int GetHashCode()
        {
            int hashCode = base.GetHashCode();
            if (Sender != null)
            {
                hashCode = hashCode * 31 + Sender.GetHashCode();
            }

            if (Recipient != null)
            {
                hashCode = hashCode * 31 + Recipient.GetHashCode();
            }

            return hashCode;
        }

        #endregion 方法
    }
}