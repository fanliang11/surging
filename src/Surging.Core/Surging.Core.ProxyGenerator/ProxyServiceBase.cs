using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Utilities;
using System;
using Autofac;
using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.CPlatform.EventBus.Implementation;

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
           if( ServiceLocator.Current.IsRegisteredWithKey<T>(key))
                 return  base.GetService<T>(key); 
            else 
                 return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy<T>(key);
        }

        public override T GetService<T>()
        {
            if (ServiceLocator.Current.IsRegistered<T>())
                return base.GetService<T>();
            else 
              return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy<T>();

        }

        public override object GetService(Type type)
        {
            if (ServiceLocator.Current.IsRegistered(type))
               return base.GetService(type);
             else
                return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(type);
        }

        public override object GetService(string key, Type type)
        {
            if (ServiceLocator.Current.IsRegisteredWithKey(key,type))
                return base.GetService(key,type);
            else
                return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(key,type);
           
        }

        public void Publish(IntegrationEvent @event)
        {
            GetService<IEventBus>().Publish(@event);
        }
    }
}
