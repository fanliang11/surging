using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Network
{
    public interface INetworkManager
    {
         IObservable<INetwork> GetNetwork(NetworkType type, string id);

        /**
         * 获取全部网络组件
         *
         * @return 网络组件
         */
        ReplaySubject<List<INetwork>> GetNetworks();

        /**
         * 获取全部网络组件提供商
         *
         * @return 提供商列表
         */
        List<INetworkProvider<NetworkProperties>> GetProviders();

        /**
         * 根据类型获取提供商
         *
         * @param type 类型
         * @return 提供商
         */
        INetworkProvider<NetworkProperties> GetProvider(String type);

        /**
         * 重新加载网络组件
         *
         * @param type 网络组件类型
         * @param id   ID
         * @return void
         */
        void Reload(NetworkType type, String id);

        /**
         * 停止网络组件
         *
         * @param type 网络组件类型
         * @param id   ID
         * @return void
         */
        void Shutdown(NetworkType type, String id);

        /**
         * 销毁网络组件
         *
         * @param type 网络组件类型
         * @param id   ID
         * @return void
         */
        void Destroy(NetworkType type, String id);

        IObservable<INetwork> CreateOrUpdate(INetworkProvider<NetworkProperties> provider, NetworkProperties properties, ISubject<NetworkLogMessage> subject);
    }
}
