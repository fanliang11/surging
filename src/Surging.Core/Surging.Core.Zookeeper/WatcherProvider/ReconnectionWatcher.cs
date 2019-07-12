using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Zookeeper.WatcherProvider
{
    /// <summary>
    /// Defines the <see cref="ReconnectionWatcher" />
    /// </summary>
    internal class ReconnectionWatcher : Watcher
    {
        #region 字段

        /// <summary>
        /// Defines the _connectioned
        /// </summary>
        private readonly Action _connectioned;

        /// <summary>
        /// Defines the _disconnect
        /// </summary>
        private readonly Action _disconnect;

        /// <summary>
        /// Defines the _reconnection
        /// </summary>
        private readonly Action _reconnection;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ReconnectionWatcher"/> class.
        /// </summary>
        /// <param name="connectioned">The connectioned<see cref="Action"/></param>
        /// <param name="disconnect">The disconnect<see cref="Action"/></param>
        /// <param name="reconnection">The reconnection<see cref="Action"/></param>
        public ReconnectionWatcher(Action connectioned, Action disconnect, Action reconnection)
        {
            _connectioned = connectioned;
            _disconnect = disconnect;
            _reconnection = reconnection;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The process
        /// </summary>
        /// <param name="watchedEvent">The event.</param>
        /// <returns></returns>
        public override async Task process(WatchedEvent watchedEvent)
        {
            var state = watchedEvent.getState();
            switch (state)
            {
                case Event.KeeperState.Expired:
                    {
                        _reconnection();
                        break;
                    }
                case Event.KeeperState.AuthFailed:
                    {
                        _disconnect();
                        break;
                    }
                case Event.KeeperState.Disconnected:
                    {
                        _reconnection();
                        break;
                    }
                default:
                    {
                        _connectioned();
                        break;
                    }
            }

#if NET
                await Task.FromResult(1);
#else
            await Task.CompletedTask;
        }

#endregion 方法
    }
}