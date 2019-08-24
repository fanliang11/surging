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

namespace Surging.Core.CPlatform.Diagnostics
{
    public static class Tags
    {
        public static readonly string RPC_METHOD = "rpc.method"; 
        public static readonly string RPC_PARAMETERS = "rpc.parameters";
        public static readonly string RPC_LOCAL_ADDRESS = "rpc.local.address";
        public static readonly string REST_METHOD = "rest.method";
        public static readonly string REST_PARAMETERS = "rpc.parameters";
        public static readonly string REST_LOCAL_ADDRESS = "rest.local.address";
        public static readonly string MQTT_METHOD = "method";
        public static readonly string MQTT_CLIENT_ID = "mqtt.client.id";
        public static readonly string MQTT_PARAMETERS = "mqtt.parameters";
        public static readonly string MQTT_BROKER_ADDRESS = "mqtt.broker.address";
    }
}