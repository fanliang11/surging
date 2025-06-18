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
    public abstract class PendingBytesTracker : IMessageSizeEstimatorHandle
    {
        readonly IMessageSizeEstimatorHandle _estimatorHandle;

        protected PendingBytesTracker(IMessageSizeEstimatorHandle estimatorHandle)
        {
            if (estimatorHandle is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.estimatorHandle); }
            _estimatorHandle = estimatorHandle;
        }

        public static PendingBytesTracker NewTracker(IChannel channel)
        {
            if (channel.Pipeline is DefaultChannelPipeline defaultPipeline)
            {
                return new DefaultChannelPipelinePendingBytesTracker(defaultPipeline);
            }
            else
            {
                var buffer = channel.Unsafe.OutboundBuffer;
                var handle = channel.Configuration.MessageSizeEstimator.NewHandle();
                // We need to guard against null as channel.unsafe().outboundBuffer() may returned null
                // if the channel was already closed when constructing the PendingBytesTracker.
                // See https://github.com/netty/netty/issues/3967
                return buffer is null 
                    ? (PendingBytesTracker)new NoopPendingBytesTracker(handle) 
                    : new ChannelOutboundBufferPendingBytesTracker(buffer, handle);
            }
        }

        public virtual int Size(object msg) => _estimatorHandle.Size(msg);

        public abstract void IncrementPendingOutboundBytes(long bytes);

        public abstract void DecrementPendingOutboundBytes(long bytes);
    }

    sealed class DefaultChannelPipelinePendingBytesTracker : PendingBytesTracker
    {
        readonly DefaultChannelPipeline _pipeline;

        public DefaultChannelPipelinePendingBytesTracker(DefaultChannelPipeline pipeline)
            : base(pipeline.EstimatorHandle)
        {
            _pipeline = pipeline;
        }

        public override void IncrementPendingOutboundBytes(long bytes)
        {
            _pipeline.IncrementPendingOutboundBytes(bytes);
        }

        public override void DecrementPendingOutboundBytes(long bytes)
        {
            _pipeline.DecrementPendingOutboundBytes(bytes);
        }
    }

    sealed class ChannelOutboundBufferPendingBytesTracker : PendingBytesTracker
    {
        readonly ChannelOutboundBuffer _buffer;

        public ChannelOutboundBufferPendingBytesTracker(ChannelOutboundBuffer buffer, IMessageSizeEstimatorHandle estimatorHandle)
            : base(estimatorHandle)
        {
            _buffer = buffer;
        }

        public override void IncrementPendingOutboundBytes(long bytes)
        {
            _buffer.IncrementPendingOutboundBytes(bytes);
        }

        public override void DecrementPendingOutboundBytes(long bytes)
        {
            _buffer.DecrementPendingOutboundBytes(bytes);
        }
    }

    sealed class NoopPendingBytesTracker : PendingBytesTracker
    {
        public NoopPendingBytesTracker(IMessageSizeEstimatorHandle estimatorHandle) : base(estimatorHandle) { }

        public override void IncrementPendingOutboundBytes(long bytes)
        {
            // Noop
        }

        public override void DecrementPendingOutboundBytes(long bytes)
        {
            // Noop
        }
    }
}
