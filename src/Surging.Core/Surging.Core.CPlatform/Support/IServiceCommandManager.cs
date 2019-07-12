using Surging.Core.CPlatform.Support;
using Surging.Core.CPlatform.Support.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Support
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IServiceCommandManager" />
    /// </summary>
    public interface IServiceCommandManager
    {
        #region 事件

        /// <summary>
        /// 服务命令被修改。
        /// </summary>
        event EventHandler<ServiceCommandChangedEventArgs> Changed;

        /// <summary>
        /// 服务命令被创建。
        /// </summary>
        event EventHandler<ServiceCommandEventArgs> Created;

        /// <summary>
        /// 服务命令被删除。
        /// </summary>
        event EventHandler<ServiceCommandEventArgs> Removed;

        #endregion 事件

        #region 方法

        /// <summary>
        /// 清空所有的服务命令。
        /// </summary>
        /// <returns>一个任务。</returns>
        Task ClearAsync();

        /// <summary>
        /// 获取所有可用的服务命令信息。
        /// </summary>
        /// <returns>服务路由集合。</returns>
        Task<IEnumerable<ServiceCommandDescriptor>> GetServiceCommandsAsync();

        /// <summary>
        /// The SetServiceCommandsAsync
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        Task SetServiceCommandsAsync();

        /// <summary>
        /// 设置服务命令。
        /// </summary>
        /// <param name="commands">服务命令集合。</param>
        /// <returns>一个任务。</returns>
        Task SetServiceCommandsAsync(IEnumerable<ServiceCommandDescriptor> commands);

        #endregion 方法
    }

    #endregion 接口
}

/// <summary>
/// 服务命令管理者扩展方法。
/// </summary>
public static class ServiceCommandManagerExtensions
{
    #region 方法

    /// <summary>
    /// 获取所有可用的服务命令信息。
    /// </summary>
    /// <param name="serviceCommandManager">The serviceCommandManager<see cref="IServiceCommandManager"/></param>
    /// <param name="serviceIds">The serviceIds<see cref="string[]"/></param>
    /// <returns>服务命令集合。</returns>
    public static async Task<IEnumerable<ServiceCommandDescriptor>> GetServiceCommandsAsync
        (this IServiceCommandManager serviceCommandManager, params string[] serviceIds)
    {
        var result = (await serviceCommandManager.GetServiceCommandsAsync());
        return result.Where(p => serviceIds.Contains(p.ServiceId));
    }

    #endregion 方法
}