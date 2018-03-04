using Surging.Core.Caching.Interfaces;
using Surging.Core.CPlatform.Cache;
using System.Threading.Tasks;

namespace Surging.Core.Caching.HealthChecks
{
    public interface IHealthCheckService
    {
        /// <summary>
        /// 监控一个地址。
        /// </summary>
        /// <param name="address">地址模型。</param>
        /// <returns>一个任务。</returns>
        void Monitor(CacheEndpoint address, string cacheId);

        /// <summary>
        /// 判断一个地址是否健康。
        /// </summary>
        /// <param name="address">地址模型。</param>
        /// <returns>健康返回true，否则返回false。</returns>
        ValueTask<bool> IsHealth(CacheEndpoint address, string cacheId);

        /// <summary>
        /// 标记一个地址为失败的。
        /// </summary>
        /// <param name="address">地址模型。</param>
        /// <returns>一个任务。</returns>
        Task MarkFailure(CacheEndpoint address, string cacheId);
    }
}
