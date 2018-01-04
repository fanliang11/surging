using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Utilities;
using System;

namespace Surging.Core.ProxyGenerator
{
    public abstract class ProxyServiceBase:  ServiceBase
    {
        [Obsolete("This method is Obsolete, use GetService")]
        public T CreateProxy<T>(string key) where T : class
        {
            return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy<T>(key);
        }

        [Obsolete("This method is Obsolete, use GetService")]
        public object CreateProxy(Type type)
        {
            return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(type);
        }

        [Obsolete("This method is Obsolete, use GetService")]
        public object CreateProxy(string key, Type type)
        {
            return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(key, type);
        }

        [Obsolete("This method is Obsolete, use GetService")]
        public T CreateProxy<T>() where T : class
        {
            return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy<T>();
        }

        public override T GetService<T>(string key) 
        {
     
            var result = base.GetService<T>(key);
            if(result==null)
            {
                  result = ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy<T>(key);
            }
            return result;
        }

        public override T GetService<T>()
        {
            var result = base.GetService<T>();
            if (result == null)
            {
                result = ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy<T>();
            }
            return result;
        }

        public override object GetService(Type type)
        {
            var result = base.GetService(type);
            if (result == null)
            {
                result = ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(type);
            }
            return result;
        }

        public override object GetService(string key, Type type)
        {
            var result = base.GetService(type);
            if (result == null)
            {
                result = ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(key,type);
            }
            return result;
        }
    }
}
