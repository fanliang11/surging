using System;
using System.Collections.Generic;

namespace Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery
{
    #region 接口

    /// <summary>
    /// 一个抽象的Clr服务条目工厂。
    /// </summary>
    public interface IClrServiceEntryFactory
    {
        #region 方法

        /// <summary>
        /// 创建服务条目。
        /// </summary>
        /// <param name="service">服务类型。</param>
        /// <returns>服务条目集合。</returns>
        IEnumerable<ServiceEntry> CreateServiceEntry(Type service);

        #endregion 方法
    }

    #endregion 接口
}