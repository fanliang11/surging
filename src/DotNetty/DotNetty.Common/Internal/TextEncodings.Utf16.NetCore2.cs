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
        public static partial class Utf16
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static OperationStatus ToUtf8(in ReadOnlySpan<char> chars, Span<byte> utf8Destination, out int charsConsumed, out int bytesWritten)
            {
                var utf16Source = MemoryMarshal.AsBytes(chars);
                var result = ToUtf8(ref MemoryMarshal.GetReference(utf16Source), utf16Source.Length, ref MemoryMarshal.GetReference(utf8Destination), utf8Destination.Length, out var bytesConsumed, out bytesWritten);
                charsConsumed = bytesConsumed >> 1;
                return result;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static OperationStatus ToUtf8(in ReadOnlySpan<char> chars, ref byte utf8Destination, int utf8Length, out int charsConsumed, out int bytesWritten)
            {
                var utf16Source = MemoryMarshal.AsBytes(chars);
                var result = ToUtf8(ref MemoryMarshal.GetReference(utf16Source), utf16Source.Length, ref utf8Destination, utf8Length, out var bytesConsumed, out bytesWritten);
                charsConsumed = bytesConsumed >> 1;
                return result;
            }

            // borrowed from https://github.com/dotnet/corefxlab/tree/master/src/System.Text.Primitives/System/Text/Encoders
            // TODO: Replace this with publicly shipping implementation: https://github.com/dotnet/corefx/issues/34094
            /// <summary>
            /// Converts a span containing a sequence of UTF-16 bytes into UTF-8 bytes.
            ///
            /// This method will consume as many of the input bytes as possible.
            ///
            /// On successful exit, the entire input was consumed and encoded successfully. In this case, <paramref name="bytesConsumed"/> will be
            /// equal to the length of the <paramref name="utf16Source"/> and <paramref name="bytesWritten"/> will equal the total number of bytes written to
            /// the <paramref name="utf8Destination"/>.
            /// </summary>
            /// <param name="utf16Source">A span containing a sequence of UTF-16 bytes.</param>
            /// <param name="utf16Length"></param>
            /// <param name="utf8Destination">A span to write the UTF-8 bytes into.</param>
            /// <param name="utf8Length"></param>
            /// <param name="bytesConsumed">On exit, contains the number of bytes that were consumed from the <paramref name="utf16Source"/>.</param>
            /// <param name="bytesWritten">On exit, contains the number of bytes written to <paramref name="utf8Destination"/></param>
            /// <returns>A <see cref="OperationStatus"/> value representing the state of the conversion.</returns>
            public unsafe static OperationStatus ToUtf8(ref byte utf16Source, int utf16Length, ref byte utf8Destination, int utf8Length,
                out int bytesConsumed, out int bytesWritten)
            {
                fixed (byte* chars = &utf16Source)
                fixed (byte* bytes = &utf8Destination)
                {
                    char* pSrc = (char*)chars;
                    byte* pTarget = bytes;

                    char* pEnd = (char*)(chars + utf16Length);
                    byte* pAllocatedBufferEnd = pTarget + utf8Length;

                    // assume that JIT will enregister pSrc, pTarget and ch

                    // Entering the fast encoding loop incurs some overhead that does not get amortized for small
                    // number of characters, and the slow encoding loop typically ends up running for the last few
                    // characters anyway since the fast encoding loop needs 5 characters on input at least.
                    // Thus don't use the fast decoding loop at all if we don't have enough characters. The threashold
                    // was choosen based on performance testing.
                    // Note that if we don't have enough bytes, pStop will prevent us from entering the fast loop.
                    while (pEnd - pSrc > 13)
                    {
                        // we need at least 1 byte per character, but Convert might allow us to convert
                        // only part of the input, so try as much as we can.  Reduce charCount if necessary
                        int available = Math.Min(PtrDiff(pEnd, pSrc), PtrDiff(pAllocatedBufferEnd, pTarget));

                        // FASTLOOP:
                        // - optimistic range checks
                        // - fallbacks to the slow loop for all special cases, exception throwing, etc.

                        // To compute the upper bound, assume that all characters are ASCII characters at this point,
                        //  the boundary will be decreased for every non-ASCII character we encounter
                        // Also, we need 5 chars reserve for the unrolled ansi decoding loop and for decoding of surrogates
                        // If there aren't enough bytes for the output, then pStop will be <= pSrc and will bypass the loop.
                        char* pStop = pSrc + available - 5;
                        if (pSrc >= pStop)
                            break;

                        do
                        {
                            int ch = *pSrc;
                            pSrc++;

                            if (ch > 0x7F)
                            {
                                goto LongCode;
                            }
                            *pTarget = (byte)ch;
                            pTarget++;

                            // get pSrc aligned
                            if ((unchecked((int)pSrc) & 0x2) != 0)
                            {
                                ch = *pSrc;
                                pSrc++;
                                if (ch > 0x7F)
                                {
                                    goto LongCode;
                                }
                                *pTarget = (byte)ch;
                                pTarget++;
                            }

                            // Run 4 characters at a time!
                            while (pSrc < pStop)
                            {
                                ch = *(int*)pSrc;
                                int chc = *(int*)(pSrc + 2);
                                if (((ch | chc) & unchecked((int)0xFF80FF80)) != 0)
                                {
                                    goto LongCodeWithMask;
                                }

                                // Unfortunately, this is endianess sensitive
                                if (!BitConverter.IsLittleEndian)
                                {
                                    *pTarget = (byte)(ch >> 16);
                                    *(pTarget + 1) = (byte)ch;
                                    pSrc += 4;
                                    *(pTarget + 2) = (byte)(chc >> 16);
                                    *(pTarget + 3) = (byte)chc;
                                    pTarget += 4;
                                }
                                else
                                {
                                    *pTarget = (byte)ch;
                                    *(pTarget + 1) = (byte)(ch >> 16);
                                    pSrc += 4;
                                    *(pTarget + 2) = (byte)chc;
                                    *(pTarget + 3) = (byte)(chc >> 16);
                                    pTarget += 4;
                                }
                            }
                            continue;

                        LongCodeWithMask:
                            if (!BitConverter.IsLittleEndian)
                            {
                                // be careful about the sign extension
                                ch = (int)(((uint)ch) >> 16);
                            }
                            else
                            {
                                ch = (char)ch;
                            }
                            pSrc++;

                            if (ch > 0x7F)
                            {
                                goto LongCode;
                            }
                            *pTarget = (byte)ch;
                            pTarget++;
                            continue;

                        LongCode:
                            // use separate helper variables for slow and fast loop so that the jit optimizations
                            // won't get confused about the variable lifetimes
                            int chd;
                            if (ch <= 0x7FF)
                            {
                                // 2 byte encoding
                                chd = unchecked((sbyte)0xC0) | (ch >> 6);
                            }
                            else
                            {
                                // if (!IsLowSurrogate(ch) && !IsHighSurrogate(ch))
                                if (!UnicodeUtility.IsInRangeInclusive(ch, HighSurrogateStart, LowSurrogateEnd))
                                {
                                    // 3 byte encoding
                                    chd = unchecked((sbyte)0xE0) | (ch >> 12);
                                }
                                else
                                {
                                    // 4 byte encoding - high surrogate + low surrogate
                                    // if (!IsHighSurrogate(ch))
                                    if (ch > HighSurrogateEnd)
                                    {
                                        // low without high -> bad
                                        goto InvalidData;
                                    }

                                    chd = *pSrc;

                                    // if (!IsLowSurrogate(chd)) {
                                    if (!UnicodeUtility.IsInRangeInclusive(chd, LowSurrogateStart, LowSurrogateEnd))
                                    {
                                        // high not followed by low -> bad
                                        goto InvalidData;
                                    }

                                    pSrc++;

                                    ch = chd + (ch << 10) +
                                        (0x10000
                                        - LowSurrogateStart
                                        - (HighSurrogateStart << 10));

                                    *pTarget = (byte)(unchecked((sbyte)0xF0) | (ch >> 18));
                                    // pStop - this byte is compensated by the second surrogate character
                                    // 2 input chars require 4 output bytes.  2 have been anticipated already
                                    // and 2 more will be accounted for by the 2 pStop-- calls below.
                                    pTarget++;

                                    chd = unchecked((sbyte)0x80) | (ch >> 12) & 0x3F;
                                }
                                *pTarget = (byte)chd;
                                pStop--;                    // 3 byte sequence for 1 char, so need pStop-- and the one below too.
                                pTarget++;

                                chd = unchecked((sbyte)0x80) | (ch >> 6) & 0x3F;
                            }
                            *pTarget = (byte)chd;
                            pStop--;                        // 2 byte sequence for 1 char so need pStop--.

                            *(pTarget + 1) = (byte)(unchecked((sbyte)0x80) | ch & 0x3F);
                            // pStop - this byte is already included

                            pTarget += 2;
                        }
                        while (pSrc < pStop);

                        Debug.Assert(pTarget <= pAllocatedBufferEnd, "[UTF8Encoding.GetBytes]pTarget <= pAllocatedBufferEnd");
                    }

                    while (pSrc < pEnd)
                    {
                        // SLOWLOOP: does all range checks, handles all special cases, but it is slow

                        // read next char. The JIT optimization seems to be getting confused when
                        // compiling "ch = *pSrc++;", so rather use "ch = *pSrc; pSrc++;" instead
                        int ch = *pSrc;
                        pSrc++;

                        if (ch <= 0x7F)
                        {
                            if (pAllocatedBufferEnd - pTarget <= 0)
                                goto DestinationFull;

                            *pTarget = (byte)ch;
                            pTarget++;
                            continue;
                        }

                        int chd;
                        if (ch <= 0x7FF)
                        {
                            if (pAllocatedBufferEnd - pTarget <= 1)
                                goto DestinationFull;

                            // 2 byte encoding
                            chd = unchecked((sbyte)0xC0) | (ch >> 6);
                        }
                        else
                        {
                            // if (!IsLowSurrogate(ch) && !IsHighSurrogate(ch))
                            if (!UnicodeUtility.IsInRangeInclusive(ch, HighSurrogateStart, LowSurrogateEnd))
                            {
                                if (pAllocatedBufferEnd - pTarget <= 2)
                                    goto DestinationFull;

                                // 3 byte encoding
                                chd = unchecked((sbyte)0xE0) | (ch >> 12);
                            }
                            else
                            {
                                if (pAllocatedBufferEnd - pTarget <= 3)
                                    goto DestinationFull;

                                // 4 byte encoding - high surrogate + low surrogate
                                // if (!IsHighSurrogate(ch))
                                if (ch > HighSurrogateEnd)
                                {
                                    // low without high -> bad
                                    goto InvalidData;
                                }

                                if (pSrc >= pEnd)
                                    goto NeedMoreData;

                                chd = *pSrc;

                                // if (!IsLowSurrogate(chd)) {
                                if (!UnicodeUtility.IsInRangeInclusive(chd, LowSurrogateStart, LowSurrogateEnd))
                                {
                                    // high not followed by low -> bad
                                    goto InvalidData;
                                }

                                pSrc++;

                                ch = chd + (ch << 10) +
                                    (0x10000
                                    - LowSurrogateStart
                                    - (HighSurrogateStart << 10));

                                *pTarget = (byte)(unchecked((sbyte)0xF0) | (ch >> 18));
                                pTarget++;

                                chd = unchecked((sbyte)0x80) | (ch >> 12) & 0x3F;
                            }
                            *pTarget = (byte)chd;
                            pTarget++;

                            chd = unchecked((sbyte)0x80) | (ch >> 6) & 0x3F;
                        }

                        *pTarget = (byte)chd;
                        *(pTarget + 1) = (byte)(unchecked((sbyte)0x80) | ch & 0x3F);

                        pTarget += 2;
                    }

                    bytesConsumed = (int)((byte*)pSrc - chars);
                    bytesWritten = (int)(pTarget - bytes);
                    return OperationStatus.Done;

                InvalidData:
                    bytesConsumed = (int)((byte*)(pSrc - 1) - chars);
                    bytesWritten = (int)(pTarget - bytes);
                    return OperationStatus.InvalidData;

                DestinationFull:
                    bytesConsumed = (int)((byte*)(pSrc - 1) - chars);
                    bytesWritten = (int)(pTarget - bytes);
                    return OperationStatus.DestinationTooSmall;

                NeedMoreData:
                    bytesConsumed = (int)((byte*)(pSrc - 1) - chars);
                    bytesWritten = (int)(pTarget - bytes);
                    return OperationStatus.NeedMoreData;
                }
            }

            /// <summary>
            /// Calculates the byte count needed to encode the UTF-8 bytes from the specified UTF-16 sequence.
            ///
            /// This method will consume as many of the input bytes as possible.
            /// </summary>
            /// <param name="utf16Source">A span containing a sequence of UTF-16 bytes.</param>
            /// <param name="bytesNeeded">On exit, contains the number of bytes required for encoding from the <paramref name="utf16Source"/>.</param>
            /// <returns>A <see cref="OperationStatus"/> value representing the expected state of the conversion.</returns>
            internal static OperationStatus ToUtf8Length(in ReadOnlySpan<byte> utf16Source, out int bytesNeeded)
            {
                bytesNeeded = 0;

                // try? because Convert.ConvertToUtf32 can throw
                // if the high/low surrogates aren't valid; no point
                // running all the tests twice per code-point
                try
                {
                    ref char utf16 = ref Unsafe.As<byte, char>(ref MemoryMarshal.GetReference(utf16Source));
                    int utf16Length = utf16Source.Length >> 1; // byte => char count

                    for (int i = 0; i < utf16Length; i++)
                    {
                        var ch = Unsafe.Add(ref utf16, i);

                        if ((ushort)ch <= 0x7f) // Fast path for ASCII
                            bytesNeeded++;
                        else if (!char.IsSurrogate(ch))
                            bytesNeeded += GetUtf8EncodedBytes((uint)ch);
                        else
                        {
                            if (++i >= utf16Length)
                                return OperationStatus.NeedMoreData;

                            uint codePoint = (uint)char.ConvertToUtf32(ch, Unsafe.Add(ref utf16, i));
                            bytesNeeded += GetUtf8EncodedBytes(codePoint);
                        }
                    }

                    if ((utf16Length << 1) != utf16Source.Length)
                        return OperationStatus.NeedMoreData;

                    return OperationStatus.Done;
                }
                catch (ArgumentOutOfRangeException)
                {
                    return OperationStatus.InvalidData;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int GetUtf8EncodedBytes(uint codePoint)
            {
                if (codePoint <= 0x7Fu) { return 1; }

                if (codePoint <= 0x7FFu) { return 2; }

                if (codePoint <= 0xFFFFu) { return 3; }

                if (codePoint <= 0x10FFFFu) { return 4; }

                return 0;
            }
        }
    }
}

#endif