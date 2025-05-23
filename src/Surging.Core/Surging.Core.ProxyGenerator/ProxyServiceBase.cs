using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Utilities;
using System;
using Autofac;
using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.CPlatform.EventBus.Implementation;
using Surging.Core.CPlatform.DependencyResolution;

namespace Surging.Core.ProxyGenerator
{
    public abstract class ProxyServiceBase:  ServiceBase
    {
        [Obsolete("This method is Obsolete, use GetService")]
        public T CreateProxy<T>(string key) where T : class
        {
           // return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy<T>(key);
            var result = ServiceResolver.Current.GetService<T>(key);
            if (result == null)
            {
                result = ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy<T>(key);
                ServiceResolver.Current.Register(key, result);

            }
            return result;
        }

        [Obsolete("This method is Obsolete, use GetService")]
        public object CreateProxy(Type type)
        {
            var result = ServiceResolver.Current.GetService(type);
            if (result == null)
            {
                result = ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(type);
                ServiceResolver.Current.Register(null, result);

            }
            return result;
        }

        [Obsolete("This method is Obsolete, use GetService")]
        public object CreateProxy(string key, Type type)
        {
            var result = ServiceResolver.Current.GetService(type,key);
            if (result == null)
            {
                result = ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(key,type);
                ServiceResolver.Current.Register(key, result);
            }
            return result;
        }

        [Obsolete("This method is Obsolete, use GetService")]
        public T CreateProxy<T>() where T : class
        {
            var result = ServiceResolver.Current.GetService<T>();
            if (result == null)
            {
                result = ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy<T>();
                ServiceResolver.Current.Register(null,result);
            }
            return result;
        }

        public override T GetService<T>(string key) 
        {
            if (ServiceLocator.Current.IsRegisteredWithKey<T>(key))
                return base.GetService<T>(key);
            else
            {
                var result = ServiceResolver.Current.GetService<T>(key);
                if (result == null)
                {
                    result = ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy<T>(key);
                    ServiceResolver.Current.Register(key, result);
                }
                return result;
            }
        }

        public override T GetService<T>()
        {
            if (ServiceLocator.Current.IsRegistered<T>())
                return base.GetService<T>();
            else
            {
                var result = ServiceResolver.Current.GetService<T>();
                if (result == null)
                {
                    result = ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy<T>();
                    ServiceResolver.Current.Register(null, result);
                }
                return result;
            }

        }

        public override object GetService(Type type)
        {
            if (ServiceLocator.Current.IsRegistered(type))
                return base.GetService(type);
            else
            {
                var result = ServiceResolver.Current.GetService(type);
                if (result == null)
                {
                    result = ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(type);
                    ServiceResolver.Current.Register(null, result); 
                }
                return result;
            }
        }

        public override object GetService(string key, Type type)
        {
            if (ServiceLocator.Current.IsRegisteredWithKey(key, type))
                return base.GetService(key, type);
            else
            {
                var result = ServiceResolver.Current.GetService(type, key);
                if (result == null)
                {
                    result = ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(key, type);
                    ServiceResolver.Current.Register(key, result);
                }
                return result;
            }
           
        }

        public void Publish(IntegrationEvent @event)
        {
            GetService<IEventBus>().Publish(@event);
        }
    }
}
