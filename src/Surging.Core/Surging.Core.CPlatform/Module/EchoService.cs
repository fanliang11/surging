using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.HashAlgorithms;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Module
{
    /// <summary>
    /// Defines the <see cref="EchoService" />
    /// </summary>
    public class EchoService : ServiceBase, IEchoService
    {
        #region 字段

        /// <summary>
        /// Defines the _addressSelector
        /// </summary>
        private readonly IAddressSelector _addressSelector;

        /// <summary>
        /// Defines the _hashAlgorithm
        /// </summary>
        private readonly IHashAlgorithm _hashAlgorithm;

        /// <summary>
        /// Defines the _serviceHeartbeatManager
        /// </summary>
        private readonly IServiceHeartbeatManager _serviceHeartbeatManager;

        /// <summary>
        /// Defines the _serviceRouteProvider
        /// </summary>
        private readonly IServiceRouteProvider _serviceRouteProvider;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="EchoService"/> class.
        /// </summary>
        /// <param name="hashAlgorithm">The hashAlgorithm<see cref="IHashAlgorithm"/></param>
        /// <param name="serviceRouteProvider">The serviceRouteProvider<see cref="IServiceRouteProvider"/></param>
        /// <param name="container">The container<see cref="CPlatformContainer"/></param>
        /// <param name="serviceHeartbeatManager">The serviceHeartbeatManager<see cref="IServiceHeartbeatManager"/></param>
        public EchoService(IHashAlgorithm hashAlgorithm, IServiceRouteProvider serviceRouteProvider,
            CPlatformContainer container, IServiceHeartbeatManager serviceHeartbeatManager)
        {
            _hashAlgorithm = hashAlgorithm;
            _addressSelector = container.GetInstances<IAddressSelector>(AddressSelectorMode.HashAlgorithm.ToString());
            _serviceRouteProvider = serviceRouteProvider;

            _serviceHeartbeatManager = serviceHeartbeatManager;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Locate
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="routePath">The routePath<see cref="string"/></param>
        /// <returns>The <see cref="Task{IpAddressModel}"/></returns>
        public async Task<IpAddressModel> Locate(string key, string routePath)
        {
            var route = await _serviceRouteProvider.SearchRoute(routePath);
            AddressModel result = new IpAddressModel();
            if (route != null)
            {
                result = await _addressSelector.SelectAsync(new AddressSelectContext()
                {
                    Address = route.Address,
                    Descriptor = route.ServiceDescriptor,
                    Item = key,
                });
                _serviceHeartbeatManager.AddWhitelist(route.ServiceDescriptor.Id);
            }
            var ipAddress = result as IpAddressModel;
            return ipAddress;
        }

        #endregion 方法
    }
}