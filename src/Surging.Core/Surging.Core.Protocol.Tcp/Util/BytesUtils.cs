using DotNetty.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Tcp.Util
{
    public class BytesUtils
    {
        public  IByteBuffer Slice(IByteBuffer byteBuffer,int index,int length)
        {
            var result=  byteBuffer.RetainedSlice(index, length).SetReaderIndex(0);
            byteBuffer.SetReaderIndex(length+index);
            result.SetReaderIndex(0);
            return result;
        }
    }
}
