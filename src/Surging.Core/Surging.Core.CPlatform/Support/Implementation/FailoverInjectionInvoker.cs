using Surging.Core.CPlatform.Convertibles;
using Surging.Core.CPlatform.Runtime.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Support.Implementation
{
    /// <summary>
    /// Defines the <see cref="FailoverInjectionInvoker" />
    /// </summary>
    public class FailoverInjectionInvoker : IClusterInvoker
    {
        #region 字段

        /// <summary>
        /// Defines the _serviceCommandProvider
        /// </summary>
        public readonly IServiceCommandProvider _serviceCommandProvider;

        /// <summary>
        /// Defines the _serviceEntryManager
        /// </summary>
        public readonly IServiceEntryManager _serviceEntryManager;

        /// <summary>
        /// Defines the _typeConvertibleService
        /// </summary>
        private readonly ITypeConvertibleService _typeConvertibleService;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="FailoverInjectionInvoker"/> class.
        /// </summary>
        /// <param name="serviceCommandProvider">The serviceCommandProvider<see cref="IServiceCommandProvider"/></param>
        /// <param name="serviceEntryManager">The serviceEntryManager<see cref="IServiceEntryManager"/></param>
        /// <param name="typeConvertibleService">The typeConvertibleService<see cref="ITypeConvertibleService"/></param>
        public FailoverInjectionInvoker(IServiceCommandProvider serviceCommandProvider, IServiceEntryManager serviceEntryManager, ITypeConvertibleService typeConvertibleService)
        {
            _serviceCommandProvider = serviceCommandProvider;
            _serviceEntryManager = serviceEntryManager;
            _typeConvertibleService = typeConvertibleService;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <param name="parameters">The parameters<see cref="IDictionary{string, object}"/></param>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <param name="serviceKey">The serviceKey<see cref="string"/></param>
        /// <param name="decodeJOject">The decodeJOject<see cref="bool"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task Invoke(IDictionary<string, object> parameters, string serviceId, string serviceKey, bool decodeJOject)
        {
            var vt = _serviceCommandProvider.GetCommand(serviceId);
            var command = vt.IsCompletedSuccessfully ? vt.Result : await vt;
            var result = await _serviceCommandProvider.Run(command.Injection, command.InjectionNamespaces);
            if (result is Boolean)
            {
                if ((bool)result)
                {
                    var entries = _serviceEntryManager.GetEntries().ToList();
                    var entry = entries.Where(p => p.Descriptor.Id == serviceId).FirstOrDefault();
                    await entry.Func(serviceKey, parameters);
                }
            }
        }

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters">The parameters<see cref="IDictionary{string, object}"/></param>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <param name="serviceKey">The serviceKey<see cref="string"/></param>
        /// <param name="decodeJOject">The decodeJOject<see cref="bool"/></param>
        /// <returns>The <see cref="Task{T}"/></returns>
        public async Task<T> Invoke<T>(IDictionary<string, object> parameters, string serviceId, string serviceKey, bool decodeJOject)
        {
            var vt = _serviceCommandProvider.GetCommand(serviceId);
            var command = vt.IsCompletedSuccessfully ? vt.Result : await vt;
            var injectionResult = await _serviceCommandProvider.Run(command.Injection, command.InjectionNamespaces);
            if (injectionResult is Boolean)
            {
                if ((bool)injectionResult)
                {
                    var entries = _serviceEntryManager.GetEntries().ToList();
                    var entry = entries.Where(p => p.Descriptor.Id == serviceId).FirstOrDefault();
                    var message = await entry.Func(serviceKey, parameters);
                    object result = default(T);
                    if (message == null && message is Task<T>)
                    {
                        result = _typeConvertibleService.Convert((message as Task<T>).Result, typeof(T));
                    }
                    return (T)result;
                }
            }
            else
            {
                var result = injectionResult;
                if (injectionResult is Task<T>)
                {
                    result = _typeConvertibleService.Convert((injectionResult as Task<T>).Result, typeof(T));
                }
                return (T)result;
            }
            return default(T);
        }

        #endregion 方法
    }
}