using Surging.Core.CPlatform.Address;
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
        public HashAlgorithmAdrSelector(IHealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        #region Overrides of AddressSelectorBase
        /// <summary>
        /// 选择一个地址。
        /// </summary>
        /// <param name="context">地址选择上下文。</param>
        /// <returns>地址模型。</returns>
        protected override async Task<AddressModel> SelectAsync(AddressSelectContext context)
        {
            var address = context.Address.ToList();
            var index = context.HashCode%address.Count;
            while (await _healthCheckService.IsHealth(address[index]) == false)
            {
                address.RemoveAt(index);
                index = context.HashCode % address.Count;
            }
           
            return address[index];
        }
        #endregion Overrides of AddressSelectorBase
    }
}
