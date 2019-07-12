using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Consul.WatcherProvider.Implementation
{
    /// <summary>
    /// Defines the <see cref="ChildWatchRegistration" />
    /// </summary>
    public class ChildWatchRegistration : WatchRegistration
    {
        #region 字段

        /// <summary>
        /// Defines the watchManager
        /// </summary>
        private readonly IClientWatchManager watchManager;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildWatchRegistration"/> class.
        /// </summary>
        /// <param name="watchManager">The watchManager<see cref="IClientWatchManager"/></param>
        /// <param name="watcher">The watcher<see cref="Watcher"/></param>
        /// <param name="clientPath">The clientPath<see cref="string"/></param>
        public ChildWatchRegistration(IClientWatchManager watchManager, Watcher watcher, string clientPath)
            : base(watcher, clientPath)
        {
            this.watchManager = watchManager;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The GetWatches
        /// </summary>
        /// <returns>The <see cref="Dictionary{string, HashSet{Watcher}}"/></returns>
        protected override Dictionary<string, HashSet<Watcher>> GetWatches()
        {
            return watchManager.DataWatches;
        }

        #endregion 方法
    }
}