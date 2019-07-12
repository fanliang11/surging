using Consul;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Consul.WatcherProvider
{
    /// <summary>
    /// Defines the <see cref="Watcher" />
    /// </summary>
    public abstract class Watcher
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="Watcher"/> class.
        /// </summary>
        protected Watcher()
        {
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Process
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        public abstract Task Process();

        #endregion 方法

        /// <summary>
        /// Defines the <see cref="Event" />
        /// </summary>
        public static class Event
        {
            #region 枚举

            /// <summary>
            /// Defines the KeeperState
            /// </summary>
            public enum KeeperState
            {
                /// <summary>
                /// Defines the Disconnected
                /// </summary>
                Disconnected = 0,

                /// <summary>
                /// Defines the SyncConnected
                /// </summary>
                SyncConnected = 3,
            }

            #endregion 枚举
        }
    }
}