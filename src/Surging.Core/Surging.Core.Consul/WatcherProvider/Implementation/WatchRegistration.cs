using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surging.Core.Consul.WatcherProvider.Implementation
{
    /// <summary>
    /// Defines the <see cref="WatchRegistration" />
    /// </summary>
    public abstract class WatchRegistration
    {
        #region 字段

        /// <summary>
        /// Defines the clientPath
        /// </summary>
        private readonly string clientPath;

        /// <summary>
        /// Defines the watcher
        /// </summary>
        private readonly Watcher watcher;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="WatchRegistration"/> class.
        /// </summary>
        /// <param name="watcher">The watcher<see cref="Watcher"/></param>
        /// <param name="clientPath">The clientPath<see cref="string"/></param>
        protected WatchRegistration(Watcher watcher, string clientPath)
        {
            this.watcher = watcher;
            this.clientPath = clientPath;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Register
        /// </summary>
        public void Register()
        {
            var watches = GetWatches();
            lock (watches)
            {
                HashSet<Watcher> watchers;
                watches.TryGetValue(clientPath, out watchers);
                if (watchers == null)
                {
                    watchers = new HashSet<Watcher>();
                    watches[clientPath] = watchers;
                }
                if (!watchers.Any(p => p.GetType() == watcher.GetType()))
                    watchers.Add(watcher);
            }
        }

        /// <summary>
        /// The GetWatches
        /// </summary>
        /// <returns>The <see cref="Dictionary{string, HashSet{Watcher}}"/></returns>
        protected abstract Dictionary<string, HashSet<Watcher>> GetWatches();

        #endregion 方法
    }
}