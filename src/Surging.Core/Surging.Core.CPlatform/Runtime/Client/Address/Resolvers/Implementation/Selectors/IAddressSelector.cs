using Surging.Core.CPlatform.Address;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors
{
    #region 接口

    /// <summary>
    /// 一个抽象的地址选择器。
    /// </summary>
    public interface IAddressSelector
    {
        #region 方法

        /// <summary>
        /// 选择一个地址。
        /// </summary>
        /// <param name="context">地址选择上下文。</param>
        /// <returns>地址模型。</returns>
        ValueTask<AddressModel> SelectAsync(AddressSelectContext context);

        #endregion 方法
    }

    #endregion 接口

    /// <summary>
    /// 地址选择上下文。
    /// </summary>
    public class AddressSelectContext
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Address
        /// 服务可用地址。
        /// </summary>
        public IEnumerable<AddressModel> Address { get; set; }

        /// <summary>
        /// Gets or sets the Descriptor
        /// 服务描述符。
        /// </summary>
        public ServiceDescriptor Descriptor { get; set; }

        /// <summary>
        /// Gets or sets the Item
        /// 哈希参数
        /// </summary>
        public string Item { get; set; }

        #endregion 属性
    }
}