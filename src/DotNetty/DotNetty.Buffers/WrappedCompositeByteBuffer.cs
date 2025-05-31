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
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common;
    using DotNetty.Common.Utilities;

    partial class WrappedCompositeByteBuffer : CompositeByteBuffer
    {
        private readonly CompositeByteBuffer _wrapped;

        internal WrappedCompositeByteBuffer(CompositeByteBuffer wrapped) : base(wrapped.Allocator)
        {
            _wrapped = wrapped;
            SetMaxCapacity(_wrapped.MaxCapacity);
        }

        public override bool Release() => _wrapped.Release();

        public override bool Release(int decrement) => _wrapped.Release(decrement);

        public sealed override int ReaderIndex => _wrapped.ReaderIndex;

        public sealed override int WriterIndex => _wrapped.WriterIndex;

        public sealed override bool IsAccessible => _wrapped.IsAccessible;

        public sealed override bool IsReadable() => _wrapped.IsReadable();

        public sealed override bool IsReadable(int numBytes) => _wrapped.IsReadable(numBytes);

        public sealed override bool IsWritable() => _wrapped.IsWritable();

        public sealed override int ReadableBytes => _wrapped.ReadableBytes;

        public sealed override int WritableBytes => _wrapped.WritableBytes;

        public sealed override int MaxWritableBytes => _wrapped.MaxWritableBytes;

        public sealed override int MaxFastWritableBytes => _wrapped.MaxFastWritableBytes;

        public override int EnsureWritable(int minWritableBytes, bool force) => _wrapped.EnsureWritable(minWritableBytes, force);

        public override short GetShort(int index) => _wrapped.GetShort(index);

        public override short GetShortLE(int index) => _wrapped.GetShortLE(index);

        public override int GetUnsignedMedium(int index) => _wrapped.GetUnsignedMedium(index);

        public override int GetUnsignedMediumLE(int index) => _wrapped.GetUnsignedMediumLE(index);

        public override int GetInt(int index) => _wrapped.GetInt(index);

        public override int GetIntLE(int index) => _wrapped.GetIntLE(index);

        public override long GetLong(int index) => _wrapped.GetLong(index);

        public override long GetLongLE(int index) => _wrapped.GetLongLE(index);

        public override IByteBuffer SetShortLE(int index, int value) => _wrapped.SetShortLE(index, value);

        public override IByteBuffer SetMediumLE(int index, int value) => _wrapped.SetMediumLE(index, value);

        public override IByteBuffer SetIntLE(int index, int value) => _wrapped.SetIntLE(index, value);

        public override IByteBuffer SetLongLE(int index, long value) => _wrapped.SetLongLE(index, value);

        public override byte ReadByte() => _wrapped.ReadByte();

        public override short ReadShort() => _wrapped.ReadShort();

        public override short ReadShortLE() => _wrapped.ReadShortLE();

        public override int ReadUnsignedMedium() => _wrapped.ReadUnsignedMedium();

        public override int ReadUnsignedMediumLE() => _wrapped.ReadUnsignedMediumLE();

        public override int ReadInt() => _wrapped.ReadInt();

        public override int ReadIntLE() => _wrapped.ReadIntLE();

        public override long ReadLong() => _wrapped.ReadLong();

        public override long ReadLongLE() => _wrapped.ReadLongLE();

        public override IByteBuffer ReadBytes(int length) => _wrapped.ReadBytes(length);

        public override IByteBuffer Slice() => _wrapped.Slice();

        public override IByteBuffer Slice(int index, int length) => _wrapped.Slice(index, length);

        public override int ForEachByte(int index, int length, IByteProcessor processor) => _wrapped.ForEachByte(index, length, processor);

        public override int ForEachByteDesc(int index, int length, IByteProcessor processor) => _wrapped.ForEachByteDesc(index, length, processor);

        public sealed override int GetHashCode() => _wrapped.GetHashCode();

        public sealed override bool Equals(IByteBuffer buf) => _wrapped.Equals(buf);

        public sealed override int CompareTo(IByteBuffer that) => _wrapped.CompareTo(that);

        public sealed override int ReferenceCount => _wrapped.ReferenceCount;

        public override IByteBuffer Duplicate() => _wrapped.Duplicate();

        public override IByteBuffer ReadSlice(int length) => _wrapped.ReadSlice(length);

        public override IByteBuffer WriteShortLE(int value) => _wrapped.WriteShortLE(value);

        public override IByteBuffer WriteMediumLE(int value) => _wrapped.WriteMediumLE(value);

        public override IByteBuffer WriteIntLE(int value) => _wrapped.WriteIntLE(value);

        public override IByteBuffer WriteLongLE(long value) => _wrapped.WriteLongLE(value);

        public override Task WriteBytesAsync(Stream stream, int length, CancellationToken cancellationToken) => _wrapped.WriteBytesAsync(stream, length, cancellationToken);

        public override CompositeByteBuffer AddComponent(IByteBuffer buffer)
        {
            _ = _wrapped.AddComponent(buffer);
            return this;
        }

        public override CompositeByteBuffer AddComponents(params IByteBuffer[] buffers)
        {
            _ = _wrapped.AddComponents(buffers);
            return this;
        }

        public override CompositeByteBuffer AddComponents(IEnumerable<IByteBuffer> buffers)
        {
            _ = _wrapped.AddComponents(buffers);
            return this;
        }

        public override CompositeByteBuffer AddComponent(int cIndex, IByteBuffer buffer)
        {
            _ = _wrapped.AddComponent(cIndex, buffer);
            return this;
        }

        public override CompositeByteBuffer AddComponents(int cIndex, params IByteBuffer[] buffers)
        {
            _ = _wrapped.AddComponents(cIndex, buffers);
            return this;
        }

        public override CompositeByteBuffer AddComponents(int cIndex, IEnumerable<IByteBuffer> buffers)
        {
            _ = _wrapped.AddComponents(cIndex, buffers);
            return this;
        }

        public override CompositeByteBuffer AddComponent(bool increaseWriterIndex, IByteBuffer buffer)
        {
            _ = _wrapped.AddComponent(increaseWriterIndex, buffer);
            return this;
        }

        public override CompositeByteBuffer AddComponents(bool increaseWriterIndex, params IByteBuffer[] buffers)
        {
            _ = _wrapped.AddComponents(increaseWriterIndex, buffers);
            return this;
        }

        public override CompositeByteBuffer AddComponents(bool increaseWriterIndex, IEnumerable<IByteBuffer> buffers)
        {
            _ = _wrapped.AddComponents(increaseWriterIndex, buffers);
            return this;
        }

        public override CompositeByteBuffer AddComponent(bool increaseWriterIndex, int cIndex, IByteBuffer buffer)
        {
            _ = _wrapped.AddComponent(increaseWriterIndex, cIndex, buffer);
            return this;
        }

        public override CompositeByteBuffer AddFlattenedComponents(bool increaseWriterIndex, IByteBuffer buffer)
        {
            _ = _wrapped.AddFlattenedComponents(increaseWriterIndex, buffer);
            return this;
        }

        public override CompositeByteBuffer RemoveComponent(int cIndex)
        {
            _ = _wrapped.RemoveComponent(cIndex);
            return this;
        }

        public override CompositeByteBuffer RemoveComponents(int cIndex, int numComponents)
        {
            _ = _wrapped.RemoveComponents(cIndex, numComponents);
            return this;
        }

        public override IEnumerator<IByteBuffer> GetEnumerator() => _wrapped.GetEnumerator();

        public override IList<IByteBuffer> Decompose(int offset, int length) => _wrapped.Decompose(offset, length);

        public sealed override bool HasArray => _wrapped.HasArray;

        public sealed override byte[] Array => _wrapped.Array;

        public sealed override int ArrayOffset => _wrapped.ArrayOffset;

        public sealed override int Capacity => _wrapped.Capacity;

        public override IByteBuffer AdjustCapacity(int newCapacity)
        {
            _ = _wrapped.AdjustCapacity(newCapacity);
            return this;
        }

        public sealed override IByteBufferAllocator Allocator => _wrapped.Allocator;

        public sealed override int NumComponents => _wrapped.NumComponents;

        public sealed override int MaxNumComponents => _wrapped.MaxNumComponents;

        public sealed override int ToComponentIndex(int offset) => _wrapped.ToComponentIndex(offset);

        public sealed override int ToByteIndex(int cIndex) => _wrapped.ToByteIndex(cIndex);

        public override byte GetByte(int index) => _wrapped.GetByte(index);

        protected internal sealed override byte _GetByte(int index) => _wrapped._GetByte(index);

        protected internal sealed override short _GetShort(int index) => _wrapped._GetShort(index);

        protected internal sealed override short _GetShortLE(int index) => _wrapped._GetShortLE(index);

        protected internal sealed override int _GetUnsignedMedium(int index) => _wrapped._GetUnsignedMedium(index);

        protected internal sealed override int _GetUnsignedMediumLE(int index) => _wrapped._GetUnsignedMediumLE(index);

        protected internal sealed override int _GetInt(int index) => _wrapped._GetInt(index);

        protected internal sealed override int _GetIntLE(int index) => _wrapped._GetIntLE(index);

        protected internal sealed override long _GetLong(int index) => _wrapped._GetLong(index);

        protected internal sealed override long _GetLongLE(int index) => _wrapped._GetLongLE(index);

        public override IByteBuffer GetBytes(int index, byte[] dst, int dstIndex, int length)
        {
            _ = _wrapped.GetBytes(index, dst, dstIndex, length);
            return this;
        }

        public override IByteBuffer GetBytes(int index, IByteBuffer dst, int dstIndex, int length)
        {
            _ = _wrapped.GetBytes(index, dst, dstIndex, length);
            return this;
        }

        public override IByteBuffer GetBytes(int index, Stream destination, int length) => _wrapped.GetBytes(index, destination, length);

        public override IByteBuffer SetByte(int index, int value)
        {
            _ = _wrapped.SetByte(index, value);
            return this;
        }

        protected internal sealed override void _SetByte(int index, int value) => _wrapped._SetByte(index, value);

        public override IByteBuffer SetShort(int index, int value)
        {
            _ = _wrapped.SetShort(index, value);
            return this;
        }

        protected internal sealed override void _SetShort(int index, int value) => _wrapped._SetShort(index, value);

        protected internal sealed override void _SetShortLE(int index, int value) => _wrapped._SetShortLE(index, value);

        public override IByteBuffer SetMedium(int index, int value)
        {
            _ = _wrapped.SetMedium(index, value);
            return this;
        }

        protected internal sealed override void _SetMedium(int index, int value) => _wrapped._SetMedium(index, value);

        protected internal sealed override void _SetMediumLE(int index, int value) => _wrapped._SetMediumLE(index, value);

        public override IByteBuffer SetInt(int index, int value)
        {
            _ = _wrapped.SetInt(index, value);
            return this;
        }

        protected internal sealed override void _SetInt(int index, int value) => _wrapped._SetInt(index, value);

        protected internal sealed override void _SetIntLE(int index, int value) => _wrapped._SetIntLE(index, value);

        public override IByteBuffer SetLong(int index, long value)
        {
            _ = _wrapped.SetLong(index, value);
            return this;
        }

        protected internal sealed override void _SetLong(int index, long value) => _wrapped._SetLong(index, value);

        protected internal sealed override void _SetLongLE(int index, long value) => _wrapped._SetLongLE(index, value);

        public override IByteBuffer SetBytes(int index, byte[] src, int srcIndex, int length)
        {
            _ = _wrapped.SetBytes(index, src, srcIndex, length);
            return this;
        }

        public override IByteBuffer SetBytes(int index, IByteBuffer src, int srcIndex, int length)
        {
            _ = _wrapped.SetBytes(index, src, srcIndex, length);
            return this;
        }

        public override Task<int> SetBytesAsync(int index, Stream src, int length, CancellationToken cancellationToken) => _wrapped.SetBytesAsync(index, src, length, cancellationToken);

        public override IByteBuffer Copy(int index, int length) => _wrapped.Copy(index, length);

        public sealed override IByteBuffer this[int cIndex] => _wrapped[cIndex];

        public sealed override IByteBuffer ComponentAtOffset(int offset) => _wrapped.ComponentAtOffset(offset);

        public sealed override IByteBuffer InternalComponent(int cIndex) => _wrapped.InternalComponent(cIndex);

        public sealed override IByteBuffer InternalComponentAtOffset(int offset) => _wrapped.InternalComponentAtOffset(offset);

        public override bool IsSingleIoBuffer => _wrapped.IsSingleIoBuffer;

        public override int IoBufferCount => _wrapped.IoBufferCount;

        public override ArraySegment<byte> GetIoBuffer(int index, int length) => _wrapped.GetIoBuffer(index, length);

        public override ArraySegment<byte>[] GetIoBuffers(int index, int length) => _wrapped.GetIoBuffers(index, length);

        public override CompositeByteBuffer Consolidate()
        {
            _ = _wrapped.Consolidate();
            return this;
        }

        public override CompositeByteBuffer Consolidate(int cIndex, int numComponents)
        {
            _ = _wrapped.Consolidate(cIndex, numComponents);
            return this;
        }

        public override CompositeByteBuffer DiscardReadComponents()
        {
            _ = _wrapped.DiscardReadComponents();
            return this;
        }

        public override IByteBuffer DiscardReadBytes()
        {
            _ = _wrapped.DiscardReadBytes();
            return this;
        }

        public sealed override string ToString() => _wrapped.ToString();

        public sealed override IByteBuffer SetReaderIndex(int readerIndex)
        {
            _ = _wrapped.SetReaderIndex(readerIndex);
            return this;
        }

        public sealed override IByteBuffer SetWriterIndex(int writerIndex)
        {
            _ = _wrapped.SetWriterIndex(writerIndex);
            return this;
        }

        public sealed override IByteBuffer SetIndex(int readerIndex, int writerIndex)
        {
            _ = _wrapped.SetIndex(readerIndex, writerIndex);
            return this;
        }

        public sealed override IByteBuffer Clear()
        {
            _ = _wrapped.Clear();
            return this;
        }

        public sealed override IByteBuffer MarkReaderIndex()
        {
            _ = _wrapped.MarkReaderIndex();
            return this;
        }

        public sealed override IByteBuffer ResetReaderIndex()
        {
            _ = _wrapped.ResetReaderIndex();
            return this;
        }

        public sealed override IByteBuffer MarkWriterIndex()
        {
            _ = _wrapped.MarkWriterIndex();
            return this;
        }

        public sealed override IByteBuffer ResetWriterIndex()
        {
            _ = _wrapped.ResetWriterIndex();
            return this;
        }

        public override IByteBuffer EnsureWritable(int minWritableBytes)
        {
            _ = _wrapped.EnsureWritable(minWritableBytes);
            return this;
        }

        public override IByteBuffer GetBytes(int index, byte[] dst)
        {
            _ = _wrapped.GetBytes(index, dst);
            return this;
        }

        public override IByteBuffer SetBoolean(int index, bool value)
        {
            _ = _wrapped.SetBoolean(index, value);
            return this;
        }

        public override IByteBuffer SetBytes(int index, IByteBuffer src, int length)
        {
            _ = _wrapped.SetBytes(index, src, length);
            return this;
        }

        public override IByteBuffer SetBytes(int index, byte[] src)
        {
            _ = _wrapped.SetBytes(index, src);
            return this;
        }

        public override IByteBuffer SetZero(int index, int length)
        {
            _ = _wrapped.SetZero(index, length);
            return this;
        }

        public override IByteBuffer ReadBytes(IByteBuffer dst, int length)
        {
            _ = _wrapped.ReadBytes(dst, length);
            return this;
        }

        public override IByteBuffer ReadBytes(IByteBuffer dst, int dstIndex, int length)
        {
            _ = _wrapped.ReadBytes(dst, dstIndex, length);
            return this;
        }

        public override IByteBuffer ReadBytes(byte[] dst)
        {
            _ = _wrapped.ReadBytes(dst);
            return this;
        }

        public override IByteBuffer ReadBytes(byte[] dst, int dstIndex, int length)
        {
            _ = _wrapped.ReadBytes(dst, dstIndex, length);
            return this;
        }

        public override bool IsReadOnly => _wrapped.IsReadOnly;

        public override IByteBuffer AsReadOnly() => _wrapped.AsReadOnly();

        public override ICharSequence GetCharSequence(int index, int length, Encoding encoding) => _wrapped.GetCharSequence(index, length, encoding);

        public override ICharSequence ReadCharSequence(int length, Encoding encoding) => _wrapped.ReadCharSequence(length, encoding);

        public override int SetCharSequence(int index, ICharSequence sequence, Encoding encoding) => _wrapped.SetCharSequence(index, sequence, encoding);

        public override string GetString(int index, int length, Encoding encoding) => _wrapped.GetString(index, length, encoding);

        public override string ReadString(int length, Encoding encoding) => _wrapped.ReadString(length, encoding);

        public override int SetString(int index, string value, Encoding encoding) => _wrapped.SetString(index, value, encoding);

        public override IByteBuffer ReadBytes(Stream destination, int length) => _wrapped.ReadBytes(destination, length);

        public override int WriteCharSequence(ICharSequence sequence, Encoding encoding) => _wrapped.WriteCharSequence(sequence, encoding);

        public override int WriteString(string value, Encoding encoding) => _wrapped.WriteString(value, encoding);

        public override IByteBuffer SkipBytes(int length)
        {
            _ = _wrapped.SkipBytes(length);
            return this;
        }

        public override IByteBuffer WriteBoolean(bool value)
        {
            _ = _wrapped.WriteBoolean(value);
            return this;
        }

        public override IByteBuffer WriteByte(int value)
        {
            _ = _wrapped.WriteByte(value);
            return this;
        }

        public override IByteBuffer WriteShort(int value)
        {
            _ = _wrapped.WriteShort(value);
            return this;
        }

        public override IByteBuffer WriteMedium(int value)
        {
            _ = _wrapped.WriteMedium(value);
            return this;
        }

        public override IByteBuffer WriteInt(int value)
        {
            _ = _wrapped.WriteInt(value);
            return this;
        }

        public override IByteBuffer WriteLong(long value)
        {
            _ = _wrapped.WriteLong(value);
            return this;
        }

        public override IByteBuffer WriteBytes(IByteBuffer src, int length)
        {
            _ = _wrapped.WriteBytes(src, length);
            return this;
        }

        public override IByteBuffer WriteBytes(IByteBuffer src, int srcIndex, int length)
        {
            _ = _wrapped.WriteBytes(src, srcIndex, length);
            return this;
        }

        public override IByteBuffer WriteBytes(byte[] src)
        {
            _ = _wrapped.WriteBytes(src);
            return this;
        }

        public override IByteBuffer WriteBytes(byte[] src, int srcIndex, int length)
        {
            _ = _wrapped.WriteBytes(src, srcIndex, length);
            return this;
        }

        public override IByteBuffer WriteZero(int length)
        {
            _ = _wrapped.WriteZero(length);
            return this;
        }

        public override IReferenceCounted Retain(int increment)
        {
            _ = _wrapped.Retain(increment);
            return this;
        }

        public override IReferenceCounted Retain()
        {
            _ = _wrapped.Retain();
            return this;
        }

        public override IReferenceCounted Touch()
        {
            _ = _wrapped.Touch();
            return this;
        }

        public override IReferenceCounted Touch(object hint)
        {
            _ = _wrapped.Touch(hint);
            return this;
        }

        public override IByteBuffer DiscardSomeReadBytes()
        {
            _ = _wrapped.DiscardSomeReadBytes();
            return this;
        }

        protected internal sealed override void Deallocate() => _wrapped.Deallocate();

        public sealed override IByteBuffer Unwrap() => _wrapped;

        public sealed override IntPtr AddressOfPinnedMemory() => _wrapped.AddressOfPinnedMemory();

        public sealed override ref byte GetPinnableMemoryAddress() => ref _wrapped.GetPinnableMemoryAddress();

        public sealed override bool HasMemoryAddress => _wrapped.HasMemoryAddress;

        public sealed override bool IsWritable(int size) => _wrapped.IsWritable(size);

        public sealed override int MaxCapacity => _wrapped.MaxCapacity;

        public sealed override bool IsDirect => _wrapped.IsDirect;

        public override IByteBuffer ReadRetainedSlice(int length) => _wrapped.ReadRetainedSlice(length);

        public override IByteBuffer RetainedDuplicate() => _wrapped.RetainedDuplicate();

        public override IByteBuffer RetainedSlice() => _wrapped.RetainedSlice();

        public override IByteBuffer RetainedSlice(int index, int length) => _wrapped.RetainedSlice(index, length);
    }
}
