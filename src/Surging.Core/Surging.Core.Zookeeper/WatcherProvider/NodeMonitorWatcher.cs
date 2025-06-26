using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.Zookeeper.WatcherProvider
{
    internal class NodeMonitorWatcher : WatcherBase
    {
        private readonly Func<ValueTask<(ManualResetEvent, ZooKeeper)>> _zooKeeperCall;
        private readonly Action<byte[], byte[]> _action;
        private byte[] _currentData;

        public NodeMonitorWatcher(Func<ValueTask<(ManualResetEvent, ZooKeeper)>> zooKeeperCall, string path, Action<byte[], byte[]> action) : base(path)
        {
            _zooKeeperCall = zooKeeperCall;
            _action = action;
        }

        public NodeMonitorWatcher SetCurrentData(byte[] currentData)
        {
            _currentData = currentData;

            return this;
        }

        #region Overrides of WatcherBase

        protected override async Task ProcessImpl(WatchedEvent watchedEvent)
        {
            var path = Path;
            switch (watchedEvent.get_Type())
            {
                case Event.EventType.NodeDataChanged:
                    var zooKeeper = await _zooKeeperCall();
                    var watcher = new NodeMonitorWatcher(_zooKeeperCall, path, _action);
                    var data = await zooKeeper.Item2.getDataAsync(path, watcher);
                    var newData = data.Data;
                    _action(_currentData, newData);
                    watcher.SetCurrentData(newData);
                    break;
            }
        }

        #endregion Overrides of WatcherBase
    }
}