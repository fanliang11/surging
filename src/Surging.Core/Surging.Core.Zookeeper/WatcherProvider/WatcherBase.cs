using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Zookeeper.WatcherProvider
{
    /// <summary>
    /// Defines the <see cref="WatcherBase" />
    /// </summary>
    public abstract class WatcherBase : Watcher
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="WatcherBase"/> class.
        /// </summary>
        /// <param name="path">The path<see cref="string"/></param>
        protected WatcherBase(string path)
        {
            Path = path;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the Path
        /// </summary>
        protected string Path { get; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The process
        /// </summary>
        /// <param name="watchedEvent">The watchedEvent<see cref="WatchedEvent"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public override async Task process(WatchedEvent watchedEvent)
        {
            if (watchedEvent.getState() != Event.KeeperState.SyncConnected || watchedEvent.getPath() != Path)
                return;
            await ProcessImpl(watchedEvent);
        }

        /// <summary>
        /// The ProcessImpl
        /// </summary>
        /// <param name="watchedEvent">The watchedEvent<see cref="WatchedEvent"/></param>
        /// <returns>The <see cref="Task"/></returns>
        protected abstract Task ProcessImpl(WatchedEvent watchedEvent);

        #endregion 方法
    }
}