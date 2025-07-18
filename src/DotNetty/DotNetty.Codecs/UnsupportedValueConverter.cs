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

    public sealed class UnsupportedValueConverter<T> : IValueConverter<T>
    {
        public static readonly UnsupportedValueConverter<T> Instance = new UnsupportedValueConverter<T>();

        UnsupportedValueConverter()
        {
        }
        
        public T ConvertObject(object value)
        {
            throw new NotSupportedException();
        }

        public T ConvertBoolean(bool value)
        {
            throw new NotSupportedException();
        }

        public bool ConvertToBoolean(T value)
        {
            throw new NotSupportedException();
        }

        public T ConvertByte(byte value)
        {
            throw new NotSupportedException();
        }

        public byte ConvertToByte(T value)
        {
            throw new NotSupportedException();
        }

        public T ConvertChar(char value)
        {
            throw new NotSupportedException();
        }

        public char ConvertToChar(T value)
        {
            throw new NotSupportedException();
        }

        public T ConvertShort(short value)
        {
            throw new NotSupportedException();
        }

        public short ConvertToShort(T value)
        {
            throw new NotSupportedException();
        }

        public T ConvertInt(int value)
        {
            throw new NotSupportedException();
        }

        public int ConvertToInt(T value)
        {
            throw new NotSupportedException();
        }

        public T ConvertLong(long value)
        {
            throw new NotSupportedException();
        }

        public long ConvertToLong(T value)
        {
            throw new NotSupportedException();
        }

        public T ConvertTimeMillis(long value)
        {
            throw new NotSupportedException();
        }

        public long ConvertToTimeMillis(T value)
        {
            throw new NotSupportedException();
        }

        public T ConvertFloat(float value)
        {
            throw new NotSupportedException();
        }

        public float ConvertToFloat(T value)
        {
            throw new NotSupportedException();
        }

        public T ConvertDouble(double value)
        {
            throw new NotSupportedException();
        }

        public double ConvertToDouble(T value)
        {
            throw new NotSupportedException();
        }
    }
}