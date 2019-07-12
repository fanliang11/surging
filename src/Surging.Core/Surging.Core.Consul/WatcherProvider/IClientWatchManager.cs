using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Consul.WatcherProvider
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IClientWatchManager" />
    /// </summary>
    public interface IClientWatchManager
    {
        #region 属性

        /// <summary>
        /// Gets or sets the DataWatches
        /// </summary>
        Dictionary<string, HashSet<Watcher>> DataWatches { get; set; }

        #endregion 属性
    }

    #endregion 接口
}