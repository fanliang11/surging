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

using System;
using System.Runtime.CompilerServices;
using DotNetty.Common.Utilities;

namespace DotNetty.Buffers
{
    #region -- ExceptionArgument --

    /// <summary>The convention for this enum is using the argument name as the enum name</summary>
    internal enum ExceptionArgument
    {
        s,

        pi,
        fi,
        ts,

        asm,
        buf,
        dst,
        key,
        obj,
        src,
        str,

        data,
        leak,
        func,
        path,
        type,
        name,
        item,
        list,
        pool,

        alloc,
        array,
        count,
        types,
        value,
        other,
        match,
        index,
        inner,

        buffer,
        output,
        values,
        source,
        policy,
        offset,
        method,
        target,
        length,
        member,

        buffers,
        options,
        feature,
        manager,
        invoker,
        newSize,

        assembly,
        capacity,
        dstIndex,
        encoding,
        fullName,
        srcIndex,
        protocol,
        typeInfo,
        typeName,

        allocator,
        decrement,
        defaultFn,
        fieldInfo,
        increment,
        predicate,

        returnType,
        memberInfo,
        bufferSize,
        byteBuffer,
        collection,
        startIndex,
        expression,
        nHeapArena,

        maxCapacity,
        destination,
        reqCapacity,
        directories,
        dirEnumArgs,
        frameLength,

        nDirectArena,
        propertyInfo,
        elementIndex,
        initialArray,
        instanceType,
        valueFactory,

        attributeType,
        initialBuffer,
        maxAllocation,

        parameterTypes,
        maxFrameLength,
        newMaxCapacity,
        trackedByteBuf,
        minNewCapacity,

        initialCapacity,

        minWritableBytes,

        assemblyPredicate,
        lengthFieldOffset,
        qualifiedTypeName,

        includedAssemblies,

        initialBytesToStrip,

        maxCachedBufferCapacity,

        freeSweepAllocationThreshold,

    }

    #endregion

    #region -- ExceptionResource --

    /// <summary>The convention for this enum is using the resource name as the enum name</summary>
    internal enum ExceptionResource
    {
    }

    #endregion

     partial class ThrowHelper
    {
        #region -- Exception --

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T FromException_ShouldNotReachHere<T>()
        {
            throw GetException();

            static Exception GetException()
            {
                return new Exception("should not reach here");
            }
        }

        #endregion

        #region -- ArgumentException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_NeedMoreData()
        {
            throw GetException();

            static ArgumentException GetException()
            {
                return new ArgumentException("Destination is too short.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_DestinationTooShort()
        {
            throw GetException();

            static ArgumentException GetException()
            {
                return new ArgumentException("Destination is too short.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_InitialCapacityMaxCapacity(int initialCapacity, int maxCapacity)
        {
            throw GetArgumentException(initialCapacity, maxCapacity);

            static ArgumentException GetArgumentException(int initialCapacity, int maxCapacity)
            {
                return new ArgumentException($"initialCapacity({initialCapacity}) > maxCapacity({maxCapacity})");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_ExpectedPowerOf2()
        {
            throw GetArgumentException();

            static ArgumentException GetArgumentException()
            {
                return new ArgumentException("Expected power of 2", "pageSize");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_CheckMaxOrder30(int maxOrder)
        {
            throw GetArgumentException(maxOrder);

            static ArgumentException GetArgumentException(int maxOrder)
            {
                return new ArgumentException("maxOrder should be < 30, but is: " + maxOrder, nameof(maxOrder));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_CheckMaxOrder14(int maxOrder)
        {
            throw GetArgumentException(maxOrder);

            static ArgumentException GetArgumentException(int maxOrder)
            {
                return new ArgumentException("maxOrder: " + maxOrder + " (expected: 0-14)", nameof(maxOrder));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_CheckMaxNumComponents(int maxNumComponents)
        {
            throw GetArgumentException(maxNumComponents);

            static ArgumentException GetArgumentException(int maxNumComponents)
            {
                return new ArgumentException("maxNumComponents: " + maxNumComponents + " (expected: >= 1)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_LenIsTooBig()
        {
            throw GetArgumentException();

            static ArgumentException GetArgumentException()
            {
                return new ArgumentException("The total length of the specified buffers is too big.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_NonNegative()
        {
            throw GetArgumentException();

            static ArgumentException GetArgumentException()
            {
                return new ArgumentException("All indexes and lengths must be non-negative");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_CopyArray()
        {
            throw GetArgumentException();

            static ArgumentException GetArgumentException()
            {
                return new ArgumentException("The total length of the specified arrays is too big.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_CopyBuffer()
        {
            throw GetArgumentException();

            static ArgumentException GetArgumentException()
            {
                return new ArgumentException("The total length of the specified buffers is too big.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_InitialCapacity(int initialCapacity, int maxCapacity)
        {
            throw GetArgumentException(initialCapacity, maxCapacity);

            static ArgumentException GetArgumentException(int initialCapacity, int maxCapacity)
            {
                return new ArgumentException($"initialCapacity({initialCapacity}) > maxCapacity({maxCapacity})");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_PageSize(int pageSize, int maxOrder, int maxChunkSize)
        {
            throw GetArgumentException(pageSize, maxOrder, maxChunkSize);

            static ArgumentException GetArgumentException(int pageSize, int maxOrder, int maxChunkSize)
            {
                return new ArgumentException($"pageSize ({pageSize}) << maxOrder ({maxOrder}) must not exceed {maxChunkSize}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_InvalidOffLen()
        {
            throw GetArgumentOutOfRangeException();

            static ArgumentException GetArgumentOutOfRangeException()
            {
                return new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_FailedToGetLargerSpan()
        {
            throw GetArgumentOutOfRangeException();

            static ArgumentException GetArgumentOutOfRangeException()
            {
                return new ArgumentException("The 'IByteBuffer' could not provide an output buffer that is large enough to continue writing.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_FailedToGetMinimumSizeSpan(int minimumSize)
        {
            throw GetArgumentOutOfRangeException(minimumSize);

            static ArgumentException GetArgumentOutOfRangeException(int minimumSize)
            {
                return new ArgumentException($"The 'IByteBuffer' could not provide an output buffer that is large enough to continue writing. Need at least {minimumSize} bytes.");
            }
        }

        #endregion

        #region -- InvalidOperationException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static InvalidOperationException GetInvalidOperationException_ShouldNotReachHere()
        {
            return new InvalidOperationException("should not reach here");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException(int capacity)
        {
            throw GetException(capacity);

            static InvalidOperationException GetException(int capacity)
            {
                return new InvalidOperationException($"Cannot advance past the end of the buffer, which has a size of {capacity}.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_Can_not_increase_by(int capacity, int readableBytes)
        {
            throw GetException(capacity, readableBytes);

            static InvalidOperationException GetException(int capacity, int readableBytes)
            {
                return new InvalidOperationException("Can't increase by " + readableBytes + " as capacity(" + capacity + ")" +
                    " would overflow " + int.MaxValue);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_EndPositionNotReached() { throw CreateInvalidOperationException_EndPositionNotReached(); }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static InvalidOperationException CreateInvalidOperationException_EndPositionNotReached()
        {
            return new InvalidOperationException("EndPositionNotReached");
        }

        #endregion

        #region -- ArgumentOutOfRangeException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException_MinimumReadableBytes(int minimumReadableBytes)
        {
            throw GetArgumentOutOfRangeException();

            ArgumentOutOfRangeException GetArgumentOutOfRangeException()
            {
                return new ArgumentOutOfRangeException("minimumReadableBytes", string.Format("minimumReadableBytes: {0} (expected: >= 0)", minimumReadableBytes));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException_InitialCapacity(int initialCapacity, int maxCapacity)
        {
            throw GetArgumentOutOfRangeException();

            ArgumentOutOfRangeException GetArgumentOutOfRangeException()
            {
                return new ArgumentOutOfRangeException("initialCapacity", string.Format("initialCapacity ({0}) must be greater than maxCapacity ({1})", initialCapacity, maxCapacity));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException_MinWritableBytes()
        {
            throw GetArgumentOutOfRangeException();

            static ArgumentOutOfRangeException GetArgumentOutOfRangeException()
            {
                return new ArgumentOutOfRangeException("minWritableBytes", "expected minWritableBytes to be greater than zero");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException_MaxCapacity(int minNewCapacity, int maxCapacity)
        {
            throw GetArgumentOutOfRangeException();

            ArgumentOutOfRangeException GetArgumentOutOfRangeException()
            {
                return new ArgumentOutOfRangeException("maxCapacity", string.Format("minNewCapacity: {0} (expected: not greater than maxCapacity({1})", minNewCapacity, maxCapacity));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException_Capacity(int newCapacity, int maxCapacity)
        {
            throw GetArgumentOutOfRangeException();

            ArgumentOutOfRangeException GetArgumentOutOfRangeException()
            {
                return new ArgumentOutOfRangeException("newCapacity", string.Format($"newCapacity: {0} (expected: 0-{1})", newCapacity, maxCapacity));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentException_Positive(int value, ExceptionArgument argument)
        {
            throw GetException(value, argument);

            static ArgumentOutOfRangeException GetException(int value, ExceptionArgument argument)
            {
                return new ArgumentOutOfRangeException(GetArgumentName(argument), $"{GetArgumentName(argument)}: {value} (expected: > 0)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentException_Positive(long value, ExceptionArgument argument)
        {
            throw GetException(value, argument);

            static ArgumentOutOfRangeException GetException(long value, ExceptionArgument argument)
            {
                return new ArgumentOutOfRangeException(GetArgumentName(argument), $"{GetArgumentName(argument)}: {value} (expected: > 0)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentException_PositiveOrZero(int value, ExceptionArgument argument)
        {
            throw GetException(value, argument);

            static ArgumentOutOfRangeException GetException(int value, ExceptionArgument argument)
            {
                return new ArgumentOutOfRangeException(GetArgumentName(argument), $"{GetArgumentName(argument)}: {value} (expected: >= 0)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentException_PositiveOrZero(long value, ExceptionArgument argument)
        {
            throw GetException(value, argument);

            static ArgumentOutOfRangeException GetException(long value, ExceptionArgument argument)
            {
                return new ArgumentOutOfRangeException(GetArgumentName(argument), $"{GetArgumentName(argument)}: {value} (expected: >= 0)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException_NeedNonNegNum(ExceptionArgument argument)
        {
            throw GetArgumentOutOfRangeException(argument);

            static ArgumentOutOfRangeException GetArgumentOutOfRangeException(ExceptionArgument argument)
            {
                return new ArgumentOutOfRangeException(GetArgumentName(argument), $"The {GetArgumentName(argument)} cannot be negative.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException_Index(int cIndex, int count)
        {
            throw GetArgumentOutOfRangeException(cIndex, count);

            static ArgumentOutOfRangeException GetArgumentOutOfRangeException(int cIndex, int count)
            {
                return new ArgumentOutOfRangeException(nameof(cIndex), $"cIndex: {cIndex} (expected: >= 0 && <= numComponents({count}))");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException_Index(int cIndex, int numComponents, int count)
        {
            throw GetArgumentOutOfRangeException(cIndex, numComponents, count);

            static ArgumentOutOfRangeException GetArgumentOutOfRangeException(int cIndex, int numComponents, int count)
            {
                return new ArgumentOutOfRangeException(nameof(cIndex), $"cIndex: {cIndex}, numComponents: {numComponents} " + $"(expected: cIndex >= 0 && cIndex + numComponents <= totalNumComponents({count}))");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException_NeedPosNum(ExceptionArgument argument, int value)
        {
            throw GetArgumentOutOfRangeException(argument, value);

            static ArgumentOutOfRangeException GetArgumentOutOfRangeException(ExceptionArgument argument, int value)
            {
                return new ArgumentOutOfRangeException(GetArgumentName(argument), value, "Positive number required.");
            }
        }

        #endregion

        #region -- IndexOutOfRangeException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIndexOutOfRangeException_Index(int index, int length, int capacity)
        {
            throw GetIndexOutOfRangeException();

            IndexOutOfRangeException GetIndexOutOfRangeException()
            {
                return new IndexOutOfRangeException(string.Format("index: {0}, length: {1} (expected: range(0, {2}))", index, length, capacity));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIndexOutOfRangeException_Index(ExceptionArgument indexName, int index, int length, int capacity)
        {
            throw GetIndexOutOfRangeException();

            IndexOutOfRangeException GetIndexOutOfRangeException()
            {
                return new IndexOutOfRangeException(string.Format("{0}: {1}, length: {2} (expected: range({1}, {3}))", indexName.ToString(), index, length, capacity));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIndexOutOfRangeException_SrcIndex(int srcIndex)
        {
            throw GetIndexOutOfRangeException();

            IndexOutOfRangeException GetIndexOutOfRangeException()
            {
                return new IndexOutOfRangeException(string.Format("srcIndex: {0}", srcIndex));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIndexOutOfRangeException_SrcIndex(int srcIndex, int length, int srcCapacity)
        {
            throw GetIndexOutOfRangeException();

            IndexOutOfRangeException GetIndexOutOfRangeException()
            {
                return new IndexOutOfRangeException(string.Format("srcIndex: {0}, length: {1} (expected: range({0}, {2}))", srcIndex, length, srcCapacity));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIndexOutOfRangeException_DstIndex(int dstIndex)
        {
            throw GetIndexOutOfRangeException();

            IndexOutOfRangeException GetIndexOutOfRangeException()
            {
                return new IndexOutOfRangeException(string.Format("dstIndex: {0}", dstIndex));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIndexOutOfRangeException_DstIndex(int dstIndex, int length, int dstCapacity)
        {
            throw GetIndexOutOfRangeException();

            IndexOutOfRangeException GetIndexOutOfRangeException()
            {
                return new IndexOutOfRangeException(string.Format("dstIndex: {0}, length: {1} (expected: range({0}, {2}))", dstIndex, length, dstCapacity));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIndexOutOfRangeException_ReaderIndex(int index, int writerIndex)
        {
            throw GetIndexOutOfRangeException();

            IndexOutOfRangeException GetIndexOutOfRangeException()
            {
                return new IndexOutOfRangeException(string.Format("ReaderIndex: {0} (expected: 0 <= readerIndex <= writerIndex({1})", index, writerIndex));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIndexOutOfRangeException_ReaderIndex(int minimumReadableBytes, int readerIndex, int writerIndex, AbstractByteBuffer buf)
        {
            throw GetIndexOutOfRangeException();

            IndexOutOfRangeException GetIndexOutOfRangeException()
            {
                return new IndexOutOfRangeException(string.Format("readerIndex({0}) + length({1}) exceeds writerIndex({2}): {3}", readerIndex, minimumReadableBytes, writerIndex, buf));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIndexOutOfRangeException_WriterIndex(int index, int readerIndex, int capacity)
        {
            throw GetIndexOutOfRangeException();

            IndexOutOfRangeException GetIndexOutOfRangeException()
            {
                return new IndexOutOfRangeException(string.Format("WriterIndex: {0} (expected: 0 <= readerIndex({1}) <= writerIndex <= capacity ({2})", index, readerIndex, capacity));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIndexOutOfRangeException_WriterIndex(int minWritableBytes, int writerIndex, int maxCapacity, AbstractByteBuffer buf)
        {
            throw GetIndexOutOfRangeException();

            IndexOutOfRangeException GetIndexOutOfRangeException()
            {
                return new IndexOutOfRangeException(string.Format($"writerIndex({0}) + minWritableBytes({1}) exceeds maxCapacity({2}): {3}", writerIndex, minWritableBytes, maxCapacity, buf));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIndexOutOfRangeException_ReaderWriterIndex(int readerIndex, int writerIndex, int capacity)
        {
            throw GetIndexOutOfRangeException();

            IndexOutOfRangeException GetIndexOutOfRangeException()
            {
                return new IndexOutOfRangeException(string.Format("ReaderIndex: {0}, WriterIndex: {1} (expected: 0 <= readerIndex <= writerIndex <= capacity ({2})", readerIndex, writerIndex, capacity));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIndexOutOfRangeException_ReadableBytes(int length, IByteBuffer src)
        {
            throw GetIndexOutOfRangeException();

            IndexOutOfRangeException GetIndexOutOfRangeException()
            {
                return new IndexOutOfRangeException(string.Format("length({0}) exceeds src.readableBytes({1}) where src is: {2}", length, src.ReadableBytes, src));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIndexOutOfRangeException_WritableBytes(int length, IByteBuffer dst)
        {
            throw GetIndexOutOfRangeException();

            IndexOutOfRangeException GetIndexOutOfRangeException()
            {
                return new IndexOutOfRangeException(string.Format("length({0}) exceeds destination.WritableBytes({1}) where destination is: {2}", length, dst.WritableBytes, dst));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIndexOutOfRangeException_Src(int srcIndex, int length, int count)
        {
            throw GetIndexOutOfRangeException();

            IndexOutOfRangeException GetIndexOutOfRangeException()
            {
                return new IndexOutOfRangeException(string.Format("expected: 0 <= srcIdx({0}) <= srcIdx + length({1}) <= srcLen({2})", srcIndex, length, count));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIndexOutOfRangeException_Exceeds_MaxCapacity(IByteBuffer buffer, int writerIdx, int minWritableBytes)
        {
            throw GetIndexOutOfRangeException();

            IndexOutOfRangeException GetIndexOutOfRangeException()
            {
                return new IndexOutOfRangeException(string.Format(
                    "writerIndex({0}) + minWritableBytes({1}) exceeds maxCapacity({2}): {3}",
                    writerIdx, minWritableBytes, buffer.MaxCapacity, buffer));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIndexOutOfRangeException_IsText(int index, int length)
        {
            throw GetIndexOutOfRangeException(index, length);

            static IndexOutOfRangeException GetIndexOutOfRangeException(int index, int length)
            {
                return new IndexOutOfRangeException($"index: {index}length: {length}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIndexOutOfRangeException_CheckSliceOutOfBounds(int index, int length, IByteBuffer buffer)
        {
            throw GetIndexOutOfRangeException(index, length, buffer);

            static IndexOutOfRangeException GetIndexOutOfRangeException(int index, int length, IByteBuffer buffer)
            {
                return new IndexOutOfRangeException($"{buffer}.Slice({index}, {length})");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIndexOutOfRangeException_Expected(int offset, int length, int capacity)
        {
            throw GetIndexOutOfRangeException(offset, length, capacity);

            static IndexOutOfRangeException GetIndexOutOfRangeException(int offset, int length, int capacity)
            {
                return new IndexOutOfRangeException($"expected: 0 <= offset({offset}) <= offset + length({length}) <= buf.capacity({capacity}{')'}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIndexOutOfRangeException_Expected_Seq(int start, int seqCount, int end)
        {
            throw GetIndexOutOfRangeException(start, seqCount, end);

            static IndexOutOfRangeException GetIndexOutOfRangeException(int start, int seqCount, int end)
            {
                return new IndexOutOfRangeException("expected: 0 <= start(" + start + ") <= end (" + end
                        + ") <= seq.length(" + seqCount + ')');
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIndexOutOfRangeException_CheckIndexBounds(int readerIndex, int writerIndex, int capacity)
        {
            throw GetIndexOutOfRangeException(readerIndex, writerIndex, capacity);

            static IndexOutOfRangeException GetIndexOutOfRangeException(int readerIndex, int writerIndex, int capacity)
            {
                return new IndexOutOfRangeException(
                    $"readerIndex: {readerIndex}, writerIndex: {writerIndex} (expected: 0 <= readerIndex <= writerIndex <= capacity({capacity}))");
            }
        }

        #endregion

        #region -- ObjectDisposedException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowObjectDisposedException_StreamIsClosed()
        {
            throw GetObjectDisposedException();

            static ObjectDisposedException GetObjectDisposedException()
            {
                return new ObjectDisposedException(null, "Cannot access a closed Stream.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowObjectDisposedException_StreamIsClosed(ExceptionArgument argument)
        {
            throw GetObjectDisposedException(argument);

            static ObjectDisposedException GetObjectDisposedException(ExceptionArgument argument)
            {
                return new ObjectDisposedException(GetArgumentName(argument), "Cannot access a closed Stream.");
            }
        }

        #endregion

        #region -- NotSupportedException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowNotSupportedException()
        {
            throw GetNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static NotSupportedException GetNotSupportedException()
        {
            return new NotSupportedException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowNotSupportedException_UncompositeBuffer()
        {
            throw GetNotSupportedException();

            static NotSupportedException GetNotSupportedException()
            {
                return new NotSupportedException("ByteBufferWriter does not support composite buffer.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowNotSupportedException_UnreadableStream()
        {
            throw GetNotSupportedException();

            static NotSupportedException GetNotSupportedException()
            {
                return new NotSupportedException("Stream does not support reading.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowNotSupportedException_UnseekableStream()
        {
            throw GetNotSupportedException();

            static NotSupportedException GetNotSupportedException()
            {
                return new NotSupportedException("Stream does not support seeking.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowNotSupportedException_UnwritableStream()
        {
            throw GetNotSupportedException();

            static NotSupportedException GetNotSupportedException()
            {
                return new NotSupportedException("Stream does not support writing.");
            }
        }

        #endregion

        #region -- IllegalReferenceCountException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIllegalReferenceCountException(int refCnt, int increment)
        {
            throw GetIllegalReferenceCountException(refCnt, increment);

            static IllegalReferenceCountException GetIllegalReferenceCountException(int refCnt, int increment)
            {
                return new IllegalReferenceCountException(refCnt, increment);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIllegalReferenceCountException(int count)
        {
            throw GetIllegalReferenceCountException();

            IllegalReferenceCountException GetIllegalReferenceCountException()
            {
                return new IllegalReferenceCountException(count);
            }
        }

        #endregion

        #region -- ReadOnlyBufferException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowReadOnlyBufferException()
        {
            throw GetReadOnlyBufferException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static T ThrowReadOnlyBufferException<T>()
        {
            throw GetReadOnlyBufferException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static ReadOnlyBufferException GetReadOnlyBufferException()
        {
            return new ReadOnlyBufferException();
        }

        #endregion
    }
}
