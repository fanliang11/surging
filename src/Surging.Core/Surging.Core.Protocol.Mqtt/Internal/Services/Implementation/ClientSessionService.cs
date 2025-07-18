﻿using Surging.Core.Protocol.Mqtt.Internal.Messages;
using System;
using System.Collections.Concurrent;

namespace Surging.Core.Protocol.Mqtt.Internal.Services.Implementation
{
    public class ClientSessionService: IClientSessionService
    {
        private  readonly ConcurrentDictionary<String, ConcurrentQueue<SessionMessage>> _clientsessionMessages = 
            new ConcurrentDictionary<String, ConcurrentQueue<SessionMessage>>();

        public ConcurrentQueue<SessionMessage> GetMessages(string deviceId)
        {
            _clientsessionMessages.TryGetValue(deviceId, out ConcurrentQueue<SessionMessage> messages);
            return messages;
        }

        public void SaveMessage(string deviceId, SessionMessage sessionMessage)
        {
            _clientsessionMessages.TryGetValue(deviceId, out ConcurrentQueue<SessionMessage> sessionMessages);
            _clientsessionMessages.AddOrUpdate(deviceId, sessionMessages, (id, message) =>
            {
                if (message == null) message = new ConcurrentQueue<SessionMessage>();
                message.Enqueue(sessionMessage);
                return message;
            });
        }
    }
}
