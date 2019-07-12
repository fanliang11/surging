using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.HashAlgorithms
{
    /// <summary>
    /// 一致性哈希算法
    /// </summary>
    public class HashAlgorithm : IHashAlgorithm
    {
        #region 常量

        /// <summary>
        /// Defines the m
        /// </summary>
        private const UInt32 m = 0x5bd1e995;

        /// <summary>
        /// Defines the r
        /// </summary>
        private const Int32 r = 24;

        #endregion 常量

        #region 方法

        /// <summary>
        /// The Hash
        /// </summary>
        /// <param name="data">The data<see cref="Byte[]"/></param>
        /// <param name="seed">The seed<see cref="UInt32"/></param>
        /// <returns>The <see cref="UInt32"/></returns>
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

        /// <summary>
        /// The Hash
        /// </summary>
        /// <param name="item">The item<see cref="string"/></param>
        /// <returns>The <see cref="int"/></returns>
        public int Hash(string item)
        {
            var hash = Hash(Encoding.ASCII.GetBytes(item ?? ""));
            return (int)hash;
        }

        #endregion 方法
    }
}