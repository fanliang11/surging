using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Network
{
    public interface INetwork
    {

        string Id { get; set; }

        Task StartAsync();
        /**
         * @return 网络类型
         * @see DefaultNetworkType
         */
        NetworkType GetType();

        /**
         * 关闭网络组件
         */
        void Shutdown();

        /**
         * @return 是否存活
         */
        bool IsAlive();

        /**
         * 当{@link Network#isAlive()}为false是,是否自动重新加载.
         *
         * @return 是否重新加载
         * @see NetworkProvider#reload(Network, Object)
         */
        bool IsAutoReload();
    }
}
