using DotNetty.Buffers;
using DotNetty.Codecs.DNS.Records;

namespace DotNetty.Codecs.DNS
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IDnsRecordDecoder" />
    /// </summary>
    public interface IDnsRecordDecoder
    {
        #region 方法

        /// <summary>
        /// The DecodeQuestion
        /// </summary>
        /// <param name="inputBuffer">The inputBuffer<see cref="IByteBuffer"/></param>
        /// <returns>The <see cref="IDnsQuestion"/></returns>
        IDnsQuestion DecodeQuestion(IByteBuffer inputBuffer);

        /// <summary>
        /// The DecodeRecord
        /// </summary>
        /// <param name="inputBuffer">The inputBuffer<see cref="IByteBuffer"/></param>
        /// <returns>The <see cref="IDnsRecord"/></returns>
        IDnsRecord DecodeRecord(IByteBuffer inputBuffer);

        #endregion 方法
    }

    #endregion 接口
}