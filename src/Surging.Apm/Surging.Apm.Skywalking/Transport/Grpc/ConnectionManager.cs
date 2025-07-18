/*
 * Licensed to the SkyAPM under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The SkyAPM licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Surging.Apm.Skywalking.Abstractions.Config;
using Surging.Apm.Skywalking.Transport.Grpc.Common;

namespace Surging.Apm.Skywalking.Transport.Grpc
{
    public class ConnectionManager
    {
        private readonly Random _random = new Random();
        private readonly AsyncLock _lock = new AsyncLock();

        private readonly ILogger _logger;
        private readonly GrpcConfig _config;

        private volatile Channel _channel;
        private volatile ConnectionState _state;
        private volatile string _server;

        public bool Ready => _channel != null && _state == ConnectionState.Connected && _channel.State == ChannelState.Ready;

        public ConnectionManager(ILoggerFactory loggerFactory, IConfigAccessor configAccessor)
        {
            _logger = loggerFactory.CreateLogger(typeof(ConnectionManager));
            _config = configAccessor.Get<GrpcConfig>();
        }

        public async Task ConnectAsync()
        {
            using (await _lock.LockAsync())
            {
                if (Ready)
                {
                    return;
                }

                if (_channel != null)
                {
                    await ShutdownAsync();
                }

                EnsureServerAddress();

                _channel = new Channel(_server, ChannelCredentials.Insecure);

                try
                {
                    await _channel.ConnectAsync(_config.GetConnectTimeout());
                    _state = ConnectionState.Connected;
                    _logger.LogInformation($"Connected server[{_channel.Target}].");
                }
                catch (TaskCanceledException ex)
                {
                    _state = ConnectionState.Failure;
                    _logger.LogError($"Connect server timeout.", ex);
                }
                catch (Exception ex)
                {
                    _state = ConnectionState.Failure;
                    _logger.LogError($"Connect server fail.", ex);
                }
            }
        }

        public async Task ShutdownAsync()
        {
            try
            {
                await _channel?.ShutdownAsync();
                _logger.LogInformation($"Shutdown connection[{_channel.Target}].");
            }
            catch (Exception e)
            {
                _logger.LogError($"Shutdown connection fail.", e);
            }
            finally
            {
                _state = ConnectionState.Failure;
            }
        }

        public void Failure(Exception exception)
        {
            var currentState = _state;

            if (ConnectionState.Connected == currentState)
            {
                _logger.LogWarning($"Connection state changed. {_state} -> {_channel.State} . {exception.Message}");
            }

            _state = ConnectionState.Failure;
        }

        public Channel GetConnection()
        {
            if (Ready) return _channel;
            _logger.LogDebug("Not found available gRPC connection.");
            return null;
        }

        private void EnsureServerAddress()
        {
            var servers = _config.Servers.Split(',').ToArray();
            if (servers.Length == 1)
            {
                _server = servers[0];
                return;
            }

            if (_server != null)
            {
                servers = servers.Where(x => x != _server).ToArray();
            }

            var index = _random.Next() % servers.Length;
            _server = servers[index];
        }
    }

    public enum ConnectionState
    {
        Idle,
        Connecting,
        Connected,
        Failure
    }
}