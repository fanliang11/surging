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
    public class SegmentReportService : ExecutionService
    {
        private readonly TransportConfig _config;
        private readonly ISegmentDispatcher _dispatcher;

        public SegmentReportService(IConfigAccessor configAccessor, ISegmentDispatcher dispatcher,
            IRuntimeEnvironment runtimeEnvironment, ILoggerFactory loggerFactory)
            : base(runtimeEnvironment, loggerFactory)
        {
            _dispatcher = dispatcher;
            _config = configAccessor.Get<TransportConfig>();
            Period = TimeSpan.FromMilliseconds(_config.Interval);
        }

        protected override TimeSpan DueTime { get; } = TimeSpan.FromSeconds(3);

        protected override TimeSpan Period { get; }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return _dispatcher.Flush(cancellationToken);
        }

        protected override Task Stopping(CancellationToken cancellationToke)
        {
            _dispatcher.Close();
            return Task.CompletedTask;
        }
    }
}