using DotNetty.Buffers;

namespace DotNetty.Codecs.DNS.Records
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IDnsRawRecord" />
    /// </summary>
    public interface IDnsRawRecord : IDnsRecord, IByteBufferHolder
    {
    }

    #endregion 接口
}