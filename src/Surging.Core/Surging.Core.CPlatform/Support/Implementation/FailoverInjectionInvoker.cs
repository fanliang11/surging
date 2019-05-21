using Surging.Core.CPlatform.Convertibles;
using Surging.Core.CPlatform.Runtime.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Support.Implementation
{
    public class FailoverInjectionInvoker : IClusterInvoker
    {
        public readonly IServiceCommandProvider _serviceCommandProvider;
        public readonly IServiceEntryManager _serviceEntryManager;
        private readonly ITypeConvertibleService _typeConvertibleService;

        public FailoverInjectionInvoker(IServiceCommandProvider serviceCommandProvider, IServiceEntryManager serviceEntryManager, ITypeConvertibleService typeConvertibleService)
        {
            _serviceCommandProvider = serviceCommandProvider;
            _serviceEntryManager = serviceEntryManager;
            _typeConvertibleService = typeConvertibleService;
        }

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
    }
}
