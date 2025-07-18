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

namespace Surging.Apm.Skywalking.Abstractions.Config
{
    [Config("SkyWalking")]
    public class InstrumentConfig
    {
        public string Namespace { get; set; }

        [Obsolete("Use ServiceName.")]
        public string ApplicationCode { get; set; }
        
        public string ServiceName { get; set; }
        
        public string[] HeaderVersions { get; set; }
    }

    public static class HeaderVersions
    {
        public static string SW3 { get; } = "sw3";
        
        public static string SW6 { get; } = "sw6";
    }
}