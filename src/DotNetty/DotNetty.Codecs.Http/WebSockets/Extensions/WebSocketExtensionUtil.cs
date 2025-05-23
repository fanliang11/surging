/*
 * Copyright 2012 The Netty Project
 *
 * The Netty Project licenses this file to you under the Apache License,
 * version 2.0 (the "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy of the License at:
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * Copyright (c) Microsoft. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com)
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs.Http.WebSockets.Extensions
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using DotNetty.Common.Internal;

    public static class WebSocketExtensionUtil
    {
        const char ExtensionSeparator = ',';
        const char ParameterSeparator = ';';
        const char ParameterEqual = '=';

        static readonly Regex Parameter = new Regex("^([^=]+)(=[\\\"]?([^\\\"]+)[\\\"]?)?$", RegexOptions.Compiled);

        internal static bool IsWebsocketUpgrade(HttpHeaders headers) =>
            headers.ContainsValue(HttpHeaderNames.Connection, HttpHeaderValues.Upgrade, true) 
                && headers.Contains(HttpHeaderNames.Upgrade, HttpHeaderValues.Websocket, true);

        public static List<WebSocketExtensionData> ExtractExtensions(string extensionHeader)
        {
            string[] rawExtensions = extensionHeader.Split(ExtensionSeparator);
            if (0u < (uint)rawExtensions.Length)
            {
                var extensions = new List<WebSocketExtensionData>(rawExtensions.Length);
                foreach (string rawExtension in rawExtensions)
                {
                    string[] extensionParameters = rawExtension.Split(ParameterSeparator);
                    string name = extensionParameters[0].Trim();
                    Dictionary<string, string> parameters;
                    if ((uint)extensionParameters.Length > 1u)
                    {
                        parameters = new Dictionary<string, string>(extensionParameters.Length - 1, System.StringComparer.Ordinal);
                        for (int i = 1; i < extensionParameters.Length; i++)
                        {
                            string parameter = extensionParameters[i].Trim();
                            
                            Match match = Parameter.Match(parameter);
                            if (match.Success)
                            {
                                parameters.Add(match.Groups[1].Value, match.Groups[3].Value);
                            }
                        }
                    }
                    else
                    {
                        parameters = new Dictionary<string, string>(System.StringComparer.Ordinal);
                    }
                    extensions.Add(new WebSocketExtensionData(name, parameters));
                }
                return extensions;
            }
            else
            {
                return new List<WebSocketExtensionData>();
            }
        }

        internal static string AppendExtension(string currentHeaderValue, string extensionName,
            Dictionary<string, string> extensionParameters)
        {
            var newHeaderValue = StringBuilderManager.Allocate(currentHeaderValue?.Length ?? extensionName.Length + 1);
            if (currentHeaderValue is object && currentHeaderValue.Trim() != string.Empty)
            {
                _ = newHeaderValue.Append(currentHeaderValue);
                _ = newHeaderValue.Append(ExtensionSeparator);
            }
            _ = newHeaderValue.Append(extensionName);
            foreach (KeyValuePair<string, string> extensionParameter in extensionParameters)
            {
                _ = newHeaderValue.Append(ParameterSeparator);
                _ = newHeaderValue.Append(extensionParameter.Key);
                if (extensionParameter.Value is object)
                {
                    _ = newHeaderValue.Append(ParameterEqual);
                    _ = newHeaderValue.Append(extensionParameter.Value);
                }
            }
            return StringBuilderManager.ReturnAndFree(newHeaderValue);
        }
    }
}
