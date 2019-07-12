using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.Zookeeper.WatcherProvider
{
    /// <summary>
    /// Defines the <see cref="NodeMonitorWatcher" />
    /// </summary>
    internal class NodeMonitorWatcher : WatcherBase
    {
        #region 字段

        /// <summary>
        /// Defines the _action
        /// </summary>
        private readonly Action<byte[], byte[]> _action;

        /// <summary>
        /// Defines the _zooKeeperCall
        /// </summary>
        private readonly Func<ValueTask<(ManualResetEvent, ZooKeeper)>> _zooKeeperCall;

        /// <summary>
        /// Defines the _currentData
        /// </summary>
        private byte[] _currentData;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeMonitorWatcher"/> class.
        /// </summary>
        /// <param name="zooKeeperCall">The zooKeeperCall<see cref="Func{ValueTask{(ManualResetEvent, ZooKeeper)}}"/></param>
        /// <param name="path">The path<see cref="string"/></param>
        /// <param name="action">The action<see cref="Action{byte[], byte[]}"/></param>
        public NodeMonitorWatcher(Func<ValueTask<(ManualResetEvent, ZooKeeper)>> zooKeeperCall, string path, Action<byte[], byte[]> action) : base(path)
        {
            _zooKeeperCall = zooKeeperCall;
            _action = action;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The SetCurrentData
        /// </summary>
        /// <param name="currentData">The currentData<see cref="byte[]"/></param>
        /// <returns>The <see cref="NodeMonitorWatcher"/></returns>
        public NodeMonitorWatcher SetCurrentData(byte[] currentData)
        {
            _currentData = currentData;

            return this;
        }

        /// <summary>
        /// The ProcessImpl
        /// </summary>
        /// <param name="watchedEvent">The watchedEvent<see cref="WatchedEvent"/></param>
        /// <returns>The <see cref="Task"/></returns>
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

        #endregion 方法
    }
}