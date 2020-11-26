using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Routing;
using Surging.Core.ProxyGenerator.Interceptors;
using Surging.Core.ProxyGenerator.Interceptors.Implementation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.System.Intercept
{
    public class LogProviderInterceptor : IInterceptor
    {
        private readonly IInterceptorProvider _interceptorProvider;

        private readonly IServiceRouteProvider _serviceRouteProvider;
        public LogProviderInterceptor(IInterceptorProvider interceptorProvider, IServiceRouteProvider serviceRouteProvider)
        {
            _interceptorProvider = interceptorProvider;
            _serviceRouteProvider = serviceRouteProvider;
        }

        public async Task Intercept(IInvocation invocation)
        {
            var route = await _serviceRouteProvider.Locate(invocation.ServiceId);
            var cacheMetadata = route.ServiceDescriptor.GetIntercept("Log");
            if (cacheMetadata != null)
            {
                var watch = Stopwatch.StartNew();
                await invocation.Proceed();
                var result = invocation.ReturnValue;
            }
        }
    }
}
