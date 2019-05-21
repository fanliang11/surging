using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.Zookeeper.Internal
{
   public interface IZookeeperClientProvider
    {
        ValueTask<(ManualResetEvent, ZooKeeper)> GetZooKeeper();

        ValueTask<IEnumerable<(ManualResetEvent, ZooKeeper)>> GetZooKeepers();

        ValueTask Check();
    }
}
