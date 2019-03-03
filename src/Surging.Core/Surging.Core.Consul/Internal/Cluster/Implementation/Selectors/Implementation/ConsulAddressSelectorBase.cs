using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Consul.Internal.Cluster.Implementation.Selectors.Implementation
{
    public abstract class ConsulAddressSelectorBase : IConsulAddressSelector
    {
        #region Implementation of IAddressSelector

        /// <summary>
        /// 选择一个地址。
        /// </summary>
        /// <param name="context">地址选择上下文。</param>
        /// <returns>地址模型。</returns>
        ValueTask<AddressModel> IAddressSelector.SelectAsync(AddressSelectContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (context.Descriptor == null)
                throw new ArgumentNullException(nameof(context.Descriptor));
            if (context.Address == null)
                throw new ArgumentNullException(nameof(context.Address));

            //  var address = context.Address.ToArray();
            if (context.Address.Count() == 0)
                throw new ArgumentException("没有任何地址信息。", nameof(context.Address));

            return context.Address.Count() == 1 ? new ValueTask<AddressModel>(context.Address.First()) : SelectAsync(context);
        }

        #endregion Implementation of IAddressSelector

        /// <summary>
        /// 选择一个地址。
        /// </summary>
        /// <param name="context">地址选择上下文。</param>
        /// <returns>地址模型。</returns>
        protected abstract ValueTask<AddressModel> SelectAsync(AddressSelectContext context);
    }
}