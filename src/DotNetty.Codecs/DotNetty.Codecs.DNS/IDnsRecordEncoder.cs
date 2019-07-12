using DotNetty.Buffers;
using DotNetty.Codecs.DNS.Records;

namespace DotNetty.Codecs.DNS
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IDnsRecordEncoder" />
    /// </summary>
    public interface IDnsRecordEncoder
    {
        #region 方法

        /// <summary>
        /// The EncodeQuestion
        /// </summary>
        /// <param name="question">The question<see cref="IDnsQuestion"/></param>
        /// <param name="output">The output<see cref="IByteBuffer"/></param>
        void EncodeQuestion(IDnsQuestion question, IByteBuffer output);

        /// <summary>
        /// The EncodeRecord
        /// </summary>
        /// <param name="record">The record<see cref="IDnsRecord"/></param>
        /// <param name="output">The output<see cref="IByteBuffer"/></param>
        void EncodeRecord(IDnsRecord record, IByteBuffer output);

        #endregion 方法
    }

    #endregion 接口
}