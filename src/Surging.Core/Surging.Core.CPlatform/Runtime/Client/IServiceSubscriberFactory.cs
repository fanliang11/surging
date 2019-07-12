﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Client
{
    #region 接口

    /// <summary>
    /// 服务订阅者工厂接口
    /// </summary>
    public interface IServiceSubscriberFactory
    {
        #region 方法

        /// <summary>
        /// 根据服务描述创建服务订阅者
        /// </summary>
        /// <param name="descriptors"></param>
        /// <returns></returns>
        Task<IEnumerable<ServiceSubscriber>> CreateServiceSubscribersAsync(IEnumerable<ServiceSubscriberDescriptor> descriptors);

        #endregion 方法
    }

    #endregion 接口
}