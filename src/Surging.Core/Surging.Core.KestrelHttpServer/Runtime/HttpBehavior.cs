using Autofac;
using Microsoft.AspNetCore.Http;
using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.CPlatform.EventBus.Implementation;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.KestrelHttpServer.Interceptors;
using Surging.Core.ProxyGenerator;
using Surging.Core.ProxyGenerator.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.KestrelHttpServer.Runtime
{
    public abstract  class HttpBehavior : IServiceBehavior
    {
        private ServerReceivedDelegate _received;
        public event ServerReceivedDelegate Received
        {
            add
            {

                _received += value;
            }
            remove
            {
                _received -= value;
            }
        }

        public abstract Task<bool> Load(HttpServiceContext context);

        public Task<bool>  CallInvoke(IHttpInvocation invocation)
        {
            var context=new HttpServiceContext() { 
              Context= invocation.Context,
               Path= invocation.Path,
                Result= invocation.Result,
            };
           return Load(context);
        }
        public ISubject<string> NetworkId { get; internal set; } = new ReplaySubject<string>();
        public string MessageId { get; internal set; } = Guid.NewGuid().ToString("N");

        public IHttpMessageSender Sender { get; set; }

        public async Task Write(object result, int statusCode = 200, string exceptionMessage = "")
        {
            if (_received == null)
                return;
            var message = new TransportMessage(MessageId, new ReactiveResultMessage
            {
                ExceptionMessage = exceptionMessage,
                StatusCode = statusCode,
                Result = result

            });
            await _received(message);
        }
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

    }
}

