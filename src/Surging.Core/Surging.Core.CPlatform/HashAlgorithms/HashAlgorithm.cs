using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.HashAlgorithms
{
    /// <summary>
    /// 一致性哈希算法
    /// </summary>
    /// <remarks>
    /// 	<para>创建：范亮</para>
    /// 	<para>日期：2016/4/2</para>
    /// </remarks>
    public class HashAlgorithm : IHashAlgorithm
    {
        #region 构造函数
        public int Hash(string item)
        {
            var hash = Hash(Encoding.ASCII.GetBytes(item??""));
            return (int)hash;
        }
        #endregion

        #region 常量
        private const UInt32 m = 0x5bd1e995;
        private const Int32 r = 24;
        #endregion

        #region 公共方法
        public static UInt32 Hash(Byte[] data, UInt32 seed = 0xc58f1a7b)
        {
            var length = data.Length;
            if (length == 0)
                return 0;

            var h = seed ^ (UInt32)length;
            var c = 0;
            while (length >= 4)
            {
                var k = (UInt32)(
                    data[c++]
                    | data[c++] << 8
                    | data[c++] << 16
                    | data[c++] << 24);
                k *= m;
                k ^= k >> r;
                k *= m;
                h *= m;
                h ^= k;
                length -= 4;
            }
            switch (length)
            {
                case 3:
                    h ^= (UInt16)(data[c++] | data[c++] << 8);
                    h ^= (UInt32)(data[c] << 16);
                    h *= m;
                    break;
                case 2:
                    h ^= (UInt16)(data[c++] | data[c] << 8);
                    h *= m;
                    break;
                case 1:
                    h ^= data[c];
                    h *= m;
                    break;
                default:
                    break;
            }

            h ^= h >> 13;
            h *= m;
            h ^= h >> 15;
            return h;
        }
        #endregion
    }
}