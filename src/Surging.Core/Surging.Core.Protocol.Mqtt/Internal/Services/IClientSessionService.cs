using Surging.Core.Protocol.Mqtt.Internal.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Services
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IClientSessionService" />
    /// </summary>
    public interface IClientSessionService
    {
        #region 方法

        /// <summary>
        /// The GetMessages
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <returns>The <see cref="ConcurrentQueue{SessionMessage}"/></returns>
        ConcurrentQueue<SessionMessage> GetMessages(string deviceId);

        /// <summary>
        /// The SaveMessage
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <param name="sessionMessage">The sessionMessage<see cref="SessionMessage"/></param>
        void SaveMessage(string deviceId, SessionMessage sessionMessage);

        #endregion 方法
    }

    #endregion 接口
}