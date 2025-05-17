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
    using System.Threading;
    using DotNetty.Buffers;

    /// <summary>
    ///     Default implementation of <see cref="IMaxMessagesRecvByteBufAllocator" /> which respects
    ///     <see cref="IChannelConfiguration.IsAutoRead" />
    ///     and also prevents overflow.
    /// </summary>
    public abstract class DefaultMaxMessagesRecvByteBufAllocator : IMaxMessagesRecvByteBufAllocator
    {
        private int _maxMessagesPerRead;
        private int _respectMaybeMoreData = SharedConstants.True;

        protected DefaultMaxMessagesRecvByteBufAllocator()
            : this(1)
        {
        }

        protected DefaultMaxMessagesRecvByteBufAllocator(int maxMessagesPerRead)
        {
            MaxMessagesPerRead = maxMessagesPerRead;
        }

        public int MaxMessagesPerRead
        {
            get { return Volatile.Read(ref _maxMessagesPerRead); }
            set
            {
                if ((uint)(value - 1) > SharedConstants.TooBigOrNegative) // <= 0
                {
                    ThrowHelper.ThrowArgumentException_Positive(value, DotNetty.Transport.ExceptionArgument.value);
                }
                Interlocked.Exchange(ref _maxMessagesPerRead, value);
            }
        }

        public bool RespectMaybeMoreData
        {
            get => SharedConstants.False < (uint)Volatile.Read(ref _respectMaybeMoreData);
            set => Interlocked.Exchange(ref _respectMaybeMoreData, value ? SharedConstants.True : SharedConstants.False);
        }

        public abstract IRecvByteBufAllocatorHandle NewHandle();

        /// <summary>Focuses on enforcing the maximum messages per read condition for <see cref="ContinueReading" />.</summary>
        protected abstract class MaxMessageHandle<T> : IRecvByteBufAllocatorHandle
            where T : IMaxMessagesRecvByteBufAllocator
        {
            protected readonly T Owner;
            private IChannelConfiguration _config;
            private int _maxMessagePerRead;
            private bool _respectMaybeMoreData;
            private int _totalMessages;
            private int _totalBytesRead;
            private int _lastBytesRead;

            protected MaxMessageHandle(T owner)
            {
                Owner = owner;
            }

            public abstract int Guess();

            /// <summary>Only <see cref="M:IChannelConfiguration.MaxMessagesPerRead" /> is used.</summary>
            public void Reset(IChannelConfiguration config)
            {
                _config = config;
                _maxMessagePerRead = Owner.MaxMessagesPerRead;
                _respectMaybeMoreData = Owner.RespectMaybeMoreData;
                _totalMessages = _totalBytesRead = 0;
            }

            public IByteBuffer Allocate(IByteBufferAllocator alloc) => alloc.Buffer(Guess());

            public void IncMessagesRead(int amt) => _totalMessages += amt;

            public virtual int LastBytesRead
            {
                get { return _lastBytesRead; }
                set
                {
                    _lastBytesRead = value;
                    if (value > 0)
                    {
                        _totalBytesRead += value;
                    }
                }
            }

            public virtual bool ContinueReading()
            {
                return _config.IsAutoRead
                    && (!_respectMaybeMoreData || AttemptedBytesRead == _lastBytesRead)
                    && _totalMessages < _maxMessagePerRead
                    && _totalBytesRead > 0;
            }

            public virtual void ReadComplete()
            {
            }

            public virtual int AttemptedBytesRead { get; set; }

            protected int TotalBytesRead() => _totalBytesRead >= 0 ? _totalBytesRead : int.MaxValue;
        }
    }
}