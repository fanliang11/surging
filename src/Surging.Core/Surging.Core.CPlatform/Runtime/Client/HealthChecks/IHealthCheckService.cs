using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks.Implementation;
using System;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Client.HealthChecks
{
    #region 接口

    /// <summary>
    /// 一个抽象的健康检查服务。
    /// </summary>
    public interface IHealthCheckService
    {
        #region 事件

        /// <summary>
        /// Defines the Changed
        /// </summary>
        event EventHandler<HealthCheckEventArgs> Changed;

        /// <summary>
        /// Defines the Removed
        /// </summary>
        event EventHandler<HealthCheckEventArgs> Removed;

        #endregion 事件

        #region 方法

        /// <summary>
        /// 判断一个地址是否健康。
        /// </summary>
        /// <param name="address">地址模型。</param>
        /// <returns>健康返回true，否则返回false。</returns>
        ValueTask<bool> IsHealth(AddressModel address);

        /// <summary>
        /// 标记一个地址为失败的。
        /// </summary>
        /// <param name="address">地址模型。</param>
        /// <returns>一个任务。</returns>
        Task MarkFailure(AddressModel address);

        /// <summary>
        /// 监控一个地址。
        /// </summary>
        /// <param name="address">地址模型。</param>
        void Monitor(AddressModel address);

        #endregion 方法
    }

    #endregion 接口
}