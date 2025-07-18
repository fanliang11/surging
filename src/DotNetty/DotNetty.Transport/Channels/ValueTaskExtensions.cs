using DotNetty.Common.Concurrency;
using DotNetty.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNetty.Transport.Channels
{
    public static class ValueTaskExtensions
    {
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static ValueTask CloseOnComplete(this ValueTask task, IChannel channel)
        {
            if (task.IsCompleted)
            {
                _ = channel.CloseAsync();
                return new ValueTask();
            }
            else
            {
                return new ValueTask(task.AsTask().ContinueWith(CloseChannelOnCompleteAction, channel, TaskContinuationOptions.ExecuteSynchronously));
            }
        }
        private static readonly Action<Task, object> CloseChannelOnCompleteAction = (t, s) => CloseChannelOnComplete(t, s);
        private static  void CloseChannelOnComplete(Task t, object c) 
        { 
            _ = ((IChannel) c).CloseAsync();
            t.Dispose();
        }


        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static ValueTask CloseOnComplete(this ValueTask task, IChannel channel, IPromise promise)
        {
            if (task.IsCompleted)
            {
                _ = channel.CloseAsync(promise);
                return new ValueTask();
            }
            else
            {
                return new ValueTask(task.AsTask().ContinueWith(CloseWrappedChannelOnCompleteAction, (channel, promise), TaskContinuationOptions.ExecuteSynchronously));
            }
        }
        private static readonly Action<Task, object> CloseWrappedChannelOnCompleteAction = (t, s) => CloseWrappedChannelOnComplete(t, s);
        private static  void CloseWrappedChannelOnComplete(Task t, object s)
        {
            var wrapped = ((IChannel, IPromise))s;
            _ = wrapped.Item1.CloseAsync(wrapped.Item2);
            t.Dispose();
        }


        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static ValueTask CloseOnComplete(this ValueTask task, IChannelHandlerContext ctx)
        {
            if (task.IsCompleted)
            {
                _ = ctx.CloseAsync();
                return new ValueTask();
            }
            else
            {
                return new ValueTask(task.AsTask().ContinueWith(CloseContextOnCompleteAction, ctx, TaskContinuationOptions.ExecuteSynchronously));
            }
        }
        private static readonly Action<Task, object> CloseContextOnCompleteAction = (t, s) => CloseContextOnComplete(t, s);
        private static  void CloseContextOnComplete(Task t, object c)
        {
             _ = ((IChannelHandlerContext)c).CloseAsync();
            t.Dispose();
        }


        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static ValueTask CloseOnComplete(this ValueTask task, IChannelHandlerContext ctx, IPromise promise)
        {
            if (task.IsCompleted)
            {
                _ = ctx.CloseAsync(promise);
                return new ValueTask();
            }
            else
            {
                return new ValueTask(task.AsTask().ContinueWith(CloseWrappedContextOnCompleteAction, (ctx, promise), TaskContinuationOptions.ExecuteSynchronously));
            }
        }
        private static readonly Action<Task, object> CloseWrappedContextOnCompleteAction = (t, s) => CloseWrappedContextOnComplete(t, s);
        private static  void CloseWrappedContextOnComplete(Task t, object s)
        {
            var wrapped = ((IChannelHandlerContext, IPromise))s;
            _ = wrapped.Item1.CloseAsync(wrapped.Item2);
            t.Dispose();
        }


        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static ValueTask CloseOnFailure(this ValueTask task, IChannel channel)
        {
            if (task.IsCompleted)
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    _ = channel.CloseAsync();
                }
                return new ValueTask();
            }
            else
            {
                return new ValueTask(task.AsTask().ContinueWith(CloseChannelOnFailureAction, channel, TaskContinuationOptions.ExecuteSynchronously));
            }
        }
        private static readonly Action<Task, object> CloseChannelOnFailureAction = (t, s) => CloseChannelOnFailure(t, s);
        private static  void CloseChannelOnFailure(Task t, object c)
        {
            if (t.IsFaulted || t.IsCanceled)
            {
                _ = ((IChannel)c).CloseAsync();
            }
            t.Dispose();
        }


        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static ValueTask CloseOnFailure(this ValueTask task, IChannel channel, IPromise promise)
        {
            if (task.IsCompleted)
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    _ = channel.CloseAsync(promise);
                }
                return new ValueTask();
            }
            else
            {
                return new ValueTask(task.AsTask().ContinueWith(CloseWrappedChannelOnFailureAction, (channel, promise), TaskContinuationOptions.ExecuteSynchronously));
            }
        }
        private static readonly Action<Task, object> CloseWrappedChannelOnFailureAction = (t, s) => CloseWrappedChannelOnFailure(t, s);
        private static  void CloseWrappedChannelOnFailure(Task t, object s)
        {
            if (t.IsFaulted || t.IsCanceled)
            {
                var wrapped = ((IChannel, IPromise))s;
                _ = wrapped.Item1.CloseAsync(wrapped.Item2);
            }
            t.Dispose();
        }


        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static ValueTask CloseOnFailure(this ValueTask task, IChannelHandlerContext ctx)
        {
            if (task.IsCompleted)
            {
                if (task.IsCompleted)
                {
                    _ = ctx.CloseAsync();
                }
                return new ValueTask();
            }
            else
            {
                return new ValueTask(task.AsTask().ContinueWith(CloseContextOnFailureAction, ctx, TaskContinuationOptions.ExecuteSynchronously));
            }
        }
        private static readonly Action<Task, object> CloseContextOnFailureAction = (t, s) => CloseContextOnFailure(t, s);
        private static  void CloseContextOnFailure(Task t, object c)
        {
            if (t.IsFaulted || t.IsCanceled)
            {
                _ = ((IChannelHandlerContext)c).CloseAsync();
            }
            t.Dispose();
        }


        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static ValueTask CloseOnFailure(this ValueTask task, IChannelHandlerContext ctx, IPromise promise)
        {
            if (task.IsCompleted)
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    _ = ctx.CloseAsync(promise);
                }
                return new ValueTask();
            }
            else
            {
                return new ValueTask(task.AsTask().ContinueWith(CloseWrappedContextOnFailureAction, (ctx, promise), TaskContinuationOptions.ExecuteSynchronously));
            }
        }
        private static readonly Action<Task, object> CloseWrappedContextOnFailureAction = (t, s) => CloseWrappedContextOnFailure(t, s);
        private static async void CloseWrappedContextOnFailure(Task t, object s)
        {
            if (t.IsFaulted || t.IsCanceled)
            {
                var wrapped = ((IChannelHandlerContext, IPromise))s;
                _ = wrapped.Item1.CloseAsync(wrapped.Item2);
            }
            t.Dispose();
        }


        public static ValueTask FireExceptionOnFailure(this ValueTask task, IChannelPipeline pipeline)
        {
            if (task.IsCompleted)
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    _ = pipeline.FireExceptionCaught(TaskUtil.Unwrap(task.AsTask().Exception));
                }
                return new ValueTask();
            }
            else
            {
                return new ValueTask(task.AsTask().ContinueWith(FirePipelineExceptionOnFailureAction, pipeline, TaskContinuationOptions.ExecuteSynchronously));
            }
        }
        private static readonly Action<Task, object> FirePipelineExceptionOnFailureAction = (t, s) => FirePipelineExceptionOnFailure(t, s);
        private static  void FirePipelineExceptionOnFailure(Task t, object s)
        {
            if (t.IsFailure())
            {
                _ = ((IChannelPipeline)s).FireExceptionCaught(TaskUtil.Unwrap(t.Exception));
            }
            t.Dispose();
        }


        public static ValueTask FireExceptionOnFailure(this ValueTask task, IChannelHandlerContext ctx)
        {
            if (task.IsCompleted)
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    _ = ctx.FireExceptionCaught(TaskUtil.Unwrap(task.AsTask().Exception));
                }
                return new ValueTask();
            }
            else
            {
                return new ValueTask(task.AsTask().ContinueWith(FireContextExceptionOnFailureAction, ctx, TaskContinuationOptions.ExecuteSynchronously));
            }
        }
        private static readonly Action<Task, object> FireContextExceptionOnFailureAction = (t, s) => FireContextExceptionOnFailure(t, s);
        private static  void FireContextExceptionOnFailure(Task t, object s)
        {
            if (t.IsFailure())
            {
                _ = ((IChannelHandlerContext)s).FireExceptionCaught(TaskUtil.Unwrap(t.Exception));
            }
            t.Dispose();
        }
    }
}
 