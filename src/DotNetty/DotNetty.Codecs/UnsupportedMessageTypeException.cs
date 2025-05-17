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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com)
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs
{
    using System;
    using DotNetty.Common.Internal;

    /// <summary>
    ///     Thrown if an unsupported message is received by an codec.
    /// </summary>
    public class UnsupportedMessageTypeException : CodecException
    {
        public UnsupportedMessageTypeException(object message, params Type[] expectedTypes)
            : base(ComposeMessage(message?.GetType().Name ?? "null", expectedTypes))
        {
        }

        public UnsupportedMessageTypeException()
        {
        }

        public UnsupportedMessageTypeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public UnsupportedMessageTypeException(string message)
            : base(message)
        {
        }

        public UnsupportedMessageTypeException(Exception innerException)
            : base(innerException)
        {
        }

        static string ComposeMessage(string actualType, params Type[] expectedTypes)
        {
            var buf = StringBuilderManager.Allocate().Append(actualType);

            if (expectedTypes is object && (uint)expectedTypes.Length > 0u)
            {
                _ = buf.Append(" (expected: ").Append(expectedTypes[0].Name);
                for (int i = 1; i < expectedTypes.Length; i++)
                {
                    Type t = expectedTypes[i];
                    if (t is null)
                    {
                        break;
                    }
                    _ = buf.Append(", ").Append(t.Name);
                }
                _ = buf.Append(')');
            }

            return StringBuilderManager.ReturnAndFree(buf);
        }
    }
}