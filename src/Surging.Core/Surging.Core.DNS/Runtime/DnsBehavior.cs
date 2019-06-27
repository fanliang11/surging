using Org.BouncyCastle.Asn1.Ocsp;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.ProxyGenerator;
using System;
using System.Collections.Generic;
using System.Net;
using Surging.Core.CPlatform.Utilities;
using ServiceLocator = Surging.Core.CPlatform.Utilities.ServiceLocator;
using Autofac;
using Surging.Core.CPlatform.EventBus.Implementation;
using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.CPlatform.Module;
using System.Threading.Tasks;

namespace Surging.Core.DNS.Runtime
{
    public abstract class DnsBehavior : IServiceBehavior
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

        public T GetService<T>(string key) where T : class
        {
            if (ServiceLocator.Current.IsRegisteredWithKey<T>(key))
                return ServiceLocator.GetService<T>(key);
            else
                return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy<T>(key);
        }

        public T GetService<T>() where T : class
        {
            if (ServiceLocator.Current.IsRegistered<T>())
                return ServiceLocator.GetService<T>();
            else
                return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy<T>();

        }

        public object GetService(Type type)
        {
            if (ServiceLocator.Current.IsRegistered(type))
                return ServiceLocator.GetService(type);
            else
                return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(type);
        }

        public object GetService(string key, Type type)
        {
            if (ServiceLocator.Current.IsRegisteredWithKey(key, type))
                return ServiceLocator.GetService(key, type);
            else
                return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(key, type);

        }

        public void Publish(IntegrationEvent @event)
        {
            GetService<IEventBus>().Publish(@event);
        }

        public abstract Task<IPAddress> Resolve(string domainName);


        internal async Task<IPAddress> DomainResolve(string domainName)
        {
            domainName = domainName.TrimEnd('.');
            var prefixLen = domainName.IndexOf(".");
            IPAddress result = null;
            if (prefixLen > 0)
            { 
                var prefixName = domainName.Substring(0, prefixLen).ToString();
                var  pathLen= domainName.LastIndexOf(".") - prefixLen - 1;
                if (pathLen > 0)
                {
                    var routePath = domainName.Substring(prefixLen + 1, pathLen).Replace(".", "/").ToString();
                    if (routePath.IndexOf("/") < 0 && routePath[0] != '/')
                        routePath = $"/{routePath}";
                    var address = await GetService<IEchoService>().Locate(prefixName, routePath);
                    if (!string.IsNullOrEmpty(address.WanIp))
                        result = IPAddress.Parse(address.WanIp);
                }
            }
            result = await Resolve(domainName) ?? result;
            return result;
        }
    }
}
