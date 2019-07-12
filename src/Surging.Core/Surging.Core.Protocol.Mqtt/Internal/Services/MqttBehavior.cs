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
    /// <summary>
    /// Defines the <see cref="MqttBehavior" />
    /// </summary>
    public abstract class MqttBehavior : ServiceBase
    {
        #region 方法

        /// <summary>
        /// The Authorized
        /// </summary>
        /// <param name="username">The username<see cref="string"/></param>
        /// <param name="password">The password<see cref="string"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
        public abstract Task<bool> Authorized(string username, string password);

        /// <summary>
        /// The GetDeviceIsOnine
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
        public async Task<bool> GetDeviceIsOnine(string deviceId)
        {
            return await this.GetService<IChannelService>().GetDeviceIsOnine(deviceId);
        }

        /// <summary>
        /// The GetService
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="T"/></returns>
        public override T GetService<T>()
        {
            if (ServiceLocator.Current.IsRegistered<T>())
                return base.GetService<T>();
            else
                return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy<T>();
        }

        /// <summary>
        /// The GetService
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key<see cref="string"/></param>
        /// <returns>The <see cref="T"/></returns>
        public override T GetService<T>(string key)
        {
            if (ServiceLocator.Current.IsRegisteredWithKey<T>(key))
                return base.GetService<T>(key);
            else
                return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy<T>(key);
        }

        /// <summary>
        /// The GetService
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="object"/></returns>
        public override object GetService(string key, Type type)
        {
            if (ServiceLocator.Current.IsRegisteredWithKey(key, type))
                return base.GetService(key, type);
            else
                return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(key, type);
        }

        /// <summary>
        /// The GetService
        /// </summary>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="object"/></returns>
        public override object GetService(Type type)
        {
            if (ServiceLocator.Current.IsRegistered(type))
                return base.GetService(type);
            else
                return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(type);
        }

        /// <summary>
        /// The Publish
        /// </summary>
        /// <param name="@event">The event<see cref="IntegrationEvent"/></param>
        public void Publish(IntegrationEvent @event)
        {
            GetService<IEventBus>().Publish(@event);
        }

        /// <summary>
        /// The Publish
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <param name="willMessage">The willMessage<see cref="MqttWillMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task Publish(string deviceId, MqttWillMessage willMessage)
        {
            await GetService<IChannelService>().Publish(deviceId, willMessage);
        }

        /// <summary>
        /// The RemotePublish
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <param name="willMessage">The willMessage<see cref="MqttWillMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task RemotePublish(string deviceId, MqttWillMessage willMessage)
        {
            await GetService<IChannelService>().RemotePublishMessage(deviceId, willMessage);
        }

        #endregion 方法
    }
}