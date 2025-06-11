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
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Common.Utilities;

    /// <summary>
    ///     Abstract base class implementation of a <see cref="T:DotNetty.Buffers.IByteBuffer" />
    /// </summary>
    public abstract partial class AbstractByteBuffer : IByteBuffer
    {
        protected const int IndexNotFound = SharedConstants.IndexNotFound;

        private static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<AbstractByteBuffer>();
        private const string LegacyPropCheckAccessible = "io.netty.buffer.bytebuf.checkAccessible";
        private const string PropCheckAccessible = "io.netty.buffer.checkAccessible";
        protected static readonly bool CheckAccessible; // accessed from CompositeByteBuf
        private const string PropCheckBounds = "io.netty.buffer.checkBounds";
        private static readonly bool CheckBounds;

        internal static readonly ResourceLeakDetector LeakDetector = ResourceLeakDetector.Create<IByteBuffer>();

        private int _readerIndex;
        private int _writerIndex;

        private int _markedReaderIndex;
        private int _markedWriterIndex;
        private int _maxCapacity;

        static AbstractByteBuffer()
        {
            if (SystemPropertyUtil.Contains(PropCheckAccessible))
            {
                CheckAccessible = SystemPropertyUtil.GetBoolean(PropCheckAccessible, true);
            }
            else
            {
                CheckAccessible = SystemPropertyUtil.GetBoolean(LegacyPropCheckAccessible, true);
            }
            CheckBounds = SystemPropertyUtil.GetBoolean(PropCheckBounds, true);
            if (Logger.DebugEnabled)
            {
                Logger.Debug("-D{}: {}", PropCheckAccessible, CheckAccessible);
                Logger.Debug("-D{}: {}", PropCheckBounds, CheckBounds);
            }
        }

        protected AbstractByteBuffer(int maxCapacity)
        {
            if ((uint)maxCapacity > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_PositiveOrZero(maxCapacity, ExceptionArgument.maxCapacity); }

            _maxCapacity = maxCapacity;
        }
        public virtual bool IsReadOnly => false;

        public virtual IByteBuffer AsReadOnly()
        {
            if (IsReadOnly) { return this; }
            return Unpooled.UnmodifiableBuffer(this);
        }

        public abstract int Capacity { get; }

        public abstract IByteBuffer AdjustCapacity(int newCapacity);

        public virtual int MaxCapacity => _maxCapacity;

        protected void SetMaxCapacity(int newMaxCapacity)
        {
            if ((uint)newMaxCapacity > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_PositiveOrZero(newMaxCapacity, ExceptionArgument.newMaxCapacity); }

            _maxCapacity = newMaxCapacity;
        }

        public abstract IByteBufferAllocator Allocator { get; }

        public virtual int ReaderIndex => _readerIndex;

        public virtual IByteBuffer SetReaderIndex(int index)
        {
            if (CheckBounds) { CheckIndexBounds(index, _writerIndex); }

            _readerIndex = index;
            return this;
        }

        public virtual int WriterIndex => _writerIndex;

        public virtual IByteBuffer SetWriterIndex(int index)
        {
            if (CheckBounds) { CheckIndexBounds(_readerIndex, index, Capacity); }

            SetWriterIndex0(index);
            return this;
        }

        internal protected void SetWriterIndex0(int index)
        {
            _writerIndex = index;
        }

        public virtual IByteBuffer SetIndex(int readerIdx, int writerIdx)
        {
            if (CheckBounds) { CheckIndexBounds(readerIdx, writerIdx, Capacity); }

            SetIndex0(readerIdx, writerIdx);
            return this;
        }

        public virtual IByteBuffer Clear()
        {
            _readerIndex = _writerIndex = 0;
            return this;
        }

        public virtual bool IsReadable() => _writerIndex > _readerIndex;

        public virtual bool IsReadable(int size) => _writerIndex - _readerIndex >= size;

        public virtual bool IsWritable() => Capacity > _writerIndex;

        public virtual bool IsWritable(int size) => Capacity - _writerIndex >= size;

        public virtual int ReadableBytes
        {
            [MethodImpl(InlineMethod.AggressiveOptimization)]
            get => _writerIndex - _readerIndex;
        }

        public virtual int WritableBytes
        {
            [MethodImpl(InlineMethod.AggressiveOptimization)]
            get => Capacity - _writerIndex;
        }

        public virtual int MaxWritableBytes
        {
            [MethodImpl(InlineMethod.AggressiveOptimization)]
            get => MaxCapacity - _writerIndex;
        }

        public virtual int MaxFastWritableBytes => WritableBytes;

        public virtual IByteBuffer MarkReaderIndex()
        {
            _markedReaderIndex = _readerIndex;
            return this;
        }

        public virtual IByteBuffer ResetReaderIndex()
        {
            _ = SetReaderIndex(_markedReaderIndex);
            return this;
        }

        public virtual IByteBuffer MarkWriterIndex()
        {
            _markedWriterIndex = _writerIndex;
            return this;
        }

        public virtual IByteBuffer ResetWriterIndex()
        {
            _ = SetWriterIndex(_markedWriterIndex);
            return this;
        }

        protected void MarkIndex()
        {
            _markedReaderIndex = _readerIndex;
            _markedWriterIndex = _writerIndex;
        }

        public virtual IByteBuffer DiscardReadBytes()
        {
            var readerIdx = _readerIndex;
            var writerIdx = _writerIndex;
            if (0u >= (uint)readerIdx)
            {
                EnsureAccessible();
                return this;
            }

            if (readerIdx != writerIdx)
            {
                _ = SetBytes(0, this, readerIdx, writerIdx - readerIdx);
                _writerIndex = writerIdx - readerIdx;
                AdjustMarkers(readerIdx);
                _readerIndex = 0;
            }
            else
            {
                EnsureAccessible();
                AdjustMarkers(readerIdx);
                _writerIndex = _readerIndex = 0;
            }

            return this;
        }

        public virtual IByteBuffer DiscardSomeReadBytes()
        {
            var readerIdx = _readerIndex;
            var writerIdx = _writerIndex;
            if (0u >= (uint)readerIdx)
            {
                EnsureAccessible();
                return this;
            }

            if (0u >= (uint)(readerIdx - writerIdx))
            {
                EnsureAccessible();
                AdjustMarkers(readerIdx);
                _writerIndex = _readerIndex = 0;
                return this;
            }

            if (readerIdx >= Capacity.RightUShift(1))
            {
                _ = SetBytes(0, this, readerIdx, writerIdx - readerIdx);
                _writerIndex = writerIdx - readerIdx;
                AdjustMarkers(readerIdx);
                _readerIndex = 0;
            }

            return this;
        }

        protected void AdjustMarkers(int decrement)
        {
            int markedReaderIdx = _markedReaderIndex;
            if (markedReaderIdx <= decrement)
            {
                _markedReaderIndex = 0;
                int markedWriterIdx = _markedWriterIndex;
                if (markedWriterIdx <= decrement)
                {
                    _markedWriterIndex = 0;
                }
                else
                {
                    _markedWriterIndex = markedWriterIdx - decrement;
                }
            }
            else
            {
                _markedReaderIndex = markedReaderIdx - decrement;
                _markedWriterIndex -= decrement;
            }
        }

        // Called after a capacity reduction
        protected internal void TrimIndicesToCapacity(int newCapacity)
        {
            if ((uint)_writerIndex > (uint)newCapacity)
            {
                SetIndex0(Math.Min(_readerIndex, newCapacity), newCapacity);
            }
        }

        public virtual IByteBuffer EnsureWritable(int minWritableBytes)
        {
            if ((uint)minWritableBytes > SharedConstants.TooBigOrNegative)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_MinWritableBytes();
            }

            EnsureWritable0(minWritableBytes);
            return this;
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        protected internal void EnsureWritable0(int minWritableBytes)
        {
            int targetCapacity = _writerIndex + minWritableBytes;
            if ((uint)targetCapacity <= (uint)Capacity)
            {
                EnsureAccessible();
                return;
            }

            EnsureWritableInternal(_writerIndex, minWritableBytes, targetCapacity);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void EnsureWritableInternal(int writerIdx, int minWritableBytes, int targetCapacity)
        {
            var maxCapacity = MaxCapacity;
            if (CheckBounds && (uint)targetCapacity > (uint)maxCapacity)
            {
                EnsureAccessible();
                ThrowHelper.ThrowIndexOutOfRangeException_Exceeds_MaxCapacity(this, writerIdx, minWritableBytes);
            }

            // Normalize the target capacity to the power of 2.
            int fastWritable = MaxFastWritableBytes;
            int newCapacity = fastWritable >= minWritableBytes ? writerIdx + fastWritable
                    : Allocator.CalculateNewCapacity(targetCapacity, maxCapacity);

            // Adjust to the new capacity.
            _ = AdjustCapacity(newCapacity);
        }

        public virtual int EnsureWritable(int minWritableBytes, bool force)
        {
            uint uminWritableBytes = (uint)minWritableBytes;
            if (uminWritableBytes > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_PositiveOrZero(minWritableBytes, ExceptionArgument.minWritableBytes); }

            EnsureAccessible();
            if (uminWritableBytes <= (uint)WritableBytes)
            {
                return 0;
            }

            var writerIdx = _writerIndex;
            var maxCapacity = MaxCapacity;
            if (uminWritableBytes > (uint)(maxCapacity - writerIdx))
            {
                if (!force || Capacity == maxCapacity)
                {
                    return 1;
                }

                _ = AdjustCapacity(maxCapacity);
                return 3;
            }

            int fastWritable = MaxFastWritableBytes;
            int newCapacity = fastWritable >= minWritableBytes ? writerIdx + fastWritable
                    : Allocator.CalculateNewCapacity(writerIdx + minWritableBytes, maxCapacity);

            // Adjust to the new capacity.
            _ = AdjustCapacity(newCapacity);
            return 2;
        }

        public virtual byte GetByte(int index)
        {
            CheckIndex(index);
            return _GetByte(index);
        }

        protected internal abstract byte _GetByte(int index);

        public bool GetBoolean(int index) => GetByte(index) != 0;

        public virtual short GetShort(int index)
        {
            CheckIndex(index, 2);
            return _GetShort(index);
        }

        protected internal abstract short _GetShort(int index);

        public virtual short GetShortLE(int index)
        {
            CheckIndex(index, 2);
            return _GetShortLE(index);
        }

        protected internal abstract short _GetShortLE(int index);

        public virtual int GetUnsignedMedium(int index)
        {
            CheckIndex(index, 3);
            return _GetUnsignedMedium(index);
        }

        protected internal abstract int _GetUnsignedMedium(int index);

        public virtual int GetUnsignedMediumLE(int index)
        {
            CheckIndex(index, 3);
            return _GetUnsignedMediumLE(index);
        }

        protected internal abstract int _GetUnsignedMediumLE(int index);

        public virtual int GetInt(int index)
        {
            CheckIndex(index, 4);
            return _GetInt(index);
        }

        protected internal abstract int _GetInt(int index);

        public virtual int GetIntLE(int index)
        {
            CheckIndex(index, 4);
            return _GetIntLE(index);
        }

        protected internal abstract int _GetIntLE(int index);

        public virtual long GetLong(int index)
        {
            CheckIndex(index, 8);
            return _GetLong(index);
        }

        protected internal abstract long _GetLong(int index);

        public virtual long GetLongLE(int index)
        {
            CheckIndex(index, 8);
            return _GetLongLE(index);
        }

        protected internal abstract long _GetLongLE(int index);

        public virtual IByteBuffer GetBytes(int index, byte[] destination)
        {
            _ = GetBytes(index, destination, 0, destination.Length);
            return this;
        }

        public abstract IByteBuffer GetBytes(int index, byte[] destination, int dstIndex, int length);

        public abstract IByteBuffer GetBytes(int index, IByteBuffer destination, int dstIndex, int length);

        public abstract IByteBuffer GetBytes(int index, Stream destination, int length);

        public virtual unsafe string GetString(int index, int length, Encoding encoding)
        {
            CheckIndex0(index, length);
            if (0u >= (uint)length)
            {
                return string.Empty;
            }

            if (HasMemoryAddress)
            {
                IntPtr ptr = AddressOfPinnedMemory();
                if (ptr != IntPtr.Zero)
                {
                    return UnsafeByteBufferUtil.GetString((byte*)(ptr + index), length, encoding);
                }
                else
                {
                    fixed (byte* p = &GetPinnableMemoryAddress())
                        return UnsafeByteBufferUtil.GetString(p + index, length, encoding);
                }
            }
            if (HasArray)
            {
                return encoding.GetString(Array, ArrayOffset + index, length);
            }

            return this.ToString(index, length, encoding);
        }

        public virtual string ReadString(int length, Encoding encoding)
        {
            var readerIdx = _readerIndex;
            string value = GetString(readerIdx, length, encoding);
            _readerIndex = readerIdx + length;
            return value;
        }

        public virtual unsafe ICharSequence GetCharSequence(int index, int length, Encoding encoding)
        {
            CheckIndex0(index, length);
            if (0u >= (uint)length)
            {
                return StringCharSequence.Empty;
            }

            if (TextEncodings.ASCIICodePage == encoding.CodePage)// || SharedConstants.ISO88591CodePage == encoding.CodePage)
            {
                // ByteBufUtil.getBytes(...) will return a new copy which the AsciiString uses directly
                return new AsciiString(ByteBufferUtil.GetBytes(this, index, length, true), false);
            }

            if (HasMemoryAddress)
            {
                IntPtr ptr = AddressOfPinnedMemory();
                if (ptr != IntPtr.Zero)
                {
                    return new StringCharSequence(UnsafeByteBufferUtil.GetString((byte*)(ptr + index), length, encoding));
                }
                else
                {
                    fixed (byte* p = &GetPinnableMemoryAddress())
                        return new StringCharSequence(UnsafeByteBufferUtil.GetString(p + index, length, encoding));
                }
            }
            if (HasArray)
            {
                return new StringCharSequence(encoding.GetString(Array, ArrayOffset + index, length));
            }

            return new StringCharSequence(this.ToString(index, length, encoding));
        }

        public virtual ICharSequence ReadCharSequence(int length, Encoding encoding)
        {
            var readerIdx = _readerIndex;
            ICharSequence sequence = GetCharSequence(readerIdx, length, encoding);
            _readerIndex = readerIdx + length;
            return sequence;
        }

        public virtual IByteBuffer SetByte(int index, int value)
        {
            CheckIndex(index);
            _SetByte(index, value);
            return this;
        }

        protected internal abstract void _SetByte(int index, int value);

        public virtual IByteBuffer SetBoolean(int index, bool value)
        {
            _ = SetByte(index, value ? 1 : 0);
            return this;
        }

        public virtual IByteBuffer SetShort(int index, int value)
        {
            CheckIndex(index, 2);
            _SetShort(index, value);
            return this;
        }

        protected internal abstract void _SetShort(int index, int value);

        public virtual IByteBuffer SetShortLE(int index, int value)
        {
            CheckIndex(index, 2);
            _SetShortLE(index, value);
            return this;
        }

        protected internal abstract void _SetShortLE(int index, int value);

        public virtual IByteBuffer SetMedium(int index, int value)
        {
            CheckIndex(index, 3);
            _SetMedium(index, value);
            return this;
        }

        protected internal abstract void _SetMedium(int index, int value);

        public virtual IByteBuffer SetMediumLE(int index, int value)
        {
            CheckIndex(index, 3);
            _SetMediumLE(index, value);
            return this;
        }

        protected internal abstract void _SetMediumLE(int index, int value);

        public virtual IByteBuffer SetInt(int index, int value)
        {
            CheckIndex(index, 4);
            _SetInt(index, value);
            return this;
        }

        protected internal abstract void _SetInt(int index, int value);

        public virtual IByteBuffer SetIntLE(int index, int value)
        {
            CheckIndex(index, 4);
            _SetIntLE(index, value);
            return this;
        }

        protected internal abstract void _SetIntLE(int index, int value);

        public virtual IByteBuffer SetLong(int index, long value)
        {
            CheckIndex(index, 8);
            _SetLong(index, value);
            return this;
        }

        protected internal abstract void _SetLong(int index, long value);

        public virtual IByteBuffer SetLongLE(int index, long value)
        {
            CheckIndex(index, 8);
            _SetLongLE(index, value);
            return this;
        }

        protected internal abstract void _SetLongLE(int index, long value);

        public virtual IByteBuffer SetBytes(int index, byte[] src)
        {
            _ = SetBytes(index, src, 0, src.Length);
            return this;
        }

        public abstract IByteBuffer SetBytes(int index, byte[] src, int srcIndex, int length);

        public virtual IByteBuffer SetBytes(int index, IByteBuffer src, int length)
        {
            if (src is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.src); }

            CheckIndex(index, length);
            if (CheckBounds) { CheckReadableBounds(src, length); }

            _ = SetBytes(index, src, src.ReaderIndex, length);
            _ = src.SetReaderIndex(src.ReaderIndex + length);
            return this;
        }

        public abstract IByteBuffer SetBytes(int index, IByteBuffer src, int srcIndex, int length);

        public abstract Task<int> SetBytesAsync(int index, Stream src, int length, CancellationToken cancellationToken);

        public virtual IByteBuffer SetZero(int index, int length)
        {
            if (0u >= (uint)length)
            {
                return this;
            }

            CheckIndex(index, length);

            int nLong = length.RightUShift(3);
            int nBytes = length & 7;
            for (int i = nLong; i > 0; i--)
            {
                _SetLong(index, 0);
                index += 8;
            }
            if (nBytes == 4)
            {
                _SetInt(index, 0);
                // Not need to update the index as we not will use it after this.
            }
            else if (nBytes < 4)
            {
                for (int i = nBytes; i > 0; i--)
                {
                    _SetByte(index, 0);
                    index++;
                }
            }
            else
            {
                _SetInt(index, 0);
                index += 4;
                for (int i = nBytes - 4; i > 0; i--)
                {
                    _SetByte(index, 0);
                    index++;
                }
            }

            return this;
        }

        public virtual int SetString(int index, string value, Encoding encoding) => SetString0(index, value, encoding, false);

        int SetString0(int index, string value, Encoding encoding, bool expand)
        {
            switch (encoding.CodePage)
            {
                case TextEncodings.UTF8CodePage:
                    int len = ByteBufferUtil.Utf8MaxBytes(value);
                    if (expand)
                    {
                        EnsureWritable0(len);
                        CheckIndex0(index, len);
                    }
                    else
                    {
                        CheckIndex(index, len);
                    }
                    return ByteBufferUtil.WriteUtf8(this, index, value);

                case TextEncodings.ASCIICodePage:
                    int length = value.Length;
                    if (expand)
                    {
                        EnsureWritable0(length);
                        CheckIndex0(index, length);
                    }
                    else
                    {
                        CheckIndex(index, length);
                    }
                    return ByteBufferUtil.WriteAscii(this, index, value);

                default:
                    byte[] bytes = encoding.GetBytes(value);
                    if (expand)
                    {
                        EnsureWritable0(bytes.Length);
                        // setBytes(...) will take care of checking the indices.
                    }
                    _ = SetBytes(index, bytes);
                    return bytes.Length;
            }
        }

        public virtual int SetCharSequence(int index, ICharSequence sequence, Encoding encoding) => SetCharSequence0(index, sequence, encoding, false);

        int SetCharSequence0(int index, ICharSequence sequence, Encoding encoding, bool expand)
        {
            switch (encoding.CodePage)
            {
                case TextEncodings.UTF8CodePage:
                    int len = ByteBufferUtil.Utf8MaxBytes(sequence);
                    if (expand)
                    {
                        EnsureWritable0(len);
                        CheckIndex0(index, len);
                    }
                    else
                    {
                        CheckIndex(index, len);
                    }
                    return ByteBufferUtil.WriteUtf8(this, index, sequence);

                case TextEncodings.ASCIICodePage:
                    int length = sequence.Count;
                    if (expand)
                    {
                        EnsureWritable0(length);
                        CheckIndex0(index, length);
                    }
                    else
                    {
                        CheckIndex(index, length);
                    }
                    return ByteBufferUtil.WriteAscii(this, index, sequence);

                default:
                    byte[] bytes = encoding.GetBytes(sequence.ToString());
                    if (expand)
                    {
                        EnsureWritable0(bytes.Length);
                        // setBytes(...) will take care of checking the indices.
                    }
                    _ = SetBytes(index, bytes);
                    return bytes.Length;
            }
        }

        public virtual byte ReadByte()
        {
            CheckReadableBytes0(1);
            int i = _readerIndex;
            byte b = _GetByte(i);
            _readerIndex = i + 1;
            return b;
        }

        public bool ReadBoolean() => ReadByte() != 0;

        public virtual short ReadShort()
        {
            CheckReadableBytes0(2);
            short v = _GetShort(_readerIndex);
            _readerIndex += 2;
            return v;
        }

        public virtual short ReadShortLE()
        {
            CheckReadableBytes0(2);
            short v = _GetShortLE(_readerIndex);
            _readerIndex += 2;
            return v;
        }

        public int ReadMedium()
        {
            uint value = (uint)ReadUnsignedMedium();
            if ((value & 0x800000) != 0)
            {
                value |= 0xff000000;
            }

            return (int)value;
        }

        public int ReadMediumLE()
        {
            uint value = (uint)ReadUnsignedMediumLE();
            if ((value & 0x800000) != 0)
            {
                value |= 0xff000000;
            }

            return (int)value;
        }

        public virtual int ReadUnsignedMedium()
        {
            CheckReadableBytes0(3);
            int v = _GetUnsignedMedium(_readerIndex);
            _readerIndex += 3;
            return v;
        }

        public virtual int ReadUnsignedMediumLE()
        {
            CheckReadableBytes0(3);
            int v = _GetUnsignedMediumLE(_readerIndex);
            _readerIndex += 3;
            return v;
        }

        public virtual int ReadInt()
        {
            CheckReadableBytes0(4);
            int v = _GetInt(_readerIndex);
            _readerIndex += 4;
            return v;
        }

        public virtual int ReadIntLE()
        {
            CheckReadableBytes0(4);
            int v = _GetIntLE(_readerIndex);
            _readerIndex += 4;
            return v;
        }

        public virtual long ReadLong()
        {
            CheckReadableBytes0(8);
            long v = _GetLong(_readerIndex);
            _readerIndex += 8;
            return v;
        }

        public virtual long ReadLongLE()
        {
            CheckReadableBytes0(8);
            long v = _GetLongLE(_readerIndex);
            _readerIndex += 8;
            return v;
        }

        public virtual IByteBuffer ReadBytes(int length)
        {
            CheckReadableBytes(length);
            if (0u >= (uint)length)
            {
                return Unpooled.Empty;
            }

            IByteBuffer buf = Allocator.Buffer(length, MaxCapacity);
            _ = buf.WriteBytes(this, _readerIndex, length);
            _readerIndex += length;
            return buf;
        }

        public virtual IByteBuffer ReadSlice(int length)
        {
            CheckReadableBytes(length);
            IByteBuffer slice = Slice(_readerIndex, length);
            _readerIndex += length;
            return slice;
        }

        public virtual IByteBuffer ReadRetainedSlice(int length)
        {
            CheckReadableBytes(length);
            IByteBuffer slice = RetainedSlice(_readerIndex, length);
            _readerIndex += length;
            return slice;
        }

        public virtual IByteBuffer ReadBytes(byte[] destination, int dstIndex, int length)
        {
            CheckReadableBytes(length);
            _ = GetBytes(_readerIndex, destination, dstIndex, length);
            _readerIndex += length;
            return this;
        }

        public virtual IByteBuffer ReadBytes(byte[] dst)
        {
            _ = ReadBytes(dst, 0, dst.Length);
            return this;
        }

        public virtual IByteBuffer ReadBytes(IByteBuffer dst, int length)
        {
            if (CheckBounds) { CheckWritableBounds(dst, length); }

            _ = ReadBytes(dst, dst.WriterIndex, length);
            _ = dst.SetWriterIndex(dst.WriterIndex + length);
            return this;
        }

        public virtual IByteBuffer ReadBytes(IByteBuffer dst, int dstIndex, int length)
        {
            CheckReadableBytes(length);
            _ = GetBytes(_readerIndex, dst, dstIndex, length);
            _readerIndex += length;
            return this;
        }

        public virtual IByteBuffer ReadBytes(Stream destination, int length)
        {
            CheckReadableBytes(length);
            _ = GetBytes(_readerIndex, destination, length);
            _readerIndex += length;
            return this;
        }

        public virtual IByteBuffer SkipBytes(int length)
        {
            CheckReadableBytes(length);
            _readerIndex += length;
            return this;
        }

        public virtual IByteBuffer WriteBoolean(bool value)
        {
            _ = WriteByte(value ? 1 : 0);
            return this;
        }

        public virtual IByteBuffer WriteByte(int value)
        {
            EnsureWritable0(1);
            _SetByte(_writerIndex++, value);
            return this;
        }

        public virtual IByteBuffer WriteShort(int value)
        {
            EnsureWritable0(2);
            int writerIdx = _writerIndex;
            _SetShort(writerIdx, value);
            _writerIndex = writerIdx + 2;
            return this;
        }

        public virtual IByteBuffer WriteShortLE(int value)
        {
            EnsureWritable0(2);
            int writerIdx = _writerIndex;
            _SetShortLE(writerIdx, value);
            _writerIndex = writerIdx + 2;
            return this;
        }

        public virtual IByteBuffer WriteMedium(int value)
        {
            EnsureWritable0(3);
            int writerIdx = _writerIndex;
            _SetMedium(writerIdx, value);
            _writerIndex = writerIdx + 3;
            return this;
        }

        public virtual IByteBuffer WriteMediumLE(int value)
        {
            EnsureWritable0(3);
            int writerIdx = _writerIndex;
            _SetMediumLE(writerIdx, value);
            _writerIndex = writerIdx + 3;
            return this;
        }

        public virtual IByteBuffer WriteInt(int value)
        {
            EnsureWritable0(4);
            int writerIdx = _writerIndex;
            _SetInt(writerIdx, value);
            _writerIndex = writerIdx + 4;
            return this;
        }

        public virtual IByteBuffer WriteIntLE(int value)
        {
            EnsureWritable0(4);
            int writerIdx = _writerIndex;
            _SetIntLE(writerIdx, value);
            _writerIndex = writerIdx + 4;
            return this;
        }

        public virtual IByteBuffer WriteLong(long value)
        {
            EnsureWritable0(8);
            int writerIdx = _writerIndex;
            _SetLong(writerIdx, value);
            _writerIndex = writerIdx + 8;
            return this;
        }

        public virtual IByteBuffer WriteLongLE(long value)
        {
            EnsureWritable0(8);
            int writerIdx = _writerIndex;
            _SetLongLE(writerIdx, value);
            _writerIndex = writerIdx + 8;
            return this;
        }

        public virtual IByteBuffer WriteBytes(byte[] src, int srcIndex, int length)
        {
            _ = EnsureWritable(length);
            int writerIdx = _writerIndex;
            _ = SetBytes(writerIdx, src, srcIndex, length);
            _writerIndex = writerIdx + length;
            return this;
        }

        public virtual IByteBuffer WriteBytes(byte[] src)
        {
            _ = WriteBytes(src, 0, src.Length);
            return this;
        }

        public virtual IByteBuffer WriteBytes(IByteBuffer src, int length)
        {
            if (CheckBounds) { CheckReadableBounds(src, length); }

            _ = WriteBytes(src, src.ReaderIndex, length);
            _ = src.SetReaderIndex(src.ReaderIndex + length);
            return this;
        }

        public virtual IByteBuffer WriteBytes(IByteBuffer src, int srcIndex, int length)
        {
            _ = EnsureWritable(length);
            int writerIdx = _writerIndex;
            _ = SetBytes(writerIdx, src, srcIndex, length);
            _writerIndex = writerIdx + length;
            return this;
        }

        public virtual async Task WriteBytesAsync(Stream stream, int length, CancellationToken cancellationToken)
        {
            _ = EnsureWritable(length);
            if (WritableBytes < length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.length);
            }

            int writerIdx = _writerIndex;
            int wrote = await SetBytesAsync(writerIdx, stream, length, cancellationToken);

            Debug.Assert(writerIdx == _writerIndex);
            _writerIndex = writerIdx + wrote;
        }

        public virtual IByteBuffer WriteZero(int length)
        {
            if (0u >= (uint)length)
            {
                return this;
            }

            _ = EnsureWritable(length);
            int wIndex = _writerIndex;
            CheckIndex0(wIndex, length);

            int nLong = length.RightUShift(3);
            int nBytes = length & 7;
            for (int i = nLong; i > 0; i--)
            {
                _SetLong(wIndex, 0);
                wIndex += 8;
            }
            if (nBytes == 4)
            {
                _SetInt(wIndex, 0);
                wIndex += 4;
            }
            else if (nBytes < 4)
            {
                for (int i = nBytes; i > 0; i--)
                {
                    _SetByte(wIndex, 0);
                    wIndex++;
                }
            }
            else
            {
                _SetInt(wIndex, 0);
                wIndex += 4;
                for (int i = nBytes - 4; i > 0; i--)
                {
                    _SetByte(wIndex, 0);
                    wIndex++;
                }
            }

            _writerIndex = wIndex;
            return this;
        }

        public virtual int WriteCharSequence(ICharSequence sequence, Encoding encoding)
        {
            int writerIdx = _writerIndex;
            int written = SetCharSequence0(writerIdx, sequence, encoding, true);
            _writerIndex = writerIdx + written;
            return written;
        }

        public virtual int WriteString(string value, Encoding encoding)
        {
            int writerIdx = _writerIndex;
            int written = SetString0(writerIdx, value, encoding, true);
            _writerIndex = writerIdx + written;
            return written;
        }

        public abstract IByteBuffer Copy(int index, int length);

        public virtual IByteBuffer Duplicate()
        {
            EnsureAccessible();
            return new UnpooledDuplicatedByteBuffer(this);
        }

        public virtual IByteBuffer RetainedDuplicate() => (IByteBuffer)Duplicate().Retain();

        public virtual IByteBuffer Slice() => Slice(_readerIndex, ReadableBytes);

        public virtual IByteBuffer RetainedSlice() => (IByteBuffer)Slice().Retain();

        public virtual IByteBuffer Slice(int index, int length)
        {
            EnsureAccessible();
            return new UnpooledSlicedByteBuffer(this, index, length);
        }

        public virtual IByteBuffer RetainedSlice(int index, int length) => (IByteBuffer)Slice(index, length).Retain();

        public virtual int ForEachByte(int index, int length, IByteProcessor processor)
        {
            CheckIndex(index, length);
            return ForEachByteAsc0(index, length, processor);
        }

        public virtual int ForEachByteDesc(int index, int length, IByteProcessor processor)
        {
            CheckIndex(index, length);
            return ForEachByteDesc0(index, length, processor);
        }

        public override int GetHashCode() => ByteBufferUtil.HashCode(this);

        public sealed override bool Equals(object o) => Equals(o as IByteBuffer);

        public virtual bool Equals(IByteBuffer buffer) =>
            ReferenceEquals(this, buffer) || buffer is object && ByteBufferUtil.Equals(this, buffer);

        public virtual int CompareTo(IByteBuffer that) => ByteBufferUtil.Compare(this, that);

        public override string ToString()
        {
            if (0u >= (uint)ReferenceCount)
            {
                return StringUtil.SimpleClassName(this) + "(freed)";
            }

            var buf = StringBuilderManager.Allocate()
                .Append(StringUtil.SimpleClassName(this))
                .Append("(ridx: ").Append(_readerIndex)
                .Append(", widx: ").Append(_writerIndex)
                .Append(", cap: ").Append(Capacity);
            if (MaxCapacity != int.MaxValue)
            {
                _ = buf.Append('/').Append(MaxCapacity);
            }

            IByteBuffer unwrapped = Unwrap();
            if (unwrapped is object)
            {
                _ = buf.Append(", unwrapped: ").Append(unwrapped);
            }
            _ = buf.Append(')');
            return StringBuilderManager.ReturnAndFree(buf);
        }

        protected void CheckIndex(int index) => CheckIndex(index, 1);

        protected internal void CheckIndex(int index, int fieldLength)
        {
            EnsureAccessible();
            CheckIndex0(index, fieldLength);
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        protected void CheckIndex0(int index, int fieldLength)
        {
            if (CheckBounds) { CheckRangeBounds(ExceptionArgument.index, index, fieldLength, Capacity); }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        protected void CheckSrcIndex(int index, int length, int srcIndex, int srcCapacity)
        {
            CheckIndex(index, length);
            if (CheckBounds) { CheckRangeBounds(ExceptionArgument.srcIndex, srcIndex, length, srcCapacity); }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        protected void CheckDstIndex(int index, int length, int dstIndex, int dstCapacity)
        {
            CheckIndex(index, length);
            if (CheckBounds) { CheckRangeBounds(ExceptionArgument.dstIndex, dstIndex, length, dstCapacity); }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        protected void CheckDstIndex(int length, int dstIndex, int dstCapacity)
        {
            CheckReadableBytes(length);
            if (CheckBounds) { CheckRangeBounds(ExceptionArgument.dstIndex, dstIndex, length, dstCapacity); }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        protected void CheckReadableBytes(int minimumReadableBytes)
        {
            if ((uint)minimumReadableBytes > SharedConstants.TooBigOrNegative) // < 0
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_MinimumReadableBytes(minimumReadableBytes);
            }

            CheckReadableBytes0(minimumReadableBytes);
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        protected void CheckNewCapacity(int newCapacity)
        {
            EnsureAccessible();
            if (CheckBounds && (/*newCapacity < 0 || */(uint)newCapacity > (uint)MaxCapacity))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_Capacity(newCapacity, MaxCapacity);
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        private void CheckReadableBytes0(int minimumReadableBytes)
        {
            EnsureAccessible();
            if (CheckBounds)
            {
                CheckMinReadableBounds(minimumReadableBytes, _readerIndex, _writerIndex, this);
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        protected void EnsureAccessible()
        {
            if (CheckAccessible && !IsAccessible)
            {
                ThrowHelper.ThrowIllegalReferenceCountException(0);
            }
        }

        protected void SetIndex0(int readerIdx, int writerIdx)
        {
            _readerIndex = readerIdx;
            _writerIndex = writerIdx;
        }

        protected void DiscardMarks()
        {
            _markedReaderIndex = _markedWriterIndex = 0;
        }

        public abstract bool IsSingleIoBuffer { get; }

        public abstract int IoBufferCount { get; }

        public abstract ArraySegment<byte> GetIoBuffer(int index, int length);

        public abstract ArraySegment<byte>[] GetIoBuffers(int index, int length);

        public abstract bool HasArray { get; }

        public abstract byte[] Array { get; }

        public abstract int ArrayOffset { get; }

        public abstract bool HasMemoryAddress { get; }

        public abstract ref byte GetPinnableMemoryAddress();

        public abstract IntPtr AddressOfPinnedMemory();

        public virtual bool IsContiguous => false;

        public abstract IByteBuffer Unwrap();

        public abstract bool IsDirect { get; }

        public virtual bool IsAccessible => (uint)ReferenceCount > 0u;

        public abstract int ReferenceCount { get; }

        public abstract IReferenceCounted Retain();

        public abstract IReferenceCounted Retain(int increment);

        public abstract IReferenceCounted Touch();

        public abstract IReferenceCounted Touch(object hint);

        public abstract bool Release();

        public abstract bool Release(int decrement);
    }
}