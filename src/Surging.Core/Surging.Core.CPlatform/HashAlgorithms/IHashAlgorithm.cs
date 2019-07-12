using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.HashAlgorithms
{
    #region 接口

    /// <summary>
    /// 一致性哈希的抽象接口
    /// </summary>
    public interface IHashAlgorithm
    {
        #region 方法

        /// <summary>
        /// 获取哈希值
        /// </summary>
        /// <param name="item">字符串</param>
        /// <returns>返回哈希值</returns>
        int Hash(string item);

        #endregion 方法
    }

    #endregion 接口
}