using Surging.Core.CPlatform.Convertibles;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Runtime.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Support.Implementation
{
    /// <summary>
    /// Defines the <see cref="FailoverHandoverInvoker" />
    /// </summary>
    public class FailoverHandoverInvoker : IClusterInvoker
    {
        #region 字段

        /// <summary>
        /// Defines the _breakeRemoteInvokeService
        /// </summary>
        private readonly IBreakeRemoteInvokeService _breakeRemoteInvokeService;

        /// <summary>
        /// Defines the _commandProvider
        /// </summary>
        private readonly IServiceCommandProvider _commandProvider;

        /// <summary>
        /// Defines the _remoteInvokeService
        /// </summary>
        private readonly IRemoteInvokeService _remoteInvokeService;

        /// <summary>
        /// Defines the _typeConvertibleService
        /// </summary>
        private readonly ITypeConvertibleService _typeConvertibleService;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="FailoverHandoverInvoker"/> class.
        /// </summary>
        /// <param name="remoteInvokeService">The remoteInvokeService<see cref="IRemoteInvokeService"/></param>
        /// <param name="commandProvider">The commandProvider<see cref="IServiceCommandProvider"/></param>
        /// <param name="typeConvertibleService">The typeConvertibleService<see cref="ITypeConvertibleService"/></param>
        /// <param name="breakeRemoteInvokeService">The breakeRemoteInvokeService<see cref="IBreakeRemoteInvokeService"/></param>
        public FailoverHandoverInvoker(IRemoteInvokeService remoteInvokeService, IServiceCommandProvider commandProvider,
            ITypeConvertibleService typeConvertibleService, IBreakeRemoteInvokeService breakeRemoteInvokeService)
        {
            _remoteInvokeService = remoteInvokeService;
            _typeConvertibleService = typeConvertibleService;
            _breakeRemoteInvokeService = breakeRemoteInvokeService;
            _commandProvider = commandProvider;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters">The parameters<see cref="IDictionary{string, object}"/></param>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <param name="_serviceKey">The _serviceKey<see cref="string"/></param>
        /// <param name="decodeJOject">The decodeJOject<see cref="bool"/></param>
        /// <returns>The <see cref="Task{T}"/></returns>
        public async Task<T> Invoke<T>(IDictionary<string, object> parameters, string serviceId, string _serviceKey, bool decodeJOject)
        {
            var time = 0;
            T result = default(T);
            RemoteInvokeResultMessage message = null;
            var vtCommand = _commandProvider.GetCommand(serviceId);
            var command = vtCommand.IsCompletedSuccessfully ? vtCommand.Result : await vtCommand;
            do
            {
                message = await _breakeRemoteInvokeService.InvokeAsync(parameters, serviceId, _serviceKey, decodeJOject);
                if (message != null && message.Result != null)
                    result = (T)_typeConvertibleService.Convert(message.Result, typeof(T));
            } while (message == null && ++time < command.FailoverCluster);
            return result;
        }

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <param name="parameters">The parameters<see cref="IDictionary{string, object}"/></param>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <param name="_serviceKey">The _serviceKey<see cref="string"/></param>
        /// <param name="decodeJOject">The decodeJOject<see cref="bool"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task Invoke(IDictionary<string, object> parameters, string serviceId, string _serviceKey, bool decodeJOject)
        {
            var time = 0;
            var vtCommand = _commandProvider.GetCommand(serviceId);
            var command = vtCommand.IsCompletedSuccessfully ? vtCommand.Result : await vtCommand;
            while (await _breakeRemoteInvokeService.InvokeAsync(parameters, serviceId, _serviceKey, decodeJOject) == null && ++time < command.FailoverCluster) ;
        }

        #endregion 方法
    }
}