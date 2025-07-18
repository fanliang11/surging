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
// Largely based from https://github.com/dotnet/corefxlab/blob/e075d78df60452b68d212e3333fd3f37cd28d4f0/src/System.Buffers.ReaderWriter/System/Buffers/Reader/BufferReader.cs#L38

namespace DotNetty.Buffers
{
    using System;
    using System.Buffers;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal static class ByteBufferReaderHelper
    {
        private const int FlagBitMask = 1 << 31;
        private const int IndexBitMask = ~FlagBitMask;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void GetFirstSpan(in ReadOnlySequence<byte> buffer, out ReadOnlySpan<byte> first, out SequencePosition next)
        {
            first = default;
            next = default;
            SequencePosition start = buffer.Start;
            int startIndex = start.GetInteger();
            object startObject = start.GetObject();

            if (startObject is object)
            {
                SequencePosition end = buffer.End;
                int endIndex = end.GetInteger();
                bool isMultiSegment = startObject != end.GetObject();

                // A == 0 && B == 0 means SequenceType.MultiSegment
                if (SharedConstants.TooBigOrNegative >= (uint)startIndex) // startIndex >= 0
                {
                    if (SharedConstants.TooBigOrNegative >= (uint)endIndex)  // endIndex >= 0 SequenceType.MultiSegment
                    {
                        ReadOnlySequenceSegment<byte> segment = (ReadOnlySequenceSegment<byte>)startObject;
                        first = segment.Memory.Span;
                        if (isMultiSegment)
                        {
                            first = first.Slice(startIndex);
                            next = new SequencePosition(segment.Next, 0);
                        }
                        else
                        {
                            first = first.Slice(startIndex, endIndex - startIndex);
                        }
                    }
                    else
                    {
                        if (isMultiSegment) { ThrowHelper.ThrowInvalidOperationException_EndPositionNotReached(); }

                        first = new ReadOnlySpan<byte>((byte[])startObject, startIndex, (endIndex & IndexBitMask) - startIndex);
                    }
                }
                else
                {
                    first = GetFirstSpanSlow(startObject, startIndex, endIndex, isMultiSegment);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ReadOnlySpan<byte> GetFirstSpanSlow(object startObject, int startIndex, int endIndex, bool isMultiSegment)
        {
            Debug.Assert(startIndex < 0 || endIndex < 0);
            if (isMultiSegment) { ThrowHelper.ThrowInvalidOperationException_EndPositionNotReached(); }

            // The type == char check here is redundant. However, we still have it to allow
            // the JIT to see when that the code is unreachable and eliminate it.
            // A == 1 && B == 1 means SequenceType.String
            //if (typeof(T) == typeof(char) && endIndex < 0)
            //{
            //    var memory = (ReadOnlyMemory<T>)(object)((string)startObject).AsMemory();

            //    // No need to remove the FlagBitMask since (endIndex - startIndex) == (endIndex & ReadOnlySequence.IndexBitMask) - (startIndex & ReadOnlySequence.IndexBitMask)
            //    return memory.Span.Slice(startIndex & IndexBitMask, endIndex - startIndex);
            //}
            //else // endIndex >= 0, A == 1 && B == 0 means SequenceType.MemoryManager
            {
                startIndex &= IndexBitMask;
                return ((MemoryManager<byte>)startObject).Memory.Span.Slice(startIndex, endIndex - startIndex);
            }
        }
    }
}
