using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.Zookeeper.WatcherProvider
{
    /// <summary>
    /// Defines the <see cref="ChildrenMonitorWatcher" />
    /// </summary>
    internal class ChildrenMonitorWatcher : WatcherBase
    {
        #region 字段

        /// <summary>
        /// Defines the _action
        /// </summary>
        private readonly Action<string[], string[]> _action;

        /// <summary>
        /// Defines the _zooKeeperCall
        /// </summary>
        private readonly Func<ValueTask<(ManualResetEvent, ZooKeeper)>> _zooKeeperCall;

        /// <summary>
        /// Defines the _currentData
        /// </summary>
        private string[] _currentData = new string[0];

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildrenMonitorWatcher"/> class.
        /// </summary>
        /// <param name="zooKeeperCall">The zooKeeperCall<see cref="Func{ValueTask{(ManualResetEvent, ZooKeeper)}}"/></param>
        /// <param name="path">The path<see cref="string"/></param>
        /// <param name="action">The action<see cref="Action{string[], string[]}"/></param>
        public ChildrenMonitorWatcher(Func<ValueTask<(ManualResetEvent, ZooKeeper)>> zooKeeperCall, string path, Action<string[], string[]> action)
                : base(path)
        {
            _zooKeeperCall = zooKeeperCall;
            _action = action;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The SetCurrentData
        /// </summary>
        /// <param name="currentData">The currentData<see cref="string[]"/></param>
        /// <returns>The <see cref="ChildrenMonitorWatcher"/></returns>
        public ChildrenMonitorWatcher SetCurrentData(string[] currentData)
        {
            _currentData = currentData ?? new string[0];

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
            var zooKeeper = await _zooKeeperCall();
            Func<ChildrenMonitorWatcher> getWatcher = () => new ChildrenMonitorWatcher(_zooKeeperCall, path, _action);
            switch (watchedEvent.get_Type())
            {
                //创建之后开始监视下面的子节点情况。
                case Event.EventType.NodeCreated:
                    await zooKeeper.Item2.getChildrenAsync(path, getWatcher());
                    break;

                //子节点修改则继续监控子节点信息并通知客户端数据变更。
                case Event.EventType.NodeChildrenChanged:
                    try
                    {
                        var watcher = getWatcher();
                        var result = await zooKeeper.Item2.getChildrenAsync(path, watcher);
                        var childrens = result.Children.ToArray();
                        _action(_currentData, childrens);
                        watcher.SetCurrentData(childrens);
                    }
                    catch (KeeperException.NoNodeException)
                    {
                        _action(_currentData, new string[0]);
                    }
                    break;

                //删除之后开始监控自身节点，并通知客户端数据被清空。
                case Event.EventType.NodeDeleted:
                    {
                        var watcher = getWatcher();
                        await zooKeeper.Item2.existsAsync(path, watcher);
                        _action(_currentData, new string[0]);
                        watcher.SetCurrentData(new string[0]);
                    }
                    break;
            }
        }

        #endregion 方法
    }
}