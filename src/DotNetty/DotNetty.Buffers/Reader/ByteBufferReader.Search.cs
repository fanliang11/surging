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
// Largely based from https://github.com/dotnet/corefx/blob/release/3.0/src/System.Memory/src/System/Buffers/SequenceReader.Search.cs

namespace DotNetty.Buffers
{
    using System;
    using System.Buffers;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using DotNetty.Common.Internal;

    partial struct ByteBufferReader
    {
        /// <summary>Try to read everything up to the given <paramref name="delimiter"/>.</summary>
        /// <param name="span">The read data, if any.</param>
        /// <param name="delimiter">The delimiter to look for.</param>
        /// <param name="advancePastDelimiter">True to move past the <paramref name="delimiter"/> if found.</param>
        /// <returns>True if the <paramref name="delimiter"/> was found.</returns>
        public bool TryReadTo(out ReadOnlySpan<byte> span, byte delimiter, bool advancePastDelimiter = true)
        {
            ReadOnlySpan<byte> remaining = UnreadSpan;
#if NET
            int index = remaining.IndexOf(delimiter);
#else
            int index = SpanHelpers.IndexOf(ref MemoryMarshal.GetReference(remaining), delimiter, remaining.Length);
#endif

            uint uIndex = (uint)index;
            if (SharedConstants.TooBigOrNegative >= uIndex) // index != -1
            {
                span = 0u >= uIndex ? default : remaining.Slice(0, index);
                AdvanceCurrentSpan(index + (advancePastDelimiter ? 1 : 0));
                return true;
            }

            return TryReadToSlow(out span, delimiter, advancePastDelimiter);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool TryReadToSlow(out ReadOnlySpan<byte> span, byte delimiter, bool advancePastDelimiter)
        {
            if (!TryReadToInternal(out ReadOnlySequence<byte> sequence, delimiter, advancePastDelimiter, _currentSpan.Length - _currentSpanIndex))
            {
                span = default;
                return false;
            }

            span = sequence.IsSingleSegment ? sequence.First.Span : sequence.ToArray();
            return true;
        }

        /// <summary>Try to read everything up to the given <paramref name="delimiter"/>, ignoring delimiters that are
        /// preceded by <paramref name="delimiterEscape"/>.</summary>
        /// <param name="span">The read data, if any.</param>
        /// <param name="delimiter">The delimiter to look for.</param>
        /// <param name="delimiterEscape">If found prior to <paramref name="delimiter"/> it will skip that occurrence.</param>
        /// <param name="advancePastDelimiter">True to move past the <paramref name="delimiter"/> if found.</param>
        /// <returns>True if the <paramref name="delimiter"/> was found.</returns>
        public bool TryReadTo(out ReadOnlySpan<byte> span, byte delimiter, byte delimiterEscape, bool advancePastDelimiter = true)
        {
            ReadOnlySpan<byte> remaining = UnreadSpan;
#if NET
            int index = remaining.IndexOf(delimiter);
#else
            int index = SpanHelpers.IndexOf(ref MemoryMarshal.GetReference(remaining), delimiter, remaining.Length);
#endif

            if ((index > 0 && remaining[index - 1] != delimiterEscape) || 0u >= (uint)index)
            {
                span = remaining.Slice(0, index);
                AdvanceCurrentSpan(index + (advancePastDelimiter ? 1 : 0));
                return true;
            }

            // This delimiter might be skipped, go down the slow path
            return TryReadToSlow(out span, delimiter, delimiterEscape, index, advancePastDelimiter);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool TryReadToSlow(out ReadOnlySpan<byte> span, byte delimiter, byte delimiterEscape, int index, bool advancePastDelimiter)
        {
            if (!TryReadToSlow(out ReadOnlySequence<byte> sequence, delimiter, delimiterEscape, index, advancePastDelimiter))
            {
                span = default;
                return false;
            }

            Debug.Assert(sequence.Length > 0);
            span = sequence.IsSingleSegment ? sequence.First.Span : sequence.ToArray();
            return true;
        }

        private bool TryReadToSlow(out ReadOnlySequence<byte> sequence, byte delimiter, byte delimiterEscape, int index, bool advancePastDelimiter)
        {
            ByteBufferReader copy = this;

            ReadOnlySpan<byte> remaining = UnreadSpan;
            bool priorEscape = false;

            do
            {
                uint uIndex = (uint)index;
                if (SharedConstants.TooBigOrNegative >= uIndex) // index >= 0
                {
                    if (0u >= uIndex && priorEscape) // index == 0
                    {
                        // We were in the escaped state, so skip this delimiter
                        priorEscape = false;
                        Advance(index + 1);
                        remaining = UnreadSpan;
                        goto Continue;
                    }
                    else if (index > 0 && remaining[index - 1] == delimiterEscape)
                    {
                        // This delimiter might be skipped

                        // Count our escapes
                        int escapeCount = 1;
                        var idx = SpanHelpers.LastIndexNotOf(
                                ref MemoryMarshal.GetReference(remaining),
                                delimiterEscape,
                                index - 1);
                        if ((uint)idx > SharedConstants.TooBigOrNegative && priorEscape) // i < 0
                        {
                            // Started and ended with escape, increment once more
                            escapeCount++;
                        }
                        escapeCount += index - 2 - idx;

                        if ((escapeCount & 1) != 0)
                        {
                            // An odd escape count means we're currently escaped,
                            // skip the delimiter and reset escaped state.
                            Advance(index + 1);
                            priorEscape = false;
                            remaining = UnreadSpan;
                            goto Continue;
                        }
                    }

                    // Found the delimiter. Move to it, slice, then move past it.
                    AdvanceCurrentSpan(index);

                    sequence = _sequence.Slice(copy.Position, Position);
                    if (advancePastDelimiter)
                    {
                        Advance(1);
                    }
                    return true;
                }
                else
                {
                    // No delimiter, need to check the end of the span for odd number of escapes then advance
                    var remainingLen = remaining.Length;
                    if ((uint)remainingLen > 0u && remaining[remainingLen - 1] == delimiterEscape)
                    {
                        int escapeCount = 1;
                        var idx = SpanHelpers.LastIndexNotOf(
                                ref MemoryMarshal.GetReference(remaining),
                                delimiterEscape,
                                remainingLen - 1);
                        escapeCount += remainingLen - 2 - idx;

                        if ((uint)idx > SharedConstants.TooBigOrNegative && priorEscape) // idx < 0
                        {
                            priorEscape = 0u >= (uint)(escapeCount & 1);  // equivalent to incrementing escapeCount before setting priorEscape
                        }
                        else
                        {
                            priorEscape = (escapeCount & 1) != 0;
                        }
                    }
                    else
                    {
                        priorEscape = false;
                    }
                }

                // Nothing in the current span, move to the end, checking for the skip delimiter
                AdvanceCurrentSpan(remaining.Length);
                remaining = _currentSpan;

            Continue:
#if NET
                index = remaining.IndexOf(delimiter);
#else
                index = SpanHelpers.IndexOf(ref MemoryMarshal.GetReference(remaining), delimiter, remaining.Length);
#endif
            } while (!End);

            // Didn't find anything, reset our original state.
            this = copy;
            sequence = default;
            return false;
        }

        /// <summary>Try to read everything up to the given <paramref name="delimiter"/>.</summary>
        /// <param name="sequence">The read data, if any.</param>
        /// <param name="delimiter">The delimiter to look for.</param>
        /// <param name="advancePastDelimiter">True to move past the <paramref name="delimiter"/> if found.</param>
        /// <returns>True if the <paramref name="delimiter"/> was found.</returns>
        public bool TryReadTo(out ReadOnlySequence<byte> sequence, byte delimiter, bool advancePastDelimiter = true)
        {
            return TryReadToInternal(out sequence, delimiter, advancePastDelimiter);
        }

        private bool TryReadToInternal(out ReadOnlySequence<byte> sequence, byte delimiter, bool advancePastDelimiter, int skip = 0)
        {
            Debug.Assert(skip >= 0);
            ByteBufferReader copy = this;
            if (skip > 0) { Advance(skip); }
            ReadOnlySpan<byte> remaining = UnreadSpan;

            while (_moreData)
            {
#if NET
                int index = remaining.IndexOf(delimiter);
#else
                int index = SpanHelpers.IndexOf(ref MemoryMarshal.GetReference(remaining), delimiter, remaining.Length);
#endif
                uint uIndex = (uint)index;
                if (SharedConstants.TooBigOrNegative >= uIndex) // index != -1
                {
                    // Found the delimiter. Move to it, slice, then move past it.
                    if (uIndex > 0u) // 此时 index 为非负值
                    {
                        AdvanceCurrentSpan(index);
                    }

                    sequence = _sequence.Slice(copy.Position, Position);
                    if (advancePastDelimiter)
                    {
                        Advance(1);
                    }
                    return true;
                }

                AdvanceCurrentSpan(remaining.Length);
                remaining = _currentSpan;
            }

            // Didn't find anything, reset our original state.
            this = copy;
            sequence = default;
            return false;
        }

        /// <summary>Try to read everything up to the given <paramref name="delimiter"/>, ignoring delimiters that are
        /// preceded by <paramref name="delimiterEscape"/>.</summary>
        /// <param name="sequence">The read data, if any.</param>
        /// <param name="delimiter">The delimiter to look for.</param>
        /// <param name="delimiterEscape">If found prior to <paramref name="delimiter"/> it will skip that occurrence.</param>
        /// <param name="advancePastDelimiter">True to move past the <paramref name="delimiter"/> if found.</param>
        /// <returns>True if the <paramref name="delimiter"/> was found.</returns>
        public bool TryReadTo(out ReadOnlySequence<byte> sequence, byte delimiter, byte delimiterEscape, bool advancePastDelimiter = true)
        {
            ByteBufferReader copy = this;

            ReadOnlySpan<byte> remaining = UnreadSpan;
            bool priorEscape = false;

            while (_moreData)
            {
#if NET
                int index = remaining.IndexOf(delimiter);
#else
                int index = SpanHelpers.IndexOf(ref MemoryMarshal.GetReference(remaining), delimiter, remaining.Length);
#endif
                uint uIndex = (uint)index;
                if (SharedConstants.TooBigOrNegative >= uIndex) // index != -1
                {
                    if (0u >= uIndex && priorEscape) // index == 0
                    {
                        // We were in the escaped state, so skip this delimiter
                        priorEscape = false;
                        Advance(index + 1);
                        remaining = UnreadSpan;
                        continue;
                    }
                    else if (uIndex > 0u && remaining[index - 1] == delimiterEscape) // 此时 index 为非负值
                    {
                        // This delimiter might be skipped

                        // Count our escapes
                        var idx = SpanHelpers.LastIndexNotOf(
                                ref MemoryMarshal.GetReference(remaining),
                                delimiterEscape,
                                index);
                        int escapeCount = SharedConstants.TooBigOrNegative >= (uint)idx ? index - idx - 1 : index;

                        if (escapeCount == index && priorEscape)
                        {
                            // Started and ended with escape, increment once more
                            escapeCount++;
                        }

                        priorEscape = false;
                        if ((escapeCount & 1) != 0)
                        {
                            // Odd escape count means we're in the escaped state, so skip this delimiter
                            Advance(index + 1);
                            remaining = UnreadSpan;
                            continue;
                        }
                    }

                    // Found the delimiter. Move to it, slice, then move past it.
                    if (uIndex > 0u) { Advance(index); } // 此时 index 为非负值

                    sequence = _sequence.Slice(copy.Position, Position);
                    if (advancePastDelimiter) { Advance(1); }
                    return true;
                }

                // No delimiter, need to check the end of the span for odd number of escapes then advance
                {
                    var remainingLen = remaining.Length;
                    var idx = SpanHelpers.LastIndexNotOf(
                            ref MemoryMarshal.GetReference(remaining),
                            delimiterEscape,
                            remainingLen);
                    int escapeCount = SharedConstants.TooBigOrNegative >= (uint)idx ? remainingLen - idx - 1 : remainingLen;

                    if (priorEscape && escapeCount == remainingLen)
                    {
                        escapeCount++;
                    }
                    priorEscape = escapeCount % 2 != 0;
                }

                // Nothing in the current span, move to the end, checking for the skip delimiter
                Advance(remaining.Length);
                remaining = _currentSpan;
            }

            // Didn't find anything, reset our original state.
            this = copy;
            sequence = default;
            return false;
        }

        /// <summary>Try to read everything up to the given <paramref name="delimiters"/>.</summary>
        /// <param name="span">The read data, if any.</param>
        /// <param name="delimiters">The delimiters to look for.</param>
        /// <param name="advancePastDelimiter">True to move past the first found instance of any of the given <paramref name="delimiters"/>.</param>
        /// <returns>True if any of the <paramref name="delimiters"/> were found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReadToAny(out ReadOnlySpan<byte> span, in ReadOnlySpan<byte> delimiters, bool advancePastDelimiter = true)
        {
            ReadOnlySpan<byte> remaining = UnreadSpan;
#if NET
            int index = delimiters.Length == 2
                ? remaining.IndexOfAny(delimiters[0], delimiters[1])
                : remaining.IndexOfAny(delimiters);
#else
            var index = SpanHelpers.IndexOfAny(ref MemoryMarshal.GetReference(remaining), remaining.Length, ref MemoryMarshal.GetReference(delimiters), delimiters.Length);
#endif

            if (SharedConstants.TooBigOrNegative >= (uint)index) // index != -1
            {
                span = remaining.Slice(0, index);
                Advance(index + (advancePastDelimiter ? 1 : 0));
                return true;
            }

            return TryReadToAnySlow(out span, delimiters, advancePastDelimiter);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool TryReadToAnySlow(out ReadOnlySpan<byte> span, in ReadOnlySpan<byte> delimiters, bool advancePastDelimiter)
        {
            if (!TryReadToAnyInternal(out ReadOnlySequence<byte> sequence, delimiters, advancePastDelimiter, _currentSpan.Length - _currentSpanIndex))
            {
                span = default;
                return false;
            }

            span = sequence.IsSingleSegment ? sequence.First.Span : sequence.ToArray();
            return true;
        }

        /// <summary>Try to read everything up to the given <paramref name="delimiters"/>.</summary>
        /// <param name="sequence">The read data, if any.</param>
        /// <param name="delimiters">The delimiters to look for.</param>
        /// <param name="advancePastDelimiter">True to move past the first found instance of any of the given <paramref name="delimiters"/>.</param>
        /// <returns>True if any of the <paramref name="delimiters"/> were found.</returns>
        public bool TryReadToAny(out ReadOnlySequence<byte> sequence, in ReadOnlySpan<byte> delimiters, bool advancePastDelimiter = true)
        {
            return TryReadToAnyInternal(out sequence, delimiters, advancePastDelimiter);
        }

        private bool TryReadToAnyInternal(out ReadOnlySequence<byte> sequence, in ReadOnlySpan<byte> delimiters, bool advancePastDelimiter, int skip = 0)
        {
            ByteBufferReader copy = this;
            if (skip > 0) { Advance(skip); }
            ReadOnlySpan<byte> remaining = UnreadSpan;

            ref byte delimiterSpace = ref MemoryMarshal.GetReference(delimiters);

            while (!End)
            {
#if NET
                int index = delimiters.Length == 2
                    ? remaining.IndexOfAny(delimiters[0], delimiters[1])
                    : remaining.IndexOfAny(delimiters);
#else
                int index = SpanHelpers.IndexOfAny(ref MemoryMarshal.GetReference(remaining), remaining.Length, ref delimiterSpace, delimiters.Length);
#endif
                uint uIndex = (uint)index;
                if (SharedConstants.TooBigOrNegative >= uIndex) // index != -1
                {
                    // Found one of the delimiters. Move to it, slice, then move past it.
                    if (uIndex > 0u) { AdvanceCurrentSpan(index); } // 此时 index 为非负值

                    sequence = _sequence.Slice(copy.Position, Position);
                    if (advancePastDelimiter) { Advance(1); }
                    return true;
                }

                Advance(remaining.Length);
                remaining = _currentSpan;
            }

            // Didn't find anything, reset our original state.
            this = copy;
            sequence = default;
            return false;
        }

        /// <summary>
        /// Try to read everything up to the given <paramref name="delimiter"/>.
        /// </summary>
        /// <param name="span">The read data, if any.</param>
        /// <param name="delimiter">The delimiter to look for.</param>
        /// <param name="advancePastDelimiter">True to move past the <paramref name="delimiter"/> if found.</param>
        /// <returns>True if the <paramref name="delimiter"/> was found.</returns>
        public bool TryReadTo(out ReadOnlySpan<byte> span, ReadOnlySpan<byte> delimiter, bool advancePastDelimiter = true)
        {
            ReadOnlySpan<byte> remaining = UnreadSpan;
#if NET
            int index = remaining.IndexOf(delimiter);
#else
            int index = SpanHelpers.IndexOf(ref MemoryMarshal.GetReference(remaining), remaining.Length, ref MemoryMarshal.GetReference(delimiter), delimiter.Length);
#endif

            if (index >= 0)
            {
                span = remaining.Slice(0, index);
                AdvanceCurrentSpan(index + (advancePastDelimiter ? delimiter.Length : 0));
                return true;
            }

            // This delimiter might be skipped, go down the slow path
            return TryReadToSlow(out span, delimiter, advancePastDelimiter);
        }

        private bool TryReadToSlow(out ReadOnlySpan<byte> span, ReadOnlySpan<byte> delimiter, bool advancePastDelimiter)
        {
            if (!TryReadTo(out ReadOnlySequence<byte> sequence, delimiter, advancePastDelimiter))
            {
                span = default;
                return false;
            }

            Debug.Assert(sequence.Length > 0);
            span = sequence.IsSingleSegment ? sequence.First.Span : sequence.ToArray();
            return true;
        }

        /// <summary>Try to read data until the entire given <paramref name="delimiter"/> matches.</summary>
        /// <param name="sequence">The read data, if any.</param>
        /// <param name="delimiter">The multi (byte) delimiter.</param>
        /// <param name="advancePastDelimiter">True to move past the <paramref name="delimiter"/> if found.</param>
        /// <returns>True if the <paramref name="delimiter"/> was found.</returns>
        public bool TryReadTo(out ReadOnlySequence<byte> sequence, in ReadOnlySpan<byte> delimiter, bool advancePastDelimiter = true)
        {
            if (0u >= (uint)delimiter.Length)
            {
                sequence = default;
                return true;
            }

            ByteBufferReader copy = this;

            bool advanced = false;
            while (!End)
            {
                if (!TryReadTo(out sequence, delimiter[0], advancePastDelimiter: false))
                {
                    this = copy;
                    return false;
                }

                if (1u >= (uint)delimiter.Length) // 此时 delimiter.Length 最小值为 1
                {
                    if (advancePastDelimiter)
                    {
                        Advance(1);
                    }
                    return true;
                }

                if (IsNext(delimiter))
                {
                    // Probably a faster way to do this, potentially by avoiding the Advance in the previous TryReadTo call
                    if (advanced)
                    {
                        sequence = copy._sequence.Slice(copy._consumed, _consumed - copy._consumed);
                    }

                    if (advancePastDelimiter)
                    {
                        Advance(delimiter.Length);
                    }
                    return true;
                }
                else
                {
                    Advance(1);
                    advanced = true;
                }
            }

            this = copy;
            sequence = default;
            return false;
        }

        /// <summary>Advance until the given <paramref name="delimiter"/>, if found.</summary>
        /// <param name="delimiter">The delimiter to search for.</param>
        /// <param name="advancePastDelimiter">True to move past the <paramref name="delimiter"/> if found.</param>
        /// <returns>True if the given <paramref name="delimiter"/> was found.</returns>
        public bool TryAdvanceTo(byte delimiter, bool advancePastDelimiter = true)
        {
            ReadOnlySpan<byte> remaining = UnreadSpan;
#if NET
            int index = remaining.IndexOf(delimiter);
#else
            int index = SpanHelpers.IndexOf(ref MemoryMarshal.GetReference(remaining), delimiter, remaining.Length);
#endif
            if (SharedConstants.TooBigOrNegative >= (uint)index) // ndex != -1
            {
                Advance(advancePastDelimiter ? index + 1 : index);
                return true;
            }

            return TryReadToInternal(out _, delimiter, advancePastDelimiter);
        }

        /// <summary>Advance until any of the given <paramref name="delimiters"/>, if found.</summary>
        /// <param name="delimiters">The delimiters to search for.</param>
        /// <param name="advancePastDelimiter">True to move past the first found instance of any of the given <paramref name="delimiters"/>.</param>
        /// <returns>True if any of the given <paramref name="delimiters"/> were found.</returns>
        public bool TryAdvanceToAny(in ReadOnlySpan<byte> delimiters, bool advancePastDelimiter = true)
        {
            ReadOnlySpan<byte> remaining = UnreadSpan;
#if NET
            int index = remaining.IndexOfAny(delimiters);
#else
            int index = SpanHelpers.IndexOfAny(ref MemoryMarshal.GetReference(remaining), remaining.Length, ref MemoryMarshal.GetReference(delimiters), delimiters.Length);
#endif
            if (SharedConstants.TooBigOrNegative >= (uint)index) // ndex != -1
            {
                AdvanceCurrentSpan(index + (advancePastDelimiter ? 1 : 0));
                return true;
            }

            return TryReadToAnyInternal(out _, delimiters, advancePastDelimiter);
        }

        /// <summary>Advance past consecutive instances of the given <paramref name="value"/>.</summary>
        /// <returns>How many positions the reader has been advanced.</returns>
        public long AdvancePast(byte value)
        {
            long start = _consumed;

            do
            {
                // Advance past all matches in the current span
                var searchSpan = _currentSpan.Slice(_currentSpanIndex);
                var idx = SpanHelpers.IndexNotOf(
                        ref MemoryMarshal.GetReference(searchSpan),
                        value,
                        searchSpan.Length);
                int advanced = SharedConstants.TooBigOrNegative >= (uint)idx ? idx : _currentSpan.Length - _currentSpanIndex;

                if (0u >= (uint)advanced)
                {
                    // Didn't advance at all in this span, exit.
                    break;
                }

                AdvanceCurrentSpan(advanced);

                // If we're at postion 0 after advancing and not at the End,
                // we're in a new span and should continue the loop.
            } while (0u >= (uint)_currentSpanIndex && !End);

            return _consumed - start;
        }

        /// <summary>Skip consecutive instances of any of the given <paramref name="values"/>.</summary>
        /// <returns>How many positions the reader has been advanced.</returns>
        public long AdvancePastAny(in ReadOnlySpan<byte> values)
        {
            long start = _consumed;

            do
            {
                // Advance past all matches in the current span
                var searchSpan = _currentSpan.Slice(_currentSpanIndex);
                var idx = SpanHelpers.IndexNotOfAny(
                        ref MemoryMarshal.GetReference(searchSpan),
                        searchSpan.Length,
                        ref MemoryMarshal.GetReference(values),
                        values.Length);
                int advanced = SharedConstants.TooBigOrNegative >= (uint)idx ? _currentSpanIndex + idx : _currentSpan.Length - _currentSpanIndex;
                if (0u >= (uint)advanced)
                {
                    // Didn't advance at all in this span, exit.
                    break;
                }

                AdvanceCurrentSpan(advanced);

                // If we're at postion 0 after advancing and not at the End,
                // we're in a new span and should continue the loop.
            } while (0u >= (uint)_currentSpanIndex && !End);

            return _consumed - start;
        }

        /// <summary>Advance past consecutive instances of any of the given values.</summary>
        /// <returns>How many positions the reader has been advanced.</returns>
        public long AdvancePastAny(byte value0, byte value1, byte value2, byte value3)
        {
            long start = _consumed;

            do
            {
                // Advance past all matches in the current span
                var searchSpan = _currentSpan.Slice(_currentSpanIndex);
                var idx = SpanHelpers.IndexNotOfAny(
                        ref MemoryMarshal.GetReference(searchSpan),
                        value0, value1, value2, value3,
                        searchSpan.Length);
                int advanced = SharedConstants.TooBigOrNegative >= (uint)idx ? idx : _currentSpan.Length - _currentSpanIndex;

                if (0u >= (uint)advanced)
                {
                    // Didn't advance at all in this span, exit.
                    break;
                }

                AdvanceCurrentSpan(advanced);

                // If we're at postion 0 after advancing and not at the End,
                // we're in a new span and should continue the loop.
            } while (0u >= (uint)_currentSpanIndex && !End);

            return _consumed - start;
        }

        /// <summary>Advance past consecutive instances of any of the given values.</summary>
        /// <returns>How many positions the reader has been advanced.</returns>
        public long AdvancePastAny(byte value0, byte value1, byte value2)
        {
            long start = _consumed;

            do
            {
                // Advance past all matches in the current span
                var searchSpan = _currentSpan.Slice(_currentSpanIndex);
                var idx = SpanHelpers.IndexNotOfAny(
                        ref MemoryMarshal.GetReference(searchSpan),
                        value0, value1, value2,
                        searchSpan.Length);
                int advanced = SharedConstants.TooBigOrNegative >= (uint)idx ? idx : _currentSpan.Length - _currentSpanIndex;

                if (0u >= (uint)advanced)
                {
                    // Didn't advance at all in this span, exit.
                    break;
                }

                AdvanceCurrentSpan(advanced);

                // If we're at postion 0 after advancing and not at the End,
                // we're in a new span and should continue the loop.
            } while (0u >= (uint)_currentSpanIndex && !End);

            return _consumed - start;
        }

        /// <summary>Advance past consecutive instances of any of the given values.</summary>
        /// <returns>How many positions the reader has been advanced.</returns>
        public long AdvancePastAny(byte value0, byte value1)
        {
            long start = _consumed;

            do
            {
                // Advance past all matches in the current span
                var searchSpan = _currentSpan.Slice(_currentSpanIndex);
                var idx = SpanHelpers.IndexNotOfAny(
                        ref MemoryMarshal.GetReference(searchSpan),
                        value0, value1,
                        searchSpan.Length);
                int advanced = SharedConstants.TooBigOrNegative >= (uint)idx ? idx : _currentSpan.Length - _currentSpanIndex;

                if (0u >= (uint)advanced)
                {
                    // Didn't advance at all in this span, exit.
                    break;
                }

                AdvanceCurrentSpan(advanced);

                // If we're at postion 0 after advancing and not at the End,
                // we're in a new span and should continue the loop.
            } while (0u >= (uint)_currentSpanIndex && !End);

            return _consumed - start;
        }

        /// <summary>
        /// Moves the reader to the end of the sequence.
        /// </summary>
        public void AdvanceToEnd()
        {
            if (_moreData)
            {
                Consumed = Length;
                CurrentSpan = default;
                CurrentSpanIndex = 0;
                _currentPosition = Sequence.End;
                _nextPosition = default;
                _moreData = false;
            }
        }

        /// <summary>Check to see if the given <paramref name="next"/> value is next.</summary>
        /// <param name="next">The value to compare the next items to.</param>
        /// <param name="advancePast">Move past the <paramref name="next"/> value if found.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNext(byte next, bool advancePast = false)
        {
            if (End) { return false; }

            if (_currentSpan[_currentSpanIndex] == next)
            {
                if (advancePast) { AdvanceCurrentSpan(1); }
                return true;
            }
            return false;
        }

        /// <summary>Check to see if the given <paramref name="next"/> values are next.</summary>
        /// <param name="next">The span to compare the next items to.</param>
        /// <param name="advancePast">Move past the <paramref name="next"/> values if found.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNext(in ReadOnlySpan<byte> next, bool advancePast = false)
        {
            ReadOnlySpan<byte> unread = UnreadSpan;
            if (unread.StartsWith(next))
            {
                if (advancePast) { AdvanceCurrentSpan(next.Length); }
                return true;
            }

            // Only check the slow path if there wasn't enough to satisfy next
            return (uint)unread.Length < (uint)next.Length && IsNextSlow(next, advancePast);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private unsafe bool IsNextSlow(ReadOnlySpan<byte> next, bool advancePast)
        {
            ReadOnlySpan<byte> currentSpan = UnreadSpan;

            // We should only come in here if we need more data than we have in our current span
            Debug.Assert(currentSpan.Length < next.Length);

            int fullLength = next.Length;
            SequencePosition nextPosition = _nextPosition;

            while (next.StartsWith(currentSpan))
            {
                if (next.Length == currentSpan.Length)
                {
                    // Fully matched
                    if (advancePast) { Advance(fullLength); }
                    return true;
                }

                // Need to check the next segment
                while (true)
                {
                    if (!_sequence.TryGet(ref nextPosition, out ReadOnlyMemory<byte> nextSegment, advance: true))
                    {
                        // Nothing left
                        return false;
                    }

                    if ((uint)nextSegment.Length > 0u)
                    {
                        next = next.Slice(currentSpan.Length);
                        currentSpan = nextSegment.Span;
                        if ((uint)currentSpan.Length > (uint)next.Length)
                        {
                            currentSpan = currentSpan.Slice(0, next.Length);
                        }
                        break;
                    }
                }
            }

            return false;
        }
    }
}
