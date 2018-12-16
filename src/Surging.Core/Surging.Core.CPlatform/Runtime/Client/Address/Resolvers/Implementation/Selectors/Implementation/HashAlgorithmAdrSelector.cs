using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.HashAlgorithms;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Routing.Implementation;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation
{
   public class HashAlgorithmAdrSelector : AddressSelectorBase
    {
        private readonly IHealthCheckService _healthCheckService;
        private readonly ConcurrentDictionary<string, ConsistentHash<AddressModel>> _concurrent =
    new ConcurrentDictionary<string, ConsistentHash<AddressModel>>();
        private readonly IHashAlgorithm _hashAlgorithm;
        public HashAlgorithmAdrSelector(IServiceRouteManager serviceRouteManager, IHealthCheckService healthCheckService, IHashAlgorithm hashAlgorithm)
        {
            _healthCheckService = healthCheckService;
            _hashAlgorithm = hashAlgorithm;
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
        protected override async Task<AddressModel> SelectAsync(AddressSelectContext context)
        {
            var key = GetCacheKey(context.Descriptor);
            var addressEntry = _concurrent.GetOrAdd(key, k => {
                var len = context.Address.Count();
                len = len < 10 ? len * 3 : len;
                return new ConsistentHash<AddressModel>(_hashAlgorithm, len);
                
                });
            AddressModel addressModel;
            do
            {
                addressModel = addressEntry.GetItemNode(context.Item);
            } while (await _healthCheckService.IsHealth(addressModel) == false) ;

            return addressModel;
        }
        #endregion Overrides of AddressSelectorBase

        #region Private Method
        private static string GetCacheKey(ServiceDescriptor descriptor)
        {
            return descriptor.Id;
        }

        private void ServiceRouteManager_Removed(object sender, ServiceRouteEventArgs e)
        {
            var key = GetCacheKey(e.Route.ServiceDescriptor); 
            _concurrent.TryRemove(key, out ConsistentHash<AddressModel> value);
        }

        #endregion
    }
}
