using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Routing.Implementation;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks;

namespace Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation
{
    public class FairPollingAdrSelector : AddressSelectorBase
    {
        private readonly IHealthCheckService _healthCheckService;

        private readonly ConcurrentDictionary<string, Lazy<AddressEntry>> _concurrent =
            new ConcurrentDictionary<string, Lazy<AddressEntry>>();

        public FairPollingAdrSelector(IServiceRouteManager serviceRouteManager, IHealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
            //路由发生变更时重建地址条目。
            serviceRouteManager.Changed += ServiceRouteManager_Removed;
            serviceRouteManager.Removed += ServiceRouteManager_Removed;
        }

        private void ServiceRouteManager_Removed(object sender, ServiceRouteEventArgs e)
        {
            var key = GetCacheKey(e.Route.ServiceDescriptor);
            Lazy<AddressEntry> value;
            _concurrent.TryRemove(key, out value);
        }

        private static string GetCacheKey(ServiceDescriptor descriptor)
        {
            return descriptor.Id;
        }

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

        #region Help Class

        protected class AddressEntry
        {
            #region Field

            private int _index;
            private int _lock;
            private readonly int _maxIndex;
            private readonly AddressModel[] _address;

            #endregion Field

            #region Constructor

            public AddressEntry(IEnumerable<AddressModel> address)
            {
                _address = address.OrderBy(p=>p.ProcessorTime).ToArray();
                _maxIndex = _address.Length - 1;
            }
            
            #endregion Constructor

            #region Public Method

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

            #endregion Public Method
        }

        #endregion Help Class
    }
}
