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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SkyWalking.NetworkProtocol;
using Surging.Apm.Skywalking.Abstractions.Transport;
using Microsoft.Extensions.Logging;
using Surging.Apm.Skywalking.Abstractions.Config;
using Surging.Apm.Skywalking.Transport.Grpc.Common;

namespace Surging.Apm.Skywalking.Transport.Grpc.V5
{
    internal class SegmentReporter : ISegmentReporter
    {
        private readonly ConnectionManager _connectionManager;
        private readonly ILogger _logger;
        private readonly GrpcConfig _config;

        public SegmentReporter(ConnectionManager connectionManager, IConfigAccessor configAccessor,
            ILoggerFactory loggerFactory)
        {
            _connectionManager = connectionManager;
            _config = configAccessor.Get<GrpcConfig>();
            _logger = loggerFactory.CreateLogger(typeof(SegmentReporter));
        }

        public async Task ReportAsync(IReadOnlyCollection<SegmentRequest> segmentRequests,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!_connectionManager.Ready)
            {
                return;
            }

            var connection = _connectionManager.GetConnection();

            try
            {
                var stopwatch = Stopwatch.StartNew();
                var client = new TraceSegmentService.TraceSegmentServiceClient(connection);
                using (var asyncClientStreamingCall =
                    client.collect(_config.GetMeta(), _config.GetReportTimeout(), cancellationToken))
                {
                    foreach (var segment in segmentRequests)
                        await asyncClientStreamingCall.RequestStream.WriteAsync(SegmentV5Helpers.Map(segment));
                    await asyncClientStreamingCall.RequestStream.CompleteAsync();
                    await asyncClientStreamingCall.ResponseAsync;
                }

                stopwatch.Stop();
                _logger.LogInformation($"Report {segmentRequests.Count} trace segment. cost: {stopwatch.Elapsed}s");
            }
            catch (Exception ex)
            {
                _logger.LogError("Report trace segment fail.", ex);
                _connectionManager.Failure(ex);
            }
        }
    }
}