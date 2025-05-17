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

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// Largely based from https://github.com/dotnet/corefx/blob/release/3.0/src/System.Memory/src/System/Buffers/SequenceReader.cs

namespace DotNetty.Buffers
{
    using System;
    using System.Buffers;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public ref partial struct ByteBufferReader
    {
        private readonly IByteBuffer _origin;
        private SequencePosition _currentPosition;
        private SequencePosition _nextPosition;
        private bool _moreData;
        private readonly long _length;

        private readonly ReadOnlySequence<byte> _sequence;
        private ReadOnlySpan<byte> _currentSpan;
        private int _currentSpanIndex;
        private long _consumed;

        /// <summary>Create a <see cref="ByteBufferReader" /> over the given <see cref="IByteBuffer"/>.</summary>
        public ByteBufferReader(IByteBuffer buffer)
        {
            _origin = buffer;
            var sequence = buffer.UnreadSequence;

            _currentSpanIndex = 0;
            _consumed = 0;
            _sequence = sequence;
            _currentPosition = sequence.Start;
            _length = -1;

            ByteBufferReaderHelper.GetFirstSpan(sequence, out ReadOnlySpan<byte> first, out _nextPosition);
            _currentSpan = first;
            _moreData = (uint)first.Length > 0u;

            if (!sequence.IsSingleSegment && !_moreData)
            {
                _moreData = true;
                GetNextSpan();
            }
        }

        /// <summary>Create a <see cref="ByteBufferReader" /> over the given <see cref="ReadOnlySequence{T}"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ByteBufferReader(ReadOnlySequence<byte> sequence)
        {
            _origin = null;
            _currentSpanIndex = 0;
            _consumed = 0;
            _sequence = sequence;
            _currentPosition = sequence.Start;
            _length = -1;

            ByteBufferReaderHelper.GetFirstSpan(sequence, out ReadOnlySpan<byte> first, out _nextPosition);
            _currentSpan = first;
            _moreData = (uint)first.Length > 0u;

            if (!sequence.IsSingleSegment && !_moreData)
            {
                _moreData = true;
                GetNextSpan();
            }
        }

        public IByteBuffer Origin => _origin;

        /// <summary>Return true if we're in the last segment.</summary>
        public readonly bool IsLastSegment => _nextPosition.GetObject() is null;

        /// <summary>True when there is no more data in the <see cref="Sequence"/>.</summary>
        public readonly bool End
        {
            [MethodImpl(InlineMethod.AggressiveOptimization)]
            get => !_moreData;
        }

        /// <summary>The underlying <see cref="ReadOnlySequence{T}"/> for the reader.</summary>
        public readonly ReadOnlySequence<byte> Sequence => _sequence;

        /// <summary>Gets the unread portion of the <see cref="Sequence"/>.</summary>
        /// <value>The unread portion of the <see cref="Sequence"/>.</value>
        public readonly ReadOnlySequence<byte> UnreadSequence => Sequence.Slice(Position);

        /// <summary>The current position in the <see cref="Sequence"/>.</summary>
        public readonly SequencePosition Position
            => _sequence.GetPosition(_currentSpanIndex, _currentPosition);

        /// <summary>The current segment in the <see cref="Sequence"/>.</summary>
        public ReadOnlySpan<byte> CurrentSpan
        {
            readonly get => _currentSpan;
            private set => _currentSpan = value;
        }

        /// <summary>The index in the <see cref="CurrentSpan"/>.</summary>
        public int CurrentSpanIndex
        {
            readonly get => _currentSpanIndex;
            private set => _currentSpanIndex = value;
        }

        /// <summary>The unread portion of the <see cref="CurrentSpan"/>.</summary>
        public readonly ReadOnlySpan<byte> UnreadSpan
        {
            [MethodImpl(InlineMethod.AggressiveOptimization)]
            get => _currentSpan.Slice(_currentSpanIndex);
        }

        /// <summary>The total number of <see cref="byte"/>'s processed by the reader.</summary>
        public long Consumed
        {
            readonly get => _consumed;
            private set => _consumed = value;
        }

        /// <summary>Remaining <see cref="byte"/>'s in the reader's <see cref="Sequence"/>.</summary>
        public readonly long Remaining => Length - _consumed;

        /// <summary>Count of <see cref="byte"/> in the reader's <see cref="Sequence"/>.</summary>
        public readonly long Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_length < 0L)
                {
                    // Cast-away readonly to initialize lazy field
                    Volatile.Write(ref Unsafe.AsRef(_length), Sequence.Length);
                }
                return _length;
            }
        }

        /// <summary>Peeks at the next value without advancing the reader.</summary>
        /// <param name="value">The next value or default if at the end.</param>
        /// <returns>False if at the end of the reader.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryPeek(out byte value)
        {
            if (_moreData)
            {
                value = _currentSpan[_currentSpanIndex];
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>Peeks at the next value at specific offset without advancing the reader.</summary>
        /// <param name="offset">The offset from current position.</param>
        /// <param name="value">The next value, or the default value if at the end of the reader.</param>
        /// <returns><c>true</c> if the reader is not at its end and the peek operation succeeded; <c>false</c> if at the end of the reader.</returns>
        public readonly bool TryPeek(long offset, out byte value)
        {
            if (offset < 0L) { ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.offset); }

            // If we've got data and offset is not out of bounds
            if (!_moreData || Remaining <= offset)
            {
                value = default;
                return false;
            }

            // Sum CurrentSpanIndex + offset could overflow as is but the value of offset should be very large
            // because we check Remaining <= offset above so to overflow we should have a ReadOnlySequence close to 8 exabytes
            Debug.Assert(CurrentSpanIndex + offset >= 0);

            // If offset doesn't fall inside current segment move to next until we find correct one
            if ((CurrentSpanIndex + offset) <= CurrentSpan.Length - 1)
            {
                Debug.Assert(offset <= int.MaxValue);

                value = CurrentSpan[CurrentSpanIndex + (int)offset];
                return true;
            }
            else
            {
                long remainingOffset = offset - (CurrentSpan.Length - CurrentSpanIndex);
                SequencePosition nextPosition = _nextPosition;
                ReadOnlyMemory<byte> currentMemory;

                while (Sequence.TryGet(ref nextPosition, out currentMemory, advance: true))
                {
                    // Skip empty segment
                    if (currentMemory.Length > 0)
                    {
                        if (remainingOffset >= currentMemory.Length)
                        {
                            // Subtract current non consumed data
                            remainingOffset -= currentMemory.Length;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                value = currentMemory.Span[(int)remainingOffset];
                return true;
            }
        }

        /// <summary>Read the next value and advance the reader.</summary>
        /// <param name="value">The next value or default if at the end.</param>
        /// <returns>False if at the end of the reader.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRead(out byte value)
        {
            if (End)
            {
                value = default;
                return false;
            }

            value = _currentSpan[_currentSpanIndex];
            _currentSpanIndex++;
            _consumed++;

            if (_currentSpanIndex >= _currentSpan.Length)
            {
                GetNextSpan();
            }

            return true;
        }

        /// <summary>Move the reader back the specified number of items.</summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if trying to rewind a negative amount or more than <see cref="Consumed"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Rewind(long count)
        {
            ulong uCount = (ulong)count;
            if (0ul >= uCount) { return; }
            if (uCount > (ulong)Consumed) { ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count); }

            _consumed -= count;

            if (_currentSpanIndex >= count)
            {
                _currentSpanIndex -= (int)count;
                _moreData = true;
            }
            else
            {
                // Current segment doesn't have enough data, scan backward through segments
                RetreatToPreviousSpan(_consumed);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void RetreatToPreviousSpan(long consumed)
        {
            ResetReader();
            Advance(consumed);
        }

        private void ResetReader()
        {
            _currentSpanIndex = 0;
            _consumed = 0;
            _currentPosition = _sequence.Start;
            _nextPosition = _currentPosition;

            if (_sequence.TryGet(ref _nextPosition, out ReadOnlyMemory<byte> memory, advance: true))
            {
                _moreData = true;

                if (0u >= (uint)memory.Length)
                {
                    _currentSpan = default;
                    // No data in the first span, move to one with data
                    GetNextSpan();
                }
                else
                {
                    _currentSpan = memory.Span;
                }
            }
            else
            {
                // No data in any spans and at end of sequence
                _moreData = false;
                _currentSpan = default;
            }
        }

        /// <summary>Get the next segment with available data, if any.</summary>
        private void GetNextSpan()
        {
            if (!_sequence.IsSingleSegment)
            {
                SequencePosition previousNextPosition = _nextPosition;
                while (_sequence.TryGet(ref _nextPosition, out ReadOnlyMemory<byte> memory, advance: true))
                {
                    _currentPosition = previousNextPosition;
                    if ((uint)memory.Length > 0u)
                    {
                        _currentSpan = memory.Span;
                        _currentSpanIndex = 0;
                        return;
                    }
                    else
                    {
                        _currentSpan = default;
                        _currentSpanIndex = 0;
                        previousNextPosition = _nextPosition;
                    }
                }
            }
            _moreData = false;
        }

        /// <summary>Move the reader ahead the specified number of items.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(long count)
        {
            const long TooBigOrNegative = unchecked((long)0xFFFFFFFF80000000);
            if (0ul >= (ulong)(count & TooBigOrNegative) && (_currentSpan.Length - _currentSpanIndex) > (int)count)
            {
                _currentSpanIndex += (int)count;
                _consumed += count;
            }
            else
            {
                // Can't satisfy from the current span
                AdvanceToNextSpan(count);
            }
        }

        /// <summary>Unchecked helper to avoid unnecessary checks where you know count is valid.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AdvanceCurrentSpan(long count)
        {
            Debug.Assert(count >= 0);

            _consumed += count;
            _currentSpanIndex += (int)count;
            if (_currentSpanIndex >= _currentSpan.Length) { GetNextSpan(); }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AdvanceWithinSpan(long count)
        {
            // Only call this helper if you know that you are advancing in the current span
            // with valid count and there is no need to fetch the next one.
            Debug.Assert(count >= 0);

            _consumed += count;
            _currentSpanIndex += (int)count;

            Debug.Assert(_currentSpanIndex < _currentSpan.Length);
        }

        private void AdvanceToNextSpan(long count)
        {
            if (count < 0) { ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count); }

            _consumed += count;
            while (_moreData)
            {
                int remaining = _currentSpan.Length - _currentSpanIndex;

                if (remaining > count)
                {
                    _currentSpanIndex += (int)count;
                    count = 0;
                    break;
                }

                // As there may not be any further segments we need to
                // push the current index to the end of the span.
                _currentSpanIndex += remaining;
                count -= remaining;
                Debug.Assert(count >= 0);

                GetNextSpan();

                if (0L >= (ulong)count) { break; }
            }

            if (!(0L >= (ulong)count)) // count != 0
            {
                // Not enough space left- adjust for where we actually ended and throw
                _consumed -= count;
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count);
            }
        }

        /// <summary>Copies data from the current <see cref="Position"/> to the given <paramref name="destination"/> span
        /// if there is enough data to fill it.</summary>
        /// <remarks>This API is used to copy a fixed amount of data out of the sequence if possible.
        /// It does not advance the reader.
        /// To look ahead for a specific stream of data <see cref="IsNext(in ReadOnlySpan{byte}, bool)"/> can be used.</remarks>
        /// <param name="destination">Destination span to copy to.</param>
        /// <returns>True if there is enough data to completely fill the <paramref name="destination"/> span.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(Span<byte> destination)
        {
            // This API doesn't advance to facilitate conditional advancement based on the data returned.
            // We don't provide an advance option to allow easier utilizing of stack allocated destination spans.
            // (Because we can make this method readonly we can guarantee that we won't capture the span.)

            ReadOnlySpan<byte> firstSpan = UnreadSpan;
            int destLen = destination.Length;
            if ((uint)firstSpan.Length >= (uint)destLen)
            {
                firstSpan.Slice(0, destLen).CopyTo(destination);
                return true;
            }

            // Not enough in the current span to satisfy the request, fall through to the slow path
            return TryCopyMultisegment(destination);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal readonly bool TryCopyMultisegment(Span<byte> destination)
        {
            int destinationLen = destination.Length;
            if ((ulong)Remaining < (ulong)destinationLen) { return false; }

            ReadOnlySpan<byte> firstSpan = UnreadSpan;
            Debug.Assert(firstSpan.Length < destinationLen);
            firstSpan.CopyTo(destination);
            int copied = firstSpan.Length;

            SequencePosition next = _nextPosition;
            while (_sequence.TryGet(ref next, out ReadOnlyMemory<byte> nextSegment, true))
            {
                if ((uint)nextSegment.Length > 0u)
                {
                    ReadOnlySpan<byte> nextSpan = nextSegment.Span;
                    int toCopy = Math.Min(nextSpan.Length, destinationLen - copied);
                    nextSpan.Slice(0, toCopy).CopyTo(destination.Slice(copied));
                    copied += toCopy;
                    if ((uint)copied >= (uint)destinationLen) { break; }
                }
            }

            return true;
        }
    }
}
