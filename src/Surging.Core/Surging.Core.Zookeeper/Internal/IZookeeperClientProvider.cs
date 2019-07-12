using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.Zookeeper.Internal
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IZookeeperClientProvider" />
    /// </summary>
    public interface IZookeeperClientProvider
    {
        #region 方法

        /// <summary>
        /// The Check
        /// </summary>
        /// <returns>The <see cref="ValueTask"/></returns>
        ValueTask Check();

        /// <summary>
        /// The GetZooKeeper
        /// </summary>
        /// <returns>The <see cref="ValueTask{(ManualResetEvent, ZooKeeper)}"/></returns>
        ValueTask<(ManualResetEvent, ZooKeeper)> GetZooKeeper();

        /// <summary>
        /// The GetZooKeepers
        /// </summary>
        /// <returns>The <see cref="ValueTask{IEnumerable{(ManualResetEvent, ZooKeeper)}}"/></returns>
        ValueTask<IEnumerable<(ManualResetEvent, ZooKeeper)>> GetZooKeepers();

        #endregion 方法
    }

    #endregion 接口
}