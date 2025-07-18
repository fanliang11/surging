using Autofac;
using Surging.Core.ApiGateWay.OAuth;
using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.CPlatform.EventBus.Implementation;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.Protocol.WebService.Runtime.Implementation;
using Surging.Core.ProxyGenerator;
using System.Xml;

namespace Surging.Core.Protocol.WebService.Runtime
{
    public abstract class WebServiceBehavior : IServiceBehavior
    {
        private HeaderValue _headerValue;

        public WebServiceBehavior()
        {
            _headerValue = new HeaderValue();
        }

        public HeaderValue HeaderValue
        {
            get { return _headerValue; }
        }
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



        public string MessageId { get; } = Guid.NewGuid().ToString("N");


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

        public void ParseHeaderFromBody(Stream body)
        {
            try
            {
                body.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(body);
                var xml = reader.ReadToEndAsync().GetAwaiter().GetResult();
                if (string.IsNullOrEmpty(xml))
                {
                    return;
                }
                var envelope = new XmlDocument();
                envelope.LoadXml(xml);
                var node = envelope.DocumentElement?.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.LocalName == "Header")?.FirstChild;
                if (node == null)
                {
                    return;
                }
                var cns = node.ChildNodes.Cast<XmlNode>();
                _headerValue.Token = cns.FirstOrDefault(d => d.LocalName == "Token")?.InnerText;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> ValidateAuthentication(string token)
        {
            return await GetService<IAuthorizationServerProvider>().ValidateClientAuthentication(token);
        }

        public void Publish(IntegrationEvent @event)
        {
            GetService<IEventBus>().Publish(@event);
        }

    }
}

