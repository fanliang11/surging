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
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Utilities;

    public static class TaskExtensions
    {
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static Task CloseOnComplete(this Task task, IChannel channel)
        {
            if (task.IsCompleted)
            {
                _ = channel.CloseAsync();
                return TaskUtil.Completed;
            }
            else
            {
                return task.ContinueWith(CloseChannelOnCompleteAction, channel, TaskContinuationOptions.ExecuteSynchronously);
            }
        }
        private static readonly Action<Task, object> CloseChannelOnCompleteAction = (t, s) => CloseChannelOnComplete(t, s);
        private static void CloseChannelOnComplete(Task t, object c) => _ = ((IChannel)c).CloseAsync();


        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static Task CloseOnComplete(this Task task, IChannel channel, IPromise promise)
        {
            if (task.IsCompleted)
            {
                _ = channel.CloseAsync(promise);
                return TaskUtil.Completed;
            }
            else
            {
                return task.ContinueWith(CloseWrappedChannelOnCompleteAction, (channel, promise), TaskContinuationOptions.ExecuteSynchronously);
            }
        }
        private static readonly Action<Task, object> CloseWrappedChannelOnCompleteAction = (t, s) => CloseWrappedChannelOnComplete(t, s);
        private static void CloseWrappedChannelOnComplete(Task t, object s)
        {
            var wrapped = ((IChannel, IPromise))s;
            _ = wrapped.Item1.CloseAsync(wrapped.Item2);
        }


        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static Task CloseOnComplete(this Task task, IChannelHandlerContext ctx)
        {
            if (task.IsCompleted)
            {
                _ = ctx.CloseAsync();
                return TaskUtil.Completed;
            }
            else
            {
                return task.ContinueWith(CloseContextOnCompleteAction, ctx, TaskContinuationOptions.ExecuteSynchronously);
            }
        }
        private static readonly Action<Task, object> CloseContextOnCompleteAction = (t, s) => CloseContextOnComplete(t, s);
        private static void CloseContextOnComplete(Task t, object c) => _ = ((IChannelHandlerContext)c).CloseAsync();


        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static Task CloseOnComplete(this Task task, IChannelHandlerContext ctx, IPromise promise)
        {
            if (task.IsCompleted)
            {
                _ = ctx.CloseAsync(promise);
                return TaskUtil.Completed;
            }
            else
            {
                return task.ContinueWith(CloseWrappedContextOnCompleteAction, (ctx, promise), TaskContinuationOptions.ExecuteSynchronously);
            }
        }
        private static readonly Action<Task, object> CloseWrappedContextOnCompleteAction = (t, s) => CloseWrappedContextOnComplete(t, s);
        private static void CloseWrappedContextOnComplete(Task t, object s)
        {
            var wrapped = ((IChannelHandlerContext, IPromise))s;
            _ = wrapped.Item1.CloseAsync(wrapped.Item2);
        }


        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static Task CloseOnFailure(this Task task, IChannel channel)
        {
            if (task.IsCompleted)
            {
                if (task.IsFailure())
                {
                    _ = channel.CloseAsync();
                }
                return TaskUtil.Completed;
            }
            else
            {
                return task.ContinueWith(CloseChannelOnFailureAction, channel, TaskContinuationOptions.ExecuteSynchronously);
            }
        }
        private static readonly Action<Task, object> CloseChannelOnFailureAction = (t, s) => CloseChannelOnFailure(t, s);
        private static void CloseChannelOnFailure(Task t, object c)
        {
            if (t.IsFailure())
            {
                _ = ((IChannel)c).CloseAsync();
            }
        }


        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static Task CloseOnFailure(this Task task, IChannel channel, IPromise promise)
        {
            if (task.IsCompleted)
            {
                if (task.IsFailure())
                {
                    _ = channel.CloseAsync(promise);
                }
                return TaskUtil.Completed;
            }
            else
            {
                return task.ContinueWith(CloseWrappedChannelOnFailureAction, (channel, promise), TaskContinuationOptions.ExecuteSynchronously);
            }
        }
        private static readonly Action<Task, object> CloseWrappedChannelOnFailureAction = (t, s) => CloseWrappedChannelOnFailure(t, s);
        private static void CloseWrappedChannelOnFailure(Task t, object s)
        {
            if (t.IsFailure())
            {
                var wrapped = ((IChannel, IPromise))s;
                _ = wrapped.Item1.CloseAsync(wrapped.Item2);
            }
        }


        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static Task CloseOnFailure(this Task task, IChannelHandlerContext ctx)
        {
            if (task.IsCompleted)
            {
                if (task.IsFailure())
                {
                    _ = ctx.CloseAsync();
                }
                return TaskUtil.Completed;
            }
            else
            {
                return task.ContinueWith(CloseContextOnFailureAction, ctx, TaskContinuationOptions.ExecuteSynchronously);
            }
        }
        private static readonly Action<Task, object> CloseContextOnFailureAction = (t, s) => CloseContextOnFailure(t, s);
        private static void CloseContextOnFailure(Task t, object c)
        {
            if (t.IsFailure())
            {
                _ = ((IChannelHandlerContext)c).CloseAsync();
            }
        }


        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static Task CloseOnFailure(this Task task, IChannelHandlerContext ctx, IPromise promise)
        {
            if (task.IsCompleted)
            {
                if (task.IsFailure())
                {
                    _ = ctx.CloseAsync(promise);
                }
                return TaskUtil.Completed;
            }
            else
            {
                return task.ContinueWith(CloseWrappedContextOnFailureAction, (ctx, promise), TaskContinuationOptions.ExecuteSynchronously);
            }
        }
        private static readonly Action<Task, object> CloseWrappedContextOnFailureAction = (t, s) => CloseWrappedContextOnFailure(t, s);
        private static void CloseWrappedContextOnFailure(Task t, object s)
        {
            if (t.IsFailure())
            {
                var wrapped = ((IChannelHandlerContext, IPromise))s;
                _ = wrapped.Item1.CloseAsync(wrapped.Item2);
            }
        }


        public static Task FireExceptionOnFailure(this Task task, IChannelPipeline pipeline)
        {
            if (task.IsCompleted)
            {
                if (task.IsFailure())
                {
                    _ = pipeline.FireExceptionCaught(TaskUtil.Unwrap(task.Exception));
                }
                return TaskUtil.Completed;
            }
            else
            {
                return task.ContinueWith(FirePipelineExceptionOnFailureAction, pipeline, TaskContinuationOptions.ExecuteSynchronously);
            }
        }
        private static readonly Action<Task, object> FirePipelineExceptionOnFailureAction = (t, s) => FirePipelineExceptionOnFailure(t, s);
        private static void FirePipelineExceptionOnFailure(Task t, object s)
        {
            if (t.IsFailure())
            {
                _ = ((IChannelPipeline)s).FireExceptionCaught(TaskUtil.Unwrap(t.Exception));
            }
        }


        public static Task FireExceptionOnFailure(this Task task, IChannelHandlerContext ctx)
        {
            if (task.IsCompleted)
            {
                if (task.IsFailure())
                {
                    _ = ctx.FireExceptionCaught(TaskUtil.Unwrap(task.Exception));
                }
                return TaskUtil.Completed;
            }
            else
            {
                return task.ContinueWith(FireContextExceptionOnFailureAction, ctx, TaskContinuationOptions.ExecuteSynchronously);
            }
        }
        private static readonly Action<Task, object> FireContextExceptionOnFailureAction = (t, s) => FireContextExceptionOnFailure(t, s);
        private static void FireContextExceptionOnFailure(Task t, object s)
        {
            if (t.IsFailure())
            {
                _ = ((IChannelHandlerContext)s).FireExceptionCaught(TaskUtil.Unwrap(t.Exception));
            }
        }
    }
}
