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
 * Copyright (c) Microsoft. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com)
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Buffers
{
    using System;
    using System.Buffers;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common;
    using DotNetty.Common.Utilities;

    public partial class CompositeByteBuffer : AbstractReferenceCountedByteBuffer, IEnumerable<IByteBuffer>
    {
        private static readonly IList<IByteBuffer> EmptyList = new ReadOnlyCollection<IByteBuffer>(new IByteBuffer[0]);

        private sealed class ComponentEntry
        {
            public readonly IByteBuffer SrcBuffer; // the originally added buffer
            public readonly IByteBuffer Buffer; // srcBuf unwrapped zero or more times

            public int SrcAdjustment; // index of the start of this CompositeByteBuf relative to srcBuf
            public int Adjustment; // index of the start of this CompositeByteBuf relative to buf

            public int Offset; // offset of this component within this CompositeByteBuf
            public int EndOffset; // end offset of this component within this CompositeByteBuf
            internal IByteBuffer _slice; // cached slice, may be null

            public ComponentEntry(IByteBuffer srcBuf, int srcOffset, IByteBuffer buf, int bufOffset,
                int offset, int len, IByteBuffer slice)
            {
                SrcBuffer = srcBuf;
                SrcAdjustment = srcOffset - offset;
                Buffer = buf;
                Adjustment = bufOffset - offset;
                Offset = offset;
                EndOffset = offset + len;
                _slice = slice;
            }

            [MethodImpl(InlineMethod.AggressiveOptimization)]
            public int SrcIdx(int index)
            {
                return index + SrcAdjustment;
            }

            [MethodImpl(InlineMethod.AggressiveOptimization)]
            public int Idx(int index)
            {
                return index + Adjustment;
            }

            public int Length()
            {
                return EndOffset - Offset;
            }

            public void Reposition(int newOffset)
            {
                int move = newOffset - Offset;
                EndOffset += move;
                SrcAdjustment -= move;
                Adjustment -= move;
                Offset = newOffset;
            }

            // copy then release
            public void TransferTo(IByteBuffer dst)
            {
                _ = dst.WriteBytes(Buffer, Idx(Offset), Length());
                Free();
            }

            public IByteBuffer Slice()
            {
                var s = _slice;
                if (s is null)
                {
                    _slice = s = SrcBuffer.Slice(SrcIdx(Offset), Length());
                }
                return s;
            }

            public IByteBuffer Duplicate()
            {
                return SrcBuffer.Duplicate().SetIndex(SrcIdx(Offset), SrcIdx(EndOffset));
            }

            public void Free()
            {
                _slice = null;
                // Release the original buffer since it may have a different
                // refcount to the unwrapped buf (e.g. if PooledSlicedByteBuf)
                _ = SrcBuffer.Release();
            }
        }

        private static readonly ArraySegment<byte> EmptyNioBuffer = Unpooled.Empty.GetIoBuffer();
        private static readonly IByteBuffer[] Empty = { Unpooled.Empty };

        private readonly IByteBufferAllocator _allocator;
        private readonly bool _direct;
        private readonly int _maxNumComponents;

        private int _componentCount;
        private ComponentEntry[] _components;

        private bool _freed;

        // weak cache - check it first when looking for component
        private ComponentEntry _lastAccessed;

        private CompositeByteBuffer(IByteBufferAllocator allocator, bool direct, int maxNumComponents, int initSize)
            : base(AbstractByteBufferAllocator.DefaultMaxCapacity)
        {
            if (allocator is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.allocator); }
            if (maxNumComponents < 1) { ThrowHelper.ThrowArgumentException_CheckMaxNumComponents(maxNumComponents); }

            _allocator = allocator;
            _direct = direct;
            _maxNumComponents = maxNumComponents;
            _components = NewCompArray(initSize, maxNumComponents);
        }

        public CompositeByteBuffer(IByteBufferAllocator alloc, bool direct, int maxNumComponents)
            : this(alloc, direct, maxNumComponents, 0)
        {
        }

        public CompositeByteBuffer(IByteBufferAllocator allocator, bool direct, int maxNumComponents, params IByteBuffer[] buffers)
            : this(allocator, direct, maxNumComponents, buffers ?? Empty, 0)
        {
        }

        internal CompositeByteBuffer(IByteBufferAllocator alloc, bool direct, int maxNumComponents, IByteBuffer[] buffers, int offset)
            : this(alloc, direct, maxNumComponents, buffers.Length - offset)
        {
            _ = AddComponents0(false, 0, buffers, offset);
            ConsolidateIfNeeded();
            SetIndex0(0, Capacity);
        }

        public CompositeByteBuffer(IByteBufferAllocator allocator, bool direct, int maxNumComponents, IEnumerable<IByteBuffer> buffers)
            : this(allocator, direct, maxNumComponents, buffers is ICollection<IByteBuffer> bufCol ? bufCol.Count : 0)
        {
            _ = AddComponents(false, 0, buffers);
            _ = SetIndex(0, Capacity);
        }

        static ComponentEntry[] NewCompArray(int initComponents, int maxNumComponents)
        {
            int capacityGuess = Math.Min(AbstractByteBufferAllocator.DefaultMaxComponents, maxNumComponents);
            return new ComponentEntry[Math.Max(initComponents, capacityGuess)];
        }

        // Special constructor used by WrappedCompositeByteBuf
        internal CompositeByteBuffer(IByteBufferAllocator allocator) : base(int.MaxValue)
        {
            _allocator = allocator;
            _direct = false;
            _maxNumComponents = 0;
            _components = null;
        }

        /// <summary>
        ///     Add the given {@link IByteBuffer}.
        ///     Be aware that this method does not increase the {@code writerIndex} of the {@link CompositeByteBuffer}.
        ///     If you need to have it increased you need to handle it by your own.
        ///     @param buffer the {@link IByteBuffer} to add
        /// </summary>
        public virtual CompositeByteBuffer AddComponent(IByteBuffer buffer) => AddComponent(false, buffer);

        /// <summary>
        ///     Add the given {@link IByteBuffer}s.
        ///     Be aware that this method does not increase the {@code writerIndex} of the {@link CompositeByteBuffer}.
        ///     If you need to have it increased you need to handle it by your own.
        ///     @param buffers the {@link IByteBuffer}s to add
        /// </summary>
        public virtual CompositeByteBuffer AddComponents(params IByteBuffer[] buffers) => AddComponents(false, buffers);

        /// <summary>
        ///     Add the given {@link IByteBuffer}s.
        ///     Be aware that this method does not increase the {@code writerIndex} of the {@link CompositeByteBuffer}.
        ///     If you need to have it increased you need to handle it by your own.
        ///     @param buffers the {@link IByteBuffer}s to add
        /// </summary>
        public virtual CompositeByteBuffer AddComponents(IEnumerable<IByteBuffer> buffers) => AddComponents(false, buffers);

        /// <summary>
        ///     Add the given {@link IByteBuffer} on the specific index.
        ///     Be aware that this method does not increase the {@code writerIndex} of the {@link CompositeByteBuffer}.
        ///     If you need to have it increased you need to handle it by your own.
        ///     @param cIndex the index on which the {@link IByteBuffer} will be added
        ///     @param buffer the {@link IByteBuffer} to add
        /// </summary>
        public virtual CompositeByteBuffer AddComponent(int cIndex, IByteBuffer buffer) => AddComponent(false, cIndex, buffer);

        public virtual CompositeByteBuffer AddComponent(bool increaseWriterIndex, IByteBuffer buffer)
        {
            return AddComponent(increaseWriterIndex, _componentCount, buffer);
        }

        public virtual CompositeByteBuffer AddComponents(bool increaseWriterIndex, params IByteBuffer[] buffers)
        {
            if (buffers is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.buffers); }
            _ = AddComponents0(increaseWriterIndex, _componentCount, buffers, 0);
            ConsolidateIfNeeded();
            return this;
        }

        public virtual CompositeByteBuffer AddComponents(bool increaseWriterIndex, IEnumerable<IByteBuffer> buffers)
        {
            return AddComponents(increaseWriterIndex, _componentCount, buffers);
        }

        public virtual CompositeByteBuffer AddComponent(bool increaseWriterIndex, int cIndex, IByteBuffer buffer)
        {
            if (buffer is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.buffer); }
            _ = AddComponent0(increaseWriterIndex, cIndex, buffer);
            ConsolidateIfNeeded();
            return this;
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        private static void CheckForOverflow(int capacity, int readableBytes)
        {
            if ((uint)(capacity + readableBytes) > SharedConstants.TooBigOrNegative) // < 0
            {
                ThrowHelper.ThrowInvalidOperationException_Can_not_increase_by(capacity, readableBytes);
            }
        }

        /// <summary>
        /// Precondition is that <code>buffer != null</code>.
        /// </summary>
        private int AddComponent0(bool increaseWriterIndex, int cIndex, IByteBuffer buffer)
        {
            bool wasAdded = false;
            try
            {
                CheckComponentIndex(cIndex);

                // No need to consolidate - just add a component to the list.
                ComponentEntry c = NewComponent(EnsureAccessible(buffer), 0);
                int readableBytes = c.Length();

                // Check if we would overflow.
                // See https://github.com/netty/netty/issues/10194
                CheckForOverflow(Capacity, readableBytes);

                AddComp(cIndex, c);
                wasAdded = true;
                if (readableBytes > 0 && cIndex < _componentCount - 1)
                {
                    UpdateComponentOffsets(cIndex);
                }
                else if (cIndex > 0)
                {
                    c.Reposition(_components[cIndex - 1].EndOffset);
                }
                if (increaseWriterIndex)
                {
                    SetWriterIndex0(WriterIndex + readableBytes);
                }
                return cIndex;
            }
            finally
            {
                if (!wasAdded)
                {
                    _ = buffer.Release();
                }
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        private static IByteBuffer EnsureAccessible(IByteBuffer buf)
        {
            if (CheckAccessible && !buf.IsAccessible)
            {
                ThrowHelper.ThrowIllegalReferenceCountException(0);
            }
            return buf;
        }

        ComponentEntry NewComponent(IByteBuffer buf, int offset)
        {
            int srcIndex = buf.ReaderIndex;
            int len = buf.ReadableBytes;

            // unpeel any intermediate outer layers (UnreleasableByteBuf, LeakAwareByteBufs, SwappedByteBuf)
            IByteBuffer unwrapped = buf;
            int unwrappedIndex = srcIndex;
            while (unwrapped is WrappedByteBuffer)
            {
                unwrapped = unwrapped.Unwrap();
            }

            // unwrap if already sliced
            var unwrappedBuf = unwrapped;
            switch (unwrappedBuf)
            {
                case AbstractUnpooledSlicedByteBuffer unpooledSliceBuf:
                    unwrappedIndex += unpooledSliceBuf.Idx(0);
                    unwrapped = unwrapped.Unwrap();
                    break;

                case PooledSlicedByteBuffer pooledSlicedBuf:
                    unwrappedIndex += pooledSlicedBuf._adjustment;
                    unwrapped = unwrapped.Unwrap();
                    break;

                case ArrayPooledSlicedByteBuffer arrayPooledSlicedBuf:
                    unwrappedIndex += arrayPooledSlicedBuf.adjustment;
                    unwrapped = unwrapped.Unwrap();
                    break;

                case PooledDuplicatedByteBuffer _:
                case ArrayPooledDuplicatedByteBuffer _:
                case UnpooledDuplicatedByteBuffer _:
                    unwrapped = unwrapped.Unwrap();
                    break;
            }

            // We don't need to slice later to expose the internal component if the readable range
            // is already the entire buffer
            var slice = buf.Capacity == len ? buf : null;

            return new ComponentEntry(buf, srcIndex,
                    unwrapped, unwrappedIndex, offset, len, slice);
        }

        /// <summary>
        ///     Add the given {@link IByteBuffer}s on the specific index
        ///     Be aware that this method does not increase the {@code writerIndex} of the {@link CompositeByteBuffer}.
        ///     If you need to have it increased you need to handle it by your own.
        ///     @param cIndex the index on which the {@link IByteBuffer} will be added.
        ///     @param buffers the {@link IByteBuffer}s to add
        /// </summary>
        public virtual CompositeByteBuffer AddComponents(int cIndex, params IByteBuffer[] buffers)
        {
            if (buffers is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.buffers); }
            _ = AddComponents0(false, cIndex, buffers, 0);
            ConsolidateIfNeeded();
            return this;
        }

        private CompositeByteBuffer AddComponents0(bool increaseWriterIndex, int cIndex, IByteBuffer[] buffers, int arrOffset)
        {
            int len = buffers.Length, count = len - arrOffset;

            int readableBytes = 0;
            int capacity = Capacity;
            for (int i = 0; i < buffers.Length; i++)
            {
                readableBytes += buffers[i].ReadableBytes;

                // Check if we would overflow.
                // See https://github.com/netty/netty/issues/10194
                CheckForOverflow(capacity, readableBytes);
            }
            // only set ci after we've shifted so that finally block logic is always correct
            int ci = int.MaxValue;
            try
            {
                CheckComponentIndex(cIndex);
                ShiftComps(cIndex, count); // will increase componentCount
                int nextOffset = cIndex > 0 ? _components[cIndex - 1].EndOffset : 0;
                for (ci = cIndex; arrOffset < len; arrOffset++, ci++)
                {
                    var b = buffers[arrOffset];
                    if (b is null) { break; }

                    ComponentEntry c = NewComponent(EnsureAccessible(b), nextOffset);
                    _components[ci] = c;
                    nextOffset = c.EndOffset;
                }
                return this;
            }
            finally
            {
                // ci is now the index following the last successfully added component
                if (ci < _componentCount)
                {
                    if (ci < cIndex + count)
                    {
                        // we bailed early
                        RemoveCompRange(ci, cIndex + count);
                        for (; arrOffset < len; ++arrOffset)
                        {
                            ReferenceCountUtil.SafeRelease(buffers[arrOffset]);
                        }
                    }
                    UpdateComponentOffsets(ci); // only need to do this here for components after the added ones
                }
                if (increaseWriterIndex && ci > cIndex && ci <= _componentCount)
                {
                    SetWriterIndex0(WriterIndex + _components[ci - 1].EndOffset - _components[cIndex].Offset);
                }
            }
        }

        /// <summary>
        ///     Add the given {@link ByteBuf}s on the specific index
        ///     Be aware that this method does not increase the {@code writerIndex} of the {@link CompositeByteBuffer}.
        ///     If you need to have it increased you need to handle it by your own.
        ///     @param cIndex the index on which the {@link IByteBuffer} will be added.
        ///     @param buffers the {@link IByteBuffer}s to add
        /// </summary>
        public virtual CompositeByteBuffer AddComponents(int cIndex, IEnumerable<IByteBuffer> buffers)
        {
            return AddComponents(false, cIndex, buffers);
        }

        /// <summary>
        /// Add the given <see cref="IByteBuffer"/> and increase the <see cref="IByteBuffer.WriterIndex"/> if <paramref name="increaseWriterIndex"/> is
        /// <c>true</c>. If the provided buffer is a <see cref="CompositeByteBuffer"/> itself, a "shallow copy" of its
        /// readable components will be performed. Thus the actual number of new components added may vary
        /// and in particular will be zero if the provided buffer is not readable.
        /// </summary>
        /// <param name="increaseWriterIndex"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public virtual CompositeByteBuffer AddFlattenedComponents(bool increaseWriterIndex, IByteBuffer buffer)
        {
            if (buffer is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.buffer); }

            int ridx = buffer.ReaderIndex;
            int widx = buffer.WriterIndex;
            if (ridx == widx)
            {
                _ = buffer.Release();
                return this;
            }
            if (buffer is CompositeByteBuffer from)
            {
                if (buffer is WrappedCompositeByteBuffer wrappedBuf)
                {
                    from = (CompositeByteBuffer)wrappedBuf.Unwrap();
                }
            }
            else
            {
                _ = AddComponent0(increaseWriterIndex, _componentCount, buffer);
                ConsolidateIfNeeded();
                return this;
            }
            from.CheckIndex(ridx, widx - ridx);
            var fromComponents = from._components;
            int compCountBefore = _componentCount;
            int writerIndexBefore = WriterIndex;
            try
            {
                for (int cidx = from.ToComponentIndex0(ridx), newOffset = Capacity; ; cidx++)
                {
                    var component = fromComponents[cidx];
                    int compOffset = component.Offset;
                    int fromIdx = Math.Max(ridx, compOffset);
                    int toIdx = Math.Min(widx, component.EndOffset);
                    int len = toIdx - fromIdx;
                    if (len > 0)
                    { // skip empty components
                        AddComp(_componentCount, new ComponentEntry(
                                (IByteBuffer)component.SrcBuffer.Retain(), component.SrcIdx(fromIdx),
                                component.Buffer, component.Idx(fromIdx), newOffset, len, null));
                    }
                    if (widx == toIdx)
                    {
                        break;
                    }
                    newOffset += len;
                }
                if (increaseWriterIndex)
                {
                    _ = SetWriterIndex(writerIndexBefore + (widx - ridx));
                }
                ConsolidateIfNeeded();
                _ = buffer.Release();
                buffer = null;
                return this;
            }
            finally
            {
                if (buffer is object)
                {
                    // if we did not succeed, attempt to rollback any components that were added
                    if (increaseWriterIndex)
                    {
                        _ = SetWriterIndex(writerIndexBefore);
                    }
                    for (int cidx = _componentCount - 1; cidx >= compCountBefore; cidx--)
                    {
                        _components[cidx].Free();
                        RemoveComp(cidx);
                    }
                }
            }
        }

        // TODO optimize further, similar to ByteBuf[] version
        // (difference here is that we don't know *always* know precise size increase in advance,
        // but we do in the most common case that the Iterable is a Collection)
        private CompositeByteBuffer AddComponents(bool increaseIndex, int cIndex, IEnumerable<IByteBuffer> buffers)
        {
            if (buffers is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.buffers); }

            if (buffers is IByteBuffer buffer)
            {
                // If buffers also implements ByteBuf (e.g. CompositeByteBuf), it has to go to addComponent(ByteBuf).
                return AddComponent(increaseIndex, cIndex, buffer);
            }

            var it = buffers.GetEnumerator();
            try
            {
                CheckComponentIndex(cIndex);

                // No need for consolidation
                while (it.MoveNext())
                {
                    IByteBuffer b = it.Current;
                    if (b is null) { break; }

                    cIndex = AddComponent0(increaseIndex, cIndex, b) + 1;
                    cIndex = Math.Min(cIndex, _componentCount);
                }
            }
            finally
            {
                while (it.MoveNext())
                {
                    ReferenceCountUtil.SafeRelease(it.Current);
                }
            }
            ConsolidateIfNeeded();
            return this;
        }

        /// <summary>
        ///     This should only be called as last operation from a method as this may adjust the underlying
        ///     array of components and so affect the index etc.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ConsolidateIfNeeded()
        {
            // Consolidate if the number of components will exceed the allowed maximum by the current operation.
            int size = _componentCount;
            if ((uint)size <= (uint)_maxNumComponents) { return; }

            Consolidate0(0, size);
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        void CheckComponentIndex(int cIndex)
        {
            EnsureAccessible();
            uint ucIndex = (uint)cIndex;
            if (ucIndex > SharedConstants.TooBigOrNegative || ucIndex > (uint)_componentCount)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_Index(cIndex, _componentCount);
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        void CheckComponentIndex(int cIndex, int numComponents)
        {
            EnsureAccessible();
            uint ucIndex = (uint)cIndex;
            if (ucIndex > SharedConstants.TooBigOrNegative || (uint)(cIndex + numComponents) > (uint)_componentCount)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_Index(cIndex, numComponents, _componentCount);
            }
        }

        void UpdateComponentOffsets(int cIndex)
        {
            int size = _componentCount;
            if (size <= cIndex)
            {
                return;
            }

            int nextIndex = cIndex > 0 ? _components[cIndex - 1].EndOffset : 0;
            for (; cIndex < size; cIndex++)
            {
                ComponentEntry c = _components[cIndex];
                c.Reposition(nextIndex);
                nextIndex = c.EndOffset;
            }
        }

        /// <summary>
        ///     Remove the {@link IByteBuffer} from the given index.
        ///     @param cIndex the index on from which the {@link IByteBuffer} will be remove
        /// </summary>
        public virtual CompositeByteBuffer RemoveComponent(int cIndex)
        {
            CheckComponentIndex(cIndex);
            ComponentEntry comp = _components[cIndex];
            if (_lastAccessed == comp)
            {
                _lastAccessed = null;
            }
            comp.Free();
            RemoveComp(cIndex);
            if (comp.Length() > 0)
            {
                // Only need to call updateComponentOffsets if the length was > 0
                UpdateComponentOffsets(cIndex);
            }
            return this;
        }

        /// <summary>
        ///     Remove the number of {@link IByteBuffer}s starting from the given index.
        ///     @param cIndex the index on which the {@link IByteBuffer}s will be started to removed
        ///     @param numComponents the number of components to remove
        /// </summary>
        public virtual CompositeByteBuffer RemoveComponents(int cIndex, int numComponents)
        {
            CheckComponentIndex(cIndex, numComponents);

            if (0u >= (uint)numComponents)
            {
                return this;
            }
            int endIndex = cIndex + numComponents;
            bool needsUpdate = false;
            for (int i = cIndex; i < endIndex; ++i)
            {
                ComponentEntry c = _components[i];
                needsUpdate |= c.Length() > 0;
                if (_lastAccessed == c)
                {
                    _lastAccessed = null;
                }
                c.Free();
            }
            RemoveCompRange(cIndex, endIndex);

            if (needsUpdate)
            {
                // Only need to call updateComponentOffsets if the length was > 0
                UpdateComponentOffsets(cIndex);
            }
            return this;
        }

        public virtual IEnumerator<IByteBuffer> GetEnumerator()
        {
            EnsureAccessible();

            var size = _componentCount;
            for (var idx = 0; idx < size; idx++)
            {
                yield return _components[idx].Slice();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        ///     Same with {@link #slice(int, int)} except that this method returns a list.
        /// </summary>
        public virtual IList<IByteBuffer> Decompose(int offset, int length)
        {
            CheckIndex(offset, length);
            if (0u >= (uint)length)
            {
                return EmptyList;
            }

            int componentId = ToComponentIndex0(offset);
            int bytesToSlice = length;
            // The first component
            ComponentEntry firstC = _components[componentId];

            IByteBuffer slice = firstC.Buffer.Slice(firstC.Idx(offset), Math.Min(firstC.EndOffset - offset, bytesToSlice));
            bytesToSlice -= slice.ReadableBytes;

            if (0u >= (uint)bytesToSlice)
            {
                return new List<IByteBuffer> { slice };
            }

            var sliceList = new List<IByteBuffer>(_componentCount - componentId);
            sliceList.Add(slice);

            // Add all the slices until there is nothing more left and then return the List.
            do
            {
                var component = _components[++componentId];
                slice = component.Buffer.Slice(component.Idx(component.Offset), Math.Min(component.Length(), bytesToSlice));
                bytesToSlice -= slice.ReadableBytes;
                sliceList.Add(slice);
            } while (bytesToSlice > 0);

            return sliceList;
        }

        public override bool IsSingleIoBuffer
        {
            get
            {
                int size = _componentCount;
                switch (size)
                {
                    case 0:
                        return true;
                    case 1:
                        return _components[0].Buffer.IsSingleIoBuffer;
                    default:
                        return false;
                }
            }
        }

        public override int IoBufferCount
        {
            get
            {
                int size = _componentCount;
                switch (size)
                {
                    case 0:
                        return 1;
                    case 1:
                        return _components[0].Buffer.IoBufferCount;
                    default:
                        int count = 0;
                        for (int i = 0; i < size; i++)
                        {
                            count += _components[i].Buffer.IoBufferCount;
                        }
                        return count;
                }
            }
        }

        public override ArraySegment<byte> GetIoBuffer(int index, int length)
        {
            CheckIndex(index, length);
            if (0u >= (uint)length) { return default; }

            switch (_componentCount)
            {
                case 0:
                    return EmptyNioBuffer;
                case 1:
                    ComponentEntry c = _components[0];
                    IByteBuffer buf = c.Buffer;
                    if (buf.IsSingleIoBuffer)
                    {
                        return buf.GetIoBuffer(c.Idx(index), length);
                    }
                    break;
            }

            var buffers = GetSequence(index, length);
            if (buffers.IsSingleSegment && MemoryMarshal.TryGetArray(buffers.First, out var segment))
            {
                return segment;
            }
            var merged = buffers.ToArray();
            return new ArraySegment<byte>(merged);
        }

        public override ArraySegment<byte>[] GetIoBuffers(int index, int length)
        {
            CheckIndex(index, length);
            if (0u >= (uint)length)
            {
                return new[] { EmptyNioBuffer };
            }

            var buffers = ThreadLocalList<ArraySegment<byte>>.NewInstance(_componentCount);
            try
            {
                int i = ToComponentIndex0(index);
                while (length > 0)
                {
                    ComponentEntry c = _components[i];
                    IByteBuffer s = c.Buffer;
                    int localLength = Math.Min(length, c.EndOffset - index);
                    switch (s.IoBufferCount)
                    {
                        case 0:
                            ThrowHelper.ThrowNotSupportedException();
                            break;
                        case 1:
                            buffers.Add(s.GetIoBuffer(c.Idx(index), localLength));
                            break;
                        default:
                            buffers.AddRange(s.GetIoBuffers(c.Idx(index), localLength));
                            break;
                    }

                    index += localLength;
                    length -= localLength;
                    i++;
                }

                return buffers.ToArray();
            }
            finally
            {
                buffers.Return();
            }
        }


        public override bool IsDirect
        {
            get
            {
                int size = _componentCount;
                if (0u >= (uint)size)
                {
                    return false;
                }
                for (int i = 0; i < size; i++)
                {
                    if (!_components[i].Buffer.IsDirect)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public override bool HasArray
        {
            get
            {
                switch (_componentCount)
                {
                    case 0:
                        return true;
                    case 1:
                        return _components[0].Buffer.HasArray;
                    default:
                        return false;
                }
            }
        }

        public override byte[] Array
        {
            get
            {
                switch (_componentCount)
                {
                    case 0:
                        return ArrayExtensions.ZeroBytes;
                    case 1:
                        return _components[0].Buffer.Array;
                    default:
                        throw ThrowHelper.GetNotSupportedException();
                }
            }
        }

        public override int ArrayOffset
        {
            get
            {
                switch (_componentCount)
                {
                    case 0:
                        return 0;
                    case 1:
                        ComponentEntry c = _components[0];
                        return c.Idx(c.Buffer.ArrayOffset);
                    default:
                        throw ThrowHelper.GetNotSupportedException();
                }
            }
        }

        public override bool HasMemoryAddress
        {
            get
            {
                switch (_componentCount)
                {
                    case 1:
                        return _components[0].Buffer.HasMemoryAddress;
                    default:
                        return false;
                }
            }
        }

        public override ref byte GetPinnableMemoryAddress()
        {
            switch (_componentCount)
            {
                case 1:
                    ComponentEntry c = _components[0];
                    return ref Unsafe.Add(ref c.Buffer.GetPinnableMemoryAddress(), c.Adjustment);
                default:
                    throw ThrowHelper.GetNotSupportedException();
            }
        }

        public override IntPtr AddressOfPinnedMemory()
        {
            switch (_componentCount)
            {
                case 1:
                    ComponentEntry c = _components[0];
                    IntPtr ptr = c.Buffer.AddressOfPinnedMemory();
                    if (ptr == IntPtr.Zero)
                    {
                        return ptr;
                    }
                    return ptr + c.Adjustment;
                default:
                    throw ThrowHelper.GetNotSupportedException();
            }
        }

        public override int Capacity
        {
            get
            {
                int size = _componentCount;
                return size > 0 ? _components[size - 1].EndOffset : 0;
            }
        }

        public override IByteBuffer AdjustCapacity(int newCapacity)
        {
            CheckNewCapacity(newCapacity);

            int size = _componentCount, oldCapacity = Capacity;
            if (newCapacity > oldCapacity)
            {
                int paddingLength = newCapacity - oldCapacity;
                IByteBuffer padding = AllocateBuffer(paddingLength).SetIndex(0, paddingLength);
                _ = AddComponent0(false, size, padding);
                if (_componentCount >= _maxNumComponents)
                {
                    // FIXME: No need to create a padding buffer and consolidate.
                    // Just create a big single buffer and put the current content there.
                    ConsolidateIfNeeded();
                }
            }
            else if (newCapacity < oldCapacity)
            {
                _lastAccessed = null;
                int i = size - 1;
                for (int bytesToTrim = oldCapacity - newCapacity; i >= 0; i--)
                {
                    ComponentEntry c = _components[i];
                    int cLength = c.Length();
                    if (bytesToTrim < cLength)
                    {
                        // Trim the last component
                        c.EndOffset -= bytesToTrim;
                        var slice = c._slice;
                        if (slice != null)
                        {
                            // We must replace the cached slice with a derived one to ensure that
                            // it can later be released properly in the case of PooledSlicedByteBuf.
                            c._slice = slice.Slice(0, c.Length());
                        }
                        break;
                    }
                    c.Free();
                    bytesToTrim -= cLength;
                }
                RemoveCompRange(i + 1, size);

                if (ReaderIndex > newCapacity)
                {
                    SetIndex0(newCapacity, newCapacity);
                }
                else if (WriterIndex > newCapacity)
                {
                    SetWriterIndex0(newCapacity);
                }
            }
            return this;
        }

        public override IByteBufferAllocator Allocator => _allocator;

        /// <summary>
        ///     Return the current number of {@link IByteBuffer}'s that are composed in this instance
        /// </summary>
        public virtual int NumComponents => _componentCount;

        /// <summary>
        ///     Return the max number of {@link IByteBuffer}'s that are composed in this instance
        /// </summary>
        public virtual int MaxNumComponents => _maxNumComponents;

        /// <summary>
        ///     Return the index for the given offset
        /// </summary>
        public virtual int ToComponentIndex(int offset)
        {
            CheckIndex(offset);
            return ToComponentIndex0(offset);
        }

        int ToComponentIndex0(int offset)
        {
            int size = _componentCount;
            var thisComponents = _components;
            if (0u >= (uint)offset) // fast-path zero offset
            {
                for (int i = 0; i < size; i++)
                {
                    if (thisComponents[i].EndOffset > 0)
                    {
                        return i;
                    }
                }
            }
            if (2u >= (uint)size) // size <= 2
            { // fast-path for 1 and 2 component count
                return size == 1 || offset < thisComponents[0].EndOffset ? 0 : 1;
            }
            for (int low = 0, high = size; low <= high;)
            {
                int mid = (low + high).RightUShift(1);
                ComponentEntry c = thisComponents[mid];
                if (offset >= c.EndOffset)
                {
                    low = mid + 1;
                }
                else if (offset < c.Offset)
                {
                    high = mid - 1;
                }
                else
                {
                    return mid;
                }
            }

            return ThrowHelper.FromException_ShouldNotReachHere<int>();
        }

        public virtual int ToByteIndex(int cIndex)
        {
            CheckComponentIndex(cIndex);
            return _components[cIndex].Offset;
        }

        public override byte GetByte(int index)
        {
            ComponentEntry c = FindComponent(index);
            return c.Buffer.GetByte(c.Idx(index));
        }

        protected internal override byte _GetByte(int index)
        {
            ComponentEntry c = FindComponent0(index);
            return c.Buffer.GetByte(c.Idx(index));
        }

        protected internal override short _GetShort(int index)
        {
            ComponentEntry c = FindComponent0(index);
            if (index + 2 <= c.EndOffset)
            {
                return c.Buffer.GetShort(c.Idx(index));
            }

            return (short)(_GetByte(index) << 8 | _GetByte(index + 1));
        }

        protected internal override short _GetShortLE(int index)
        {
            ComponentEntry c = FindComponent0(index);
            if (index + 2 <= c.EndOffset)
            {
                return c.Buffer.GetShortLE(c.Idx(index));
            }

            return (short)(_GetByte(index) << 8 | _GetByte(index + 1));
        }

        protected internal override int _GetUnsignedMedium(int index)
        {
            ComponentEntry c = FindComponent0(index);
            if (index + 3 <= c.EndOffset)
            {
                return c.Buffer.GetUnsignedMedium(c.Idx(index));
            }

            return (_GetShort(index) & 0xffff) << 8 | _GetByte(index + 2);
        }

        protected internal override int _GetUnsignedMediumLE(int index)
        {
            ComponentEntry c = FindComponent0(index);
            if (index + 3 <= c.EndOffset)
            {
                return c.Buffer.GetUnsignedMediumLE(c.Idx(index));
            }

            return (_GetShortLE(index) & 0xffff) << 8 | _GetByte(index + 2);
        }

        protected internal override int _GetInt(int index)
        {
            ComponentEntry c = FindComponent0(index);
            if (index + 4 <= c.EndOffset)
            {
                return c.Buffer.GetInt(c.Idx(index));
            }

            return _GetShort(index) << 16 | (ushort)_GetShort(index + 2);
        }

        protected internal override int _GetIntLE(int index)
        {
            ComponentEntry c = FindComponent0(index);
            if (index + 4 <= c.EndOffset)
            {
                return c.Buffer.GetIntLE(c.Idx(index));
            }

            return (_GetShortLE(index) << 16 | (ushort)_GetShortLE(index + 2));
        }

        protected internal override long _GetLong(int index)
        {
            ComponentEntry c = FindComponent0(index);
            if (index + 8 <= c.EndOffset)
            {
                return c.Buffer.GetLong(c.Idx(index));
            }

            return (long)_GetInt(index) << 32 | (uint)_GetInt(index + 4);
        }

        protected internal override long _GetLongLE(int index)
        {
            ComponentEntry c = FindComponent0(index);
            if (index + 8 <= c.EndOffset)
            {
                return c.Buffer.GetLongLE(c.Idx(index));
            }

            return (_GetIntLE(index) << 32 | _GetIntLE(index + 4));
        }

        public override IByteBuffer GetBytes(int index, byte[] dst, int dstIndex, int length)
        {
            CheckDstIndex(index, length, dstIndex, dst.Length);
            if (0u >= (uint)length)
            {
                return this;
            }

            int i = ToComponentIndex0(index);
            while (length > 0)
            {
                ComponentEntry c = _components[i];
                int localLength = Math.Min(length, c.EndOffset - index);
                _ = c.Buffer.GetBytes(c.Idx(index), dst, dstIndex, localLength);
                index += localLength;
                dstIndex += localLength;
                length -= localLength;
                i++;
            }
            return this;
        }

        public override IByteBuffer GetBytes(int index, Stream destination, int length)
        {
            CheckIndex(index, length);
            if (0u >= (uint)length)
            {
                return this;
            }

            int i = ToComponentIndex0(index);
            while (length > 0)
            {
                ComponentEntry c = _components[i];
                int localLength = Math.Min(length, c.EndOffset - index);
                _ = c.Buffer.GetBytes(c.Idx(index), destination, localLength);
                index += localLength;
                length -= localLength;
                i++;
            }
            return this;
        }

        public override IByteBuffer GetBytes(int index, IByteBuffer dst, int dstIndex, int length)
        {
            CheckDstIndex(index, length, dstIndex, dst.Capacity);
            if (0u >= (uint)length)
            {
                return this;
            }

            int i = ToComponentIndex0(index);
            while (length > 0)
            {
                ComponentEntry c = _components[i];
                int localLength = Math.Min(length, c.EndOffset - index);
                _ = c.Buffer.GetBytes(c.Idx(index), dst, dstIndex, localLength);
                index += localLength;
                dstIndex += localLength;
                length -= localLength;
                i++;
            }
            return this;
        }

        public override IByteBuffer SetByte(int index, int value)
        {
            ComponentEntry c = FindComponent(index);
            _ = c.Buffer.SetByte(c.Idx(index), value);
            return this;
        }

        protected internal override void _SetByte(int index, int value)
        {
            ComponentEntry c = FindComponent0(index);
            _ = c.Buffer.SetByte(c.Idx(index), value);
        }

        protected internal override void _SetShort(int index, int value)
        {
            ComponentEntry c = FindComponent0(index);
            if (index + 2 <= c.EndOffset)
            {
                _ = c.Buffer.SetShort(c.Idx(index), value);
            }
            else
            {
                _SetByte(index, (byte)((uint)value >> 8));
                _SetByte(index + 1, (byte)value);
            }
        }

        protected internal override void _SetShortLE(int index, int value)
        {
            ComponentEntry c = FindComponent0(index);
            if (index + 2 <= c.EndOffset)
            {
                _ = c.Buffer.SetShortLE(c.Idx(index), value);
            }
            else
            {
                _SetByte(index, (byte)(value.RightUShift(8)));
                _SetByte(index + 1, (byte)value);
            }
        }

        protected internal override void _SetMedium(int index, int value)
        {
            ComponentEntry c = FindComponent0(index);
            if (index + 3 <= c.EndOffset)
            {
                _ = c.Buffer.SetMedium(c.Idx(index), value);
            }
            else
            {
                _SetShort(index, (short)(value >> 8));
                _SetByte(index + 2, (byte)value);
            }
        }

        protected internal override void _SetMediumLE(int index, int value)
        {
            ComponentEntry c = FindComponent0(index);
            if (index + 3 <= c.EndOffset)
            {
                _ = c.Buffer.SetMediumLE(c.Idx(index), value);
            }
            else
            {
                _SetShortLE(index, (short)(value >> 8));
                _SetByte(index + 2, (byte)value);
            }
        }

        protected internal override void _SetInt(int index, int value)
        {
            ComponentEntry c = FindComponent0(index);
            if (index + 4 <= c.EndOffset)
            {
                _ = c.Buffer.SetInt(c.Idx(index), value);
            }
            else
            {
                _SetShort(index, (short)((uint)value >> 16));
                _SetShort(index + 2, (short)value);
            }
        }

        protected internal override void _SetIntLE(int index, int value)
        {
            ComponentEntry c = FindComponent0(index);
            if (index + 4 <= c.EndOffset)
            {
                _ = c.Buffer.SetIntLE(c.Idx(index), value);
            }
            else
            {
                _SetShortLE(index, (short)value.RightUShift(16));
                _SetShortLE(index + 2, (short)value);
            }
        }

        protected internal override void _SetLong(int index, long value)
        {
            ComponentEntry c = FindComponent0(index);
            if (index + 8 <= c.EndOffset)
            {
                _ = c.Buffer.SetLong(c.Idx(index), value);
            }
            else
            {
                _SetInt(index, (int)((ulong)value >> 32));
                _SetInt(index + 4, (int)value);
            }
        }

        protected internal override void _SetLongLE(int index, long value)
        {
            ComponentEntry c = FindComponent0(index);
            if (index + 8 <= c.EndOffset)
            {
                _ = c.Buffer.SetLongLE(c.Idx(index), value);
            }
            else
            {
                _SetIntLE(index, (int)value.RightUShift(32));
                _SetIntLE(index + 4, (int)value);
            }
        }

        public override IByteBuffer SetBytes(int index, byte[] src, int srcIndex, int length)
        {
            CheckSrcIndex(index, length, srcIndex, src.Length);
            if (0u >= (uint)length)
            {
                return this;
            }

            int i = ToComponentIndex0(index);
            while (length > 0)
            {
                ComponentEntry c = _components[i];
                int localLength = Math.Min(length, c.EndOffset - index);
                _ = c.Buffer.SetBytes(c.Idx(index), src, srcIndex, localLength);
                index += localLength;
                srcIndex += localLength;
                length -= localLength;
                i++;
            }
            return this;
        }

        public override async Task<int> SetBytesAsync(int index, Stream src, int length, CancellationToken cancellationToken)
        {
            CheckIndex(index, length);
            if (0u >= (uint)length)
            {
                return 0;
                //return src.Read(EmptyArrays.EMPTY_BYTES);
            }

            int i = ToComponentIndex0(index);
            int readBytes = 0;
            do
            {
                ComponentEntry c = _components[i];
                int localLength = Math.Min(length, c.EndOffset - index);
                if (0u >= (uint)localLength)
                {
                    // Skip empty buffer
                    i++;
                    continue;
                }
                int localReadBytes = await c.Buffer.SetBytesAsync(c.Idx(index), src, localLength, cancellationToken);
                if (localReadBytes < 0)
                {
                    if (0u >= (uint)readBytes)
                    {
                        return -1;
                    }
                    else
                    {
                        break;
                    }
                }

                index += localReadBytes;
                length -= localReadBytes;
                readBytes += localReadBytes;
                if (localReadBytes == localLength)
                {
                    i++;
                }
            }
            while (length > 0);

            return readBytes;
        }

        public override IByteBuffer SetBytes(int index, IByteBuffer src, int srcIndex, int length)
        {
            CheckSrcIndex(index, length, srcIndex, src.Capacity);
            if (0u >= (uint)length)
            {
                return this;
            }

            int i = ToComponentIndex0(index);
            while (length > 0)
            {
                ComponentEntry c = _components[i];
                int localLength = Math.Min(length, c.EndOffset - index);
                _ = c.Buffer.SetBytes(c.Idx(index), src, srcIndex, localLength);
                index += localLength;
                srcIndex += localLength;
                length -= localLength;
                i++;
            }
            return this;
        }

        public override IByteBuffer SetZero(int index, int length)
        {
            CheckIndex(index, length);
            if (0u >= (uint)length)
            {
                return this;
            }

            int i = ToComponentIndex(index);
            while (length > 0)
            {
                ComponentEntry c = _components[i];
                IByteBuffer s = c.Buffer;
                int adjustment = c.Offset;
                int localLength = Math.Min(length, s.Capacity - (index - adjustment));
                _ = s.SetZero(index - adjustment, localLength);
                index += localLength;
                length -= localLength;
                i++;
            }
            return this;
        }

        public override IByteBuffer Copy(int index, int length)
        {
            CheckIndex(index, length);
            IByteBuffer dst = AllocateBuffer(length);
            if (length != 0)
            {
                CopyTo(index, length, ToComponentIndex0(index), dst);
            }
            return dst;
        }

        void CopyTo(int index, int length, int componentId, IByteBuffer dst)
        {
            int dstIndex = 0;
            int i = componentId;

            while (length > 0)
            {
                ComponentEntry c = _components[i];
                int localLength = Math.Min(length, c.EndOffset - index);
                _ = c.Buffer.GetBytes(c.Idx(index), dst, dstIndex, localLength);
                index += localLength;
                dstIndex += localLength;
                length -= localLength;
                i++;
            }

            _ = dst.SetWriterIndex(dst.Capacity);
        }

        /// <summary>
        ///     Return the {@link IByteBuffer} on the specified index
        ///     @param cIndex the index for which the {@link IByteBuffer} should be returned
        ///     @return buffer the {@link IByteBuffer} on the specified index
        /// </summary>
        public virtual IByteBuffer this[int cIndex]
        {
            get
            {
                CheckComponentIndex(cIndex);
                return _components[cIndex].Duplicate();
            }
        }

        /// <summary>
        ///     Return the {@link IByteBuffer} on the specified index
        ///     @param offset the offset for which the {@link IByteBuffer} should be returned
        ///     @return the {@link IByteBuffer} on the specified index
        /// </summary>
        public virtual IByteBuffer ComponentAtOffset(int offset) => FindComponent(offset).Duplicate();

        /// <summary>
        ///     Return the internal {@link IByteBuffer} on the specified index. Note that updating the indexes of the returned
        ///     buffer will lead to an undefined behavior of this buffer.
        ///     @param cIndex the index for which the {@link IByteBuffer} should be returned
        /// </summary>
        public virtual IByteBuffer InternalComponent(int cIndex)
        {
            CheckComponentIndex(cIndex);
            return _components[cIndex].Slice();
        }

        /// <summary>
        ///     Return the internal {@link IByteBuffer} on the specified offset. Note that updating the indexes of the returned
        ///     buffer will lead to an undefined behavior of this buffer.
        ///     @param offset the offset for which the {@link IByteBuffer} should be returned
        /// </summary>
        public virtual IByteBuffer InternalComponentAtOffset(int offset) => FindComponent(offset).Slice();

        ComponentEntry FindComponent(int offset)
        {
            var la = _lastAccessed;
            if (la is object && offset >= la.Offset && offset < la.EndOffset)
            {
                EnsureAccessible();
                return la;
            }
            CheckIndex(offset);
            return FindIt(offset);
        }

        ComponentEntry FindComponent0(int offset)
        {
            var la = _lastAccessed;
            if (la is object && offset >= la.Offset && offset < la.EndOffset)
            {
                return la;
            }
            return FindIt(offset);
        }

        ComponentEntry FindIt(int offset)
        {
            for (int low = 0, high = _componentCount; low <= high;)
            {
                int mid = (low + high).RightUShift(1);
                ComponentEntry c = _components[mid];
                if (offset >= c.EndOffset)
                {
                    low = mid + 1;
                }
                else if (offset < c.Offset)
                {
                    high = mid - 1;
                }
                else
                {
                    _lastAccessed = c;
                    return c;
                }
            }

            return ThrowHelper.FromException_ShouldNotReachHere<ComponentEntry>();
        }

        /// <summary>
        ///     Consolidate the composed {@link IByteBuffer}s
        /// </summary>
        public virtual CompositeByteBuffer Consolidate()
        {
            EnsureAccessible();
            Consolidate0(0, _componentCount);
            return this;
        }

        /// <summary>
        ///     Consolidate the composed {@link IByteBuffer}s
        ///     @param cIndex the index on which to start to compose
        ///     @param numComponents the number of components to compose
        /// </summary>
        public virtual CompositeByteBuffer Consolidate(int cIndex, int numComponents)
        {
            CheckComponentIndex(cIndex, numComponents);
            Consolidate0(cIndex, numComponents);
            return this;
        }

        private void Consolidate0(int cIndex, int numComponents)
        {
            if (numComponents <= 1) { return; }

            int endCIndex = cIndex + numComponents;
            var thisComponents = _components;
            int startOffset = cIndex != 0 ? thisComponents[cIndex].Offset : 0;
            int capacity = thisComponents[endCIndex - 1].EndOffset - startOffset;
            IByteBuffer consolidated = AllocateBuffer(capacity);

            for (int i = cIndex; i < endCIndex; i++)
            {
                thisComponents[i].TransferTo(consolidated);
            }
            _lastAccessed = null;
            RemoveCompRange(cIndex + 1, endCIndex);
            thisComponents[cIndex] = NewComponent(consolidated, 0);
            if (cIndex != 0 || numComponents != _componentCount)
            {
                UpdateComponentOffsets(cIndex);
            }
        }

        /// <summary>
        ///     Discard all {@link IByteBuffer}s which are read.
        /// </summary>
        public virtual CompositeByteBuffer DiscardReadComponents()
        {
            EnsureAccessible();
            int readerIndex = ReaderIndex;
            if (0u >= (uint)readerIndex)
            {
                return this;
            }

            // Discard everything if (readerIndex = writerIndex = capacity).
            int writerIndex = WriterIndex;
            if (readerIndex == writerIndex && writerIndex == Capacity)
            {
                var size = _componentCount;
                for (var idx = 0; idx < size; idx++)
                {
                    _components[idx].Free();
                }
                _lastAccessed = null;
                ClearComps();
                _ = SetIndex(0, 0);
                AdjustMarkers(readerIndex);
                return this;
            }

            // Remove read components.
            int firstComponentId = 0;
            ComponentEntry c = null;
            for (int size = _componentCount; firstComponentId < size; firstComponentId++)
            {
                c = _components[firstComponentId];
                if (c.EndOffset > readerIndex)
                {
                    break;
                }
                c.Free();
            }
            if (0u >= (uint)firstComponentId)
            {
                return this; // Nothing to discard
            }
            ComponentEntry la = _lastAccessed;
            if (la is object && la.EndOffset <= readerIndex)
            {
                _lastAccessed = null;
            }
            RemoveCompRange(0, firstComponentId);

            // Update indexes and markers.
            int offset = c.Offset;
            UpdateComponentOffsets(0);
            _ = SetIndex(readerIndex - offset, writerIndex - offset);
            AdjustMarkers(offset);
            return this;
        }

        public override IByteBuffer DiscardReadBytes()
        {
            EnsureAccessible();
            int readerIndex = ReaderIndex;
            if (0u >= (uint)readerIndex)
            {
                return this;
            }

            // Discard everything if (readerIndex = writerIndex = capacity).
            int writerIndex = WriterIndex;
            if (readerIndex == writerIndex && writerIndex == Capacity)
            {
                var size = _componentCount;
                for (var idx = 0; idx < size; idx++)
                {
                    _components[idx].Free();
                }
                _lastAccessed = null;
                ClearComps();
                _ = SetIndex(0, 0);
                AdjustMarkers(readerIndex);
                return this;
            }

            int firstComponentId = 0;
            ComponentEntry c = null;
            for (int size = _componentCount; firstComponentId < size; firstComponentId++)
            {
                c = _components[firstComponentId];
                if (c.EndOffset > readerIndex)
                {
                    break;
                }
                c.Free();
            }

            // Replace the first readable component with a new slice.
            int trimmedBytes = readerIndex - c.Offset;
            c.Offset = 0;
            c.EndOffset -= readerIndex;
            c.SrcAdjustment += readerIndex;
            c.Adjustment += readerIndex;
            var slice = c._slice;
            if (slice is object)
            {
                // We must replace the cached slice with a derived one to ensure that
                // it can later be released properly in the case of PooledSlicedByteBuf.
                c._slice = slice.Slice(trimmedBytes, c.Length());
            }
            var la = _lastAccessed;
            if (la is object && la.EndOffset <= readerIndex)
            {
                _lastAccessed = null;
            }

            RemoveCompRange(0, firstComponentId);

            // Update indexes and markers.
            UpdateComponentOffsets(0);
            _ = SetIndex(0, writerIndex - readerIndex);
            AdjustMarkers(readerIndex);
            return this;
        }

        IByteBuffer AllocateBuffer(int capacity) =>
            _direct ? Allocator.DirectBuffer(capacity) : Allocator.HeapBuffer(capacity);

        public override string ToString()
        {
            string result = base.ToString();
            result = result.Substring(0, result.Length - 1);
            return $"{result}, components={_componentCount})";
        }

        public override IReferenceCounted Touch() => this;

        public override IReferenceCounted Touch(object hint) => this;

        public override IByteBuffer DiscardSomeReadBytes() => DiscardReadComponents();

        protected internal override void Deallocate()
        {
            if (_freed)
            {
                return;
            }

            _freed = true;
            int size = _componentCount;
            // We're not using foreach to avoid creating an iterator.
            // see https://github.com/netty/netty/issues/2642
            for (int i = 0; i < size; i++)
            {
                _components[i].Free();
            }
        }

        public override bool IsAccessible => !_freed;

        public override IByteBuffer Unwrap() => null;

        // Component array manipulation - range checking omitted

        void ClearComps()
        {
            RemoveCompRange(0, _componentCount);
        }

        void RemoveComp(int i)
        {
            RemoveCompRange(i, i + 1);
        }

        void RemoveCompRange(int from, int to)
        {
            if (from >= to) { return; }

            int size = _componentCount;
            Debug.Assert(from >= 0 && to <= size);
            if (to < size)
            {
                System.Array.Copy(_components, to, _components, from, size - to);
            }
            int newSize = size - to + from;
            for (int i = newSize; i < size; i++)
            {
                _components[i] = null;
            }
            _componentCount = newSize;
        }

        void AddComp(int i, ComponentEntry c)
        {
            ShiftComps(i, 1);
            _components[i] = c;
        }

        void ShiftComps(int i, int count)
        {
            int size = _componentCount, newSize = size + count;
            Debug.Assert(i >= 0 && i <= size && count > 0);
            if (newSize > _components.Length)
            {
                // grow the array
                int newArrSize = Math.Max(size + (size >> 1), newSize);
                ComponentEntry[] newArr;
                newArr = new ComponentEntry[newArrSize];
                if (i == size)
                {
                    System.Array.Copy(_components, 0, newArr, 0, Math.Min(_components.Length, newArrSize));
                }
                else
                {
                    if (i > 0)
                    {
                        System.Array.Copy(_components, 0, newArr, 0, i);
                    }
                    if (i < size)
                    {
                        System.Array.Copy(_components, i, newArr, i + count, size - i);
                    }
                }
                _components = newArr;
            }
            else if (i < size)
            {
                System.Array.Copy(_components, i, _components, i + count, size - i);
            }
            _componentCount = newSize;
        }
    }
}