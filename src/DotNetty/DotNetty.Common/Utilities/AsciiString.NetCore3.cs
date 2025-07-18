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

#if NETCOREAPP_3_0_GREATER

namespace DotNetty.Common.Utilities
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using DotNetty.Common.Internal;

    partial class AsciiString : IHasAsciiSpan, IHasUtf16Span
    {
        private static unsafe void GetBytes(in ReadOnlySpan<char> chars, Span<byte> bytes)
        {
            // It's ok for us to operate on null / empty spans.

            fixed (char* charsPtr = &MemoryMarshal.GetReference(chars))
            fixed (byte* bytesPtr = &MemoryMarshal.GetReference(bytes))
            {
                if (!TryGetBytesFast(charsPtr, chars.Length, bytesPtr, bytes.Length, out _))
                {
                    GetBytes(charsPtr, chars.Length, bytesPtr);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe bool TryGetBytesFast(char* pChars, int charCount, byte* pBytes, int byteCount, out int bytesWritten)
        {
            // Common helper method for all non-EncoderNLS entry points to GetBytes.
            // A modification of this method should be copied in to each of the supported encodings: ASCII, UTF8, UTF16, UTF32.

            //Debug.Assert(charCount >= 0, "Caller shouldn't specify negative length buffer.");
            //Debug.Assert(pChars is object || charCount == 0, "Input pointer shouldn't be null if non-zero length specified.");
            //Debug.Assert(byteCount >= 0, "Caller shouldn't specify negative length buffer.");
            //Debug.Assert(pBytes is object || byteCount == 0, "Input pointer shouldn't be null if non-zero length specified.");

            // First call into the fast path.

            bytesWritten = GetBytesFast(pChars, charCount, pBytes, byteCount, out int charsConsumed);

            if (charsConsumed == charCount)
            {
                // All elements converted - return immediately.

                return true;
            }
            bytesWritten = 0; return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] // called directly by GetBytesCommon
        private static unsafe int GetBytesFast(char* pChars, int charsLength, byte* pBytes, int bytesLength, out int charsConsumed)
        {
            int bytesWritten = (int)ASCIIUtility.NarrowUtf16ToAscii(pChars, pBytes, (uint)Math.Min(charsLength, bytesLength));

            charsConsumed = bytesWritten;
            return bytesWritten;
        }
    }
}

#endif