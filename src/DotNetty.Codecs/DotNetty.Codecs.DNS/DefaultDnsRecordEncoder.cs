using DotNetty.Buffers;
using DotNetty.Codecs.DNS.Records;
using DotNetty.Common.Utilities;
using System;
using System.Net.Sockets;
using System.Text;

namespace DotNetty.Codecs.DNS
{
    /// <summary>
    /// Defines the <see cref="DefaultDnsRecordEncoder" />
    /// </summary>
    public class DefaultDnsRecordEncoder : IDnsRecordEncoder
    {
        #region 常量

        /// <summary>
        /// Defines the PREFIX_MASK
        /// </summary>
        private const int PREFIX_MASK = sizeof(byte) - 1;

        /// <summary>
        /// Defines the ROOT
        /// </summary>
        private const string ROOT = ".";

        #endregion 常量

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDnsRecordEncoder"/> class.
        /// </summary>
        internal DefaultDnsRecordEncoder()
        {
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The EncodeQuestion
        /// </summary>
        /// <param name="question">The question<see cref="IDnsQuestion"/></param>
        /// <param name="output">The output<see cref="IByteBuffer"/></param>
        public void EncodeQuestion(IDnsQuestion question, IByteBuffer output)
        {
            EncodeName(question.Name, output);
            output.WriteShort(question.Type.IntValue);
            output.WriteShort((int)question.DnsClass);
        }

        /// <summary>
        /// The EncodeRecord
        /// </summary>
        /// <param name="record">The record<see cref="IDnsRecord"/></param>
        /// <param name="output">The output<see cref="IByteBuffer"/></param>
        public void EncodeRecord(IDnsRecord record, IByteBuffer output)
        {
            if (record is IDnsQuestion)
            {
                EncodeQuestion((IDnsQuestion)record, output);
            }
            else if (record is IDnsPtrRecord)
            {
                EncodePtrRecord((IDnsPtrRecord)record, output);
            }
            else if (record is IDnsOptEcsRecord)
            {
                EncodeOptEcsRecord((IDnsOptEcsRecord)record, output);
            }
            else if (record is IDnsOptPseudoRecord)
            {
                EncodeOptPseudoRecord((IDnsOptPseudoRecord)record, output);
            }
            else if (record is IDnsRawRecord)
            {
                EncodeRawRecord((IDnsRawRecord)record, output);
            }
            else
            {
                throw new UnsupportedMessageTypeException(record.Type.Name);
            }
        }

        /// <summary>
        /// The EncodeName
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="buffer">The buffer<see cref="IByteBuffer"/></param>
        protected void EncodeName(string name, IByteBuffer buffer)
        {
            if (ROOT.Equals(name))
            {
                buffer.WriteByte(0);
                return;
            }

            string[] labels = name.Split('.');
            foreach (var label in labels)
            {
                int labelLen = label.Length;
                if (labelLen == 0)
                    break;

                buffer.WriteByte(labelLen);
                buffer.WriteBytes(Encoding.UTF8.GetBytes(label)); //TODO: Use ByteBufferUtil.WriteAscii() when available
            }
            buffer.WriteByte(0);
        }

        /// <summary>
        /// The CalculateEcsAddressLength
        /// </summary>
        /// <param name="sourcePrefixLength">The sourcePrefixLength<see cref="int"/></param>
        /// <param name="lowOrderBitsToPreserve">The lowOrderBitsToPreserve<see cref="int"/></param>
        /// <returns>The <see cref="int"/></returns>
        private static int CalculateEcsAddressLength(int sourcePrefixLength, int lowOrderBitsToPreserve)
        {
            return sourcePrefixLength.RightUShift(3) + (lowOrderBitsToPreserve != 0 ? 1 : 0);
        }

        /// <summary>
        /// The PadWithZeros
        /// </summary>
        /// <param name="b">The b<see cref="byte"/></param>
        /// <param name="lowOrderBitsToPreserve">The lowOrderBitsToPreserve<see cref="int"/></param>
        /// <returns>The <see cref="byte"/></returns>
        private static byte PadWithZeros(byte b, int lowOrderBitsToPreserve)
        {
            switch (lowOrderBitsToPreserve)
            {
                case 0:
                    return 0;

                case 1:
                    return (byte)(0x80 & b);

                case 2:
                    return (byte)(0xC0 & b);

                case 3:
                    return (byte)(0xE0 & b);

                case 4:
                    return (byte)(0xF0 & b);

                case 5:
                    return (byte)(0xF8 & b);

                case 6:
                    return (byte)(0xFC & b);

                case 7:
                    return (byte)(0xFE & b);

                case 8:
                    return b;

                default:
                    throw new ArgumentException($"lowOrderBitsToPreserve: {lowOrderBitsToPreserve}");
            }
        }

        /// <summary>
        /// The EncodeOptEcsRecord
        /// </summary>
        /// <param name="record">The record<see cref="IDnsOptEcsRecord"/></param>
        /// <param name="output">The output<see cref="IByteBuffer"/></param>
        private void EncodeOptEcsRecord(IDnsOptEcsRecord record, IByteBuffer output)
        {
            EncodeRecordCore(record, output);

            int sourcePrefixLength = record.SourcePrefixLength;
            int scopePrefixLength = record.ScopePrefixLength;
            int lowOrderBitsToPreserve = sourcePrefixLength & PREFIX_MASK;

            byte[] bytes = record.Address;
            int addressBits = bytes.Length << 3;
            if (addressBits < sourcePrefixLength || sourcePrefixLength < 0)
                throw new ArgumentException($"{sourcePrefixLength}: {sourcePrefixLength} (expected 0 >= {addressBits})");

            short addressNumber = (short)(bytes.Length == 4 ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6);
            int payloadLength = CalculateEcsAddressLength(sourcePrefixLength, lowOrderBitsToPreserve);
            int fullPayloadLength = 2 + 2 + 2 + 1 + 1 + payloadLength;

            output.WriteShort(fullPayloadLength);
            output.WriteShort(8);
            output.WriteShort(fullPayloadLength - 4);
            output.WriteShort(addressNumber);
            output.WriteByte(sourcePrefixLength);
            output.WriteByte(scopePrefixLength);

            if (lowOrderBitsToPreserve > 0)
            {
                int bytesLength = payloadLength - 1;
                output.WriteBytes(bytes, 0, bytesLength);
                output.WriteByte(PadWithZeros(bytes[bytesLength], lowOrderBitsToPreserve));
            }
            else
            {
                output.WriteBytes(bytes, 0, payloadLength);
            }
        }

        /// <summary>
        /// The EncodeOptPseudoRecord
        /// </summary>
        /// <param name="record">The record<see cref="IDnsOptPseudoRecord"/></param>
        /// <param name="output">The output<see cref="IByteBuffer"/></param>
        private void EncodeOptPseudoRecord(IDnsOptPseudoRecord record, IByteBuffer output)
        {
            EncodeRecordCore(record, output);
            output.WriteShort(0);
        }

        /// <summary>
        /// The EncodePtrRecord
        /// </summary>
        /// <param name="record">The record<see cref="IDnsPtrRecord"/></param>
        /// <param name="output">The output<see cref="IByteBuffer"/></param>
        private void EncodePtrRecord(IDnsPtrRecord record, IByteBuffer output)
        {
            EncodeRecordCore(record, output);
            EncodeName(record.HostName, output);
        }

        /// <summary>
        /// The EncodeRawRecord
        /// </summary>
        /// <param name="record">The record<see cref="IDnsRawRecord"/></param>
        /// <param name="output">The output<see cref="IByteBuffer"/></param>
        private void EncodeRawRecord(IDnsRawRecord record, IByteBuffer output)
        {
            EncodeRecordCore(record, output);

            IByteBuffer content = record.Content;
            int contentLen = content.ReadableBytes;
            output.WriteShort(contentLen);
            output.WriteBytes(content, content.ReaderIndex, contentLen);
        }

        /// <summary>
        /// The EncodeRecordCore
        /// </summary>
        /// <param name="record">The record<see cref="IDnsRecord"/></param>
        /// <param name="output">The output<see cref="IByteBuffer"/></param>
        private void EncodeRecordCore(IDnsRecord record, IByteBuffer output)
        {
            EncodeName(record.Name, output);
            output.WriteShort(record.Type.IntValue);
            output.WriteShort((int)record.DnsClass);
            output.WriteInt((int)record.TimeToLive);
        }

        #endregion 方法
    }
}