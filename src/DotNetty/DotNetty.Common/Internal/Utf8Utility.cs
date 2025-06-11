// borrowed from https://github.com/dotnet/corefx/tree/release/3.1/src/Common/src/CoreLib/System/Text/Unicode

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if NETCOREAPP_3_0_GREATER
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DotNetty.Common.Internal
{
    internal static partial class Utf8Utility
    {
        /// <summary>
        /// The maximum number of bytes that can result from UTF-8 transcoding
        /// any Unicode scalar value.
        /// </summary>
        internal const int MaxBytesPerScalar = 4;

        /// <summary>
        /// The UTF-8 representation of <see cref="UnicodeUtility.ReplacementChar"/>.
        /// </summary>
        private static ReadOnlySpan<byte> ReplacementCharSequence => new byte[] { 0xEF, 0xBF, 0xBD };

        /// <summary>
        /// Returns the byte index in <paramref name="utf8Data"/> where the first invalid UTF-8 sequence begins,
        /// or -1 if the buffer contains no invalid sequences. Also outs the <paramref name="isAscii"/> parameter
        /// stating whether all data observed (up to the first invalid sequence or the end of the buffer, whichever
        /// comes first) is ASCII.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static int GetIndexOfFirstInvalidUtf8Sequence(in ReadOnlySpan<byte> utf8Data, out bool isAscii)
        {
            fixed (byte* pUtf8Data = &MemoryMarshal.GetReference(utf8Data))
            {
                byte* pFirstInvalidByte = GetPointerToFirstInvalidByte(pUtf8Data, utf8Data.Length, out int utf16CodeUnitCountAdjustment, out _);
                int index = (int)(void*)Unsafe.ByteOffset(ref *pUtf8Data, ref *pFirstInvalidByte);

                isAscii = (0u >= (uint)utf16CodeUnitCountAdjustment); // If UTF-16 char count == UTF-8 byte count, it's ASCII.
                return ((uint)index < (uint)utf8Data.Length) ? index : -1;
            }
        }
    }
}
#endif
