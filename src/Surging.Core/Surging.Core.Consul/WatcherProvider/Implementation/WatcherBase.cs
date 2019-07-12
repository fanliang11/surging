using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.Consul.WatcherProvider
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
        protected WatcherBase()
        {
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Process
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        public override async Task Process()
        {
            await ProcessImpl();
        }

        /// <summary>
        /// The ProcessImpl
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        protected abstract Task ProcessImpl();

        #endregion 方法
    }
}