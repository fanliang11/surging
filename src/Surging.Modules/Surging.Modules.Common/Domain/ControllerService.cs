using Surging.Core.CPlatform.Utilities;
using Surging.Core.Protocol.Mqtt.Internal.Enums;
using Surging.Core.Protocol.Mqtt.Internal.Messages;
using Surging.Core.Protocol.Mqtt.Internal.Services;
using Surging.Core.ProxyGenerator;
using Surging.IModuleServices.Common;
using Surging.IModuleServices.Common.Models;
using Surging.IModuleServices.User;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Modules.Common.Domain
{
    /// <summary>
    /// Defines the <see cref="ControllerService" />
    /// </summary>
    public class ControllerService : MqttBehavior, IControllerService
    {
        #region 方法

        /// <summary>
        /// The Authorized
        /// </summary>
        /// <param name="username">The username<see cref="string"/></param>
        /// <param name="password">The password<see cref="string"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
        public override async Task<bool> Authorized(string username, string password)
        {
            bool result = false;
            if (username == "admin" && password == "123456")
                result = true;
            return await Task.FromResult(result);
        }

        /// <summary>
        /// The IsOnline
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
        public async Task<bool> IsOnline(string deviceId)
        {
            var text = await GetService<IManagerService>().SayHello("fanly");
            return await base.GetDeviceIsOnine(deviceId);
        }

        /// <summary>
        /// The Publish
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <param name="message">The message<see cref="WillMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task Publish(string deviceId, WillMessage message)
        {
            var willMessage = new MqttWillMessage
            {
                WillMessage = message.Message,
                Qos = message.Qos,
                Topic = message.Topic,
                WillRetain = message.WillRetain
            };
            await Publish(deviceId, willMessage);
            await RemotePublish(deviceId, willMessage);
        }

        #endregion 方法
    }
}