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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Common.Concurrency
{
    using System.Globalization;
    using System.Threading;
    using DotNetty.Common.Utilities;
    using Thread = XThread;

    /// <summary>
    /// A <see cref="IThreadFactory"/> implementation with a simple naming rule.
    /// </summary>
    public class DefaultThreadFactory<TPool> : DefaultThreadFactory
    {
        private static readonly string s_poolName;

        public static readonly DefaultThreadFactory<TPool> Instance;

        static DefaultThreadFactory()
        {
            s_poolName = ToPoolName();
            Instance = new DefaultThreadFactory<TPool>();
        }

        private DefaultThreadFactory()
            : base(s_poolName)
        {
        }

        private static string ToPoolName()
        {
            string poolName = StringUtil.SimpleClassName<TPool>();
            if (poolName.Length == 1)
            {
                return poolName.ToLowerInvariant();
            }
            if (char.IsUpper(poolName[0]) && char.IsLower(poolName[1]))
            {
                return char.ToLowerInvariant(poolName[0]) + poolName.Substring(1);
            }
            else
            {
                return poolName;
            }
        }
    }

    /// <summary>
    /// A <see cref="IThreadFactory"/> implementation with a simple naming rule.
    /// </summary>
    public class DefaultThreadFactory : IThreadFactory
    {
        private const string c_poolName = "default";

        private static int s_poolId;

        private readonly string _threadPrefix;
        private int v_nextId;

        public DefaultThreadFactory()
            : this(c_poolName)
        {
        }

        public DefaultThreadFactory(string poolName)
        {
            if (string.IsNullOrEmpty(poolName)) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.poolName); }

            _threadPrefix = poolName + '-' + Interlocked.Increment(ref s_poolId).ToString(CultureInfo.InvariantCulture) + '-';
        }

        public Thread NewThread(XParameterizedThreadStart r)
        {
            var threadId = Interlocked.Increment(ref v_nextId);
            return NewThread(r, _threadPrefix + threadId.ToString(CultureInfo.InvariantCulture));
        }

        public Thread NewThread(XParameterizedThreadStart r, string threadName)
        {
            return new Thread(r)
            {
                Name = threadName
            };
        }
    }
}
