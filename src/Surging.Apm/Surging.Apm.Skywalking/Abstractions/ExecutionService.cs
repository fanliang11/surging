﻿/*
 * Licensed to the Surging.Apm.Skywalking.Abstractions under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The Surging.Apm.Skywalking.Abstractions licenses this file to You under the Apache License, Version 2.0
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

using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Apm.Skywalking.Abstractions
{
    public abstract class ExecutionService : IExecutionService, IDisposable
    {
        private Timer _timer;
        private CancellationTokenSource _cancellationTokenSource;

        protected readonly ILogger Logger;
        protected readonly IRuntimeEnvironment RuntimeEnvironment;

        protected ExecutionService(IRuntimeEnvironment runtimeEnvironment, ILoggerFactory loggerFactory)
        {
            RuntimeEnvironment = runtimeEnvironment;
            Logger = loggerFactory.CreateLogger(GetType());
        }

        public Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var source = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, cancellationToken);
            _timer = new Timer(Callback, source, DueTime, Period);
            Logger.LogInformation($"Loaded instrument service [{GetType().FullName}].");
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _cancellationTokenSource?.Cancel();
            await Stopping(cancellationToken);
            Logger.LogInformation($"Stopped instrument service {GetType().Name}.");
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        private async void Callback(object state)
        {
            if (!(state is CancellationTokenSource token) || token.IsCancellationRequested || !CanExecute()) return;

            try
            {
                await ExecuteAsync(token.Token);
            }
            catch (Exception ex)
            {
                Logger.LogError(GetType().FullName + ".ExecuteAsync(token.Token) fail", ex);
            }
        }

        protected virtual bool CanExecute() => RuntimeEnvironment.Initialized;

        protected virtual Task Stopping(CancellationToken cancellationToke) => Task.CompletedTask;

        protected abstract TimeSpan DueTime { get; }

        protected abstract TimeSpan Period { get; }

        protected abstract Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
