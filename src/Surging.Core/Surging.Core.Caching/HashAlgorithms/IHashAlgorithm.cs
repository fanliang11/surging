using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Caching.HashAlgorithms
{
    /// <summary>
    /// 一致性哈希的抽象接口
    /// </summary>
    /// <remarks>
    /// 	<para>创建：范亮</para>
    /// 	<para>日期：2016/4/2</para>
    /// </remarks>
    public interface IHashAlgorithm
    {
        /// <summary>
        /// 获取哈希值
        /// </summary>
        /// <param name="item">字符串</param>
        /// <returns>返回哈希值</returns>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        int Hash(string item);
    }
}
