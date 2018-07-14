using Autofac;
using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.CPlatform.EventBus.Implementation;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.Protocol.WS.Runtime;
using Surging.Core.ProxyGenerator;
using System;
using WebSocketCore.Server;
using System.Linq;

namespace Surging.Core.Protocol.WS
{
   public abstract class WSServiceBase : WebSocketBehavior, IServiceBehavior
    { 
        public T CreateProxy<T>(string key) where T : class
        {
            return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy<T>(key);
        }
         
        public object CreateProxy(Type type)
        {
            return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(type);
        }
         
        public object CreateProxy(string key, Type type)
        {
            return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(key, type);
        }
         
        public T CreateProxy<T>() where T : class
        {
            return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy<T>();
        }

        public  T GetService<T>(string key) where T : class
        {
            if (ServiceLocator.Current.IsRegisteredWithKey<T>(key))
                return ServiceLocator.GetService<T>(key);
            else
                return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy<T>(key);
        }

        public   T GetService<T>() where T : class
        {
            if (ServiceLocator.Current.IsRegistered<T>())
                return ServiceLocator.GetService<T>();
            else
                return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy<T>();

        }

        public   object GetService(Type type)
        {
            if (ServiceLocator.Current.IsRegistered(type))
                return ServiceLocator.GetService(type);
            else
                return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(type);
        }

        public   object GetService(string key, Type type)
        {
            if (ServiceLocator.Current.IsRegisteredWithKey(key, type))
                return ServiceLocator.GetService(key, type);
            else
                return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(key, type);

        }

        public WebSocketSessionManager GetClient()
        {
            WebSocketSessionManager result = null;
            var server = ServiceLocator.GetService<DefaultWSServerMessageListener>().Server;
            var entries = ServiceLocator.GetService<IWSServiceEntryProvider>().GetEntries();
            var entry = entries.Where(p => p.Type == this.GetType()).FirstOrDefault();
            if (server.WebSocketServices.TryGetServiceHost(entry.Path, out WebSocketServiceHostBase webSocketServiceHost))
                result = webSocketServiceHost.Sessions;
            return result;
        }

        public void Publish(IntegrationEvent @event)
        {
            GetService<IEventBus>().Publish(@event);
        }
    }
}
