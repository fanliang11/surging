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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com)
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs
{
    using System;
    using System.Buffers;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Buffers;
    using DotNetty.Common;
    using DotNetty.Common.Utilities;

    /// <summary>
    /// Special <see cref="IByteBuffer"/> implementation which is used by the <see cref="ReplayingDecoder{T}"/>
    /// </summary>
    sealed class ReplayingDecoderByteBuffer : IByteBuffer, IByteBuffer2
    {
        internal static readonly Signal REPLAY;
        public static readonly ReplayingDecoderByteBuffer Empty;

        static ReplayingDecoderByteBuffer()
        {
            REPLAY = Signal.ValueOf(typeof(ReplayingDecoderByteBuffer), "REPLAY");
            Empty = new ReplayingDecoderByteBuffer(Unpooled.Empty);
        }

        private IByteBuffer _buffer;
        private bool _terminated;

        public ReplayingDecoderByteBuffer() { }

        public ReplayingDecoderByteBuffer(IByteBuffer buffer)
        {
            SetCumulation(buffer);
        }

        internal void SetCumulation(IByteBuffer buffer)
        {
            _buffer = buffer;
        }

        internal void Terminate()
        {
            _terminated = true;
        }

        public int Capacity => _terminated ? _buffer.Capacity : int.MaxValue;

        public int MaxCapacity => Capacity;

        public IByteBufferAllocator Allocator => _buffer.Allocator;

        public bool IsDirect => _buffer.IsDirect;

        public bool IsReadOnly => false;

        public int ReaderIndex => _buffer.ReaderIndex;

        public int WriterIndex => _buffer.WriterIndex;

        public int ReadableBytes
        {
            get
            {
                if (_terminated)
                {
                    return _buffer.ReadableBytes;
                }
                else
                {
                    return int.MaxValue - _buffer.ReaderIndex;
                }
            }
        }

        public int WritableBytes => 0;

        public int MaxWritableBytes => 0;

        public int MaxFastWritableBytes => WritableBytes;

        public bool IsAccessible => (uint)ReferenceCount > 0u;

        public bool IsSingleIoBuffer => _buffer.IsSingleIoBuffer;

        public int IoBufferCount => _buffer.IoBufferCount;

        public bool HasArray => false;

        public byte[] Array => throw ThrowHelper.GetNotSupportedException();

        public bool HasMemoryAddress => false;

        public bool IsContiguous => false;

        public int ArrayOffset => throw ThrowHelper.GetNotSupportedException();

        public ReadOnlyMemory<byte> UnreadMemory => _buffer.UnreadMemory;

        public ReadOnlySpan<byte> UnreadSpan => _buffer.UnreadSpan;

        public ReadOnlySequence<byte> UnreadSequence => _buffer.UnreadSequence;

        public Memory<byte> FreeMemory => throw Reject();

        public Span<byte> FreeSpan => throw Reject();

        public int ReferenceCount => _buffer.ReferenceCount;

        public IntPtr AddressOfPinnedMemory()
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        public IByteBuffer AdjustCapacity(int newCapacity)
        {
            throw Reject();
        }

        public void Advance(int count)
        {
            throw Reject();
        }

        public void AdvanceReader(int count)
        {
            _buffer.AdvanceReader(count);
        }

        public IByteBuffer AsReadOnly()
        {
            return Unpooled.UnmodifiableBuffer(this);
        }

        public IByteBuffer Clear()
        {
            throw Reject();
        }

        public int CompareTo(IByteBuffer other)
        {
            throw Reject();
        }

        public IByteBuffer Copy()
        {
            throw Reject();
        }

        public IByteBuffer Copy(int index, int length)
        {
            CheckIndex(index, length);
            return _buffer.Copy(index, length);
        }

        public IByteBuffer DiscardReadBytes()
        {
            throw Reject();
        }

        public IByteBuffer DiscardSomeReadBytes()
        {
            throw Reject();
        }

        public IByteBuffer Duplicate()
        {
            throw Reject();
        }

        public IByteBuffer EnsureWritable(int minWritableBytes)
        {
            throw Reject();
        }

        public int EnsureWritable(int minWritableBytes, bool force)
        {
            throw Reject();
        }

        public int FindIndex(Predicate<byte> match)
        {
            int ret = _buffer.FindIndex(match);
            if ((uint)ret > SharedConstants.TooBigOrNegative)
            {
                ThrowReplay();
            }
            return ret;
        }

        public int FindIndex(int index, int count, Predicate<byte> match)
        {
            int writerIndex = _buffer.WriterIndex;
            uint uWriterIndex = (uint)writerIndex;
            if ((uint)index >= uWriterIndex)
            {
                ThrowReplay();
            }

            if ((uint)(index + count) <= uWriterIndex)
            {
                return _buffer.FindIndex(index, count, match);
            }

            int ret = _buffer.FindIndex(index, writerIndex - index, match);
            if ((uint)ret > SharedConstants.TooBigOrNegative)
            {
                ThrowReplay();
            }
            return ret;
        }

        public int FindLastIndex(Predicate<byte> match)
        {
            if (!_terminated)
            {
                ThrowReplay();
            }
            return _buffer.FindLastIndex(match);
        }

        public int FindLastIndex(int index, int count, Predicate<byte> match)
        {
            if ((uint)(index + count) > (uint)_buffer.WriterIndex)
            {
                ThrowReplay();
            }

            return _buffer.FindLastIndex(index, count, match);
        }

        public int ForEachByte(IByteProcessor processor)
        {
            int ret = _buffer.ForEachByte(processor);
            if ((uint)ret > SharedConstants.TooBigOrNegative)
            {
                ThrowReplay();
            }
            return ret;
        }

        public int ForEachByte(int index, int length, IByteProcessor processor)
        {
            int writerIndex = _buffer.WriterIndex;
            uint uWriterIndex = (uint)writerIndex;
            if ((uint)index >= uWriterIndex)
            {
                ThrowReplay();
            }

            if ((uint)(index + length) <= uWriterIndex)
            {
                return _buffer.ForEachByte(index, length, processor);
            }

            int ret = _buffer.ForEachByte(index, writerIndex - index, processor);
            if ((uint)ret > SharedConstants.TooBigOrNegative)
            {
                ThrowReplay();
            }
            return ret;
        }

        public int ForEachByteDesc(IByteProcessor processor)
        {
            if (!_terminated)
            {
                ThrowReplay();
            }
            return _buffer.ForEachByteDesc(processor);
        }

        public int ForEachByteDesc(int index, int length, IByteProcessor processor)
        {
            if ((uint)(index + length) > (uint)_buffer.WriterIndex)
            {
                ThrowReplay();
            }

            return _buffer.ForEachByteDesc(index, length, processor);
        }

        public bool GetBoolean(int index)
        {
            CheckIndex(index, 1);
            return _buffer.GetBoolean(index);
        }

        public byte GetByte(int index)
        {
            CheckIndex(index, 1);
            return _buffer.GetByte(index);
        }

        public IByteBuffer GetBytes(int index, IByteBuffer destination, int dstIndex, int length)
        {
            CheckIndex(index, length);
            _ = _buffer.GetBytes(index, destination, dstIndex, length);
            return this;
        }

        public IByteBuffer GetBytes(int index, byte[] destination)
        {
            CheckIndex(index, destination.Length);
            _ = _buffer.GetBytes(index, destination);
            return this;
        }

        public IByteBuffer GetBytes(int index, byte[] destination, int dstIndex, int length)
        {
            CheckIndex(index, length);
            _ = _buffer.GetBytes(index, destination, dstIndex, length);
            return this;
        }

        public IByteBuffer GetBytes(int index, Stream destination, int length)
        {
            throw Reject();
        }

        public int GetBytes(int index, Span<byte> destination)
        {
            CheckIndex(index, destination.Length);
            return _buffer.GetBytes(index, destination);
        }

        public int GetBytes(int index, Memory<byte> destination)
        {
            CheckIndex(index, destination.Length);
            return _buffer.GetBytes(index, destination);
        }

        public ICharSequence GetCharSequence(int index, int length, Encoding encoding)
        {
            CheckIndex(index, length);
            return _buffer.GetCharSequence(index, length, encoding);
        }

        public int GetInt(int index)
        {
            CheckIndex(index, 4);
            return _buffer.GetInt(index);
        }

        public int GetIntLE(int index)
        {
            CheckIndex(index, 4);
            return _buffer.GetIntLE(index);
        }

        public ArraySegment<byte> GetIoBuffer()
        {
            throw Reject();
        }

        public ArraySegment<byte> GetIoBuffer(int index, int length)
        {
            CheckIndex(index, length);
            return _buffer.GetIoBuffer(index, length);
        }

        public ArraySegment<byte>[] GetIoBuffers()
        {
            throw Reject();
        }

        public ArraySegment<byte>[] GetIoBuffers(int index, int length)
        {
            CheckIndex(index, length);
            return _buffer.GetIoBuffers(index, length);
        }

        public long GetLong(int index)
        {
            CheckIndex(index, 8);
            return _buffer.GetLong(index);
        }

        public long GetLongLE(int index)
        {
            CheckIndex(index, 8);
            return _buffer.GetLongLE(index);
        }

        public Memory<byte> GetMemory(int index, int count)
        {
            throw Reject();
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            throw Reject();
        }

        public ref byte GetPinnableMemoryAddress()
        {
            throw Reject();
        }

        public ReadOnlyMemory<byte> GetReadableMemory(int index, int count)
        {
            return _buffer.GetReadableMemory(index, count);
        }

        public ReadOnlySpan<byte> GetReadableSpan(int index, int count)
        {
            return _buffer.GetReadableSpan(index, count);
        }

        public ReadOnlySequence<byte> GetSequence(int index, int count)
        {
            return _buffer.GetSequence(index, count);
        }

        public short GetShort(int index)
        {
            CheckIndex(index, 2);
            return _buffer.GetShort(index);
        }

        public short GetShortLE(int index)
        {
            CheckIndex(index, 2);
            return _buffer.GetShortLE(index);
        }

        public Span<byte> GetSpan(int index, int count)
        {
            throw Reject();
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            throw Reject();
        }

        public string GetString(int index, int length, Encoding encoding)
        {
            CheckIndex(index, length);
            return _buffer.GetString(index, length, encoding);
        }

        public int GetUnsignedMedium(int index)
        {
            CheckIndex(index, 3);
            return _buffer.GetUnsignedMedium(index);
        }

        public int GetUnsignedMediumLE(int index)
        {
            CheckIndex(index, 3);
            return _buffer.GetUnsignedMediumLE(index);
        }

        public int IndexOf(byte value)
        {
            return IndexOf(_buffer.ReaderIndex, _buffer.WriterIndex, value);
        }

        public int IndexOf(int fromIndex, int toIndex, byte value)
        {
            if (0u >= (uint)(fromIndex - toIndex))
            {
                return SharedConstants.IndexNotFound;
            }

            if ((uint)Math.Max(fromIndex, toIndex) > (uint)_buffer.WriterIndex)
            {
                ThrowReplay();
            }

            return _buffer.IndexOf(fromIndex, toIndex, value);
        }

        public int IndexOf(int fromIndex, int toIndex, in ReadOnlySpan<byte> values)
        {
            if (0u >= (uint)(fromIndex - toIndex))
            {
                return SharedConstants.IndexNotFound;
            }

            if ((uint)Math.Max(fromIndex, toIndex) > (uint)_buffer.WriterIndex)
            {
                ThrowReplay();
            }

            return _buffer.IndexOf(fromIndex, toIndex, values);
        }

        public int IndexOfAny(int fromIndex, int toIndex, byte value0, byte value1)
        {
            if (0u >= (uint)(fromIndex - toIndex))
            {
                return SharedConstants.IndexNotFound;
            }

            if ((uint)Math.Max(fromIndex, toIndex) > (uint)_buffer.WriterIndex)
            {
                ThrowReplay();
            }

            return _buffer.IndexOfAny(fromIndex, toIndex, value0, value1);
        }

        public int IndexOfAny(int fromIndex, int toIndex, byte value0, byte value1, byte value2)
        {
            if (0u >= (uint)(fromIndex - toIndex))
            {
                return SharedConstants.IndexNotFound;
            }

            if ((uint)Math.Max(fromIndex, toIndex) > (uint)_buffer.WriterIndex)
            {
                ThrowReplay();
            }

            return _buffer.IndexOfAny(fromIndex, toIndex, value0, value1, value2);
        }

        public int IndexOfAny(int fromIndex, int toIndex, in ReadOnlySpan<byte> values)
        {
            if (0u >= (uint)(fromIndex - toIndex))
            {
                return SharedConstants.IndexNotFound;
            }

            if ((uint)Math.Max(fromIndex, toIndex) > (uint)_buffer.WriterIndex)
            {
                ThrowReplay();
            }

            return _buffer.IndexOfAny(fromIndex, toIndex, values);
        }

        public int BytesBefore(byte value)
        {
            int bytes = _buffer.BytesBefore(value);
            if ((uint)bytes > SharedConstants.TooBigOrNegative) // < 0
            {
                ThrowReplay();
            }
            return bytes;
        }

        public int BytesBefore(int length, byte value)
        {
            return BytesBefore(_buffer.ReaderIndex, length, value);
        }

        public int BytesBefore(int index, int length, byte value)
        {
            int writerIndex = _buffer.WriterIndex;
            uint uWriterIndex = (uint)writerIndex;
            if ((uint)index >= uWriterIndex)
            {
                ThrowReplay();
            }

            if ((uint)(index + length) <= uWriterIndex)
            {
                return _buffer.BytesBefore(index, length, value);
            }

            int res = _buffer.BytesBefore(index, writerIndex - index, value);
            if ((uint)res > SharedConstants.TooBigOrNegative) // < 0
            {
                ThrowReplay();
            }
            return res;
        }

        public bool IsReadable()
        {
            return !_terminated || _buffer.IsReadable();
        }

        public bool IsReadable(int size)
        {
            return !_terminated || _buffer.IsReadable(size);
        }

        public bool IsWritable()
        {
            return false;
        }

        public bool IsWritable(int size)
        {
            return false;
        }

        public IByteBuffer MarkReaderIndex()
        {
            _ = _buffer.MarkReaderIndex();
            return this;
        }

        public IByteBuffer MarkWriterIndex()
        {
            throw Reject();
        }

        public bool ReadBoolean()
        {
            CheckReadableBytes(1);
            return _buffer.ReadBoolean();
        }

        public byte ReadByte()
        {
            CheckReadableBytes(1);
            return _buffer.ReadByte();
        }

        public IByteBuffer ReadBytes(int length)
        {
            CheckReadableBytes(length);
            return _buffer.ReadBytes(length);
        }

        public IByteBuffer ReadBytes(IByteBuffer destination, int length)
        {
            CheckReadableBytes(length);
            _ = _buffer.ReadBytes(destination, length);
            return this;
        }

        public IByteBuffer ReadBytes(IByteBuffer destination, int dstIndex, int length)
        {
            CheckReadableBytes(length);
            _ = _buffer.ReadBytes(destination, dstIndex, length);
            return this;
        }

        public IByteBuffer ReadBytes(byte[] destination)
        {
            CheckReadableBytes(destination.Length);
            _ = _buffer.ReadBytes(destination);
            return this;
        }

        public IByteBuffer ReadBytes(byte[] destination, int dstIndex, int length)
        {
            CheckReadableBytes(length);
            _ = _buffer.ReadBytes(destination, dstIndex, length);
            return this;
        }

        public IByteBuffer ReadBytes(Stream destination, int length)
        {
            throw Reject();
        }

        public int ReadBytes(Span<byte> destination)
        {
            return _buffer.ReadBytes(destination);
        }

        public int ReadBytes(Memory<byte> destination)
        {
            return _buffer.ReadBytes(destination);
        }

        public ICharSequence ReadCharSequence(int length, Encoding encoding)
        {
            CheckReadableBytes(length);
            return _buffer.ReadCharSequence(length, encoding);
        }

        public int ReadInt()
        {
            CheckReadableBytes(4);
            return _buffer.ReadInt();
        }

        public int ReadIntLE()
        {
            CheckReadableBytes(4);
            return _buffer.ReadIntLE();
        }

        public long ReadLong()
        {
            CheckReadableBytes(8);
            return _buffer.ReadLong();
        }

        public long ReadLongLE()
        {
            CheckReadableBytes(8);
            return _buffer.ReadLongLE();
        }

        public int ReadMedium()
        {
            CheckReadableBytes(3);
            return _buffer.ReadMedium();
        }

        public int ReadMediumLE()
        {
            CheckReadableBytes(3);
            return _buffer.ReadMediumLE();
        }

        public IByteBuffer ReadRetainedSlice(int length)
        {
            CheckReadableBytes(length);
            return _buffer.ReadRetainedSlice(length);
        }

        public short ReadShort()
        {
            CheckReadableBytes(2);
            return _buffer.ReadShort();
        }

        public short ReadShortLE()
        {
            CheckReadableBytes(2);
            return _buffer.ReadShortLE();
        }

        public IByteBuffer ReadSlice(int length)
        {
            CheckReadableBytes(length);
            return _buffer.ReadSlice(length);
        }

        public string ReadString(int length, Encoding encoding)
        {
            CheckReadableBytes(length);
            return _buffer.ReadString(length, encoding);
        }

        public int ReadUnsignedMedium()
        {
            CheckReadableBytes(3);
            return _buffer.ReadUnsignedMedium();
        }

        public int ReadUnsignedMediumLE()
        {
            CheckReadableBytes(3);
            return _buffer.ReadUnsignedMediumLE();
        }

        public bool Release()
        {
            throw Reject();
        }

        public bool Release(int decrement)
        {
            throw Reject();
        }

        public IByteBuffer ResetReaderIndex()
        {
            _ = _buffer.ResetReaderIndex();
            return this;
        }

        public IByteBuffer ResetWriterIndex()
        {
            throw Reject();
        }

        public IReferenceCounted Retain()
        {
            throw Reject();
        }

        public IReferenceCounted Retain(int increment)
        {
            throw Reject();
        }

        public IByteBuffer RetainedDuplicate()
        {
            throw Reject();
        }

        public IByteBuffer RetainedSlice()
        {
            throw Reject();
        }

        public IByteBuffer RetainedSlice(int index, int length)
        {
            CheckIndex(index, length);
            return _buffer.Slice(index, length);
        }

        public IByteBuffer SetBoolean(int index, bool value)
        {
            throw Reject();
        }

        public IByteBuffer SetByte(int index, int value)
        {
            throw Reject();
        }

        public IByteBuffer SetBytes(int index, IByteBuffer src, int length)
        {
            throw Reject();
        }

        public IByteBuffer SetBytes(int index, IByteBuffer src, int srcIndex, int length)
        {
            throw Reject();
        }

        public IByteBuffer SetBytes(int index, byte[] src)
        {
            throw Reject();
        }

        public IByteBuffer SetBytes(int index, byte[] src, int srcIndex, int length)
        {
            throw Reject();
        }

        public IByteBuffer SetBytes(int index, in ReadOnlySpan<byte> src)
        {
            throw Reject();
        }

        public IByteBuffer SetBytes(int index, in ReadOnlyMemory<byte> src)
        {
            throw Reject();
        }

        public Task<int> SetBytesAsync(int index, Stream src, int length, CancellationToken cancellationToken)
        {
            return TaskUtil.FromException<int>(Reject());
        }

        public int SetCharSequence(int index, ICharSequence sequence, Encoding encoding)
        {
            throw Reject();
        }

        public IByteBuffer SetIndex(int readerIndex, int writerIndex)
        {
            throw Reject();
        }

        public IByteBuffer SetInt(int index, int value)
        {
            throw Reject();
        }

        public IByteBuffer SetIntLE(int index, int value)
        {
            throw Reject();
        }

        public IByteBuffer SetLong(int index, long value)
        {
            throw Reject();
        }

        public IByteBuffer SetLongLE(int index, long value)
        {
            throw Reject();
        }

        public IByteBuffer SetMedium(int index, int value)
        {
            throw Reject();
        }

        public IByteBuffer SetMediumLE(int index, int value)
        {
            throw Reject();
        }

        public IByteBuffer SetReaderIndex(int readerIndex)
        {
            _ = _buffer.SetReaderIndex(readerIndex);
            return this;
        }

        public IByteBuffer SetShort(int index, int value)
        {
            throw Reject();
        }

        public IByteBuffer SetShortLE(int index, int value)
        {
            throw Reject();
        }

        public int SetString(int index, string value, Encoding encoding)
        {
            throw Reject();
        }

        public IByteBuffer SetWriterIndex(int writerIndex)
        {
            throw Reject();
        }

        public IByteBuffer SetZero(int index, int length)
        {
            throw Reject();
        }

        public IByteBuffer SkipBytes(int length)
        {
            CheckReadableBytes(length);
            _ = _buffer.SkipBytes(length);
            return this;
        }

        public IByteBuffer Slice()
        {
            throw Reject();
        }

        public IByteBuffer Slice(int index, int length)
        {
            CheckIndex(index, length);
            return _buffer.Slice(index, length);
        }

        public IReferenceCounted Touch()
        {
            _ = _buffer.Touch();
            return this;
        }

        public IReferenceCounted Touch(object hint)
        {
            _ = _buffer.Touch(hint);
            return this;
        }

        public IByteBuffer Unwrap()
        {
            throw Reject();
        }

        public IByteBuffer WriteBoolean(bool value)
        {
            throw Reject();
        }

        public IByteBuffer WriteByte(int value)
        {
            throw Reject();
        }

        public IByteBuffer WriteBytes(IByteBuffer src, int length)
        {
            throw Reject();
        }

        public IByteBuffer WriteBytes(IByteBuffer src, int srcIndex, int length)
        {
            throw Reject();
        }

        public IByteBuffer WriteBytes(byte[] src)
        {
            throw Reject();
        }

        public IByteBuffer WriteBytes(byte[] src, int srcIndex, int length)
        {
            throw Reject();
        }

        public IByteBuffer WriteBytes(in ReadOnlySpan<byte> src)
        {
            throw Reject();
        }

        public IByteBuffer WriteBytes(in ReadOnlyMemory<byte> src)
        {
            throw Reject();
        }

        public Task WriteBytesAsync(Stream stream, int length, CancellationToken cancellationToken)
        {
            return TaskUtil.FromException(Reject());
        }

        public int WriteCharSequence(ICharSequence sequence, Encoding encoding)
        {
            throw Reject();
        }

        public IByteBuffer WriteInt(int value)
        {
            throw Reject();
        }

        public IByteBuffer WriteIntLE(int value)
        {
            throw Reject();
        }

        public IByteBuffer WriteLong(long value)
        {
            throw Reject();
        }

        public IByteBuffer WriteLongLE(long value)
        {
            throw Reject();
        }

        public IByteBuffer WriteMedium(int value)
        {
            throw Reject();
        }

        public IByteBuffer WriteMediumLE(int value)
        {
            throw Reject();
        }

        public IByteBuffer WriteShort(int value)
        {
            throw Reject();
        }

        public IByteBuffer WriteShortLE(int value)
        {
            throw Reject();
        }

        public int WriteString(string value, Encoding encoding)
        {
            throw Reject();
        }

        public IByteBuffer WriteZero(int length)
        {
            throw Reject();
        }

        public override string ToString()
        {
            return $"{StringUtil.SimpleClassName(this)}(ridx={ReaderIndex}, widx={WriterIndex})";
        }

        public string ToString(Encoding encoding)
        {
            throw Reject();
        }

        public string ToString(int index, int length, Encoding encoding)
        {
            CheckIndex(index, length);
            return _buffer.ToString(index, length, encoding);
        }

        public bool Equals(IByteBuffer other)
        {
            return other.Equals(this);
        }

        public override bool Equals(object obj)
        {
            return obj is IByteBuffer other && other.Equals(this);
        }

        public override int GetHashCode()
        {
            throw Reject();
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        private void CheckIndex(int index, int length)
        {
            if ((uint)(index + length) > (uint)_buffer.WriterIndex)
            {
                ThrowReplay();
            }
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        private void CheckReadableBytes(int readableBytes)
        {
            if ((uint)_buffer.ReadableBytes < (uint)readableBytes)
            {
                ThrowReplay();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowReplay()
        {
            throw REPLAY;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static NotSupportedException Reject()
        {
            return new NotSupportedException("not a replayable operation");
        }
    }
}