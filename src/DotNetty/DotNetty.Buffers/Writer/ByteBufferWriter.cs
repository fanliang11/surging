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
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotNetty.Buffers
{
    public ref partial struct ByteBufferWriter
    {
        public const uint StackallocThreshold = 256u;

        private readonly IByteBuffer _output;
        private int _buffered;
        private int _bytesCommitted;
        private Span<byte> _buffer;

        /// <summary>Returns the total amount of bytes written by the <see cref="ByteBufferWriter"/> so far
        /// for the current instance of the <see cref="ByteBufferWriter"/>.
        /// This includes data that has been written beyond what has already been committed.</summary>
        public int BytesWritten
        {
            get
            {
                Debug.Assert(BytesCommitted <= _output.MaxCapacity - _buffered);
                return _bytesCommitted + _buffered;
            }
        }

        /// <summary>Returns the total amount of bytes committed to the output by the <see cref="ByteBufferWriter"/> so far
        /// for the current instance of the <see cref="ByteBufferWriter"/>.
        /// This is how much the <see cref="IByteBuffer"/> has advanced.</summary>
        public int BytesCommitted => _bytesCommitted;

        /// <summary>Constructs a new <see cref="ByteBufferWriter"/> instance with a specified <paramref name="byteBuffer"/>.</summary>
        /// <param name="byteBuffer">An instance of <see cref="IByteBuffer" /> used as a destination for writing.</param>
        /// <exception cref="ArgumentNullException">Thrown when the instance of <see cref="IByteBuffer" /> that is passed in is null.</exception>
        public ByteBufferWriter(IByteBuffer byteBuffer)
        {
            if (byteBuffer is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.byteBuffer); }
            if (!byteBuffer.IsSingleIoBuffer) { ThrowHelper.ThrowNotSupportedException_UncompositeBuffer(); }

            _output = byteBuffer;
            _buffered = 0;
            _bytesCommitted = 0;
            _buffer = _output.FreeSpan;
        }

        public void WriteBoolean(bool value) => WriteByte(value ? SharedConstants.True : SharedConstants.False);
        public void WriteByte(int value)
        {
            GrowAndEnsureIf(1);
            _buffer[0] = unchecked((byte)value);
            AdvanceCore(1);
        }

        public void WriteBytes(in ReadOnlySpan<byte> source)
        {
            GrowAndEnsureIf();
            var idx = 0;
            CopyLoop(source, ref idx);
            Advance(ref idx);
        }

        /// <summary>Writes a 32-bit integer in a compressed format.</summary>
        /// <param name="value">The 32-bit integer to be written.</param>
        public void Write7BitEncodedInt(int value)
        {
            var idx = 0;
            Write7BitEncodedInt0(value, ref idx);
            Advance(ref idx);
        }

        /// <summary>Advances the underlying <see cref="IByteBuffer" /> based on what has been written so far.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Flush()
        {
            if (0u >= (uint)_buffered) { return; }

            _output.Advance(_buffered);
            _bytesCommitted += _buffered;
            _buffered = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Advance(ref int alreadyWritten)
        {
            if ((uint)alreadyWritten >= (uint)_buffer.Length)
            {
                AdvanceAndGrow(ref alreadyWritten);
            }
            else
            {
                AdvanceCore(alreadyWritten);
                alreadyWritten = 0;
            }
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        private void AdvanceAndGrowIf(ref int alreadyWritten)
        {
            if ((uint)alreadyWritten >= (uint)_buffer.Length) { AdvanceAndGrow(ref alreadyWritten); }
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        private void AdvanceAndGrowIf(ref int alreadyWritten, int sizeHintt)
        {
            if ((uint)sizeHintt >= (uint)(_buffer.Length - alreadyWritten)) { AdvanceAndGrow(ref alreadyWritten, sizeHintt); }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AdvanceAndGrow(ref int alreadyWritten)
        {
            Debug.Assert(alreadyWritten >= 0);
            AdvanceCore(alreadyWritten);
            GrowAndEnsure();
            alreadyWritten = 0;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AdvanceAndGrow(ref int alreadyWritten, int sizeHintt)
        {
            AdvanceCore(alreadyWritten);
            GrowAndEnsure(sizeHintt);
            alreadyWritten = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AdvanceCore(int count)
        {
            Debug.Assert(count >= 0 && _buffered <= _output.MaxCapacity - count);

            if (0u >= (uint)count) { return; }

            _buffered += count;
            _buffer = _buffer.Slice(count);
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        private void GrowAndEnsureIf()
        {
            if (0u >= (uint)_buffer.Length) { GrowAndEnsure(); }
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        private void GrowAndEnsureIf(int sizeHintt)
        {
            if ((uint)sizeHintt >= (uint)_buffer.Length) { GrowAndEnsure(sizeHintt); }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void GrowAndEnsure()
        {
            Flush();
            var previousSpanLength = _buffer.Length;
            _buffer = _output.GetSpan(previousSpanLength + 1);
            if ((uint)previousSpanLength >= (uint)_buffer.Length)
            {
                ThrowHelper.ThrowArgumentException_FailedToGetLargerSpan();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void GrowAndEnsure(int sizeHintt)
        {
            Flush();
            _buffer = _output.GetSpan(sizeHintt);
            if ((uint)sizeHintt > (uint)_buffer.Length)
            {
                ThrowHelper.ThrowArgumentException_FailedToGetMinimumSizeSpan(sizeHintt);
            }
        }

        private void CopyLoop(in ReadOnlySpan<byte> source, ref int idx)
        {
            if ((uint)source.Length <= (uint)(_buffer.Length - idx))
            {
                source.CopyTo(_buffer.Slice(idx));
                idx += source.Length;
                return;
            }

            CopyLoopSlow(source, ref idx);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void CopyLoopSlow(in ReadOnlySpan<byte> source, ref int idx)
        {
            var copyLen = _buffer.Length - idx;
            source.Slice(0, copyLen).CopyTo(_buffer.Slice(idx));
            var remaining = source.Slice(copyLen);
            idx = _buffer.Length;
            CopyLoopCore(ref remaining, ref idx);
        }

        private void CopyLoopCore(ref ReadOnlySpan<byte> remaining, ref int idx)
        {
            AdvanceAndGrow(ref idx);

            while (true)
            {
                if ((uint)remaining.Length <= (uint)(_buffer.Length - idx))
                {
                    remaining.CopyTo(_buffer.Slice(idx));
                    idx += remaining.Length;
                    break;
                }

                var copyLen = _buffer.Length - idx;
                remaining.Slice(0, copyLen).CopyTo(_buffer.Slice(idx));
                remaining = remaining.Slice(copyLen);
                idx = _buffer.Length;
                AdvanceAndGrow(ref idx);
            }
        }
    }
}
