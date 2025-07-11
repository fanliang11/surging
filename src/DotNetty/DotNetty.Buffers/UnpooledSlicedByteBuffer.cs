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
    partial class UnpooledSlicedByteBuffer : AbstractUnpooledSlicedByteBuffer
    {
        internal UnpooledSlicedByteBuffer(AbstractByteBuffer buffer, int index, int length)
            : base(buffer, index, length)
        {
        }

        public sealed override int Capacity => MaxCapacity;

        protected internal sealed override byte _GetByte(int index) => UnwrapCore()._GetByte(Idx(index));

        protected internal sealed override short _GetShort(int index) => UnwrapCore()._GetShort(Idx(index));

        protected internal sealed override short _GetShortLE(int index) => UnwrapCore()._GetShortLE(Idx(index));

        protected internal sealed override int _GetUnsignedMedium(int index) => UnwrapCore()._GetUnsignedMedium(Idx(index));

        protected internal sealed override int _GetUnsignedMediumLE(int index) => UnwrapCore()._GetUnsignedMediumLE(Idx(index));

        protected internal sealed override int _GetInt(int index) => UnwrapCore()._GetInt(Idx(index));

        protected internal sealed override int _GetIntLE(int index) => UnwrapCore()._GetIntLE(Idx(index));

        protected internal sealed override long _GetLong(int index) => UnwrapCore()._GetLong(Idx(index));

        protected internal sealed override long _GetLongLE(int index) => UnwrapCore()._GetLongLE(Idx(index));

        protected internal sealed override void _SetByte(int index, int value) => UnwrapCore()._SetByte(Idx(index), value);

        protected internal sealed override void _SetShort(int index, int value) => UnwrapCore()._SetShort(Idx(index), value);

        protected internal sealed override void _SetShortLE(int index, int value) => UnwrapCore()._SetShortLE(Idx(index), value);

        protected internal sealed override void _SetMedium(int index, int value) => UnwrapCore()._SetMedium(Idx(index), value);

        protected internal sealed override void _SetMediumLE(int index, int value) => UnwrapCore()._SetMediumLE(Idx(index), value);

        protected internal sealed override void _SetInt(int index, int value) => UnwrapCore()._SetInt(Idx(index), value);

        protected internal sealed override void _SetIntLE(int index, int value) => UnwrapCore()._SetIntLE(Idx(index), value);

        protected internal sealed override void _SetLong(int index, long value) => UnwrapCore()._SetLong(Idx(index), value);

        protected internal sealed override void _SetLongLE(int index, long value) => UnwrapCore()._SetLongLE(Idx(index), value);
    }
}
