using Autofac;
using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.CPlatform.EventBus.Implementation;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.Protocol.Mqtt.Internal.Enums;
using Surging.Core.Protocol.Mqtt.Internal.Messages;
using Surging.Core.ProxyGenerator;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Internal.Services
{
    public abstract class MqttBehavior: ServiceBase
    {
        public async Task Publish(string deviceId, MqttWillMessage willMessage)
        {
            await GetService<IChannelService>().Publish(deviceId, willMessage);
        }

        public async Task RemotePublish(string deviceId, MqttWillMessage willMessage)
        {
            await GetService<IChannelService>().RemotePublishMessage(deviceId, willMessage);
        }

        public override T GetService<T>(string key)
        {
            if (ServiceLocator.Current.IsRegisteredWithKey<T>(key))
                return base.GetService<T>(key);
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
            if (ServiceLocator.Current.IsRegisteredWithKey(key, type))
                return base.GetService(key, type);
            else
                return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(key, type);

        }

        public void Publish(IntegrationEvent @event)
        {
            GetService<IEventBus>().Publish(@event);
        }

        public async Task<bool> GetDeviceIsOnine(string deviceId)
        {
           return  await this.GetService<IChannelService>().GetDeviceIsOnine(deviceId);
        }

        public abstract Task<bool> Authorized(string username, string password);
    }
}
