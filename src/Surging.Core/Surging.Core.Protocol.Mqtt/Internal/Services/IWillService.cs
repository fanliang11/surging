using Surging.Core.Protocol.Mqtt.Internal.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Internal.Services
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IWillService" />
    /// </summary>
    public interface IWillService
    {
        #region 方法

        /// <summary>
        /// The Add
        /// </summary>
        /// <param name="deviceid">The deviceid<see cref="string"/></param>
        /// <param name="willMessage">The willMessage<see cref="MqttWillMessage"/></param>
        void Add(string deviceid, MqttWillMessage willMessage);

        /// <summary>
        /// The Remove
        /// </summary>
        /// <param name="deviceid">The deviceid<see cref="string"/></param>
        void Remove(string deviceid);

        /// <summary>
        /// The SendWillMessage
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task SendWillMessage(string deviceId);

        #endregion 方法
    }

    #endregion 接口
}