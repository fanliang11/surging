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

namespace DotNetty.Transport.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DotNetty.Common;
    using DotNetty.Common.Utilities;

    public static class IChannelExtensions
    {
        public static Task WriteAndFlushManyAsync(this IChannel channel, params object[] msgs) => WriteAndFlushManyAsync(channel, messages: msgs);

        public static Task WriteAndFlushManyAsync(this IChannel channel, ICollection<object> messages)
        {
            if (messages is null || 0u >= (uint)messages.Count) { return TaskUtil.Completed; }

            var taskList = ThreadLocalList<Task>.NewInstance();
            foreach (object m in messages)
            {
                taskList.Add(channel.WriteAsync(m));
            }
            channel.Flush();

            var writeCloseCompletion = Task.WhenAll(taskList);
            writeCloseCompletion.ContinueWith(s_returnAfterWriteAction, taskList, TaskContinuationOptions.ExecuteSynchronously);
            return writeCloseCompletion;
        }

        private static readonly Action<Task, object> s_returnAfterWriteAction = (t, s) => ReturnAfterWriteAction(t, s);
        private static void ReturnAfterWriteAction(Task t, object s) => ((ThreadLocalList<Task>)s).Return();
    }
}
