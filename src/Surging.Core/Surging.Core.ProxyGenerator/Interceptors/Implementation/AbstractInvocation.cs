using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ProxyGenerator.Interceptors.Implementation
{
    public abstract class AbstractInvocation : IInvocation,ICacheInvocation
    {
        private readonly IDictionary<string, object> _arguments;
        private readonly string _serviceId;
        private readonly string[] _cacheKey;
        private readonly List<Attribute> _attributes;
        private readonly Type _returnType;
        protected readonly object proxyObject;
        protected object _returnValue;

        protected AbstractInvocation(
          IDictionary<string, object> arguments,
           string serviceId,
            string[] cacheKey,
            List<Attribute> attributes,
            Type returnType,
            object proxy
            )
        {
            _arguments = arguments;
            _serviceId = serviceId;
            _cacheKey = cacheKey;
            _attributes = attributes;
            _returnType = returnType;
            proxyObject = proxy;
        }

        public object Proxy => proxyObject;

        public string ServiceId => _serviceId;
        public string[] CacheKey => _cacheKey;

        public IDictionary<string, object> Arguments => _arguments;

        public List<Attribute> Attributes => _attributes;

        object IInvocation.ReturnValue { get => _returnValue; set => _returnValue = value; }

        public Type ReturnType => _returnType;

        public abstract Task Proceed();


        public void SetArgumentValue(int index, object value)
        {
            throw new NotImplementedException();
        }
    }
}