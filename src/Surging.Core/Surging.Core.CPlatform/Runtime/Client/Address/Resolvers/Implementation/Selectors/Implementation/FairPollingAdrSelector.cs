using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Routing.Implementation;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation
{
    /// <summary>
    /// Defines the <see cref="FairPollingAdrSelector" />
    /// </summary>
    public class FairPollingAdrSelector : AddressSelectorBase
    {
        #region 字段

        /// <summary>
        /// Defines the _concurrent
        /// </summary>
        private readonly ConcurrentDictionary<string, Lazy<AddressEntry>> _concurrent =
            new ConcurrentDictionary<string, Lazy<AddressEntry>>();

        /// <summary>
        /// Defines the _healthCheckService
        /// </summary>
        private readonly IHealthCheckService _healthCheckService;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="FairPollingAdrSelector"/> class.
        /// </summary>
        /// <param name="serviceRouteManager">The serviceRouteManager<see cref="IServiceRouteManager"/></param>
        /// <param name="healthCheckService">The healthCheckService<see cref="IHealthCheckService"/></param>
        public FairPollingAdrSelector(IServiceRouteManager serviceRouteManager, IHealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
            //路由发生变更时重建地址条目。
            serviceRouteManager.Changed += ServiceRouteManager_Removed;
            serviceRouteManager.Removed += ServiceRouteManager_Removed;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The SelectAsync
        /// </summary>
        /// <param name="context">The context<see cref="AddressSelectContext"/></param>
        /// <returns>The <see cref="ValueTask{AddressModel}"/></returns>
        protected override async ValueTask<AddressModel> SelectAsync(AddressSelectContext context)
        {
            var key = GetCacheKey(context.Descriptor);
            //根据服务id缓存服务地址。
            var addressEntry = _concurrent.GetOrAdd(key, k => new Lazy<AddressEntry>(() => new AddressEntry(context.Address))).Value;
            AddressModel addressModel;
            var index = 0;
            var len = context.Address.Count();
            do
            {
                addressModel = addressEntry.GetAddress();
                if (len <= index)
                {
                    addressModel = null;
                    break;
                }
                index++;
            } while (await _healthCheckService.IsHealth(addressModel) == false);
            return addressModel;
        }

        /// <summary>
        /// The GetCacheKey
        /// </summary>
        /// <param name="descriptor">The descriptor<see cref="ServiceDescriptor"/></param>
        /// <returns>The <see cref="string"/></returns>
        private static string GetCacheKey(ServiceDescriptor descriptor)
        {
            return descriptor.Id;
        }

        /// <summary>
        /// The ServiceRouteManager_Removed
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="ServiceRouteEventArgs"/></param>
        private void ServiceRouteManager_Removed(object sender, ServiceRouteEventArgs e)
        {
            var key = GetCacheKey(e.Route.ServiceDescriptor);
            Lazy<AddressEntry> value;
            _concurrent.TryRemove(key, out value);
        }

        #endregion 方法

        /// <summary>
        /// Defines the <see cref="AddressEntry" />
        /// </summary>
        protected class AddressEntry
        {
            #region 字段

            /// <summary>
            /// Defines the _address
            /// </summary>
            private readonly AddressModel[] _address;

            /// <summary>
            /// Defines the _maxIndex
            /// </summary>
            private readonly int _maxIndex;

            /// <summary>
            /// Defines the _index
            /// </summary>
            private int _index;

            /// <summary>
            /// Defines the _lock
            /// </summary>
            private int _lock;

            #endregion 字段

            #region 构造函数

            /// <summary>
            /// Initializes a new instance of the <see cref="AddressEntry"/> class.
            /// </summary>
            /// <param name="address">The address<see cref="IEnumerable{AddressModel}"/></param>
            public AddressEntry(IEnumerable<AddressModel> address)
            {
                _address = address.OrderBy(p => p.ProcessorTime).ToArray();
                _maxIndex = _address.Length - 1;
            }

            #endregion 构造函数

            #region 方法

            /// <summary>
            /// The GetAddress
            /// </summary>
            /// <returns>The <see cref="AddressModel"/></returns>
            public AddressModel GetAddress()
            {
                while (true)
                {
                    //如果无法得到锁则等待
                    if (Interlocked.Exchange(ref _lock, 1) != 0)
                    {
                        default(SpinWait).SpinOnce();
                        continue;
                    }

                    var address = _address[_index];

                    //设置为下一个
                    if (_maxIndex > _index)
                        _index++;
                    else
                        _index = 0;

                    //释放锁
                    Interlocked.Exchange(ref _lock, 0);

                    return address;
                }
            }

            #endregion 方法
        }
    }
}