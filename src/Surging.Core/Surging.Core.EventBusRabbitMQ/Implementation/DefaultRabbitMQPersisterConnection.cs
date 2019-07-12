using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Surging.Core.CPlatform.EventBus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Surging.Core.EventBusRabbitMQ.Implementation
{
    /// <summary>
    /// Defines the <see cref="DefaultRabbitMQPersistentConnection" />
    /// </summary>
    public class DefaultRabbitMQPersistentConnection
       : IRabbitMQPersistentConnection
    {
        #region 字段

        /// <summary>
        /// Defines the _connectionFactory
        /// </summary>
        private readonly IConnectionFactory _connectionFactory;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<DefaultRabbitMQPersistentConnection> _logger;

        /// <summary>
        /// Defines the _connection
        /// </summary>
        internal IConnection _connection;

        /// <summary>
        /// Defines the _disposed
        /// </summary>
        internal bool _disposed;

        /// <summary>
        /// Defines the sync_root
        /// </summary>
        internal object sync_root = new object();

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRabbitMQPersistentConnection"/> class.
        /// </summary>
        /// <param name="connectionFactory">The connectionFactory<see cref="IConnectionFactory"/></param>
        /// <param name="logger">The logger<see cref="ILogger{DefaultRabbitMQPersistentConnection}"/></param>
        public DefaultRabbitMQPersistentConnection(IConnectionFactory connectionFactory,
            ILogger<DefaultRabbitMQPersistentConnection> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion 构造函数

        #region 事件

        /// <summary>
        /// Defines the OnRabbitConnectionShutdown
        /// </summary>
        public event EventHandler<ShutdownEventArgs> OnRabbitConnectionShutdown;

        #endregion 事件

        #region 属性

        /// <summary>
        /// Gets a value indicating whether IsConnected
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return _connection != null && _connection.IsOpen && !_disposed;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The CreateModel
        /// </summary>
        /// <returns>The <see cref="IModel"/></returns>
        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
            }

            return _connection.CreateModel();
        }

        /// <summary>
        /// The Dispose
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            try
            {
                _connection.Dispose();
            }
            catch (IOException ex)
            {
                _logger.LogCritical(ex.ToString());
            }
        }

        /// <summary>
        /// The TryConnect
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        public bool TryConnect()
        {
            _logger.LogInformation("RabbitMQ Client is trying to connect");

            lock (sync_root)
            {
                var policy = RetryPolicy.Handle<SocketException>()
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                    {
                        _logger.LogWarning(ex.ToString());
                    }
                );

                policy.Execute(() =>
                {
                    _connection = _connectionFactory
                          .CreateConnection();
                });

                if (IsConnected)
                {
                    _connection.ConnectionShutdown += OnConnectionShutdown;
                    _connection.CallbackException += OnCallbackException;
                    _connection.ConnectionBlocked += OnConnectionBlocked;

                    _logger.LogInformation($"RabbitMQ persistent connection acquired a connection {_connection.Endpoint.HostName} and is subscribed to failure events");

                    return true;
                }
                else
                {
                    _logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");
                    return false;
                }
            }
        }

        /// <summary>
        /// The OnCallbackException
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="CallbackExceptionEventArgs"/></param>
        internal void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (_disposed) return;

            _logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");

            TryConnect();
        }

        /// <summary>
        /// The OnConnectionShutdown
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="reason">The reason<see cref="ShutdownEventArgs"/></param>
        internal void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (_disposed) return;

            _logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");
            OnRabbitConnectionShutdown(sender, reason);
            TryConnect();
        }

        /// <summary>
        /// The OnConnectionBlocked
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="ConnectionBlockedEventArgs"/></param>
        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (_disposed) return;

            _logger.LogWarning("A RabbitMQ connection is shutdown. Trying to re-connect...");

            TryConnect();
        }

        #endregion 方法
    }
}