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
    using static AdvancedLeakAwareByteBuffer;

    partial class AdvancedLeakAwareCompositeByteBuffer
    {
        public override ReadOnlyMemory<byte> UnreadMemory
        {
            get
            {
                RecordLeakNonRefCountingOperation(Leak);
                return base.UnreadMemory;
            }
        }

        public override ReadOnlyMemory<byte> GetReadableMemory(int index, int count)
        {
            RecordLeakNonRefCountingOperation(Leak);
            return base.GetReadableMemory(index, count);
        }

        public override ReadOnlySpan<byte> UnreadSpan
        {
            get
            {
                RecordLeakNonRefCountingOperation(Leak);
                return base.UnreadSpan;
            }
        }

        public override ReadOnlySpan<byte> GetReadableSpan(int index, int count)
        {
            RecordLeakNonRefCountingOperation(Leak);
            return base.GetReadableSpan(index, count);
        }

        public override ReadOnlySequence<byte> UnreadSequence
        {
            get
            {
                RecordLeakNonRefCountingOperation(Leak);
                return base.UnreadSequence;
            }
        }

        public override ReadOnlySequence<byte> GetSequence(int index, int count)
        {
            RecordLeakNonRefCountingOperation(Leak);
            return base.GetSequence(index, count);
        }

        public override Memory<byte> FreeMemory
        {
            get
            {
                RecordLeakNonRefCountingOperation(Leak);
                return base.FreeMemory;
            }
        }

        public override Memory<byte> GetMemory(int sizeHintt = 0)
        {
            RecordLeakNonRefCountingOperation(Leak);
            return base.GetMemory(sizeHintt);
        }
        public override Memory<byte> GetMemory(int index, int count)
        {
            RecordLeakNonRefCountingOperation(Leak);
            return base.GetMemory(index, count);
        }

        public override Span<byte> FreeSpan
        {
            get
            {
                RecordLeakNonRefCountingOperation(Leak);
                return base.FreeSpan;
            }
        }

        public override Span<byte> GetSpan(int sizeHintt = 0)
        {
            RecordLeakNonRefCountingOperation(Leak);
            return base.GetSpan(sizeHintt);
        }
        public override Span<byte> GetSpan(int index, int count)
        {
            RecordLeakNonRefCountingOperation(Leak);
            return base.GetSpan(index, count);
        }

        public override int GetBytes(int index, Span<byte> destination)
        {
            RecordLeakNonRefCountingOperation(Leak);
            return base.GetBytes(index, destination);
        }
        public override int GetBytes(int index, Memory<byte> destination)
        {
            RecordLeakNonRefCountingOperation(Leak);
            return base.GetBytes(index, destination);
        }

        public override int ReadBytes(Span<byte> destination)
        {
            RecordLeakNonRefCountingOperation(Leak);
            return base.ReadBytes(destination);
        }
        public override int ReadBytes(Memory<byte> destination)
        {
            RecordLeakNonRefCountingOperation(Leak);
            return base.ReadBytes(destination);
        }

        public override IByteBuffer SetBytes(int index, in ReadOnlySpan<byte> src)
        {
            RecordLeakNonRefCountingOperation(Leak);
            return base.SetBytes(index, src);
        }
        public override IByteBuffer SetBytes(int index, in ReadOnlyMemory<byte> src)
        {
            RecordLeakNonRefCountingOperation(Leak);
            return base.SetBytes(index, src);
        }

        public override IByteBuffer WriteBytes(in ReadOnlySpan<byte> src)
        {
            RecordLeakNonRefCountingOperation(Leak);
            return base.WriteBytes(src);
        }
        public override IByteBuffer WriteBytes(in ReadOnlyMemory<byte> src)
        {
            RecordLeakNonRefCountingOperation(Leak);
            return base.WriteBytes(src);
        }
    }
}
