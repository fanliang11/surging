// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
namespace DotNetty.Common.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using DotNetty.Common.Utilities;

    //
    // This is used exclusively for http headers as a buffer
    //
    // In original Netty, this is backed by char array because http parsing
    // converts each byte to char, then chars to string which implements ICharSequence 
    // in java and can be used in the same way as AsciiString.
    //
    // This approach performs poorly on .net because DotNetty only uses AsciiString 
    // for headers. DotNetty converts each byte to char, then chars back to bytes 
    // again when reading out to AsciiString. 
    //
    // Each byte to char and each char to byte forwards and backwards!
    //
    // Therefore this is backed by bytes directly in DotNetty to avoid double conversions, 
    // and all chars are assumed to be ASCII!
    // 
    public sealed partial class AppendableCharSequence : ICharSequence, IAppendable, IEquatable<AppendableCharSequence>
    {
        private byte[] _chars;
        private int _pos;

        public AppendableCharSequence(int length)
        {
            if ((uint)(length - 1) > SharedConstants.TooBigOrNegative) // <= 0
            {
                ThrowHelper.ThrowArgumentException_Positive(length, ExceptionArgument.length);
            }

            _chars = new byte[length];
        }

        public AppendableCharSequence(byte[] chars)
        {
            if (chars is null || 0u >= (uint)chars.Length) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.chars); }

            _chars = chars;
            _pos = chars.Length;
        }

        public IEnumerator<char> GetEnumerator() => new CharSequenceEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void SetCount(int count)
        {
            if (/*count < 0 || */(uint)count > (uint)_pos)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_AppendableCharSequence_Count(count, _pos);
            }
            _pos = count;
        }

        public int Count => _pos;

        public char this[int index]
        {
            get
            {
                if (index > _pos) { ThrowHelper.ThrowIndexOutOfRangeException(); }
                return AsciiString.ByteToChar(_chars[index]);
            }
        }

        public ref byte[] Bytes => ref _chars;

        public ICharSequence SubSequence(int start) => SubSequence(start, _pos);

        public ICharSequence SubSequence(int start, int end)
        {
            int length = end - start;
            if (0u >= (uint)length)
            {
                // If start and end index is the same we need to return an empty sequence to conform to the interface.
                // As our expanding logic depends on the fact that we have a char[] with length > 0 we need to construct
                // an instance for which this is true.
                return new AppendableCharSequence(Math.Min(16, _chars.Length));
            }
            var data = new byte[length];
            PlatformDependent.CopyMemory(_chars, start, data, 0, length);
            return new AppendableCharSequence(data);
        }

        public int IndexOf(char ch, int start = 0) => CharUtil.IndexOf(this, ch, start);

        public bool RegionMatches(int thisStart, ICharSequence seq, int start, int length) =>
            CharUtil.RegionMatches(this, thisStart, seq, start, length);

        public bool RegionMatchesIgnoreCase(int thisStart, ICharSequence seq, int start, int length) =>
            CharUtil.RegionMatchesIgnoreCase(this, thisStart, seq, start, length);

        public bool ContentEquals(ICharSequence other) => CharUtil.ContentEquals(this, other);

        public bool ContentEqualsIgnoreCase(ICharSequence other) => CharUtil.ContentEqualsIgnoreCase(this, other);

        public int HashCode(bool ignoreCase) => ignoreCase
            ? StringComparer.OrdinalIgnoreCase.GetHashCode(ToString())
            : StringComparer.Ordinal.GetHashCode(ToString());

        public override int GetHashCode() => HashCode(true);

        public IAppendable Append(char c) => Append((byte)c);

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public IAppendable Append(byte c)
        {
            if ((uint)_pos >= (uint)_chars.Length)
            {
                byte[] old = _chars;
                _chars = new byte[old.Length << 1];
                PlatformDependent.CopyMemory(old, 0, _chars, 0, old.Length);
            }
            _chars[_pos++] = c;
            return this;
        }

        public IAppendable Append(ICharSequence sequence) => Append(sequence, 0, sequence.Count);

        public IAppendable Append(ICharSequence sequence, int start, int end)
        {
            if ((uint)end > (uint)sequence.Count) { ThrowHelper.ThrowIndexOutOfRangeException(); }
            if ((uint)start > (uint)end) { ThrowHelper.ThrowIndexOutOfRangeException(); }

            int length = end - start;
            if ((uint)length > (uint)(_chars.Length - _pos))
            {
                _chars = Expand(_chars, _pos + length, _pos);
            }

            switch (sequence)
            {
                case AppendableCharSequence seq:
                    // Optimize append operations via array copy
                    byte[] src = seq._chars;
                    PlatformDependent.CopyMemory(src, start, _chars, _pos, length);
                    _pos += length;
                    break;

                case IHasAsciiSpan hasAscii:
                    hasAscii.AsciiSpan.Slice(start, length).CopyTo(_chars.AsSpan(_pos, length));
                    _pos += length;
                    break;

                default:
                    for (int idx = start; idx < end; idx++)
                    {
                        _chars[_pos++] = (byte)sequence[idx];
                    }
                    break;
            }

            return this;
        }

        /// <summary>
        /// Resets the <see cref="AppendableCharSequence"/>. Be aware this will only reset the current internal
        /// position and not shrink the internal char array.
        /// </summary>
        public void Reset() => _pos = 0;

        public string ToString(int start)
        {
            if ((uint)start >= (uint)_pos) { ThrowHelper.ThrowIndexOutOfRangeException(); }
            return Encoding.ASCII.GetString(_chars, start, _pos - start);
        }

        public override string ToString() => 0u >= (uint)_pos ? string.Empty : ToString(0);

        public AsciiString ToAsciiString() => 0u >= (uint)_pos ? AsciiString.Empty : new AsciiString(_chars, 0, _pos, true);

        /// <summary>
        /// Create a new ascii string, this method assumes all chars has been sanitized to ascii chars when appending
        /// to the array.
        /// </summary>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public unsafe AsciiString SubStringUnsafe(int start, int end)
        {
            var bytes = new byte[end - start];
            fixed (byte* src = &_chars[start])
            fixed (byte* dst = bytes)
            {
                PlatformDependent.CopyMemory(src, dst, bytes.Length);
            }
            return new AsciiString(bytes);
        }

        static byte[] Expand(byte[] array, int neededSpace, int size)
        {
            int newCapacity = array.Length;
            do
            {
                // double capacity until it is big enough
                newCapacity <<= 1;

                if (newCapacity < 0)
                {
                    ThrowHelper.ThrowInvalidOperationException_CapacityMustBePositive(newCapacity);
                }
            }
            while (neededSpace > newCapacity);

            var newArray = new byte[newCapacity];
            PlatformDependent.CopyMemory(array, 0, newArray, 0, size);
            return newArray;
        }
    }
}
