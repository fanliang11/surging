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
 * Copyright (c) The DotNetty Project (Microsoft). All rights reserved.
 *
 *   https://github.com/azure/dotnetty
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
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
    using System.Threading;
    using DotNetty.Buffers;

    /// <summary>
    ///     Shared configuration for SocketAsyncChannel. Provides access to pre-configured resources like ByteBuf allocator and
    ///     IO buffer pools
    /// </summary>
    public class DefaultChannelConfiguration : IChannelConfiguration
    {
        private static readonly TimeSpan DefaultConnectTimeout = TimeSpan.FromSeconds(30);

        private IByteBufferAllocator _allocator = ByteBufferUtil.DefaultAllocator;
        private IRecvByteBufAllocator _recvByteBufAllocator = FixedRecvByteBufAllocator.Default;
        private IMessageSizeEstimator _messageSizeEstimator = DefaultMessageSizeEstimator.Default;

        private int _autoRead = SharedConstants.True;
        private int _autoClose = SharedConstants.True;
        private int _writeSpinCount = 16;
        private int _writeBufferHighWaterMark = 64 * 1024;
        private int _writeBufferLowWaterMark = 32 * 1024;
        private long _connectTimeout = DefaultConnectTimeout.Ticks;
        private int _pinEventExecutor = SharedConstants.True;

        protected readonly IChannel Channel;

        public DefaultChannelConfiguration(IChannel channel)
            : this(channel, new AdaptiveRecvByteBufAllocator())
        {
        }

        public DefaultChannelConfiguration(IChannel channel, IRecvByteBufAllocator allocator)
        {
            if (channel is null) { ThrowHelper.ThrowArgumentNullException(DotNetty.Transport.ExceptionArgument.channel); }

            Channel = channel;
            if (allocator is IMaxMessagesRecvByteBufAllocator maxMessagesAllocator)
            {
                maxMessagesAllocator.MaxMessagesPerRead = channel.Metadata.DefaultMaxMessagesPerRead;
            }
            else if (allocator is null)
            {
                ThrowHelper.ThrowArgumentNullException(DotNetty.Transport.ExceptionArgument.allocator);
            }
            RecvByteBufAllocator = allocator;
        }

        public virtual T GetOption<T>(ChannelOption<T> option)
        {
            if (option is null) { ThrowHelper.ThrowArgumentNullException(DotNetty.Transport.ExceptionArgument.option); }

            if (ChannelOption.ConnectTimeout.Equals(option))
            {
                return (T)(object)ConnectTimeout; // no boxing will happen, compiler optimizes away such casts
            }
            if (ChannelOption.WriteSpinCount.Equals(option))
            {
                return (T)(object)WriteSpinCount;
            }
            if (ChannelOption.Allocator.Equals(option))
            {
                return (T)Allocator;
            }
            if (ChannelOption.RcvbufAllocator.Equals(option))
            {
                return (T)RecvByteBufAllocator;
            }
            if (ChannelOption.AutoRead.Equals(option))
            {
                return (T)(object)IsAutoRead;
            }
            if (ChannelOption.AutoClose.Equals(option))
            {
                return (T)(object)IsAutoClose;
            }
            if (ChannelOption.WriteBufferHighWaterMark.Equals(option))
            {
                return (T)(object)WriteBufferHighWaterMark;
            }
            if (ChannelOption.WriteBufferLowWaterMark.Equals(option))
            {
                return (T)(object)WriteBufferLowWaterMark;
            }
            if (ChannelOption.MessageSizeEstimator.Equals(option))
            {
                return (T)MessageSizeEstimator;
            }
            if (ChannelOption.MaxMessagesPerRead.Equals(option))
            {
                if (RecvByteBufAllocator is IMaxMessagesRecvByteBufAllocator)
                {
                    return (T)(object)MaxMessagesPerRead;
                }
            }
            if (ChannelOption.SingleEventexecutorPerGroup.Equals(option))
            {
                return (T)(object)PinEventExecutorPerGroup;
            }
            return default;
        }

        public bool SetOption(ChannelOption option, object value) => option.Set(this, value);

        public virtual bool SetOption<T>(ChannelOption<T> option, T value)
        {
            Validate(option, value);

            if (ChannelOption.ConnectTimeout.Equals(option))
            {
                ConnectTimeout = (TimeSpan)(object)value;
            }
            else if (ChannelOption.WriteSpinCount.Equals(option))
            {
                WriteSpinCount = (int)(object)value;
            }
            else if (ChannelOption.Allocator.Equals(option))
            {
                Allocator = (IByteBufferAllocator)value;
            }
            else if (ChannelOption.RcvbufAllocator.Equals(option))
            {
                RecvByteBufAllocator = (IRecvByteBufAllocator)value;
            }
            else if (ChannelOption.AutoRead.Equals(option))
            {
                IsAutoRead = (bool)(object)value;
            }
            else if (ChannelOption.AutoClose.Equals(option))
            {
                IsAutoClose = (bool)(object)value;
            }
            else if (ChannelOption.WriteBufferHighWaterMark.Equals(option))
            {
                WriteBufferHighWaterMark = (int)(object)value;
            }
            else if (ChannelOption.WriteBufferLowWaterMark.Equals(option))
            {
                WriteBufferLowWaterMark = (int)(object)value;
            }
            else if (ChannelOption.MessageSizeEstimator.Equals(option))
            {
                MessageSizeEstimator = (IMessageSizeEstimator)value;
            }
            else if (ChannelOption.MaxMessagesPerRead.Equals(option))
            {
                if (RecvByteBufAllocator is IMaxMessagesRecvByteBufAllocator)
                {
                    MaxMessagesPerRead = (int)(object)value;
                }
                else
                {
                    return false;
                }
            }
            else if (ChannelOption.SingleEventexecutorPerGroup.Equals(option))
            {
                PinEventExecutorPerGroup = (bool)(object)value;
            }
            else
            {
                return false;
            }

            return true;
        }

        protected virtual void Validate<T>(ChannelOption<T> option, T value)
        {
            if (option is null) { ThrowHelper.ThrowArgumentNullException(DotNetty.Transport.ExceptionArgument.option); }
            option.Validate(value);
        }

        public TimeSpan ConnectTimeout
        {
            get { return new TimeSpan(Volatile.Read(ref _connectTimeout)); }
            set
            {
                if (value < TimeSpan.Zero) { ThrowHelper.ThrowArgumentException_MustBeGreaterThanZero(value, DotNetty.Transport.ExceptionArgument.value); }
                Interlocked.Exchange(ref _connectTimeout, value.Ticks);
            }
        }

        public IByteBufferAllocator Allocator
        {
            get { return Volatile.Read(ref _allocator); }
            set
            {
                if (value is null) { ThrowHelper.ThrowArgumentNullException(DotNetty.Transport.ExceptionArgument.value); }
                Interlocked.Exchange(ref _allocator, value);
            }
        }

        public IRecvByteBufAllocator RecvByteBufAllocator
        {
            get { return Volatile.Read(ref _recvByteBufAllocator); }
            set
            {
                if (value is null) { ThrowHelper.ThrowArgumentNullException(DotNetty.Transport.ExceptionArgument.value); }
                Interlocked.Exchange(ref _recvByteBufAllocator, value);
            }
        }

        public virtual IMessageSizeEstimator MessageSizeEstimator
        {
            get { return Volatile.Read(ref _messageSizeEstimator); }
            set
            {
                if (value is null) { ThrowHelper.ThrowArgumentNullException(DotNetty.Transport.ExceptionArgument.value); }
                Interlocked.Exchange(ref _messageSizeEstimator, value);
            }
        }

        [Obsolete("Please use IsAutoRead instead.")]
        public bool AutoRead
        {
            get => IsAutoRead;
            set => IsAutoRead = value;
        }

        public bool IsAutoRead
        {
            get { return SharedConstants.False < (uint)Volatile.Read(ref _autoRead); }
            set
            {
#pragma warning disable 420 // atomic exchange is ok
                bool oldAutoRead = SharedConstants.False < (uint)Interlocked.Exchange(ref _autoRead, value ? SharedConstants.True : SharedConstants.False);
#pragma warning restore 420
                if (value && !oldAutoRead)
                {
                    Channel.Read();
                }
                else if (!value && oldAutoRead)
                {
                    AutoReadCleared();
                }
            }
        }

        protected virtual void AutoReadCleared()
        {
        }

        [Obsolete("Please use IsAutoClose instead.")]
        public bool AutoClose
        {
            get => IsAutoClose;
            set => IsAutoClose = value;
        }

        public bool IsAutoClose
        {
            get { return SharedConstants.False < (uint)Volatile.Read(ref _autoClose); }
            set { Interlocked.Exchange(ref _autoClose, value ? SharedConstants.True : SharedConstants.False); }
        }

        public virtual int WriteBufferHighWaterMark
        {
            get { return Volatile.Read(ref _writeBufferHighWaterMark); }
            set
            {
                if ((uint)value > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_PositiveOrZero(value, DotNetty.Transport.ExceptionArgument.value); }
                if (value < Volatile.Read(ref _writeBufferLowWaterMark)) { ThrowHelper.ThrowArgumentOutOfRangeException(); }

                Interlocked.Exchange(ref _writeBufferHighWaterMark, value);
            }
        }

        public virtual int WriteBufferLowWaterMark
        {
            get { return Volatile.Read(ref _writeBufferLowWaterMark); }
            set
            {
                if ((uint)value > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_PositiveOrZero(value, DotNetty.Transport.ExceptionArgument.value); }
                if (value > Volatile.Read(ref _writeBufferHighWaterMark)) { ThrowHelper.ThrowArgumentOutOfRangeException(); }

                Interlocked.Exchange(ref _writeBufferLowWaterMark, value);
            }
        }

        public int WriteSpinCount
        {
            get { return Volatile.Read(ref _writeSpinCount); }
            set
            {
                if (value < 1) { ThrowHelper.ThrowArgumentException_PositiveOrOne(value, DotNetty.Transport.ExceptionArgument.value); }

                Interlocked.Exchange(ref _writeSpinCount, value);
            }
        }

        public int MaxMessagesPerRead
        {
            get { return ((IMaxMessagesRecvByteBufAllocator)RecvByteBufAllocator).MaxMessagesPerRead; }
            set { ((IMaxMessagesRecvByteBufAllocator)RecvByteBufAllocator).MaxMessagesPerRead = value; }
        }

        public bool PinEventExecutorPerGroup
        {
            get { return SharedConstants.False < (uint)Volatile.Read(ref _pinEventExecutor); }
            set { Interlocked.Exchange(ref _pinEventExecutor, value ? SharedConstants.True : SharedConstants.False); }
        }
    }
}