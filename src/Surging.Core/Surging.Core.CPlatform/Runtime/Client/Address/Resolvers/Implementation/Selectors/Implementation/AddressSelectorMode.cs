using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation
{
    /// <summary>
    /// 负载均衡模式
    /// </summary>
    public enum AddressSelectorMode
    {
        /// <summary>
        /// Hash算法
        /// </summary>
        HashAlgorithm,
        /// <summary>
        /// 轮训
        /// </summary>
        Polling,
        /// <summary>
        /// 随机
        /// </summary>
        Random,
        /// <summary>
        /// 压力最小优先
        /// </summary>
        FairPolling,
    }
}
