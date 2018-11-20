using Surging.Core.Protocol.Mqtt.Internal.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Services
{
   public interface IClientSessionService
    {
        void SaveMessage(string deviceId, SessionMessage sessionMessage);

        ConcurrentQueue<SessionMessage> GetMessages(string deviceId);
    }
}
