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

namespace DotNetty.Common.Utilities
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using DotNetty.Common.Internal;

    partial class AsciiString : IHasAsciiSpan, IHasUtf16Span
    {
        public ReadOnlySpan<byte> AsciiSpan
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var count = this.length;
                if (0u >= (uint)count) { return ReadOnlySpan<byte>.Empty; }
                return new ReadOnlySpan<byte>(this.value, this.offset, count);
            }
        }

        public ReadOnlySpan<char> Utf16Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (0u >= (uint)this.length) { return ReadOnlySpan<char>.Empty; }
                var thisValue = this.stringValue;
                if (thisValue is object) { return thisValue.AsSpan(); }
                return this.ToString().AsSpan();
            }
        }

        public int ForEachByte(IByteProcessor visitor)
        {
            var thisLength = this.length;
            if (0u >= (uint)thisLength) { return IndexNotFound; }
            return SpanHelpers.ForEachByte(ref this.value[this.offset], visitor, thisLength);
        }

        public int ForEachByte(int index, int count, IByteProcessor visitor)
        {
            var thisLength = this.length;
            if (0u >= (uint)thisLength) { return IndexNotFound; }
            if (MathUtil.IsOutOfBounds(index, count, thisLength))
            {
                ThrowIndexOutOfRangeException_Index(index, count, thisLength);
            }
            var idx = SpanHelpers.ForEachByte(ref this.value[this.offset + index], visitor, count);
            if ((uint)count > (uint)idx) { return index + idx; }
            return IndexNotFound;
        }

        public int ForEachByteDesc(IByteProcessor visitor)
        {
            var thisLength = this.length;
            if (0u >= (uint)thisLength) { return IndexNotFound; }
            return SpanHelpers.ForEachByteDesc(ref this.value[this.offset], visitor, thisLength);
        }

        public int ForEachByteDesc(int index, int count, IByteProcessor visitor)
        {
            var thisLength = this.length;
            if (0u >= (uint)thisLength) { return IndexNotFound; }
            if (MathUtil.IsOutOfBounds(index, count, thisLength))
            {
                ThrowIndexOutOfRangeException_Index(index, count, thisLength);
            }

            var idx = SpanHelpers.ForEachByteDesc(ref this.value[this.offset + index], visitor, count);
            if ((uint)count > (uint)idx) { return index + idx; }
            return IndexNotFound;
        }

        public int CompareTo(ICharSequence other)
        {
            if (ReferenceEquals(this, other)) { return 0; }

            switch (other)
            {
                case IHasAsciiSpan ascii:
                    return this.AsciiSpan.SequenceCompareTo(ascii.AsciiSpan);

                case IHasUtf16Span utf16Span:
                    return this.Utf16Span.SequenceCompareTo(utf16Span.Utf16Span);

                default:
                    return CompareTo0(other);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private int CompareTo0(ICharSequence other)
        {
            int length1 = this.length;
            int length2 = other.Count;
            int minLength = Math.Min(length1, length2);
            for (int i = 0, j = this.offset; i < minLength; i++, j++)
            {
                int result = ByteToChar(this.value[j]) - other[i];
                if (result != 0)
                {
                    return result;
                }
            }

            return length1 - length2;
        }

        public AsciiString Concat(ICharSequence charSequence)
        {
            int thisLen = this.length;
            int thatLen = charSequence.Count;
            if (0u >= (uint)thatLen) { return this; }

            if (this.IsEmpty)
            {
                return charSequence is AsciiString asciiStr ? asciiStr : new AsciiString(charSequence);
            }

            if (charSequence is IHasAsciiSpan that)
            {
                var newValue = new byte[thisLen + thatLen];
                var span = new Span<byte>(newValue);
                this.AsciiSpan.CopyTo(span);
                that.AsciiSpan.CopyTo(span.Slice(thisLen));
                return new AsciiString(newValue, false);
            }

            return Concat0(charSequence);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private AsciiString Concat0(ICharSequence charSequence)
        {
            int thisLen = this.length;
            int thatLen = charSequence.Count;

            var newValue = new byte[thisLen + thatLen];
            PlatformDependent.CopyMemory(this.value, this.offset, newValue, 0, thisLen);
            for (int i = thisLen, j = 0; i < newValue.Length; i++, j++)
            {
                newValue[i] = CharToByte(charSequence[j]);
            }

            return new AsciiString(newValue, false);
        }

        public bool ContentEquals(ICharSequence other)
        {
            if (ReferenceEquals(this, other)) { return true; }

            var thisLength = this.length;
            if (other is null || thisLength != other.Count) { return false; }

            switch (other)
            {
                case AsciiString asciiStr:
                    return this.GetHashCode() == asciiStr.GetHashCode()
#if NET
                        && this.AsciiSpan.SequenceEqual(asciiStr.AsciiSpan);
#else
                        && SpanHelpers.SequenceEqual(ref MemoryMarshal.GetReference(this.AsciiSpan), ref MemoryMarshal.GetReference(asciiStr.AsciiSpan), thisLength);
#endif

                case IHasAsciiSpan hasAscii:
#if NET
                    return this.AsciiSpan.SequenceEqual(hasAscii.AsciiSpan);
#else
                    return SpanHelpers.SequenceEqual(ref MemoryMarshal.GetReference(this.AsciiSpan), ref MemoryMarshal.GetReference(hasAscii.AsciiSpan), thisLength);
#endif

                case IHasUtf16Span hasUtf16:
#if NET
                    return this.Utf16Span.SequenceEqual(hasUtf16.Utf16Span);
#else
                    return SpanHelpers.SequenceEqual(ref MemoryMarshal.GetReference(this.Utf16Span), ref MemoryMarshal.GetReference(hasUtf16.Utf16Span), thisLength);
#endif

                default:
                    return ContentEquals0(other);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool ContentEquals0(ICharSequence other)
        {
            for (int i = this.offset, j = 0; j < other.Count; ++i, ++j)
            {
                if (ByteToChar(this.value[i]) != other[j])
                {
                    return false;
                }
            }

            return true;
        }

        public bool ContentEqualsIgnoreCase(ICharSequence other)
        {
            if (ReferenceEquals(this, other)) { return true; }

            switch (other)
            {
                case null:
                    return false;

                case IHasUtf16Span utf16Span:
                    return this.Utf16Span.Equals(utf16Span.Utf16Span, StringComparison.OrdinalIgnoreCase);

                default:
                    return other.Count == this.length && ContentEqualsIgnoreCase0(other);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool ContentEqualsIgnoreCase0(ICharSequence other)
        {
            if (other is AsciiString rhs)
            {
                for (int i = this.offset, j = rhs.offset, end = i + this.length; i < end; ++i, ++j)
                {
                    if (!EqualsIgnoreCase(this.value[i], rhs.value[j]))
                    {
                        return false;
                    }
                }
                return true;
            }

            for (int i = this.offset, j = 0, end = this.length; i < end; ++i, ++j)
            {
                if (!EqualsIgnoreCase(ByteToChar(this.value[i]), other[j]))
                {
                    return false;
                }
            }

            return true;
        }

        public char[] ToCharArray(int start, int end)
        {
            int count = end - start;
            if (0u >= (uint)count)
            {
                return EmptyArrays.EmptyChars;
            }

            return this.Utf16Span.Slice(start, count).ToArray();
        }

        public void Copy(int srcIdx, char[] dst, int dstIdx, int count)
        {
            if (dst is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dst); }

            this.Utf16Span.Slice(srcIdx, count).CopyTo(dst.AsSpan(dstIdx, count));
        }

        public int IndexOf(ICharSequence subString, int start)
        {
            int thisLen = this.length;
            uint uThisLen = (uint)thisLen;
            if (0u >= uThisLen) { return IndexNotFound; }

            uint uStart = (uint)start;
            if (uStart > SharedConstants.TooBigOrNegative)
            {
                start = 0;
                uStart = 0u;
            }

            int subCount = subString.Count;
            uint uSubCount = (uint)subCount;
            if (0u >= uSubCount) { return uStart < uThisLen ? start : thisLen; }
            var searchLen = thisLen - start;
            if (uSubCount > (uint)searchLen) { return IndexNotFound; }

            char firstChar = subString[0];
            if ((uint)firstChar > uMaxCharValue) { return IndexNotFound; }

            if (0u >= uStart)
            {
                if (subString is IHasAsciiSpan hasAscii)
                {
#if NET
                    return this.AsciiSpan.IndexOf(hasAscii.AsciiSpan);
#else
                    return SpanHelpers.IndexOf(ref MemoryMarshal.GetReference(this.AsciiSpan), thisLen, ref MemoryMarshal.GetReference(hasAscii.AsciiSpan), subCount);
#endif
                }
                if (subString is IHasUtf16Span hasUtf16)
                {
#if NET
                    return this.Utf16Span.IndexOf(hasUtf16.Utf16Span);
#else
                    return SpanHelpers.IndexOf(ref MemoryMarshal.GetReference(this.Utf16Span), thisLen, ref MemoryMarshal.GetReference(hasUtf16.Utf16Span), subCount);
#endif
                }
            }
            else
            {
                if (subString is IHasAsciiSpan hasAscii)
                {
#if NET
                    var result = this.AsciiSpan.Slice(start, searchLen).IndexOf(hasAscii.AsciiSpan);
#else
                    var result = SpanHelpers.IndexOf(
                        ref Unsafe.Add(ref MemoryMarshal.GetReference(this.AsciiSpan), start), searchLen,
                        ref MemoryMarshal.GetReference(hasAscii.AsciiSpan), subCount);
#endif
                    return SharedConstants.TooBigOrNegative >= (uint)result ? start + result : IndexNotFound;
                }
                if (subString is IHasUtf16Span hasUtf16)
                {
#if NET
                    var result = this.Utf16Span.Slice(start, searchLen).IndexOf(hasUtf16.Utf16Span);
#else
                    var result = SpanHelpers.IndexOf(
                        ref Unsafe.Add(ref MemoryMarshal.GetReference(this.Utf16Span), start), searchLen,
                        ref MemoryMarshal.GetReference(hasUtf16.Utf16Span), subCount);
#endif
                    return SharedConstants.TooBigOrNegative >= (uint)result ? start + result : IndexNotFound;
                }
            }

            return IndexOf0(subString, start);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private int IndexOf0(ICharSequence subString, int start)
        {
            int subCount = subString.Count;
            char firstChar = subString[0];

            var thisOffset = this.offset;
            var firstCharAsByte = (byte)firstChar;
            var len = thisOffset + this.length - subCount;
            var thisValue = this.value;
            for (int i = start + thisOffset; i <= len; ++i)
            {
                if (thisValue[i] == firstCharAsByte)
                {
                    int o1 = i, o2 = 0;
                    while (++o2 < subCount && ByteToChar(thisValue[++o1]) == subString[o2])
                    {
                        // Intentionally empty
                    }
                    if (o2 == subCount)
                    {
                        return i - thisOffset;
                    }
                }
            }
            return IndexNotFound;
        }

        public int IndexOf(char ch, int start)
        {
            int thisLen = this.length;
            uint uThisLen = (uint)thisLen;
            if (0u >= uThisLen) { return IndexNotFound; }
            if ((uint)ch > uMaxCharValue) { return IndexNotFound; }

            uint uStart = (uint)start;
            if (uStart >= uThisLen)
            {
                start = 0;
                uStart = 0u;
            }

            if (0u >= uStart)
            {
#if NET
                return this.AsciiSpan.IndexOf((byte)ch);
#else
                return SpanHelpers.IndexOf(ref MemoryMarshal.GetReference(this.AsciiSpan), (byte)ch, thisLen);
#endif
            }
            var seachSpan = this.AsciiSpan.Slice(start);
#if NET
            var result = seachSpan.IndexOf((byte)ch);
#else
            var result = SpanHelpers.IndexOf(ref MemoryMarshal.GetReference(seachSpan), (byte)ch, seachSpan.Length);
#endif
            return SharedConstants.TooBigOrNegative >= (uint)result ? start + result : IndexNotFound;
        }

        public int LastIndexOf(ICharSequence subString, int start)
        {
            int thisLen = this.length;
            uint uThisLen = (uint)thisLen;
            if (0u >= uThisLen) { return IndexNotFound; }

            int subCount = subString.Count;
            start = Math.Min(start, thisLen - subCount);
            uint uStart = (uint)start;

            if (uStart > SharedConstants.TooBigOrNegative) { return IndexNotFound; }
            if (0u >= (uint)subCount) { return start; }

            if (subString is IHasAsciiSpan hasAscii)
            {
#if NET
                var searchLength = start + subCount;
                if (searchLength > thisLen)
                {
                    return this.AsciiSpan.LastIndexOf(hasAscii.AsciiSpan);
                }
                return this.AsciiSpan.Slice(0, searchLength).LastIndexOf(hasAscii.AsciiSpan);
#else
                return SpanHelpers.LastIndexOf(
                    ref MemoryMarshal.GetReference(this.AsciiSpan), start + subCount,
                    ref MemoryMarshal.GetReference(hasAscii.AsciiSpan), subCount);
#endif
            }
            if (subString is IHasUtf16Span hasUtf16)
            {
#if NET
                var searchLength = start + subCount;
                if (searchLength > thisLen)
                {
                    return this.Utf16Span.LastIndexOf(hasUtf16.Utf16Span);
                }
                return this.Utf16Span.Slice(0, searchLength).LastIndexOf(hasUtf16.Utf16Span);
#else
                return SpanHelpers.LastIndexOf(
                    ref MemoryMarshal.GetReference(this.Utf16Span), start + subCount,
                    ref MemoryMarshal.GetReference(hasUtf16.Utf16Span), subCount);
#endif
            }

            return LastIndexOf0(subString, start);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private int LastIndexOf0(ICharSequence subString, int start)
        {
            char firstChar = subString[0];
            if ((uint)firstChar > uMaxCharValue) { return IndexNotFound; }

            int subCount = subString.Count;

            byte firstCharAsByte = (byte)firstChar;
            var thisOffset = this.offset;
            for (int i = thisOffset + start; i >= 0; --i)
            {
                if (value[i] == firstCharAsByte)
                {
                    int o1 = i, o2 = 0;
                    while (++o2 < subCount && ByteToChar(value[++o1]) == subString[o2])
                    {
                        // Intentionally empty
                    }
                    if (o2 == subCount)
                    {
                        return i - thisOffset;
                    }
                }
            }
            return IndexNotFound;
        }

        public bool RegionMatches(int thisStart, ICharSequence seq, int start, int count)
        {
            if (seq is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.seq); }

            uint uCount = (uint)count;
            if (0u >= uCount)
            {
                return true;
            }

            if ((uint)start > SharedConstants.TooBigOrNegative || uCount > (uint)(seq.Count - start))
            {
                return false;
            }

            int thisLen = this.length;
            if ((uint)thisStart > SharedConstants.TooBigOrNegative || uCount > (uint)(thisLen - thisStart))
            {
                return false;
            }

            if (seq is IHasAsciiSpan hasAscii)
            {
#if NET
                return this.AsciiSpan.Slice(thisStart, count).SequenceEqual(hasAscii.AsciiSpan.Slice(start, count));
#else
                return SpanHelpers.SequenceEqual(
                    ref Unsafe.Add(ref MemoryMarshal.GetReference(this.AsciiSpan), thisStart),
                    ref Unsafe.Add(ref MemoryMarshal.GetReference(hasAscii.AsciiSpan), start),
                    count);
#endif
            }
            if (seq is IHasUtf16Span hasUtf16)
            {
#if NET
                return this.Utf16Span.Slice(thisStart, count).SequenceEqual(hasUtf16.Utf16Span.Slice(start, count));
#else
                return SpanHelpers.SequenceEqual(
                    ref Unsafe.Add(ref MemoryMarshal.GetReference(this.Utf16Span), thisStart),
                    ref Unsafe.Add(ref MemoryMarshal.GetReference(hasUtf16.Utf16Span), start),
                    count);
#endif
            }

            return RegionMatches0(thisStart, seq, start, count);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool RegionMatches0(int thisStart, ICharSequence seq, int start, int count)
        {
            int thatEnd = start + count;
            for (int i = start, j = thisStart + this.offset; i < thatEnd; i++, j++)
            {
                if (ByteToChar(this.value[j]) != seq[i])
                {
                    return false;
                }
            }

            return true;
        }

        public bool RegionMatchesIgnoreCase(int thisStart, ICharSequence seq, int start, int count)
        {
            if (seq is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.seq); }

            uint uCount = (uint)count;
            if (0u >= uCount)
            {
                return true;
            }

            if ((uint)start > SharedConstants.TooBigOrNegative || uCount > (uint)(seq.Count - start))
            {
                return false;
            }

            int thisLen = this.length;
            if ((uint)thisStart > SharedConstants.TooBigOrNegative || uCount > (uint)(thisLen - thisStart))
            {
                return false;
            }

            if (seq is IHasUtf16Span hasUtf16)
            {
                return this.Utf16Span.Slice(thisStart, count).Equals(hasUtf16.Utf16Span.Slice(start, count), StringComparison.OrdinalIgnoreCase);
            }

            return RegionMatchesIgnoreCase0(thisStart, seq, start, count);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool RegionMatchesIgnoreCase0(int thisStart, ICharSequence seq, int start, int count)
        {
            thisStart += this.offset;
            int thisEnd = thisStart + count;
            while (thisStart < thisEnd)
            {
                if (!EqualsIgnoreCase(ByteToChar(this.value[thisStart++]), seq[start++]))
                {
                    return false;
                }
            }

            return true;
        }

        public AsciiString Replace(char oldChar, char newChar)
        {
            if ((uint)oldChar > uMaxCharValue)
            {
                return this;
            }

            var thisLen = this.length;
            if (0u >= thisLen) { return this; }

            var oldCharAsByte = CharToByte(oldChar);
            var newCharAsByte = CharToByte(newChar);

            var thisSpan = this.AsciiSpan;
            var pos = thisSpan.IndexOf(oldCharAsByte);
            uint uPos = (uint)pos;
            if (uPos > SharedConstants.TooBigOrNegative) { return this; }

            byte[] buffer = new byte[thisLen];
            var span = new Span<byte>(buffer);
            if (uPos > 0u)
            {
                thisSpan.Slice(0, pos).CopyTo(span.Slice(0, pos));
            }
            span[pos++] = newCharAsByte;
            for (var idx = pos; idx < thisLen; idx++)
            {
                byte oldValue = thisSpan[idx];
                span[idx] = oldValue != oldCharAsByte ? oldValue : newCharAsByte;
            }
            return new AsciiString(buffer, false);
        }

        public AsciiString ToLowerCase()
        {
            var thisLen = this.length;
            if (0u >= (uint)thisLen) { return this; }

            var thisSpan = this.AsciiSpan;
            var index = SpanHelpers.FindIndex(ref MemoryMarshal.GetReference(thisSpan), x => IsUpperCase(x), thisLen);
            uint uIndex = (uint)index;
            if (uIndex > SharedConstants.TooBigOrNegative) { return this; }

            byte[] buffer = new byte[thisLen];
            var span = new Span<byte>(buffer);
            if (uIndex > 0u)
            {
                thisSpan.Slice(0, index).CopyTo(span.Slice(0, index));
            }
            for (var idx = index; idx < thisLen; idx++)
            {
                byte oldValue = thisSpan[idx];
                span[idx] = ToLowerCase(thisSpan[idx]);
            }
            return new AsciiString(buffer, false);
        }

        public AsciiString ToUpperCase()
        {
            var thisLen = this.length;
            if (0u >= (uint)thisLen) { return this; }

            var thisSpan = this.AsciiSpan;
            var index = SpanHelpers.FindIndex(ref MemoryMarshal.GetReference(thisSpan), x => IsLowerCase(x), thisLen);
            uint uIndex = (uint)index;
            if (uIndex > SharedConstants.TooBigOrNegative) { return this; }

            byte[] buffer = new byte[thisLen];
            var span = new Span<byte>(buffer);
            if (uIndex > 0u)
            {
                thisSpan.Slice(0, index).CopyTo(span.Slice(0, index));
            }
            for (var idx = index; idx < thisLen; idx++)
            {
                byte oldValue = thisSpan[idx];
                span[idx] = ToUpperCase(thisSpan[idx]);
            }
            return new AsciiString(buffer, false);
        }

        public bool Equals(AsciiString other)
        {
            if (ReferenceEquals(this, other)) { return true; }

            return other is object && this.length == other.length
                && this.GetHashCode() == other.GetHashCode()
                && this.AsciiSpan.SequenceEqual(other.AsciiSpan);
        }

        public override bool Equals(object obj)
        {
            return this.ContentEquals(obj as ICharSequence);
        }

        bool IEquatable<ICharSequence>.Equals(ICharSequence other)
        {
            return this.ContentEquals(other);
        }
    }
}
