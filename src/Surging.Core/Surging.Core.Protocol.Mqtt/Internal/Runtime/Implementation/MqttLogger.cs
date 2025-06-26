using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Internal.Runtime.Implementation
{
    public class MqttLogger : ILogger<DotNettyMqttServerMessageListener>
    {

        private readonly ISubject<NetworkLogMessage> _subject;
        private readonly string _id;
        public MqttLogger()
        {
        }
        public MqttLogger(ISubject<NetworkLogMessage> subject, string id)
        {
            _subject = subject;
            _id = id;
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var i = 0;
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }
            string? message = null;
            if (null != formatter)
            {
                message = formatter(state, exception);
            }
            _subject.OnNext(new NetworkLogMessage
            {
                Content = message,
                CreateDate = DateTime.UtcNow,
                EventName = eventId.Name,
                NetworkType = NetworkType.Tcp,
                Id = _id,
                logLevel = logLevel,
            });
        }
    }
}

