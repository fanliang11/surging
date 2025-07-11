// borrowed from https://github.com/dotnet/corefx/blob/release/3.1/src/Common/src/CoreLib/System/Text/ASCIIUtility.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if NETCOREAPP_3_0_GREATER
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#if NET
using System.Runtime.Intrinsics.Arm;
#endif

namespace DotNetty.Common.Internal
{
    internal static partial class ASCIIUtility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AllBytesInUInt64AreAscii(ulong value)
        {
            // If the high bit of any byte is set, that byte is non-ASCII.

            return 0ul >= (value & UInt64HighBitsOnlyMask);
        }

        /// <summary>
        /// Returns <see langword="true"/> iff all chars in <paramref name="value"/> are ASCII.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AllCharsInUInt32AreAscii(uint value)
        {
            return 0u >= (value & ~0x007F007Fu);
        }

        /// <summary>
        /// Returns <see langword="true"/> iff all chars in <paramref name="value"/> are ASCII.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AllCharsInUInt64AreAscii(ulong value)
        {
            return 0ul >= (value & ~0x007F007F_007F007Ful);
        }

        /// <summary>
        /// Given a DWORD which represents two packed chars in machine-endian order,
        /// <see langword="true"/> iff the first char (in machine-endian order) is ASCII.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool FirstCharInUInt32IsAscii(uint value)
        {
            return (BitConverter.IsLittleEndian && 0u >= (value & 0xFF80u))
                || (!BitConverter.IsLittleEndian && 0u >= (value & 0xFF800000u));
        }

        private static unsafe nuint GetIndexOfFirstNonAsciiByte_Default(byte* pBuffer, nuint bufferLength)
        {
            // Squirrel away the original buffer reference. This method works by determining the exact
            // byte reference where non-ASCII data begins, so we need this base value to perform the
            // final subtraction at the end of the method to get the index into the original buffer.

            byte* pOriginalBuffer = pBuffer;

            // Before we drain off byte-by-byte, try a generic vectorized loop.
            // Only run the loop if we have at least two vectors we can pull out.
            // Note use of SBYTE instead of BYTE below; we're using the two's-complement
            // representation of negative integers to act as a surrogate for "is ASCII?".

            if (Vector.IsHardwareAccelerated && bufferLength >= 2 * (uint)Vector<sbyte>.Count)
            {
                uint SizeOfVectorInBytes = (uint)Vector<sbyte>.Count; // JIT will make this a const

                if (Vector.GreaterThanOrEqualAll(Unsafe.ReadUnaligned<Vector<sbyte>>(pBuffer), Vector<sbyte>.Zero))
                {
                    // The first several elements of the input buffer were ASCII. Bump up the pointer to the
                    // next aligned boundary, then perform aligned reads from here on out until we find non-ASCII
                    // data or we approach the end of the buffer. It's possible we'll reread data; this is ok.

                    byte* pFinalVectorReadPos = pBuffer + bufferLength - SizeOfVectorInBytes;
                    pBuffer = (byte*)(((nuint)pBuffer + SizeOfVectorInBytes) & ~(nuint)(SizeOfVectorInBytes - 1));

#if DEBUG
                    long numBytesRead = pBuffer - pOriginalBuffer;
                    Debug.Assert(0 < numBytesRead && numBytesRead <= SizeOfVectorInBytes, "We should've made forward progress of at least one byte.");
                    Debug.Assert((nuint)numBytesRead <= bufferLength, "We shouldn't have read past the end of the input buffer.");
#endif

                    Debug.Assert(pBuffer <= pFinalVectorReadPos, "Should be able to read at least one vector.");

                    do
                    {
                        Debug.Assert((nuint)pBuffer % SizeOfVectorInBytes == 0, "Vector read should be aligned.");
                        if (Vector.LessThanAny(Unsafe.Read<Vector<sbyte>>(pBuffer), Vector<sbyte>.Zero))
                        {
                            break; // found non-ASCII data
                        }

                        pBuffer += SizeOfVectorInBytes;
                    } while (pBuffer <= pFinalVectorReadPos);

                    // Adjust the remaining buffer length for the number of elements we just consumed.

                    bufferLength -= (nuint)pBuffer;
                    bufferLength += (nuint)pOriginalBuffer;
                }
            }

            // At this point, the buffer length wasn't enough to perform a vectorized search, or we did perform
            // a vectorized search and encountered non-ASCII data. In either case go down a non-vectorized code
            // path to drain any remaining ASCII bytes.
            //
            // We're going to perform unaligned reads, so prefer 32-bit reads instead of 64-bit reads.
            // This also allows us to perform more optimized bit twiddling tricks to count the number of ASCII bytes.

            uint currentUInt32;

            // Try reading 64 bits at a time in a loop.

            for (; bufferLength >= 8; bufferLength -= 8)
            {
                currentUInt32 = Unsafe.ReadUnaligned<uint>(pBuffer);
                uint nextUInt32 = Unsafe.ReadUnaligned<uint>(pBuffer + 4);

                if (!AllBytesInUInt32AreAscii(currentUInt32 | nextUInt32))
                {
                    // One of these two values contains non-ASCII bytes.
                    // Figure out which one it is, then put it in 'current' so that we can drain the ASCII bytes.

                    if (AllBytesInUInt32AreAscii(currentUInt32))
                    {
                        currentUInt32 = nextUInt32;
                        pBuffer += 4;
                    }

                    goto FoundNonAsciiData;
                }

                pBuffer += 8; // consumed 8 ASCII bytes
            }

            // From this point forward we don't need to update bufferLength.
            // Try reading 32 bits.

            if ((bufferLength & 4) != 0)
            {
                currentUInt32 = Unsafe.ReadUnaligned<uint>(pBuffer);
                if (!AllBytesInUInt32AreAscii(currentUInt32))
                {
                    goto FoundNonAsciiData;
                }

                pBuffer += 4;
            }

            // Try reading 16 bits.

            if ((bufferLength & 2) != 0)
            {
                currentUInt32 = Unsafe.ReadUnaligned<ushort>(pBuffer);
                if (!AllBytesInUInt32AreAscii(currentUInt32))
                {
                    goto FoundNonAsciiData;
                }

                pBuffer += 2;
            }

            // Try reading 8 bits

            if ((bufferLength & 1) != 0)
            {
                // If the buffer contains non-ASCII data, the comparison below will fail, and
                // we'll end up not incrementing the buffer reference.

                if (*(sbyte*)pBuffer >= 0)
                {
                    pBuffer++;
                }
            }

        Finish:

            nuint totalNumBytesRead = (nuint)pBuffer - (nuint)pOriginalBuffer;
            return totalNumBytesRead;

        FoundNonAsciiData:

            Debug.Assert(!AllBytesInUInt32AreAscii(currentUInt32), "Shouldn't have reached this point if we have an all-ASCII input.");

            // The method being called doesn't bother looking at whether the high byte is ASCII. There are only
            // two scenarios: (a) either one of the earlier bytes is not ASCII and the search terminates before
            // we get to the high byte; or (b) all of the earlier bytes are ASCII, so the high byte must be
            // non-ASCII. In both cases we only care about the low 24 bits.

            pBuffer += CountNumberOfLeadingAsciiBytesFromUInt32WithSomeNonAsciiData(currentUInt32);
            goto Finish;
        }

        /// <summary>
        /// Returns the index in <paramref name="pBuffer"/> where the first non-ASCII char is found.
        /// Returns <paramref name="bufferLength"/> if the buffer is empty or all-ASCII.
        /// </summary>
        /// <returns>An ASCII char is defined as 0x0000 - 0x007F, inclusive.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe nuint GetIndexOfFirstNonAsciiChar(char* pBuffer, nuint bufferLength /* in chars */)
        {
            // If SSE2 is supported, use those specific intrinsics instead of the generic vectorized
            // code below. This has two benefits: (a) we can take advantage of specific instructions like
            // pmovmskb which we know are optimized, and (b) we can avoid downclocking the processor while
            // this method is running.

            return (Sse2.IsSupported)
                ? GetIndexOfFirstNonAsciiChar_Sse2(pBuffer, bufferLength)
                : GetIndexOfFirstNonAsciiChar_Default(pBuffer, bufferLength);
        }

        private static unsafe nuint GetIndexOfFirstNonAsciiChar_Default(char* pBuffer, nuint bufferLength /* in chars */)
        {
            // Squirrel away the original buffer reference.This method works by determining the exact
            // char reference where non-ASCII data begins, so we need this base value to perform the
            // final subtraction at the end of the method to get the index into the original buffer.

            char* pOriginalBuffer = pBuffer;

#if NET
            Debug.Assert(bufferLength <= nuint.MaxValue / sizeof(char));
#endif

            // Before we drain off char-by-char, try a generic vectorized loop.
            // Only run the loop if we have at least two vectors we can pull out.

            if (Vector.IsHardwareAccelerated && bufferLength >= 2 * (uint)Vector<ushort>.Count)
            {
                uint SizeOfVectorInChars = (uint)Vector<ushort>.Count; // JIT will make this a const
                uint SizeOfVectorInBytes = (uint)Vector<byte>.Count; // JIT will make this a const

                Vector<ushort> maxAscii = new Vector<ushort>(0x007F);

                if (Vector.LessThanOrEqualAll(Unsafe.ReadUnaligned<Vector<ushort>>(pBuffer), maxAscii))
                {
                    // The first several elements of the input buffer were ASCII. Bump up the pointer to the
                    // next aligned boundary, then perform aligned reads from here on out until we find non-ASCII
                    // data or we approach the end of the buffer. It's possible we'll reread data; this is ok.

                    char* pFinalVectorReadPos = pBuffer + bufferLength - SizeOfVectorInChars;
                    pBuffer = (char*)(((nuint)pBuffer + SizeOfVectorInBytes) & ~(nuint)(SizeOfVectorInBytes - 1));

#if DEBUG
                    long numCharsRead = pBuffer - pOriginalBuffer;
                    Debug.Assert(0 < numCharsRead && numCharsRead <= SizeOfVectorInChars, "We should've made forward progress of at least one char.");
                    Debug.Assert((nuint)numCharsRead <= bufferLength, "We shouldn't have read past the end of the input buffer.");
#endif

                    Debug.Assert(pBuffer <= pFinalVectorReadPos, "Should be able to read at least one vector.");

                    do
                    {
                        Debug.Assert((nuint)pBuffer % SizeOfVectorInChars == 0, "Vector read should be aligned.");
                        if (Vector.GreaterThanAny(Unsafe.Read<Vector<ushort>>(pBuffer), maxAscii))
                        {
                            break; // found non-ASCII data
                        }
                        pBuffer += SizeOfVectorInChars;
                    } while (pBuffer <= pFinalVectorReadPos);

                    // Adjust the remaining buffer length for the number of elements we just consumed.

                    bufferLength -= ((nuint)pBuffer - (nuint)pOriginalBuffer) / sizeof(char);
                }
            }

            // At this point, the buffer length wasn't enough to perform a vectorized search, or we did perform
            // a vectorized search and encountered non-ASCII data. In either case go down a non-vectorized code
            // path to drain any remaining ASCII chars.
            //
            // We're going to perform unaligned reads, so prefer 32-bit reads instead of 64-bit reads.
            // This also allows us to perform more optimized bit twiddling tricks to count the number of ASCII chars.

            uint currentUInt32;

            // Try reading 64 bits at a time in a loop.

            for (; bufferLength >= 4; bufferLength -= 4) // 64 bits = 4 * 16-bit chars
            {
                currentUInt32 = Unsafe.ReadUnaligned<uint>(pBuffer);
                uint nextUInt32 = Unsafe.ReadUnaligned<uint>(pBuffer + 4 / sizeof(char));

                if (!AllCharsInUInt32AreAscii(currentUInt32 | nextUInt32))
                {
                    // One of these two values contains non-ASCII chars.
                    // Figure out which one it is, then put it in 'current' so that we can drain the ASCII chars.

                    if (AllCharsInUInt32AreAscii(currentUInt32))
                    {
                        currentUInt32 = nextUInt32;
                        pBuffer += 2;
                    }

                    goto FoundNonAsciiData;
                }

                pBuffer += 4; // consumed 4 ASCII chars
            }

            // From this point forward we don't need to keep track of the remaining buffer length.
            // Try reading 32 bits.

            if ((bufferLength & 2) != 0) // 32 bits = 2 * 16-bit chars
            {
                currentUInt32 = Unsafe.ReadUnaligned<uint>(pBuffer);
                if (!AllCharsInUInt32AreAscii(currentUInt32))
                {
                    goto FoundNonAsciiData;
                }

                pBuffer += 2;
            }

            // Try reading 16 bits.
            // No need to try an 8-bit read after this since we're working with chars.

            if ((bufferLength & 1) != 0)
            {
                // If the buffer contains non-ASCII data, the comparison below will fail, and
                // we'll end up not incrementing the buffer reference.

                if (*pBuffer <= 0x007F)
                {
                    pBuffer++;
                }
            }

        Finish:

            nuint totalNumBytesRead = (nuint)pBuffer - (nuint)pOriginalBuffer;
            Debug.Assert(totalNumBytesRead % sizeof(char) == 0, "Total number of bytes read should be even since we're working with chars.");
            return totalNumBytesRead / sizeof(char); // convert byte count -> char count before returning

        FoundNonAsciiData:

            Debug.Assert(!AllCharsInUInt32AreAscii(currentUInt32), "Shouldn't have reached this point if we have an all-ASCII input.");

            // We don't bother looking at the second char - only the first char.

            if (FirstCharInUInt32IsAscii(currentUInt32))
            {
                pBuffer++;
            }

            goto Finish;
        }

        /// <summary>
        /// Given a QWORD which represents a buffer of 4 ASCII chars in machine-endian order,
        /// narrows each WORD to a BYTE, then writes the 4-byte result to the output buffer
        /// also in machine-endian order.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void NarrowFourUtf16CharsToAsciiAndWriteToBuffer(ref byte outputBuffer, ulong value)
        {
            Debug.Assert(AllCharsInUInt64AreAscii(value));

#if NETCOREAPP3_1
            if (Bmi2.X64.IsSupported)
            {
                // BMI2 will work regardless of the processor's endianness.
                Unsafe.WriteUnaligned(ref outputBuffer, (uint)Bmi2.X64.ParallelBitExtract(value, 0x00FF00FF_00FF00FFul));
            }
#else
            if (Sse2.X64.IsSupported)
            {
                // Narrows a vector of words [ w0 w1 w2 w3 ] to a vector of bytes
                // [ b0 b1 b2 b3 b0 b1 b2 b3 ], then writes 4 bytes (32 bits) to the destination.

                Vector128<short> vecWide = Sse2.X64.ConvertScalarToVector128UInt64(value).AsInt16();
                Vector128<uint> vecNarrow = Sse2.PackUnsignedSaturate(vecWide, vecWide).AsUInt32();
                Unsafe.WriteUnaligned<uint>(ref outputBuffer, Sse2.ConvertToUInt32(vecNarrow));
            }
            else if (AdvSimd.IsSupported)
            {
                // Narrows a vector of words [ w0 w1 w2 w3 ] to a vector of bytes
                // [ b0 b1 b2 b3 * * * * ], then writes 4 bytes (32 bits) to the destination.

                Vector128<short> vecWide = Vector128.CreateScalarUnsafe(value).AsInt16();
                Vector64<byte> lower = AdvSimd.ExtractNarrowingSaturateUnsignedLower(vecWide);
                Unsafe.WriteUnaligned<uint>(ref outputBuffer, lower.AsUInt32().ToScalar());
            }
#endif
            else
            {
                if (BitConverter.IsLittleEndian)
                {
                    outputBuffer = (byte)value;
                    value >>= 16;
                    Unsafe.Add(ref outputBuffer, 1) = (byte)value;
                    value >>= 16;
                    Unsafe.Add(ref outputBuffer, 2) = (byte)value;
                    value >>= 16;
                    Unsafe.Add(ref outputBuffer, 3) = (byte)value;
                }
                else
                {
                    Unsafe.Add(ref outputBuffer, 3) = (byte)value;
                    value >>= 16;
                    Unsafe.Add(ref outputBuffer, 2) = (byte)value;
                    value >>= 16;
                    Unsafe.Add(ref outputBuffer, 1) = (byte)value;
                    value >>= 16;
                    outputBuffer = (byte)value;
                }
            }
        }

        /// <summary>
        /// Given a DWORD which represents a buffer of 2 ASCII chars in machine-endian order,
        /// narrows each WORD to a BYTE, then writes the 2-byte result to the output buffer also in
        /// machine-endian order.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void NarrowTwoUtf16CharsToAsciiAndWriteToBuffer(ref byte outputBuffer, uint value)
        {
            Debug.Assert(AllCharsInUInt32AreAscii(value));

            if (BitConverter.IsLittleEndian)
            {
                outputBuffer = (byte)value;
                Unsafe.Add(ref outputBuffer, 1) = (byte)(value >> 16);
            }
            else
            {
                Unsafe.Add(ref outputBuffer, 1) = (byte)value;
                outputBuffer = (byte)(value >> 16);
            }
        }

        /// <summary>
        /// Copies as many ASCII characters (U+0000..U+007F) as possible from <paramref name="pUtf16Buffer"/>
        /// to <paramref name="pAsciiBuffer"/>, stopping when the first non-ASCII character is encountered
        /// or once <paramref name="elementCount"/> elements have been converted. Returns the total number
        /// of elements that were able to be converted.
        /// </summary>
        public static unsafe nuint NarrowUtf16ToAscii(char* pUtf16Buffer, byte* pAsciiBuffer, nuint elementCount)
        {
            nuint currentOffset = 0;

            uint utf16Data32BitsHigh = 0, utf16Data32BitsLow = 0;
            ulong utf16Data64Bits = 0;

            // If SSE2 is supported, use those specific intrinsics instead of the generic vectorized
            // code below. This has two benefits: (a) we can take advantage of specific instructions like
            // pmovmskb, ptest, vpminuw which we know are optimized, and (b) we can avoid downclocking the
            // processor while this method is running.

            if (Sse2.IsSupported)
            {
                Debug.Assert(BitConverter.IsLittleEndian, "Assume little endian if SSE2 is supported.");

                if (elementCount >= 2 * (uint)Unsafe.SizeOf<Vector128<byte>>())
                {
                    // Since there's overhead to setting up the vectorized code path, we only want to
                    // call into it after a quick probe to ensure the next immediate characters really are ASCII.
                    // If we see non-ASCII data, we'll jump immediately to the draining logic at the end of the method.

                    if (PlatformDependent.Is64BitProcess)
                    {
                        utf16Data64Bits = Unsafe.ReadUnaligned<ulong>(pUtf16Buffer);
                        if (!AllCharsInUInt64AreAscii(utf16Data64Bits))
                        {
                            goto FoundNonAsciiDataIn64BitRead;
                        }
                    }
                    else
                    {
                        utf16Data32BitsHigh = Unsafe.ReadUnaligned<uint>(pUtf16Buffer);
                        utf16Data32BitsLow = Unsafe.ReadUnaligned<uint>(pUtf16Buffer + 4 / sizeof(char));
                        if (!AllCharsInUInt32AreAscii(utf16Data32BitsHigh | utf16Data32BitsLow))
                        {
                            goto FoundNonAsciiDataIn64BitRead;
                        }
                    }

                    currentOffset = NarrowUtf16ToAscii_Sse2(pUtf16Buffer, pAsciiBuffer, elementCount);
                }
            }
            else if (Vector.IsHardwareAccelerated)
            {
                uint SizeOfVector = (uint)Unsafe.SizeOf<Vector<byte>>(); // JIT will make this a const

                // Only bother vectorizing if we have enough data to do so.
                if (elementCount >= 2 * SizeOfVector)
                {
                    // Since there's overhead to setting up the vectorized code path, we only want to
                    // call into it after a quick probe to ensure the next immediate characters really are ASCII.
                    // If we see non-ASCII data, we'll jump immediately to the draining logic at the end of the method.

                    if (PlatformDependent.Is64BitProcess)
                    {
                        utf16Data64Bits = Unsafe.ReadUnaligned<ulong>(pUtf16Buffer);
                        if (!AllCharsInUInt64AreAscii(utf16Data64Bits))
                        {
                            goto FoundNonAsciiDataIn64BitRead;
                        }
                    }
                    else
                    {
                        utf16Data32BitsHigh = Unsafe.ReadUnaligned<uint>(pUtf16Buffer);
                        utf16Data32BitsLow = Unsafe.ReadUnaligned<uint>(pUtf16Buffer + 4 / sizeof(char));
                        if (!AllCharsInUInt32AreAscii(utf16Data32BitsHigh | utf16Data32BitsLow))
                        {
                            goto FoundNonAsciiDataIn64BitRead;
                        }
                    }

                    Vector<ushort> maxAscii = new Vector<ushort>(0x007F);

                    nuint finalOffsetWhereCanLoop = elementCount - 2 * SizeOfVector;
                    do
                    {
                        Vector<ushort> utf16VectorHigh = Unsafe.ReadUnaligned<Vector<ushort>>(pUtf16Buffer + currentOffset);
                        Vector<ushort> utf16VectorLow = Unsafe.ReadUnaligned<Vector<ushort>>(pUtf16Buffer + currentOffset + Vector<ushort>.Count);

                        if (Vector.GreaterThanAny(Vector.BitwiseOr(utf16VectorHigh, utf16VectorLow), maxAscii))
                        {
                            break; // found non-ASCII data
                        }

                        // TODO: Is the below logic also valid for big-endian platforms?
                        Vector<byte> asciiVector = Vector.Narrow(utf16VectorHigh, utf16VectorLow);
                        Unsafe.WriteUnaligned<Vector<byte>>(pAsciiBuffer + currentOffset, asciiVector);

                        currentOffset += SizeOfVector;
                    } while (currentOffset <= finalOffsetWhereCanLoop);
                }
            }

            Debug.Assert(currentOffset <= elementCount);
            nuint remainingElementCount = elementCount - currentOffset;

            // Try to narrow 64 bits -> 32 bits at a time.
            // We needn't update remainingElementCount after this point.

            if (remainingElementCount >= 4)
            {
                nuint finalOffsetWhereCanLoop = currentOffset + remainingElementCount - 4;
                do
                {
                    if (PlatformDependent.Is64BitProcess)
                    {
                        // Only perform QWORD reads on a 64-bit platform.
                        utf16Data64Bits = Unsafe.ReadUnaligned<ulong>(pUtf16Buffer + currentOffset);
                        if (!AllCharsInUInt64AreAscii(utf16Data64Bits))
                        {
                            goto FoundNonAsciiDataIn64BitRead;
                        }

                        NarrowFourUtf16CharsToAsciiAndWriteToBuffer(ref pAsciiBuffer[currentOffset], utf16Data64Bits);
                    }
                    else
                    {
                        utf16Data32BitsHigh = Unsafe.ReadUnaligned<uint>(pUtf16Buffer + currentOffset);
                        utf16Data32BitsLow = Unsafe.ReadUnaligned<uint>(pUtf16Buffer + currentOffset + 4 / sizeof(char));
                        if (!AllCharsInUInt32AreAscii(utf16Data32BitsHigh | utf16Data32BitsLow))
                        {
                            goto FoundNonAsciiDataIn64BitRead;
                        }

                        NarrowTwoUtf16CharsToAsciiAndWriteToBuffer(ref pAsciiBuffer[currentOffset], utf16Data32BitsHigh);
                        NarrowTwoUtf16CharsToAsciiAndWriteToBuffer(ref pAsciiBuffer[currentOffset + 2], utf16Data32BitsLow);
                    }

                    currentOffset += 4;
                } while (currentOffset <= finalOffsetWhereCanLoop);
            }

            // Try to narrow 32 bits -> 16 bits.

            if (((uint)remainingElementCount & 2) != 0)
            {
                utf16Data32BitsHigh = Unsafe.ReadUnaligned<uint>(pUtf16Buffer + currentOffset);
                if (!AllCharsInUInt32AreAscii(utf16Data32BitsHigh))
                {
                    goto FoundNonAsciiDataInHigh32Bits;
                }

                NarrowTwoUtf16CharsToAsciiAndWriteToBuffer(ref pAsciiBuffer[currentOffset], utf16Data32BitsHigh);
                currentOffset += 2;
            }

            // Try to narrow 16 bits -> 8 bits.

            if (((uint)remainingElementCount & 1) != 0)
            {
                utf16Data32BitsHigh = pUtf16Buffer[currentOffset];
                if (utf16Data32BitsHigh <= 0x007Fu)
                {
                    pAsciiBuffer[currentOffset] = (byte)utf16Data32BitsHigh;
                    currentOffset++;
                }
            }

        Finish:

            return currentOffset;

        FoundNonAsciiDataIn64BitRead:

            if (PlatformDependent.Is64BitProcess)
            {
                // Try checking the first 32 bits of the buffer for non-ASCII data.
                // Regardless, we'll move the non-ASCII data into the utf16Data32BitsHigh local.

                if (BitConverter.IsLittleEndian)
                {
                    utf16Data32BitsHigh = (uint)utf16Data64Bits;
                }
                else
                {
                    utf16Data32BitsHigh = (uint)(utf16Data64Bits >> 32);
                }

                if (AllCharsInUInt32AreAscii(utf16Data32BitsHigh))
                {
                    NarrowTwoUtf16CharsToAsciiAndWriteToBuffer(ref pAsciiBuffer[currentOffset], utf16Data32BitsHigh);

                    if (BitConverter.IsLittleEndian)
                    {
                        utf16Data32BitsHigh = (uint)(utf16Data64Bits >> 32);
                    }
                    else
                    {
                        utf16Data32BitsHigh = (uint)utf16Data64Bits;
                    }

                    currentOffset += 2;
                }
            }
            else
            {
                // Need to determine if the high or the low 32-bit value contained non-ASCII data.
                // Regardless, we'll move the non-ASCII data into the utf16Data32BitsHigh local.

                if (AllCharsInUInt32AreAscii(utf16Data32BitsHigh))
                {
                    NarrowTwoUtf16CharsToAsciiAndWriteToBuffer(ref pAsciiBuffer[currentOffset], utf16Data32BitsHigh);
                    utf16Data32BitsHigh = utf16Data32BitsLow;
                    currentOffset += 2;
                }
            }

        FoundNonAsciiDataInHigh32Bits:

            Debug.Assert(!AllCharsInUInt32AreAscii(utf16Data32BitsHigh), "Shouldn't have reached this point if we have an all-ASCII input.");

            // There's at most one char that needs to be drained.

            if (FirstCharInUInt32IsAscii(utf16Data32BitsHigh))
            {
                if (!BitConverter.IsLittleEndian)
                {
                    utf16Data32BitsHigh >>= 16; // move high char down to low char
                }

                pAsciiBuffer[currentOffset] = (byte)utf16Data32BitsHigh;
                currentOffset++;
            }

            goto Finish;
        }
    }
}
#endif
