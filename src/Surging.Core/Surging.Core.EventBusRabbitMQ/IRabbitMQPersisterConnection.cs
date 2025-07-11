using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.EventBusRabbitMQ
{
    public interface IRabbitMQPersistentConnection
         : IDisposable
    {
        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateModel();

        event EventHandler<ShutdownEventArgs> OnRabbitConnectionShutdown;
    }
}