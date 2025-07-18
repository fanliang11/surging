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
 * Copyright (c) The DotNetty Project (Microsoft). All rights reserved.
 *
 *   https://github.com/azure/dotnetty
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Common.Utilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using DotNetty.Common.Internal;

    public sealed partial class AsciiString : ICharSequence, IEquatable<AsciiString>, IComparable<AsciiString>, IComparable
    {
        readonly byte[] value;
        readonly int offset;
        readonly int length;

        int hash;

        //Used to cache the ToString() value.
        string stringValue;

        // Called by AppendableCharSequence for http headers
        internal AsciiString(byte[] value)
        {
            this.value = value;
            this.offset = 0;
            this.length = value.Length;
        }

        public AsciiString(byte[] value, bool copy) : this(value, 0, value.Length, copy)
        {
        }

        public AsciiString(byte[] value, int start, int length, bool copy)
        {
            if (copy)
            {
                this.value = new byte[length];
                PlatformDependent.CopyMemory(value, start, this.value, 0, length);
                this.offset = 0;
            }
            else
            {
                if (MathUtil.IsOutOfBounds(start, length, value.Length))
                {
                    ThrowIndexOutOfRangeException_Start(start, length, value.Length);
                }

                this.value = value;
                this.offset = start;
            }

            this.length = length;
        }

        public AsciiString(char[] value) : this(value, 0, value.Length)
        {
        }

        public unsafe AsciiString(char[] value, int start, int length)
        {
            if (MathUtil.IsOutOfBounds(start, length, value.Length))
            {
                ThrowIndexOutOfRangeException_Start(start, length, value.Length);
            }

#if NETCOREAPP_3_0_GREATER
            Span<byte> span = this.value = new byte[length];
            GetBytes(value.AsSpan(start, length), span);
#else
            this.value = new byte[length];
            fixed (char* chars = value)
            fixed (byte* bytes = this.value)
                GetBytes(chars + start, length, bytes);
#endif

            this.offset = 0;
            this.length = length;
        }

        public AsciiString(char[] value, Encoding encoding) : this(value, encoding, 0, value.Length)
        {
        }

        public AsciiString(char[] value, Encoding encoding, int start, int length)
        {
            this.value = encoding.GetBytes(value, start, length);
            this.offset = 0;
            this.length = this.value.Length;
        }

        public AsciiString(ICharSequence value) : this(value, 0, value.Count)
        {
        }

        public AsciiString(ICharSequence value, int start, int length)
        {
            if (MathUtil.IsOutOfBounds(start, length, value.Count))
            {
                ThrowIndexOutOfRangeException_Start(start, length, value.Count);
            }

            var thisVal = new byte[length];
#if NETCOREAPP_3_0_GREATER
            switch (value)
            {
                case IHasAsciiSpan asciiSpan:
                    asciiSpan.AsciiSpan.Slice(start, length).CopyTo(thisVal);
                    break;
                case IHasUtf16Span utf16Span:
                    GetBytes(utf16Span.Utf16Span.Slice(start, length), thisVal);
                    break;
                default:
#endif
            for (int i = 0, j = start; i < length; i++, j++)
            {
                thisVal[i] = CharToByte(value[j]);
            }
#if NETCOREAPP_3_0_GREATER
                    break;
            }
#endif

            this.offset = 0;
            this.length = length;
            this.value = thisVal;
        }

        public AsciiString(string value, Encoding encoding) : this(value, encoding, 0, value.Length)
        {
        }

        public AsciiString(string value, Encoding encoding, int start, int length)
        {
            int count = encoding.GetMaxByteCount(length);
            var bytes = new byte[count];
            count = encoding.GetBytes(value, start, length, bytes, 0);

            var thisVal = new byte[count];
            PlatformDependent.CopyMemory(bytes, 0, thisVal, 0, count);

            this.offset = 0;
            this.length = thisVal.Length;
            this.value = thisVal;
        }

        public AsciiString(string value) : this(value, 0, value.Length)
        {
        }

        public AsciiString(string value, int start, int length)
        {
            if (MathUtil.IsOutOfBounds(start, length, value.Length))
            {
                ThrowIndexOutOfRangeException_Start(start, length, value.Length);
            }

            var thisVal = new byte[length];
#if NETCOREAPP_3_0_GREATER
            GetBytes(value.AsSpan(start, length), thisVal);
#else
            var len = start + length;
            var idx = 0;
            for (int i = start; i < len; i++)
            {
                thisVal[idx++] = CharToByte(value[i]);
            }
#endif

            this.offset = 0;
            this.length = length;
            this.value = thisVal;
        }

        public byte ByteAt(int index)
        {
            // We must do a range check here to enforce the access does not go outside our sub region of the array.
            // We rely on the array access itself to pick up the array out of bounds conditions
            if ((uint)index >= (uint)this.length)
            {
                ThrowIndexOutOfRangeException_Index(index, this.length);
            }

            return this.value[index + this.offset];
        }

        public bool IsEmpty => 0u >= (uint)this.length;

        public int Count => this.length;

        /// <summary>
        /// During normal use cases the AsciiString should be immutable, but if the
        /// underlying array is shared, and changes then this needs to be called.
        /// </summary>
        public void ArrayChanged()
        {
            this.stringValue = null;
            this.hash = 0;
        }

        public byte[] Array => this.value;

        public int Offset => this.offset;

        public bool IsEntireArrayUsed => 0u >= (uint)this.offset && this.length == this.value.Length;

        public byte[] ToByteArray(int start, int end)
        {
            int count = end - start;
            var bytes = new byte[count];
            PlatformDependent.CopyMemory(this.value, this.offset + start, bytes, 0, count);

            return bytes;
        }

        public void Copy(int srcIdx, byte[] dst, int dstIdx, int count)
        {
            if (dst is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dst); }

            if (MathUtil.IsOutOfBounds(srcIdx, count, this.length))
            {
                ThrowIndexOutOfRangeException_SrcIndex(srcIdx, count, this.length);
            }
            if (0u >= (uint)count)
            {
                return;
            }

            PlatformDependent.CopyMemory(this.value, srcIdx + this.offset, dst, dstIdx, count);
        }

        public char this[int index] => ByteToChar(this.ByteAt(index));

        public ICharSequence SubSequence(int start) => this.SubSequence(start, this.length);

        public ICharSequence SubSequence(int start, int end) => this.SubSequence(start, end, true);

        public AsciiString SubSequence(int start, int end, bool copy)
        {
            var thisLen = this.length;
            if (MathUtil.IsOutOfBounds(start, end - start, thisLen))
            {
                ThrowIndexOutOfRangeException_StartEnd(start, end, thisLen);
            }

            if (0u >= (uint)start && end == thisLen)
            {
                return this;
            }

            return end == start ? Empty : new AsciiString(this.value, start + this.offset, end - start, copy);
        }

        public AsciiString Trim()
        {
            if (0u >= (uint)this.length) { return this; }

            int start = this.offset;
            int last = this.offset + this.length - 1;
            int end = last;
            var thisValue = this.value;
            while (start <= end && thisValue[start] <= uSpace)
            {
                start++;
            }
            while (end >= start && thisValue[end] <= uSpace)
            {
                end--;
            }
            if (0u >= (uint)start && end == last)
            {
                return this;
            }

            return new AsciiString(thisValue, start, end - start + 1, false);
        }

        public unsafe bool ContentEquals(string a)
        {
            if (a is null) { return false; }

            if (this.stringValue is object)
            {
#if NETCOREAPP_3_0_GREATER || NETSTANDARD_2_0_GREATER
                return string.Equals(this.stringValue, a);
#else
                return string.Equals(this.stringValue, a, StringComparison.Ordinal);
#endif
            }
            uint uThisLen = (uint)this.length;
            if (uThisLen != (uint)a.Length)
            {
                return false;
            }

            if (uThisLen > 0u)
            {
                fixed (char* p = a)
                fixed (byte* b = &this.value[this.offset])
                    for (int i = 0; i < this.length; ++i)
                    {
                        if (CharToByte(*(p + i)) != *(b + i))
                        {
                            return false;
                        }
                    }
            }

            return true;
        }

        public AsciiString[] Split(char delim)
        {
            List<AsciiString> res = InternalThreadLocalMap.Get().AsciiStringList();

            int start = 0;
            int count = this.length;
            for (int i = start; i < count; i++)
            {
                if (this[i] == delim)
                {
                    if (start == i)
                    {
                        res.Add(Empty);
                    }
                    else
                    {
                        res.Add(new AsciiString(this.value, start + this.offset, i - start, false));
                    }
                    start = i + 1;
                }
            }

            if (0u >= (uint)start)
            {
                // If no delimiter was found in the value
                res.Add(this);
            }
            else
            {
                if (start != count)
                {
                    // Add the last element if it's not empty.
                    res.Add(new AsciiString(this.value, start + this.offset, count - start, false));
                }
                else
                {
                    // Truncate trailing empty elements.
                    while ((uint)res.Count > 0u)
                    {
                        int i = res.Count - 1;
                        if (!res[i].IsEmpty)
                        {
                            res.RemoveAt(i);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            var strings = new AsciiString[res.Count];
            res.CopyTo(strings);
            return strings;
        }

        // ReSharper disable NonReadonlyMemberInGetHashCode
        public override int GetHashCode()
        {
            int h = this.hash;
            if (0u >= (uint)h)
            {
                h = PlatformDependent.HashCodeAscii(this.value, this.offset, this.length);
                this.hash = h;
            }

            return h;
        }

        public override string ToString()
        {
            if (this.stringValue is object)
            {
                return this.stringValue;
            }

            this.stringValue = this.ToString(0);
            return this.stringValue;
        }

        public string ToString(int start) => this.ToString(start, this.length);

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public unsafe string ToString(int start, int end)
        {
            int count = end - start;
            if (MathUtil.IsOutOfBounds(start, count, this.length))
            {
                ThrowIndexOutOfRangeException_SrcIndex(start, count, this.length);
            }
            if (0u >= (uint)count)
            {
                return string.Empty;
            }

            fixed (byte* p = &this.value[this.offset + start])
            {
                return Marshal.PtrToStringAnsi((IntPtr)p, count);
            }
        }

        public static explicit operator string(AsciiString value) => value?.ToString() ?? string.Empty;

        public static explicit operator AsciiString(string value) => value is object ? new AsciiString(value) : Empty;

        static unsafe void GetBytes(char* chars, int length, byte* bytes)
        {
            char* charEnd = chars + length;
            while (chars < charEnd)
            {
                char ch = *(chars++);
                // ByteToChar
                if (ch > uMaxCharValue)
                {
                    *(bytes++) = Replacement;
                }
                else
                {
                    *(bytes++) = unchecked((byte)ch);
                }
            }
        }

        public int HashCode(bool ignoreCase) => !ignoreCase ? this.GetHashCode() : CaseInsensitiveHasher.GetHashCode(this);

        //
        // Compares the specified string to this string using the ASCII values of the characters. Returns 0 if the strings
        // contain the same characters in the same order. Returns a negative integer if the first non-equal character in
        // this string has an ASCII value which is less than the ASCII value of the character at the same position in the
        // specified string, or if this string is a prefix of the specified string. Returns a positive integer if the first
        // non-equal character in this string has a ASCII value which is greater than the ASCII value of the character at
        // the same position in the specified string, or if the specified string is a prefix of this string.
        // 
        public int CompareTo(AsciiString other)
        {
            if (ReferenceEquals(this, other)) { return 0; }

#if NET
            return this.AsciiSpan.SequenceCompareTo(other.AsciiSpan);
#else
            return SpanHelpers.SequenceCompareTo(
                ref MemoryMarshal.GetReference(this.AsciiSpan), this.length,
                ref MemoryMarshal.GetReference(other.AsciiSpan), other.Count);
#endif
        }

        public int CompareTo(object obj) => this.CompareTo(obj as AsciiString);

        public IEnumerator<char> GetEnumerator() => new CharSequenceEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
