using System.Collections.Generic;

namespace Surging.Core.CPlatform.Runtime.Server
{
    /// <summary>
    /// 一个抽象的服务条目管理者。
    /// </summary>
    public interface IServiceEntryManager
    {
        /// <summary>
        /// 获取服务条目集合。
        /// </summary>
        /// <returns>服务条目集合。</returns>
        IEnumerable<ServiceEntry> GetEntries();

        void UpdateEntries(IEnumerable<IServiceEntryProvider> providers);

        IEnumerable<ServiceEntry> GetAllEntries();
    }
}