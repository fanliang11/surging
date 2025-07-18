/*
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

using System;
using System.Threading;
using System.Threading.Tasks;
using Surging.Apm.Skywalking.Abstractions.Common;

namespace Surging.Apm.Skywalking.Abstractions.Transport
{
    public interface ISkyApmClientV5
    {
        Task<NullableValue> RegisterApplicationAsync(string applicationCode, CancellationToken cancellationToken = default(CancellationToken));

        Task<NullableValue> RegisterApplicationInstanceAsync(int applicationId, Guid agentUUID, long registerTime, AgentOsInfoRequest osInfoRequest,
            CancellationToken cancellationToken = default(CancellationToken));

        Task HeartbeatAsync(int applicationInstance, long heartbeatTime, CancellationToken cancellationToken = default(CancellationToken));
    }
}
