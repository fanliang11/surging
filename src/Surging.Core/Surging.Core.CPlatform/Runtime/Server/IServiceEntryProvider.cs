using System;
using System.Collections.Generic;

namespace Surging.Core.CPlatform.Runtime.Server
{
    #region 接口

    /// <summary>
    /// 一个抽象的服务条目提供程序。
    /// </summary>
    public interface IServiceEntryProvider
    {
        #region 方法

        /// <summary>
        /// The GetALLEntries
        /// </summary>
        /// <returns>The <see cref="IEnumerable{ServiceEntry}"/></returns>
        IEnumerable<ServiceEntry> GetALLEntries();

        /// <summary>
        /// 获取服务条目集合。
        /// </summary>
        /// <returns>服务条目集合。</returns>
        IEnumerable<ServiceEntry> GetEntries();

        /// <summary>
        /// The GetTypes
        /// </summary>
        /// <returns>The <see cref="IEnumerable{Type}"/></returns>
        IEnumerable<Type> GetTypes();

        #endregion 方法
    }

    #endregion 接口
}