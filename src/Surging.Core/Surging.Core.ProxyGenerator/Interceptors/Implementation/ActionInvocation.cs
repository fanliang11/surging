using Surging.Core.ProxyGenerator.Implementation;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ProxyGenerator.Interceptors.Implementation
{
    public class ActionInvocation : AbstractInvocation
    {
        protected ActionInvocation(
             IDictionary<string, object> arguments,
           string serviceId,
            string[] cacheKey,
            List<Attribute> attributes,
            Type returnType,
            object proxy
            ) : base(arguments, serviceId, cacheKey, attributes, returnType, proxy)
        {
        }

        public override async Task Proceed()
        {
            try
            {
                _returnValue = await (Proxy as ServiceProxyBase).CallInvoke(parameters: Arguments, serviceId: ServiceId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}