using Surging.Core.Protocol.Mqtt.Internal.Messages;
using System;
using System.Collections.Concurrent;

namespace Surging.Core.Protocol.Mqtt.Internal.Services.Implementation
{
    /// <summary>
    /// Defines the <see cref="ClientSessionService" />
    /// </summary>
    public class ClientSessionService : IClientSessionService
    {
        #region 字段

        /// <summary>
        /// Defines the _clientsessionMessages
        /// </summary>
        private readonly ConcurrentDictionary<String, ConcurrentQueue<SessionMessage>> _clientsessionMessages =
            new ConcurrentDictionary<String, ConcurrentQueue<SessionMessage>>();

        #endregion 字段

        #region 方法

        /// <summary>
        /// The GetMessages
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <returns>The <see cref="ConcurrentQueue{SessionMessage}"/></returns>
        public ConcurrentQueue<SessionMessage> GetMessages(string deviceId)
        {
            _clientsessionMessages.TryGetValue(deviceId, out ConcurrentQueue<SessionMessage> messages);
            return messages;
        }

        /// <summary>
        /// The SaveMessage
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <param name="sessionMessage">The sessionMessage<see cref="SessionMessage"/></param>
        public void SaveMessage(string deviceId, SessionMessage sessionMessage)
        {
            _clientsessionMessages.TryGetValue(deviceId, out ConcurrentQueue<SessionMessage> sessionMessages);
            _clientsessionMessages.AddOrUpdate(deviceId, sessionMessages, (id, message) =>
            {
                message.Enqueue(sessionMessage);
                return sessionMessages;
            });
        }

        #endregion 方法
    }
}