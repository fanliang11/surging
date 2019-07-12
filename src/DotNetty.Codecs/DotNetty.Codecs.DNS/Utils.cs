using DotNetty.Codecs.DNS.Messages;
using DotNetty.Codecs.DNS.Records;
using DotNetty.Transport.Channels;
using System;
using System.Reflection;
using System.Text;

namespace DotNetty.Codecs.DNS
{
    /// <summary>
    /// Defines the <see cref="Utils" />
    /// </summary>
    internal static class Utils
    {
        #region 方法

        /// <summary>
        /// The AppendRecordClass
        /// </summary>
        /// <param name="builder">The builder<see cref="StringBuilder"/></param>
        /// <param name="dnsClass">The dnsClass<see cref="DnsRecordClass"/></param>
        /// <returns>The <see cref="StringBuilder"/></returns>
        internal static StringBuilder AppendRecordClass(this StringBuilder builder, DnsRecordClass dnsClass)
        {
            string name;
            switch (dnsClass)
            {
                case DnsRecordClass.IN:
                    name = "IN";
                    break;

                case DnsRecordClass.CSNET:
                    name = "CSNET";
                    break;

                case DnsRecordClass.CHAOS:
                    name = "CHAOS";
                    break;

                case DnsRecordClass.HESIOD:
                    name = "HESIOD";
                    break;

                case DnsRecordClass.NONE:
                    name = "NONE";
                    break;

                case DnsRecordClass.ANY:
                    name = "ANY";
                    break;

                default:
                    name = $"UNKNOWN({dnsClass})";
                    break;
            }

            builder.Append(name);
            return builder;
        }

        /// <summary>
        /// The AppendResponse
        /// </summary>
        /// <param name="builder">The builder<see cref="StringBuilder"/></param>
        /// <param name="response">The response<see cref="IDnsResponse"/></param>
        /// <returns>The <see cref="StringBuilder"/></returns>
        internal static StringBuilder AppendResponse(this StringBuilder builder, IDnsResponse response)
        {
            builder.AppendResponseHeader(response)
                .AppendAllRecords(response);
            return builder;
        }

        /// <summary>
        /// The AppendResponseHeader
        /// </summary>
        /// <param name="builder">The builder<see cref="StringBuilder"/></param>
        /// <param name="response">The response<see cref="IDnsResponse"/></param>
        /// <returns>The <see cref="StringBuilder"/></returns>
        internal static StringBuilder AppendResponseHeader(this StringBuilder builder, IDnsResponse response)
        {
            builder.Append(response.GetType().GetTypeInfo().Name)
               .Append('(')
               .AppendAddresses(response)
               .Append(response.Id)
               .Append(", ")
               .Append(response.OpCode)
               .Append(", ")
               .Append(response.Code)
               .Append(',');

            bool hasComma = true;
            if (response.IsRecursionDesired)
            {
                hasComma = false;
                builder.Append(" RD");
            }
            if (response.IsAuthoritativeAnswer)
            {
                hasComma = false;
                builder.Append(" AA");
            }
            if (response.IsTruncated)
            {
                hasComma = false;
                builder.Append(" TC");
            }
            if (response.IsRecursionAvailable)
            {
                hasComma = false;
                builder.Append(" RA");
            }
            if (response.Z != 0)
            {
                if (!hasComma)
                {
                    builder.Append(',');
                }
                builder.Append(" Z: ")
                   .Append(response.Z);
            }

            if (hasComma)
            {
                builder[builder.Length - 1] = ')';
            }
            else
            {
                builder.Append(')');
            }

            return builder;
        }

        /// <summary>
        /// The AppendAddresses
        /// </summary>
        /// <param name="builder">The builder<see cref="StringBuilder"/></param>
        /// <param name="response">The response<see cref="IDnsMessage"/></param>
        /// <returns>The <see cref="StringBuilder"/></returns>
        private static StringBuilder AppendAddresses(this StringBuilder builder, IDnsMessage response)
        {
            if (!(response is IAddressedEnvelope<IDnsMessage>))
                return builder;

            IAddressedEnvelope<IDnsMessage> envelope = (IAddressedEnvelope<IDnsMessage>)response;

            var addr = envelope.Sender;
            if (addr != null)
            {
                builder.Append("from: ")
                   .Append(addr)
                   .Append(", ");
            }

            addr = envelope.Recipient;
            if (addr != null)
            {
                builder.Append("to: ")
                   .Append(addr)
                   .Append(", ");
            }

            return builder;
        }

        /// <summary>
        /// The AppendAllRecords
        /// </summary>
        /// <param name="builder">The builder<see cref="StringBuilder"/></param>
        /// <param name="msg">The msg<see cref="IDnsMessage"/></param>
        /// <returns>The <see cref="StringBuilder"/></returns>
        private static StringBuilder AppendAllRecords(this StringBuilder builder, IDnsMessage msg)
        {
            return builder.AppendRecords(msg, DnsSection.QUESTION)
                 .AppendRecords(msg, DnsSection.ANSWER)
                 .AppendRecords(msg, DnsSection.AUTHORITY)
                 .AppendRecords(msg, DnsSection.ADDITIONAL);
        }

        /// <summary>
        /// The AppendRecords
        /// </summary>
        /// <param name="builder">The builder<see cref="StringBuilder"/></param>
        /// <param name="message">The message<see cref="IDnsMessage"/></param>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        /// <returns>The <see cref="StringBuilder"/></returns>
        private static StringBuilder AppendRecords(this StringBuilder builder, IDnsMessage message, DnsSection section)
        {
            int count = message.Count(section);
            for (int i = 0; i < count; i++)
            {
                builder.Append(Environment.NewLine)
                   .Append('\t')
                   .Append(message.GetRecord<IDnsRecord>(section, i));
            }

            return builder;
        }

        #endregion 方法
    }
}