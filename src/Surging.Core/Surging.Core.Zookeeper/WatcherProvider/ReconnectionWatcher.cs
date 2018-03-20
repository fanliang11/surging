using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Zookeeper.WatcherProvider
{
    internal class ReconnectionWatcher : Watcher
    {
        private readonly Action _connectioned;
        private readonly Action _disconnect;

        public ReconnectionWatcher(Action connectioned, Action disconnect)
        {
            _connectioned = connectioned;
            _disconnect = disconnect;
        }

        #region Overrides of Watcher

        /// <summary>Processes the specified event.</summary>
        /// <param name="watchedEvent">The event.</param>
        /// <returns></returns>
        public override async Task process(WatchedEvent watchedEvent)
        {
            if (watchedEvent.getState() == Event.KeeperState.SyncConnected)
            {
                _connectioned();
            }
            else
            {
                _disconnect();
            }
#if NET
                await Task.FromResult(1);
#else
            await Task.CompletedTask;
#endif
        }

        #endregion Overrides of Watcher
    }
}
