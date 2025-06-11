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

namespace DotNetty.Codecs
{
    using System;
    using System.Collections.Generic;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;

    /// <summary>
    /// Provides utility methods related to <see cref="IHeaders{TKey, TValue}"/>.
    /// </summary>
    public static class HeadersUtils
    {
        /// <summary>
        /// <see cref="IHeaders{TKey, TValue}.GetAll(TKey)"/> and convert each element of <see cref="List{TValue}"/> to a <see cref="String"/>.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="headers">the headers to get the <paramref name="name"/> from</param>
        /// <param name="name">the name of the header to retrieve</param>
        /// <returns>a <see cref="List{String}"/> of header values or an empty <see cref="List{String}"/> if no values are found.</returns>
        public static List<string> GetAllAsString<TKey, TValue>(IHeaders<TKey, TValue> headers, TKey name)
            where TKey : class
        {
            IList<TValue> allNames = headers.GetAll(name);
            var values = new List<string>();

            // ReSharper disable once ForCanBeConvertedToForeach
            // Avoid enumerator allocation
            for (int i = 0; i < allNames.Count; i++)
            {
                TValue value = allNames[i];
                values.Add(value?.ToString());
            }

            return values;
        }

        /// <summary>
        /// <see cref="IHeaders{TKey, TValue}.Get(TKey, TValue)"/> and convert the result to a <see cref="String"/>.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="headers">the headers to get the <paramref name="name"/> from</param>
        /// <param name="name">the name of the header to retrieve</param>
        /// <param name="value">the first header value if the header is found. <c>null</c> if there's no such entry.</param>
        public static bool TryGetAsString<TKey, TValue>(IHeaders<TKey, TValue> headers, TKey name, out string value)
            where TKey : class
        {
            if (headers.TryGet(name, out TValue orig))
            {
                value = orig.ToString();
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        /// <summary>
        /// Helper for implementing toString for <see cref="DefaultHeaders{TKey, TValue}"/> and wrappers such as DefaultHttpHeaders.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="headers">the headers</param>
        /// <param name="size">the size of the headers</param>
        /// <returns>a String representation of the headers</returns>
        public static string ToString<TKey, TValue>(IEnumerable<HeaderEntry<TKey, TValue>> headers, int size)
            where TKey : class
        {
            string simpleName = StringUtil.SimpleClassName(headers);
            if (0u >= (uint)size)
            {
                return simpleName + "[]";
            }
            else
            {
                // original capacity assumes 20 chars per headers
                var sb = StringBuilderManager.Allocate(simpleName.Length + 2 + size * 20)
                    .Append(simpleName)
                    .Append('[');
                foreach (HeaderEntry<TKey, TValue> header in headers)
                {
                    _ = sb.Append(header.Key).Append(": ").Append(header.Value).Append(", ");
                }
                sb.Length -= 2;
                return StringBuilderManager.ReturnAndFree(sb.Append(']'));
            }
        }

        /// <summary>
        /// <see cref="IHeaders{TKey, TValue}.Names"/> and convert each element of <see cref="ISet{ICharSequence}"/> to a <see cref="String"/>.
        /// </summary>
        /// <param name="headers">the headers to get the names from</param>
        /// <returns>a <see cref="IList{String}"/> of header values or an empty <see cref="IList{String}"/> if no values are found.</returns>
        public static IList<string> NamesAsString(IHeaders<ICharSequence, ICharSequence> headers)
        {
            ISet<ICharSequence> allNames = headers.Names();

            var names = new List<string>();

            foreach (ICharSequence name in allNames)
            {
                names.Add(name.ToString());
            }

            return names;
        }
    }
}
