﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Network
{
    public interface INetworkProvider<T>
    {

        /**
         * @return 类型
         * @see DefaultNetworkType
         */ 
        NetworkType GetNetworkType();

        INetwork CreateNetwork(T properties);

        public INetwork CreateNetwork(NetworkProperties properties, ISubject<NetworkLogMessage> subject);

        void Shutdown(string id);
        /**
         * 重新加载网络组件
         *
         * @param network    网络组件
         * @param properties 配置信息
         */
        void ReloadAsync(T properties);


        IDictionary<string, object> GetConfigMetadata();
    }
}
