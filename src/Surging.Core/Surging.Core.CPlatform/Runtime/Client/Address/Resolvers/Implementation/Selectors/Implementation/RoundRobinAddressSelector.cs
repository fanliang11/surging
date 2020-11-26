using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Routing.Implementation;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation
{
    public class RoundRobinAddressSelector : AddressSelectorBase
    {
        private const int RECYCLE_PERIOD = 60000;
        private readonly IHealthCheckService _healthCheckService;

        private readonly ConcurrentDictionary<string, Lazy<AddressEntry>> _concurrent =
            new ConcurrentDictionary<string, Lazy<AddressEntry>>();

        public RoundRobinAddressSelector(IServiceRouteManager serviceRouteManager, IHealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
            //路由发生变更时重建地址条目。
            serviceRouteManager.Changed += ServiceRouteManager_Removed;
            serviceRouteManager.Removed += ServiceRouteManager_Removed;
        }

        #region Overrides of AddressSelectorBase

        /// <summary>
        /// 选择一个地址。
        /// </summary>
        /// <param name="context">地址选择上下文。</param>
        /// <returns>地址模型。</returns>
        protected override async ValueTask<AddressModel> SelectAsync(AddressSelectContext context)
        {
            var key = GetCacheKey(context.Descriptor);
            //根据服务id缓存服务地址。
            var addressEntry = _concurrent.GetOrAdd(key, k => new Lazy<AddressEntry>(() => new AddressEntry(context.Address))).Value;
            AddressModel addressModel;
            var index = 0;
            var len = context.Address.Count();
            ValueTask<bool> vt;
            do
            {

                addressModel = addressEntry.GetAddress();
                if (len <= index)
                {
                    addressModel = null;
                    break;
                }
                index++;
                vt = _healthCheckService.IsHealth(addressModel);
            } while (!(vt.IsCompletedSuccessfully ? vt.Result : await vt));
            return addressModel;
        }

        #endregion Overrides of AddressSelectorBase

        #region Private Method

        private static string GetCacheKey(ServiceDescriptor descriptor)
        {
            return descriptor.Id;
        }

        public async ValueTask<bool> CheckHealth(AddressModel addressModel)
        {
            var vt = _healthCheckService.IsHealth(addressModel);
            return vt.IsCompletedSuccessfully ? vt.Result : await vt;
        }

        private void ServiceRouteManager_Removed(object sender, ServiceRouteEventArgs e)
        {
            var key = GetCacheKey(e.Route.ServiceDescriptor);
            var addressEntry = _concurrent.GetOrAdd(key, k => new Lazy<AddressEntry>(() => new AddressEntry(e.Route.Address))).Value;
            addressEntry.SetAddresses(e.Route.Address.ToArray());
        }

        #endregion Private Method
        protected class WeightedRoundRobin
        {
            private int _weight;
            private readonly AtomicLong current = new AtomicLong(0);
            private long lastUpdate;

            public int GetWeight()
            {
                return _weight;
            }

            public void SetWeight(int weight)
            {
                this._weight = weight;
                current.Set(0);
            }

            public long IncreaseCurrent()
            {
                return current.AddAndGet(_weight);
            }

            public void Sel(int total)
            {
                current.AddAndGet(-1 * total);
            }

            public long GetLastUpdate()
            {
                return lastUpdate;
            }

            public void SetLastUpdate(long lastUpdate)
            {
                this.lastUpdate = lastUpdate;
            }
        }

        #region Help Class

        protected class AddressEntry
        {
            #region Field

            private   AddressModel[] _address;

            private readonly ConcurrentDictionary<string, Lazy<WeightedRoundRobin>> _concurrent =
                new ConcurrentDictionary<string, Lazy<WeightedRoundRobin>>();
            #endregion Field

            #region Constructor

            public AddressEntry(IEnumerable<AddressModel> address)
            {
                _address = address.ToArray();
            }

            #endregion Constructor

            #region Public Method

            public AddressModel GetAddress()
            {
                int totalWeight = 0;
                long maxCurrent = long.MinValue;
                var now = DateTimeConverter.DateTimeToUnixTimestamp(DateTime.Now); 
                AddressModel selectedAddr = null;
                WeightedRoundRobin selectedWRR = null;
                foreach (var address in _address)
                {
                    var identifyString = address.ToString();
                    int weight = GetWeight(address);
                    var weightedRoundRobin = _concurrent.GetOrAdd(identifyString, k => new Lazy<WeightedRoundRobin>(() =>
                    {
                        WeightedRoundRobin wrr = new WeightedRoundRobin();
                        wrr.SetWeight(weight);
                        return wrr;
                    })).Value;
                    if (weight != weightedRoundRobin.GetWeight())
                    {
                        //weight changed
                        weightedRoundRobin.SetWeight(weight);
                    }
                    long cur = weightedRoundRobin.IncreaseCurrent();
                    weightedRoundRobin.SetLastUpdate(now);
                    if (cur > maxCurrent)
                    {
                        maxCurrent = cur;
                        selectedAddr = address;
                        selectedWRR = weightedRoundRobin;
                    }
                    totalWeight += weight;
                }

                if (_address.Count() != _concurrent.Count())
                {
                   var concurrentsToRemove = _concurrent.Where(p => (now - p.Value.Value.GetLastUpdate())*1000 > RECYCLE_PERIOD).ToList();
                    concurrentsToRemove.ForEach(concurrent => _concurrent.TryRemove(concurrent.Key, out Lazy<WeightedRoundRobin> obj));
                }
                if (selectedAddr != null)
                {
                    selectedWRR.Sel(totalWeight);
                    return selectedAddr;
                }
                return _address[0];
            }

            public void SetAddresses(AddressModel[] addresses)
            {
                _address= addresses;
            }
            #endregion Public Method
        }

        #endregion Help Class
    }
}