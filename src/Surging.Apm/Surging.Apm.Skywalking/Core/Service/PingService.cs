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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Surging.Apm.Skywalking.Abstractions;
using Surging.Apm.Skywalking.Abstractions.Config;
using Surging.Apm.Skywalking.Abstractions.Transport;

namespace Surging.Apm.Skywalking.Core.Service
{
    public class PingService : ExecutionService
    {
        private readonly IPingCaller _pingCaller;
        private readonly TransportConfig _transportConfig;

        public PingService(IConfigAccessor configAccessor, IPingCaller pingCaller,
            IRuntimeEnvironment runtimeEnvironment,
            ILoggerFactory loggerFactory) : base(
            runtimeEnvironment, loggerFactory)
        {
            _pingCaller = pingCaller;
            _transportConfig = configAccessor.Get<TransportConfig>();
        }

        protected override bool CanExecute() =>
            _transportConfig.ProtocolVersion == ProtocolVersions.V6 && base.CanExecute();

        protected override TimeSpan DueTime { get; } = TimeSpan.FromSeconds(30);
        protected override TimeSpan Period { get; } = TimeSpan.FromSeconds(60);

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _pingCaller.PingAsync(
                    new PingRequest
                    {
                        ServiceInstanceId = RuntimeEnvironment.ServiceInstanceId.Value,
                        InstanceId = RuntimeEnvironment.InstanceId.ToString("N")
                    }, cancellationToken);
                Logger.LogInformation($"Ping server @{DateTimeOffset.UtcNow}");
            }
            catch (Exception exception)
            {
                Logger.LogError($"Ping server fail @{DateTimeOffset.UtcNow}", exception);
            }
        }
    }
}