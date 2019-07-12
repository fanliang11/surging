using Surging.Core.CPlatform.Messages;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Surging.Core.CPlatform.Runtime.Server.Implementation
{
    /// <summary>
    /// 默认的服务条目定位器。
    /// </summary>
    public class DefaultServiceEntryLocate : IServiceEntryLocate
    {
        #region 字段

        /// <summary>
        /// Defines the _serviceEntryManager
        /// </summary>
        private readonly IServiceEntryManager _serviceEntryManager;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultServiceEntryLocate"/> class.
        /// </summary>
        /// <param name="serviceEntryManager">The serviceEntryManager<see cref="IServiceEntryManager"/></param>
        public DefaultServiceEntryLocate(IServiceEntryManager serviceEntryManager)
        {
            _serviceEntryManager = serviceEntryManager;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Locate
        /// </summary>
        /// <param name="httpMessage">The httpMessage<see cref="HttpMessage"/></param>
        /// <returns>The <see cref="ServiceEntry"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ServiceEntry Locate(HttpMessage httpMessage)
        {
            string routePath = httpMessage.RoutePath;
            if (httpMessage.RoutePath.AsSpan().IndexOf("/") == -1)
                routePath = $"/{routePath}";
            var serviceEntries = _serviceEntryManager.GetEntries();
            return serviceEntries.SingleOrDefault(i => i.RoutePath == routePath && !i.Descriptor.GetMetadata<bool>("IsOverload"));
        }

        /// <summary>
        /// 定位服务条目。
        /// </summary>
        /// <param name="invokeMessage">远程调用消息。</param>
        /// <returns>服务条目。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ServiceEntry Locate(RemoteInvokeMessage invokeMessage)
        {
            var serviceEntries = _serviceEntryManager.GetEntries();
            return serviceEntries.SingleOrDefault(i => i.Descriptor.Id == invokeMessage.ServiceId);
        }

        #endregion 方法
    }
}