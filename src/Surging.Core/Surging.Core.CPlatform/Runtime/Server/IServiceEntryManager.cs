using System.Collections.Generic;

namespace Surging.Core.CPlatform.Runtime.Server
{
    #region 接口

    /// <summary>
    /// 一个抽象的服务条目管理者。
    /// </summary>
    public interface IServiceEntryManager
    {
        #region 方法

        /// <summary>
        /// The GetAllEntries
        /// </summary>
        /// <returns>The <see cref="IEnumerable{ServiceEntry}"/></returns>
        IEnumerable<ServiceEntry> GetAllEntries();

        /// <summary>
        /// 获取服务条目集合。
        /// </summary>
        /// <returns>服务条目集合。</returns>
        IEnumerable<ServiceEntry> GetEntries();

        /// <summary>
        /// The UpdateEntries
        /// </summary>
        /// <param name="providers">The providers<see cref="IEnumerable{IServiceEntryProvider}"/></param>
        void UpdateEntries(IEnumerable<IServiceEntryProvider> providers);

        #endregion 方法
    }

    #endregion 接口
}