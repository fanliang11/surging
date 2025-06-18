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

namespace DotNetty.Buffers
{
    using System;
    using System.Runtime.InteropServices;

    public ref partial struct ByteBufferWriter
    {
        public void WriteFloat(float value)
        {
            WriteInt(ByteBufferUtil.SingleToInt32Bits(value));
        }

        public void WriteFloatLE(float value)
        {
            WriteIntLE(ByteBufferUtil.SingleToInt32Bits(value));
        }

        public void WriteDouble(double value)
        {
            WriteLong(BitConverter.DoubleToInt64Bits(value));
        }

        public void WriteDoubleLE(double value)
        {
            WriteLongLE(BitConverter.DoubleToInt64Bits(value));
        }

        public void WriteDecimal(decimal value)
        {
            GrowAndEnsureIf(DecimalValueLength);
            SetDecimal(ref MemoryMarshal.GetReference(_buffer), value);
            AdvanceCore(DecimalValueLength);
        }

        public void WriteDecimalLE(decimal value)
        {
            GrowAndEnsureIf(DecimalValueLength);
            SetDecimalLE(ref MemoryMarshal.GetReference(_buffer), value);
            AdvanceCore(DecimalValueLength);
        }

        public void WriteDatetime(DateTime value)
        {
            WriteLong(value.ToBinary());
        }

        public void WriteDatetimeLE(DateTime value)
        {
            WriteLongLE(value.ToBinary());
        }

        public void WriteTimeSpan(TimeSpan value)
        {
            WriteLong(value.Ticks);
        }

        public void WriteTimeSpanLE(TimeSpan value)
        {
            WriteLongLE(value.Ticks);
        }

        public void WriteGuid(Guid value)
        {
            GrowAndEnsureIf(GuidValueLength);
            value.ToByteArray().AsSpan().CopyTo(_buffer);
            AdvanceCore(GuidValueLength);
        }
    }
}
