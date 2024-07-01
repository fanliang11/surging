using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Ioc
{
    public abstract class ServiceBase: IServiceBehavior
    {
        private ServerReceivedDelegate received;
        public event ServerReceivedDelegate Received
        {
            add
            {
                if (received == null)
                {
                    received += value;
                }
            }
            remove
            {
                received -= value;
            }
        }

        public string MessageId { get; } = Guid.NewGuid().ToString("N");
        public async Task Write(object result, int statusCode = 200, string exceptionMessage = "")
        {
            if (received == null)
                return;
            var message = new TransportMessage(MessageId, new ReactiveResultMessage
            {
                ExceptionMessage = exceptionMessage,
                StatusCode = statusCode,
                Result = result

            });
            await received(message);
        }
        public virtual T GetService<T>() where T : class
        {
            return ServiceLocator.GetService<T>();
        }

        public virtual T GetService<T>(string key) where T : class
        {
            return ServiceLocator.GetService<T>(key);
        }

        public virtual object GetService(Type type)
        {
            return ServiceLocator.GetService(type);
        }

        public virtual object GetService(string key, Type type)
        {
            return ServiceLocator.GetService(key, type);
        }
    }
}
