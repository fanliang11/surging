using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Consul.Internal.Cluster.Implementation.Selectors.Implementation
{
    /// <summary>
    /// Defines the <see cref="ConsulRandomAddressSelector" />
    /// </summary>
    public class ConsulRandomAddressSelector : ConsulAddressSelectorBase
    {
        #region 字段

        /// <summary>
        /// Defines the _generate
        /// </summary>
        private readonly Func<int, int, int> _generate;

        /// <summary>
        /// Defines the _random
        /// </summary>
        private readonly Random _random;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsulRandomAddressSelector"/> class.
        /// </summary>
        public ConsulRandomAddressSelector()
        {
            _random = new Random();
            _generate = (min, max) => _random.Next(min, max);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsulRandomAddressSelector"/> class.
        /// </summary>
        /// <param name="generate">随机数生成委托，第一个参数为最小值，第二个参数为最大值（不可以超过该值）。</param>
        public ConsulRandomAddressSelector(Func<int, int, int> generate)
        {
            if (generate == null)
                throw new ArgumentNullException(nameof(generate));
            _generate = generate;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// 选择一个地址。
        /// </summary>
        /// <param name="context">地址选择上下文。</param>
        /// <returns>地址模型。</returns>
        protected override ValueTask<AddressModel> SelectAsync(AddressSelectContext context)
        {
            var address = context.Address.ToArray();
            var length = address.Length;

            var index = _generate(0, length);
            return new ValueTask<AddressModel>(address[index]);
        }

        #endregion 方法
    }
}