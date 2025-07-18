#if !NETCOREAPP_3_0_GREATER

namespace DotNetty.Common.Internal
{
    using System;
    using System.Buffers;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public static partial class TextEncodings
    {
        public static partial class Utf8
        {
            // Largely based from https://github.com/dotnet/corefxlab/tree/master/src/System.Text.Primitives/System/Text/Encoders

            /// <summary>Converts a span containing a sequence of UTF-8 bytes into UTF-16 bytes.
            ///
            /// This method will consume as many of the input bytes as possible.
            ///
            /// On successful exit, the entire input was consumed and encoded successfully. In this case, <paramref name="bytesConsumed"/> will be
            /// equal to the length of the <paramref name="utf8Source"/> and <paramref name="bytesWritten"/> will equal the total number of bytes written to
            /// the <paramref name="utf16Destination"/>.</summary>
            /// <param name="utf8Source">A span containing a sequence of UTF-8 bytes.</param>
            /// <param name="utf16Destination">A span to write the UTF-16 bytes into.</param>
            /// <param name="bytesConsumed">On exit, contains the number of bytes that were consumed from the <paramref name="utf8Source"/>.</param>
            /// <param name="bytesWritten">On exit, contains the number of bytes written to <paramref name="utf16Destination"/></param>
            /// <returns>A <see cref="OperationStatus"/> value representing the state of the conversion.</returns>
            public unsafe static OperationStatus ToUtf16(in ReadOnlySpan<byte> utf8Source, Span<byte> utf16Destination, out int bytesConsumed, out int bytesWritten)
            {
                fixed (byte* pUtf8 = &MemoryMarshal.GetReference(utf8Source))
                fixed (byte* pUtf16 = &MemoryMarshal.GetReference(utf16Destination))
                {
                    byte* pSrc = pUtf8;
                    byte* pSrcEnd = pSrc + utf8Source.Length;
                    char* pDst = (char*)pUtf16;
                    char* pDstEnd = pDst + (utf16Destination.Length >> 1);   // Conversion from bytes to chars - div by sizeof(char)

                    int ch = 0;
                    while (pSrc < pSrcEnd && pDst < pDstEnd)
                    {
                        // we may need as many as 1 character per byte, so reduce the byte count if necessary.
                        // If availableChars is too small, pStop will be before pTarget and we won't do fast loop.
                        int availableChars = PtrDiff(pDstEnd, pDst);
                        int availableBytes = PtrDiff(pSrcEnd, pSrc);

                        if (availableChars < availableBytes)
                            availableBytes = availableChars;

                        // don't fall into the fast decoding loop if we don't have enough bytes
                        if (availableBytes <= 13)
                        {
                            // try to get over the remainder of the ascii characters fast though
                            byte* pLocalEnd = pSrc + availableBytes;
                            while (pSrc < pLocalEnd)
                            {
                                ch = *pSrc;
                                pSrc++;

                                if (ch > 0x7F)
                                    goto LongCodeSlow;

                                *pDst = (char)ch;
                                pDst++;
                            }

                            // we are done
                            break;
                        }

                        // To compute the upper bound, assume that all characters are ASCII characters at this point,
                        //  the boundary will be decreased for every non-ASCII character we encounter
                        // Also, we need 7 chars reserve for the unrolled ansi decoding loop and for decoding of multibyte sequences
                        char* pStop = pDst + availableBytes - 7;

                        // Fast loop
                        while (pDst < pStop)
                        {
                            ch = *pSrc;
                            pSrc++;

                            if (ch > 0x7F)
                                goto LongCode;

                            *pDst = (char)ch;
                            pDst++;

                            // 2-byte align
                            if ((unchecked((int)pSrc) & 0x1) != 0)
                            {
                                ch = *pSrc;
                                pSrc++;

                                if (ch > 0x7F)
                                    goto LongCode;

                                *pDst = (char)ch;
                                pDst++;
                            }

                            // 4-byte align
                            if ((unchecked((int)pSrc) & 0x2) != 0)
                            {
                                ch = *(ushort*)pSrc;
                                if ((ch & 0x8080) != 0)
                                    goto LongCodeWithMask16;

                                // Unfortunately, endianness sensitive
                                if (!BitConverter.IsLittleEndian)
                                {
                                    *pDst = (char)((ch >> 8) & 0x7F);
                                    pSrc += 2;
                                    *(pDst + 1) = (char)(ch & 0x7F);
                                    pDst += 2;
                                }
                                else
                                {
                                    *pDst = (char)(ch & 0x7F);
                                    pSrc += 2;
                                    *(pDst + 1) = (char)((ch >> 8) & 0x7F);
                                    pDst += 2;
                                }
                            }

                            // Run 8 characters at a time!
                            while (pDst < pStop)
                            {
                                ch = *(int*)pSrc;
                                int chb = *(int*)(pSrc + 4);
                                if (((ch | chb) & unchecked((int)0x80808080)) != 0)
                                    goto LongCodeWithMask32;

                                // Unfortunately, endianness sensitive
                                if (!BitConverter.IsLittleEndian)
                                {
                                    *pDst = (char)((ch >> 24) & 0x7F);
                                    *(pDst+1) = (char)((ch >> 16) & 0x7F);
                                    *(pDst+2) = (char)((ch >> 8) & 0x7F);
                                    *(pDst+3) = (char)(ch & 0x7F);
                                    pSrc += 8;
                                    *(pDst+4) = (char)((chb >> 24) & 0x7F);
                                    *(pDst+5) = (char)((chb >> 16) & 0x7F);
                                    *(pDst+6) = (char)((chb >> 8) & 0x7F);
                                    *(pDst+7) = (char)(chb & 0x7F);
                                    pDst += 8;
                                }
                                else
                                {
                                    *pDst = (char)(ch & 0x7F);
                                    *(pDst + 1) = (char)((ch >> 8) & 0x7F);
                                    *(pDst + 2) = (char)((ch >> 16) & 0x7F);
                                    *(pDst + 3) = (char)((ch >> 24) & 0x7F);
                                    pSrc += 8;
                                    *(pDst + 4) = (char)(chb & 0x7F);
                                    *(pDst + 5) = (char)((chb >> 8) & 0x7F);
                                    *(pDst + 6) = (char)((chb >> 16) & 0x7F);
                                    *(pDst + 7) = (char)((chb >> 24) & 0x7F);
                                    pDst += 8;
                                }
                            }

                            break;

                        LongCodeWithMask32:
                            if (!BitConverter.IsLittleEndian)
                            {
                                // be careful about the sign extension
                                ch = (int)(((uint)ch) >> 16);
                            }
                        LongCodeWithMask16:
                            if (!BitConverter.IsLittleEndian)
                            {
                                ch = (int)(((uint)ch) >> 8);
                            }
                            else
                            {
                                ch &= 0xFF;
                            }
                            pSrc++;
                            if (ch <= 0x7F)
                            {
                                *pDst = (char)ch;
                                pDst++;
                                continue;
                            }

                        LongCode:
                            int chc = *pSrc;
                            pSrc++;

                            // Bit 6 should be 0, and trailing byte should be 10vvvvvv
                            if (0u >= (uint)(ch & 0x40) || (chc & unchecked((sbyte)0xC0)) != 0x80)
                                goto InvalidData;

                            chc &= 0x3F;

                            if ((ch & 0x20) != 0)
                            {
                                // Handle 3 or 4 byte encoding.

                                // Fold the first 2 bytes together
                                chc |= (ch & 0x0F) << 6;

                                if ((ch & 0x10) != 0)
                                {
                                    // 4 byte - surrogate pair
                                    ch = *pSrc;

                                    // Bit 4 should be zero + the surrogate should be in the range 0x000000 - 0x10FFFF
                                    // and the trailing byte should be 10vvvvvv
                                    if (!UnicodeUtility.IsInRangeInclusive(chc >> 4, 0x01, 0x10) || (ch & unchecked((sbyte)0xC0)) != 0x80)
                                        goto InvalidData;

                                    // Merge 3rd byte then read the last byte
                                    chc = (chc << 6) | (ch & 0x3F);
                                    ch = *(pSrc + 1);

                                    // The last trailing byte still holds the form 10vvvvvv
                                    if ((ch & unchecked((sbyte)0xC0)) != 0x80)
                                        goto InvalidData;

                                    pSrc += 2;
                                    ch = (chc << 6) | (ch & 0x3F);

                                    *pDst = (char)(((ch >> 10) & 0x7FF) + unchecked((short)(HighSurrogateStart - (0x10000 >> 10))));
                                    pDst++;

                                    ch = (ch & 0x3FF) + unchecked((short)(LowSurrogateStart));
                                }
                                else
                                {
                                    // 3 byte encoding
                                    ch = *pSrc;

                                    // Check for non-shortest form of 3 byte sequence
                                    // No surrogates
                                    // Trailing byte must be in the form 10vvvvvv
                                    if (0u >= (uint)(chc & (0x1F << 5)) ||
                                        (chc & (0xF800 >> 6)) == (0xD800 >> 6) ||
                                        (ch & unchecked((sbyte)0xC0)) != 0x80)
                                        goto InvalidData;

                                    pSrc++;
                                    ch = (chc << 6) | (ch & 0x3F);
                                }

                                // extra byte, we're already planning 2 chars for 2 of these bytes,
                                // but the big loop is testing the target against pStop, so we need
                                // to subtract 2 more or we risk overrunning the input.  Subtract
                                // one here and one below.
                                pStop--;
                            }
                            else
                            {
                                // 2 byte encoding
                                ch &= 0x1F;

                                // Check for non-shortest form
                                if (ch <= 1)
                                    goto InvalidData;

                                ch = (ch << 6) | chc;
                            }

                            *pDst = (char)ch;
                            pDst++;

                            // extra byte, we're only expecting 1 char for each of these 2 bytes,
                            // but the loop is testing the target (not source) against pStop.
                            // subtract an extra count from pStop so that we don't overrun the input.
                            pStop--;
                        }

                        continue;

                    LongCodeSlow:
                        if (pSrc >= pSrcEnd)
                        {
                            // This is a special case where hit the end of the buffer but are in the middle
                            // of decoding a long code. The error exit thinks we have read 2 extra bytes already,
                            // so we add +1 to pSrc to get the count correct for the bytes consumed value.
                            pSrc++;
                            goto NeedMoreData;
                        }

                        int chd = *pSrc;
                        pSrc++;

                        // Bit 6 should be 0, and trailing byte should be 10vvvvvv
                        if (0u >= (uint)(ch & 0x40) || (chd & unchecked((sbyte)0xC0)) != 0x80)
                            goto InvalidData;

                        chd &= 0x3F;

                        if ((ch & 0x20) != 0)
                        {
                            // Handle 3 or 4 byte encoding.

                            // Fold the first 2 bytes together
                            chd |= (ch & 0x0F) << 6;

                            if ((ch & 0x10) != 0)
                            {
                                // 4 byte - surrogate pair
                                // We need 2 more bytes
                                if (pSrc >= pSrcEnd - 1)
                                    goto NeedMoreData;

                                ch = *pSrc;

                                // Bit 4 should be zero + the surrogate should be in the range 0x000000 - 0x10FFFF
                                // and the trailing byte should be 10vvvvvv
                                if (!UnicodeUtility.IsInRangeInclusive(chd >> 4, 0x01, 0x10) || (ch & unchecked((sbyte)0xC0)) != 0x80)
                                    goto InvalidData;

                                // Merge 3rd byte then read the last byte
                                chd = (chd << 6) | (ch & 0x3F);
                                ch = *(pSrc + 1);

                                // The last trailing byte still holds the form 10vvvvvv
                                // We only know for sure we have room for one more char, but we need an extra now.
                                if ((ch & unchecked((sbyte)0xC0)) != 0x80)
                                    goto InvalidData;

                                if (PtrDiff(pDstEnd, pDst) < 2)
                                    goto DestinationFull;

                                pSrc += 2;
                                ch = (chd << 6) | (ch & 0x3F);

                                *pDst = (char)(((ch >> 10) & 0x7FF) + unchecked((short)(HighSurrogateStart - (0x10000 >> 10))));
                                pDst++;

                                ch = (ch & 0x3FF) + unchecked((short)(LowSurrogateStart));
                            }
                            else
                            {
                                // 3 byte encoding
                                if (pSrc >= pSrcEnd)
                                    goto NeedMoreData;

                                ch = *pSrc;

                                // Check for non-shortest form of 3 byte sequence
                                // No surrogates
                                // Trailing byte must be in the form 10vvvvvv
                                if (0u >= (uint)(chd & (0x1F << 5)) ||
                                    (chd & (0xF800 >> 6)) == (0xD800 >> 6) ||
                                    (ch & unchecked((sbyte)0xC0)) != 0x80)
                                    goto InvalidData;

                                pSrc++;
                                ch = (chd << 6) | (ch & 0x3F);
                            }
                        }
                        else
                        {
                            // 2 byte encoding
                            ch &= 0x1F;

                            // Check for non-shortest form
                            if (ch <= 1)
                                goto InvalidData;

                            ch = (ch << 6) | chd;
                        }

                        *pDst = (char)ch;
                        pDst++;
                    }

                DestinationFull:
                    bytesConsumed = PtrDiff(pSrc, pUtf8);
                    bytesWritten = PtrDiff((byte*)pDst, pUtf16);
                    return (0u >= (uint)PtrDiff(pSrcEnd, pSrc)) ? OperationStatus.Done : OperationStatus.DestinationTooSmall;

                NeedMoreData:
                    bytesConsumed = PtrDiff(pSrc - 2, pUtf8);
                    bytesWritten = PtrDiff((byte*)pDst, pUtf16);
                    return OperationStatus.NeedMoreData;

                InvalidData:
                    bytesConsumed = PtrDiff(pSrc - 2, pUtf8);
                    bytesWritten = PtrDiff((byte*)pDst, pUtf16);
                    return OperationStatus.InvalidData;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool TryGetCharCountFast(in ReadOnlySpan<byte> utf8Bytes, out int totalCharCount)
            {
                if ((uint)Utf8Util.GetIndexOfFirstInvalidUtf8Sequence(utf8Bytes, out int scalarCount, out int surrogatePairCount) > SharedConstants.TooBigOrNegative)
                {
                    // Well-formed UTF-8 string.

                    // 'scalarCount + surrogatePairCount' is guaranteed not to overflow because
                    // the UTF-16 representation of a string will never have a greater number of
                    // of code units than its UTF-8 representation.
                    totalCharCount = scalarCount + surrogatePairCount;
                    return true;
                }
                totalCharCount = 0; return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int GetChars(in ReadOnlySpan<byte> utf8Bytes, Span<char> chars)
            {
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
                return UTF8NoBOM.GetChars(utf8Bytes, chars);
#else
                // It's ok for us to pass null pointers down to the workhorse below.
                unsafe
                {
                    fixed (byte* bytesPtr = &MemoryMarshal.GetReference(utf8Bytes))
                    fixed (char* charsPtr = &MemoryMarshal.GetReference(chars))
                    {
                        return UTF8NoBOM.GetChars(bytesPtr, utf8Bytes.Length, charsPtr, chars.Length);
                    }
                }
#endif
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static string GetString(in ReadOnlySpan<byte> utf8Bytes)
            {
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
                return UTF8NoBOM.GetString(utf8Bytes);
#else
                if (utf8Bytes.IsEmpty) { return string.Empty; }

                var totalCharCount = GetCharCount(utf8Bytes);
                var bytesNeeded = checked(totalCharCount * 2);
                var result = new String(' ', totalCharCount);
                unsafe
                {
                    fixed (char* pResult = result)
                    {
                        var resultBytes = new Span<byte>((void*)pResult, bytesNeeded);
                        if (ToUtf16(utf8Bytes, resultBytes, out int consumed, out int written) == OperationStatus.Done)
                        {
                            Debug.Assert(written == resultBytes.Length);
                            return result;
                        }
                    }
                }
                return String.Empty; // TODO: is this what we want to do if Bytes are invalid UTF8? Can Bytes be invalid UTF8?
#endif
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool TryGetByteCountFast(in ReadOnlySpan<char> chars, out int bytesNeeded)
            {
                var utf16Source = MemoryMarshal.AsBytes(chars);
                return Utf16.ToUtf8Length(utf16Source, out bytesNeeded) == OperationStatus.Done;
            }
        }
    }
}

#endif
