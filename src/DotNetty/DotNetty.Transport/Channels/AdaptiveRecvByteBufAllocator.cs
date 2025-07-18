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
    using System.Collections.Generic;
    using DotNetty.Common.Utilities;

    /// <summary>
    ///     The <see cref="IRecvByteBufAllocator" /> that automatically increases and
    ///     decreases the predicted buffer size on feed back.
    ///     <p />
    ///     It gradually increases the expected number of readable bytes if the previous
    ///     read fully filled the allocated buffer. It gradually decreases the expected
    ///     number of readable bytes if the read operation was not able to fill a certain
    ///     amount of the allocated buffer two times consecutively. Otherwise, it keeps
    ///     returning the same prediction.
    /// </summary>
    public class AdaptiveRecvByteBufAllocator : DefaultMaxMessagesRecvByteBufAllocator
    {
        const int DefaultMinimum = 64;
        const int DefaultInitial = 1024;
        const int DefaultMaximum = 65536;

        const int IndexIncrement = 4;
        const int IndexDecrement = 1;

        static readonly int[] SizeTable;

        static AdaptiveRecvByteBufAllocator()
        {
            var sizeTable = new List<int>();
            for (int i = 16; i < 512; i += 16)
            {
                sizeTable.Add(i);
            }

            for (int i = 512; i > 0; i <<= 1)
            {
                sizeTable.Add(i);
            }

            SizeTable = sizeTable.ToArray();
        }

        static int GetSizeTableIndex(int size)
        {
            for (int low = 0, high = SizeTable.Length - 1; ;)
            {
                if (high < low)
                {
                    return low;
                }
                if (high == low)
                {
                    return high;
                }

                int mid = (low + high).RightUShift(1);
                int a = SizeTable[mid];
                int b = SizeTable[mid + 1];
                if (size > b)
                {
                    low = mid + 1;
                }
                else if (size < a)
                {
                    high = mid - 1;
                }
                else if (size == a)
                {
                    return mid;
                }
                else
                {
                    return mid + 1;
                }
            }
        }

        sealed class HandleImpl : MaxMessageHandle<AdaptiveRecvByteBufAllocator>
        {
            readonly int _minIndex;
            readonly int _maxIndex;
            int _index;
            int _nextReceiveBufferSize;
            bool _decreaseNow;

            public HandleImpl(AdaptiveRecvByteBufAllocator owner, int minIndex, int maxIndex, int initial)
                : base(owner)
            {
                _minIndex = minIndex;
                _maxIndex = maxIndex;

                _index = GetSizeTableIndex(initial);
                _nextReceiveBufferSize = SizeTable[_index];
            }

            public override int LastBytesRead
            {
                get => base.LastBytesRead;
                set
                {
                    // If we read as much as we asked for we should check if we need to ramp up the size of our next guess.
                    // This helps adjust more quickly when large amounts of data is pending and can avoid going back to
                    // the selector to check for more data. Going back to the selector can add significant latency for large
                    // data transfers.
                    if (value == AttemptedBytesRead)
                    {
                        Record(value);
                    }
                    base.LastBytesRead = value;
                }
            }

            public override int Guess() => _nextReceiveBufferSize;

            void Record(int actualReadBytes)
            {
                if (actualReadBytes <= SizeTable[Math.Max(0, _index - IndexDecrement)])
                {
                    if (_decreaseNow)
                    {
                        _index = Math.Max(_index - IndexDecrement, _minIndex);
                        _nextReceiveBufferSize = SizeTable[_index];
                        _decreaseNow = false;
                    }
                    else
                    {
                        _decreaseNow = true;
                    }
                }
                else if (actualReadBytes >= _nextReceiveBufferSize)
                {
                    _index = Math.Min(_index + IndexIncrement, _maxIndex);
                    _nextReceiveBufferSize = SizeTable[_index];
                    _decreaseNow = false;
                }
            }

            public override void ReadComplete() => Record(TotalBytesRead());
        }

        readonly int _minIndex;
        readonly int _maxIndex;
        readonly int _initial;

        /// <summary>
        ///     Creates a new predictor with the default parameters.  With the default
        ///     parameters, the expected buffer size starts from <c>1024</c>, does not
        ///     go down below <c>64</c>, and does not go up above <c>65536</c>.
        /// </summary>
        public AdaptiveRecvByteBufAllocator()
            : this(DefaultMinimum, DefaultInitial, DefaultMaximum)
        {
        }

        /// <summary>Creates a new predictor with the specified parameters.</summary>
        /// <param name="minimum">the inclusive lower bound of the expected buffer size</param>
        /// <param name="initial">the initial buffer size when no feed back was received</param>
        /// <param name="maximum">the inclusive upper bound of the expected buffer size</param>
        public AdaptiveRecvByteBufAllocator(int minimum, int initial, int maximum)
        {
            if ((uint)(minimum - 1) > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_Positive(minimum, ExceptionArgument.minimum); }
            if (initial < minimum) { ThrowHelper.ThrowArgumentOutOfRangeException(); }
            if (maximum < initial) { ThrowHelper.ThrowArgumentOutOfRangeException(); }

            int min = GetSizeTableIndex(minimum);
            if (SizeTable[min] < minimum)
            {
                _minIndex = min + 1;
            }
            else
            {
                _minIndex = min;
            }

            int max = GetSizeTableIndex(maximum);
            if (SizeTable[max] > maximum)
            {
                _maxIndex = max - 1;
            }
            else
            {
                _maxIndex = max;
            }

            _initial = initial;
        }

        public override IRecvByteBufAllocatorHandle NewHandle() => new HandleImpl(this, _minIndex, _maxIndex, _initial);
    }
}