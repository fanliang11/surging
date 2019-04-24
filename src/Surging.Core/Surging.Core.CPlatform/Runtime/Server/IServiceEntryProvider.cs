using System;
using System.Collections.Generic;

namespace Surging.Core.CPlatform.Runtime.Server
{
    /// <summary>
    /// 一个抽象的服务条目提供程序。
    /// </summary>
    public interface IServiceEntryProvider
    {
        /// <summary>
        /// 获取服务条目集合。
        /// </summary>
        /// <returns>服务条目集合。</returns>
        IEnumerable<ServiceEntry> GetEntries();

        IEnumerable<ServiceEntry> GetALLEntries();

        IEnumerable<Type> GetTypes();
    }
}