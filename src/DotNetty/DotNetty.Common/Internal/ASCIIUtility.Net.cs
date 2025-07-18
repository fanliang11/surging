// borrowed from https://github.com/dotnet/corefx/blob/release/3.1/src/Common/src/CoreLib/System/Text/ASCIIUtility.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if NET
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace DotNetty.Common.Internal
{
    partial class ASCIIUtility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetIndexOfFirstNonAsciiByteInLane_AdvSimd(Vector128<byte> value, Vector128<byte> bitmask)
        {
            if (!AdvSimd.Arm64.IsSupported || !BitConverter.IsLittleEndian)
            {
                throw new PlatformNotSupportedException();
            }

            // extractedBits[i] = (value[i] >> 7) & (1 << (12 * (i % 2)));
            Vector128<byte> mostSignificantBitIsSet = AdvSimd.ShiftRightArithmetic(value.AsSByte(), 7).AsByte();
            Vector128<byte> extractedBits = AdvSimd.And(mostSignificantBitIsSet, bitmask);

            // collapse mask to lower bits
            extractedBits = AdvSimd.Arm64.AddPairwise(extractedBits, extractedBits);
            ulong mask = extractedBits.AsUInt64().ToScalar();

            // calculate the index
            int index = BitOperations.TrailingZeroCount(mask) >> 2;
            Debug.Assert((mask != 0) ? index < 16 : index >= 16);
            return index;
        }

        /// <summary>
        /// Returns the index in <paramref name="pBuffer"/> where the first non-ASCII byte is found.
        /// Returns <paramref name="bufferLength"/> if the buffer is empty or all-ASCII.
        /// </summary>
        /// <returns>An ASCII byte is defined as 0x00 - 0x7F, inclusive.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe nuint GetIndexOfFirstNonAsciiByte(byte* pBuffer, nuint bufferLength)
        {
            // If SSE2 is supported, use those specific intrinsics instead of the generic vectorized
            // code below. This has two benefits: (a) we can take advantage of specific instructions like
            // pmovmskb which we know are optimized, and (b) we can avoid downclocking the processor while
            // this method is running.

            return (Sse2.IsSupported || AdvSimd.Arm64.IsSupported && BitConverter.IsLittleEndian)
                ? GetIndexOfFirstNonAsciiByte_Intrinsified(pBuffer, bufferLength)
                : GetIndexOfFirstNonAsciiByte_Default(pBuffer, bufferLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ContainsNonAsciiByte_Sse2(uint sseMask)
        {
            Debug.Assert(sseMask != uint.MaxValue);
            Debug.Assert(Sse2.IsSupported);
            return sseMask != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ContainsNonAsciiByte_AdvSimd(uint advSimdIndex)
        {
            Debug.Assert(advSimdIndex != uint.MaxValue);
            Debug.Assert(AdvSimd.IsSupported);
            return advSimdIndex < 16;
        }

        private static unsafe nuint GetIndexOfFirstNonAsciiByte_Intrinsified(byte* pBuffer, nuint bufferLength)
        {
            // JIT turns the below into constants

            uint SizeOfVector128 = (uint)Unsafe.SizeOf<Vector128<byte>>();
            nuint MaskOfAllBitsInVector128 = (nuint)(SizeOfVector128 - 1);

            Debug.Assert(Sse2.IsSupported || AdvSimd.Arm64.IsSupported, "Sse2 or AdvSimd64 required.");
            Debug.Assert(BitConverter.IsLittleEndian, "This SSE2/Arm64 implementation assumes little-endian.");

            Vector128<byte> bitmask = BitConverter.IsLittleEndian ?
                Vector128.Create((ushort)0x1001).AsByte() :
                Vector128.Create((ushort)0x0110).AsByte();

            uint currentSseMask = uint.MaxValue, secondSseMask = uint.MaxValue;
            uint currentAdvSimdIndex = uint.MaxValue, secondAdvSimdIndex = uint.MaxValue;
            byte* pOriginalBuffer = pBuffer;

            // This method is written such that control generally flows top-to-bottom, avoiding
            // jumps as much as possible in the optimistic case of a large enough buffer and
            // "all ASCII". If we see non-ASCII data, we jump out of the hot paths to targets
            // after all the main logic.

            if (bufferLength < SizeOfVector128)
            {
                goto InputBufferLessThanOneVectorInLength; // can't vectorize; drain primitives instead
            }

            // Read the first vector unaligned.

            if (Sse2.IsSupported)
            {
                currentSseMask = (uint)Sse2.MoveMask(Sse2.LoadVector128(pBuffer)); // unaligned load
                if (ContainsNonAsciiByte_Sse2(currentSseMask))
                {
                    goto FoundNonAsciiDataInCurrentChunk;
                }
            }
            else if (AdvSimd.Arm64.IsSupported)
            {
                currentAdvSimdIndex = (uint)GetIndexOfFirstNonAsciiByteInLane_AdvSimd(AdvSimd.LoadVector128(pBuffer), bitmask); // unaligned load
                if (ContainsNonAsciiByte_AdvSimd(currentAdvSimdIndex))
                {
                    goto FoundNonAsciiDataInCurrentChunk;
                }
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            // If we have less than 32 bytes to process, just go straight to the final unaligned
            // read. There's no need to mess with the loop logic in the middle of this method.

            if (bufferLength < 2 * SizeOfVector128)
            {
                goto IncrementCurrentOffsetBeforeFinalUnalignedVectorRead;
            }

            // Now adjust the read pointer so that future reads are aligned.

            pBuffer = (byte*)(((nuint)pBuffer + SizeOfVector128) & ~(nuint)MaskOfAllBitsInVector128);

#if DEBUG
            long numBytesRead = pBuffer - pOriginalBuffer;
            Debug.Assert(0 < numBytesRead && numBytesRead <= SizeOfVector128, "We should've made forward progress of at least one byte.");
            Debug.Assert((nuint)numBytesRead <= bufferLength, "We shouldn't have read past the end of the input buffer.");
#endif

            // Adjust the remaining length to account for what we just read.

            bufferLength += (nuint)pOriginalBuffer;
            bufferLength -= (nuint)pBuffer;

            // The buffer is now properly aligned.
            // Read 2 vectors at a time if possible.

            if (bufferLength >= 2 * SizeOfVector128)
            {
                byte* pFinalVectorReadPos = (byte*)((nuint)pBuffer + bufferLength - 2 * SizeOfVector128);

                // After this point, we no longer need to update the bufferLength value.

                do
                {
                    if (Sse2.IsSupported)
                    {
                        Vector128<byte> firstVector = Sse2.LoadAlignedVector128(pBuffer);
                        Vector128<byte> secondVector = Sse2.LoadAlignedVector128(pBuffer + SizeOfVector128);

                        currentSseMask = (uint)Sse2.MoveMask(firstVector);
                        secondSseMask = (uint)Sse2.MoveMask(secondVector);
                        if (ContainsNonAsciiByte_Sse2(currentSseMask | secondSseMask))
                        {
                            goto FoundNonAsciiDataInInnerLoop;
                        }
                    }
                    else if (AdvSimd.Arm64.IsSupported)
                    {
                        Vector128<byte> firstVector = AdvSimd.LoadVector128(pBuffer);
                        Vector128<byte> secondVector = AdvSimd.LoadVector128(pBuffer + SizeOfVector128);

                        currentAdvSimdIndex = (uint)GetIndexOfFirstNonAsciiByteInLane_AdvSimd(firstVector, bitmask);
                        secondAdvSimdIndex = (uint)GetIndexOfFirstNonAsciiByteInLane_AdvSimd(secondVector, bitmask);
                        if (ContainsNonAsciiByte_AdvSimd(currentAdvSimdIndex) || ContainsNonAsciiByte_AdvSimd(secondAdvSimdIndex))
                        {
                            goto FoundNonAsciiDataInInnerLoop;
                        }
                    }
                    else
                    {
                        throw new PlatformNotSupportedException();
                    }

                    pBuffer += 2 * SizeOfVector128;
                } while (pBuffer <= pFinalVectorReadPos);
            }

            // We have somewhere between 0 and (2 * vector length) - 1 bytes remaining to read from.
            // Since the above loop doesn't update bufferLength, we can't rely on its absolute value.
            // But we _can_ rely on it to tell us how much remaining data must be drained by looking
            // at what bits of it are set. This works because had we updated it within the loop above,
            // we would've been adding 2 * SizeOfVector128 on each iteration, but we only care about
            // bits which are less significant than those that the addition would've acted on.

            // If there is fewer than one vector length remaining, skip the next aligned read.

            if ((bufferLength & SizeOfVector128) == 0)
            {
                goto DoFinalUnalignedVectorRead;
            }

            // At least one full vector's worth of data remains, so we can safely read it.
            // Remember, at this point pBuffer is still aligned.

            if (Sse2.IsSupported)
            {
                currentSseMask = (uint)Sse2.MoveMask(Sse2.LoadAlignedVector128(pBuffer));
                if (ContainsNonAsciiByte_Sse2(currentSseMask))
                {
                    goto FoundNonAsciiDataInCurrentChunk;
                }
            }
            else if (AdvSimd.Arm64.IsSupported)
            {
                currentAdvSimdIndex = (uint)GetIndexOfFirstNonAsciiByteInLane_AdvSimd(AdvSimd.LoadVector128(pBuffer), bitmask);
                if (ContainsNonAsciiByte_AdvSimd(currentAdvSimdIndex))
                {
                    goto FoundNonAsciiDataInCurrentChunk;
                }
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

        IncrementCurrentOffsetBeforeFinalUnalignedVectorRead:

            pBuffer += SizeOfVector128;

        DoFinalUnalignedVectorRead:

            if (((byte)bufferLength & MaskOfAllBitsInVector128) != 0)
            {
                // Perform an unaligned read of the last vector.
                // We need to adjust the pointer because we're re-reading data.

                pBuffer += (bufferLength & MaskOfAllBitsInVector128) - SizeOfVector128;

                if (Sse2.IsSupported)
                {
                    currentSseMask = (uint)Sse2.MoveMask(Sse2.LoadVector128(pBuffer)); // unaligned load
                    if (ContainsNonAsciiByte_Sse2(currentSseMask))
                    {
                        goto FoundNonAsciiDataInCurrentChunk;
                    }

                }
                else if (AdvSimd.Arm64.IsSupported)
                {
                    currentAdvSimdIndex = (uint)GetIndexOfFirstNonAsciiByteInLane_AdvSimd(AdvSimd.LoadVector128(pBuffer), bitmask); // unaligned load
                    if (ContainsNonAsciiByte_AdvSimd(currentAdvSimdIndex))
                    {
                        goto FoundNonAsciiDataInCurrentChunk;
                    }

                }
                else
                {
                    throw new PlatformNotSupportedException();
                }

                pBuffer += SizeOfVector128;
            }

        Finish:
            return (nuint)pBuffer - (nuint)pOriginalBuffer; // and we're done!

        FoundNonAsciiDataInInnerLoop:

            // If the current (first) mask isn't the mask that contains non-ASCII data, then it must
            // instead be the second mask. If so, skip the entire first mask and drain ASCII bytes
            // from the second mask.

            if (Sse2.IsSupported)
            {
                if (!ContainsNonAsciiByte_Sse2(currentSseMask))
                {
                    pBuffer += SizeOfVector128;
                    currentSseMask = secondSseMask;
                }
            }
            else if (AdvSimd.IsSupported)
            {
                if (!ContainsNonAsciiByte_AdvSimd(currentAdvSimdIndex))
                {
                    pBuffer += SizeOfVector128;
                    currentAdvSimdIndex = secondAdvSimdIndex;
                }
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        FoundNonAsciiDataInCurrentChunk:


            if (Sse2.IsSupported)
            {
                // The mask contains - from the LSB - a 0 for each ASCII byte we saw, and a 1 for each non-ASCII byte.
                // Tzcnt is the correct operation to count the number of zero bits quickly. If this instruction isn't
                // available, we'll fall back to a normal loop.
                Debug.Assert(ContainsNonAsciiByte_Sse2(currentSseMask), "Shouldn't be here unless we see non-ASCII data.");
                pBuffer += (uint)BitOperations.TrailingZeroCount(currentSseMask);
            }
            else if (AdvSimd.Arm64.IsSupported)
            {
                Debug.Assert(ContainsNonAsciiByte_AdvSimd(currentAdvSimdIndex), "Shouldn't be here unless we see non-ASCII data.");
                pBuffer += currentAdvSimdIndex;
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            goto Finish;

        FoundNonAsciiDataInCurrentDWord:

            uint currentDWord;
            Debug.Assert(!AllBytesInUInt32AreAscii(currentDWord), "Shouldn't be here unless we see non-ASCII data.");
            pBuffer += CountNumberOfLeadingAsciiBytesFromUInt32WithSomeNonAsciiData(currentDWord);

            goto Finish;

        InputBufferLessThanOneVectorInLength:

            // These code paths get hit if the original input length was less than one vector in size.
            // We can't perform vectorized reads at this point, so we'll fall back to reading primitives
            // directly. Note that all of these reads are unaligned.

            Debug.Assert(bufferLength < SizeOfVector128);

            // QWORD drain

            if ((bufferLength & 8) != 0)
            {
                if (UIntPtr.Size == sizeof(ulong))
                {
                    // If we can use 64-bit tzcnt to count the number of leading ASCII bytes, prefer it.

                    ulong candidateUInt64 = Unsafe.ReadUnaligned<ulong>(pBuffer);
                    if (!AllBytesInUInt64AreAscii(candidateUInt64))
                    {
                        // Clear everything but the high bit of each byte, then tzcnt.
                        // Remember to divide by 8 at the end to convert bit count to byte count.

                        candidateUInt64 &= UInt64HighBitsOnlyMask;
                        pBuffer += (nuint)(BitOperations.TrailingZeroCount(candidateUInt64) >> 3);
                        goto Finish;
                    }
                }
                else
                {
                    // If we can't use 64-bit tzcnt, no worries. We'll just do 2x 32-bit reads instead.

                    currentDWord = Unsafe.ReadUnaligned<uint>(pBuffer);
                    uint nextDWord = Unsafe.ReadUnaligned<uint>(pBuffer + 4);

                    if (!AllBytesInUInt32AreAscii(currentDWord | nextDWord))
                    {
                        // At least one of the values wasn't all-ASCII.
                        // We need to figure out which one it was and stick it in the currentMask local.

                        if (AllBytesInUInt32AreAscii(currentDWord))
                        {
                            currentDWord = nextDWord; // this one is the culprit
                            pBuffer += 4;
                        }

                        goto FoundNonAsciiDataInCurrentDWord;
                    }
                }

                pBuffer += 8; // successfully consumed 8 ASCII bytes
            }

            // DWORD drain

            if ((bufferLength & 4) != 0)
            {
                currentDWord = Unsafe.ReadUnaligned<uint>(pBuffer);

                if (!AllBytesInUInt32AreAscii(currentDWord))
                {
                    goto FoundNonAsciiDataInCurrentDWord;
                }

                pBuffer += 4; // successfully consumed 4 ASCII bytes
            }

            // WORD drain
            // (We movzx to a DWORD for ease of manipulation.)

            if ((bufferLength & 2) != 0)
            {
                currentDWord = Unsafe.ReadUnaligned<ushort>(pBuffer);

                if (!AllBytesInUInt32AreAscii(currentDWord))
                {
                    // We only care about the 0x0080 bit of the value. If it's not set, then we
                    // increment currentOffset by 1. If it's set, we don't increment it at all.

                    pBuffer += (nuint)((nint)(sbyte)currentDWord >> 7) + 1;
                    goto Finish;
                }

                pBuffer += 2; // successfully consumed 2 ASCII bytes
            }

            // BYTE drain

            if ((bufferLength & 1) != 0)
            {
                // sbyte has non-negative value if byte is ASCII.

                if (*(sbyte*)(pBuffer) >= 0)
                {
                    pBuffer++; // successfully consumed a single byte
                }
            }

            goto Finish;
        }

        private static unsafe nuint GetIndexOfFirstNonAsciiChar_Sse2(char* pBuffer, nuint bufferLength /* in chars */)
        {
            // This method contains logic optimized for both SSE2 and SSE41. Much of the logic in this method
            // will be elided by JIT once we determine which specific ISAs we support.

            // Quick check for empty inputs.

            if (bufferLength == 0)
            {
                return 0;
            }

            // JIT turns the below into constants

            uint SizeOfVector128InBytes = (uint)Unsafe.SizeOf<Vector128<byte>>();
            uint SizeOfVector128InChars = SizeOfVector128InBytes / sizeof(char);

            Debug.Assert(Sse2.IsSupported, "Should've been checked by caller.");
            Debug.Assert(BitConverter.IsLittleEndian, "SSE2 assumes little-endian.");

            Vector128<ushort> firstVector, secondVector;
            uint currentMask;
            char* pOriginalBuffer = pBuffer;

            if (bufferLength < SizeOfVector128InChars)
            {
                goto InputBufferLessThanOneVectorInLength; // can't vectorize; drain primitives instead
            }

            // This method is written such that control generally flows top-to-bottom, avoiding
            // jumps as much as possible in the optimistic case of "all ASCII". If we see non-ASCII
            // data, we jump out of the hot paths to targets at the end of the method.

            Vector128<ushort> asciiMaskForTestZ = Vector128.Create((ushort)0xFF80); // used for PTEST on supported hardware
            Vector128<ushort> asciiMaskForAddSaturate = Vector128.Create((ushort)0x7F80); // used for PADDUSW
            const uint NonAsciiDataSeenMask = 0b_1010_1010_1010_1010; // used for determining whether 'currentMask' contains non-ASCII data

//#if SYSTEM_PRIVATE_CORELIB
            Debug.Assert(bufferLength <= nuint.MaxValue / sizeof(char));
//#endif

            // Read the first vector unaligned.

            firstVector = Sse2.LoadVector128((ushort*)pBuffer); // unaligned load

            // The operation below forces the 0x8000 bit of each WORD to be set iff the WORD element
            // has value >= 0x0800 (non-ASCII). Then we'll treat the vector as a BYTE vector in order
            // to extract the mask. Reminder: the 0x0080 bit of each WORD should be ignored.

            currentMask = (uint)Sse2.MoveMask(Sse2.AddSaturate(firstVector, asciiMaskForAddSaturate).AsByte());

            if ((currentMask & NonAsciiDataSeenMask) != 0)
            {
                goto FoundNonAsciiDataInCurrentMask;
            }

            // If we have less than 32 bytes to process, just go straight to the final unaligned
            // read. There's no need to mess with the loop logic in the middle of this method.

            // Adjust the remaining length to account for what we just read.
            // For the remainder of this code path, bufferLength will be in bytes, not chars.

            bufferLength <<= 1; // chars to bytes

            if (bufferLength < 2 * SizeOfVector128InBytes)
            {
                goto IncrementCurrentOffsetBeforeFinalUnalignedVectorRead;
            }

            // Now adjust the read pointer so that future reads are aligned.

            pBuffer = (char*)(((nuint)pBuffer + SizeOfVector128InBytes) & ~(nuint)(SizeOfVector128InBytes - 1));

#if DEBUG
            long numCharsRead = pBuffer - pOriginalBuffer;
            Debug.Assert(0 < numCharsRead && numCharsRead <= SizeOfVector128InChars, "We should've made forward progress of at least one char.");
            Debug.Assert((nuint)numCharsRead <= bufferLength, "We shouldn't have read past the end of the input buffer.");
#endif

            // Adjust remaining buffer length.

            bufferLength += (nuint)pOriginalBuffer;
            bufferLength -= (nuint)pBuffer;

            // The buffer is now properly aligned.
            // Read 2 vectors at a time if possible.

            if (bufferLength >= 2 * SizeOfVector128InBytes)
            {
                char* pFinalVectorReadPos = (char*)((nuint)pBuffer + bufferLength - 2 * SizeOfVector128InBytes);

                // After this point, we no longer need to update the bufferLength value.

                do
                {
                    firstVector = Sse2.LoadAlignedVector128((ushort*)pBuffer);
                    secondVector = Sse2.LoadAlignedVector128((ushort*)pBuffer + SizeOfVector128InChars);
                    Vector128<ushort> combinedVector = Sse2.Or(firstVector, secondVector);

                    if (Sse41.IsSupported)
                    {
                        // If a non-ASCII bit is set in any WORD of the combined vector, we have seen non-ASCII data.
                        // Jump to the non-ASCII handler to figure out which particular vector contained non-ASCII data.
                        if (!Sse41.TestZ(combinedVector, asciiMaskForTestZ))
                        {
                            goto FoundNonAsciiDataInFirstOrSecondVector;
                        }
                    }
                    else
                    {
                        // See comment earlier in the method for an explanation of how the below logic works.
                        currentMask = (uint)Sse2.MoveMask(Sse2.AddSaturate(combinedVector, asciiMaskForAddSaturate).AsByte());
                        if ((currentMask & NonAsciiDataSeenMask) != 0)
                        {
                            goto FoundNonAsciiDataInFirstOrSecondVector;
                        }
                    }

                    pBuffer += 2 * SizeOfVector128InChars;
                } while (pBuffer <= pFinalVectorReadPos);
            }

            // We have somewhere between 0 and (2 * vector length) - 1 bytes remaining to read from.
            // Since the above loop doesn't update bufferLength, we can't rely on its absolute value.
            // But we _can_ rely on it to tell us how much remaining data must be drained by looking
            // at what bits of it are set. This works because had we updated it within the loop above,
            // we would've been adding 2 * SizeOfVector128 on each iteration, but we only care about
            // bits which are less significant than those that the addition would've acted on.

            // If there is fewer than one vector length remaining, skip the next aligned read.
            // Remember, at this point bufferLength is measured in bytes, not chars.

            if ((bufferLength & SizeOfVector128InBytes) == 0)
            {
                goto DoFinalUnalignedVectorRead;
            }

            // At least one full vector's worth of data remains, so we can safely read it.
            // Remember, at this point pBuffer is still aligned.

            firstVector = Sse2.LoadAlignedVector128((ushort*)pBuffer);

            if (Sse41.IsSupported)
            {
                // If a non-ASCII bit is set in any WORD of the combined vector, we have seen non-ASCII data.
                // Jump to the non-ASCII handler to figure out which particular vector contained non-ASCII data.
                if (!Sse41.TestZ(firstVector, asciiMaskForTestZ))
                {
                    goto FoundNonAsciiDataInFirstVector;
                }
            }
            else
            {
                // See comment earlier in the method for an explanation of how the below logic works.
                currentMask = (uint)Sse2.MoveMask(Sse2.AddSaturate(firstVector, asciiMaskForAddSaturate).AsByte());
                if ((currentMask & NonAsciiDataSeenMask) != 0)
                {
                    goto FoundNonAsciiDataInCurrentMask;
                }
            }

        IncrementCurrentOffsetBeforeFinalUnalignedVectorRead:

            pBuffer += SizeOfVector128InChars;

        DoFinalUnalignedVectorRead:

            if (((byte)bufferLength & (SizeOfVector128InBytes - 1)) != 0)
            {
                // Perform an unaligned read of the last vector.
                // We need to adjust the pointer because we're re-reading data.

                pBuffer = (char*)((byte*)pBuffer + (bufferLength & (SizeOfVector128InBytes - 1)) - SizeOfVector128InBytes);
                firstVector = Sse2.LoadVector128((ushort*)pBuffer); // unaligned load

                if (Sse41.IsSupported)
                {
                    // If a non-ASCII bit is set in any WORD of the combined vector, we have seen non-ASCII data.
                    // Jump to the non-ASCII handler to figure out which particular vector contained non-ASCII data.
                    if (!Sse41.TestZ(firstVector, asciiMaskForTestZ))
                    {
                        goto FoundNonAsciiDataInFirstVector;
                    }
                }
                else
                {
                    // See comment earlier in the method for an explanation of how the below logic works.
                    currentMask = (uint)Sse2.MoveMask(Sse2.AddSaturate(firstVector, asciiMaskForAddSaturate).AsByte());
                    if ((currentMask & NonAsciiDataSeenMask) != 0)
                    {
                        goto FoundNonAsciiDataInCurrentMask;
                    }
                }

                pBuffer += SizeOfVector128InChars;
            }

        Finish:

            Debug.Assert(((nuint)pBuffer - (nuint)pOriginalBuffer) % 2 == 0, "Shouldn't have incremented any pointer by an odd byte count.");
            return ((nuint)pBuffer - (nuint)pOriginalBuffer) / sizeof(char); // and we're done! (remember to adjust for char count)

        FoundNonAsciiDataInFirstOrSecondVector:

            // We don't know if the first or the second vector contains non-ASCII data. Check the first
            // vector, and if that's all-ASCII then the second vector must be the culprit. Either way
            // we'll make sure the first vector local is the one that contains the non-ASCII data.

            // See comment earlier in the method for an explanation of how the below logic works.
            if (Sse41.IsSupported)
            {
                if (!Sse41.TestZ(firstVector, asciiMaskForTestZ))
                {
                    goto FoundNonAsciiDataInFirstVector;
                }
            }
            else
            {
                currentMask = (uint)Sse2.MoveMask(Sse2.AddSaturate(firstVector, asciiMaskForAddSaturate).AsByte());
                if ((currentMask & NonAsciiDataSeenMask) != 0)
                {
                    goto FoundNonAsciiDataInCurrentMask;
                }
            }

            // Wasn't the first vector; must be the second.

            pBuffer += SizeOfVector128InChars;
            firstVector = secondVector;

        FoundNonAsciiDataInFirstVector:

            // See comment earlier in the method for an explanation of how the below logic works.
            currentMask = (uint)Sse2.MoveMask(Sse2.AddSaturate(firstVector, asciiMaskForAddSaturate).AsByte());

        FoundNonAsciiDataInCurrentMask:

            // See comment earlier in the method accounting for the 0x8000 and 0x0080 bits set after the WORD-sized operations.

            currentMask &= NonAsciiDataSeenMask;

            // Now, the mask contains - from the LSB - a 0b00 pair for each ASCII char we saw, and a 0b10 pair for each non-ASCII char.
            //
            // (Keep endianness in mind in the below examples.)
            // A non-ASCII char followed by two ASCII chars is 0b..._00_00_10. (tzcnt = 1)
            // An ASCII char followed by two non-ASCII chars is 0b..._10_10_00. (tzcnt = 3)
            // Two ASCII chars followed by a non-ASCII char is 0b..._10_00_00. (tzcnt = 5)
            //
            // This means tzcnt = 2 * numLeadingAsciiChars + 1. We can conveniently take advantage of the fact
            // that the 2x multiplier already matches the char* stride length, then just subtract 1 at the end to
            // compute the correct final ending pointer value.

            Debug.Assert(currentMask != 0, "Shouldn't be here unless we see non-ASCII data.");
            pBuffer = (char*)((byte*)pBuffer + (uint)BitOperations.TrailingZeroCount(currentMask) - 1);

            goto Finish;

        FoundNonAsciiDataInCurrentDWord:

            uint currentDWord;
            Debug.Assert(!AllCharsInUInt32AreAscii(currentDWord), "Shouldn't be here unless we see non-ASCII data.");

            if (FirstCharInUInt32IsAscii(currentDWord))
            {
                pBuffer++; // skip past the ASCII char
            }

            goto Finish;

        InputBufferLessThanOneVectorInLength:

            // These code paths get hit if the original input length was less than one vector in size.
            // We can't perform vectorized reads at this point, so we'll fall back to reading primitives
            // directly. Note that all of these reads are unaligned.

            // Reminder: If this code path is hit, bufferLength is still a char count, not a byte count.
            // We skipped the code path that multiplied the count by sizeof(char).

            Debug.Assert(bufferLength < SizeOfVector128InChars);

            // QWORD drain

            if ((bufferLength & 4) != 0)
            {
                if (UIntPtr.Size == sizeof(ulong))
                {
                    // If we can use 64-bit tzcnt to count the number of leading ASCII chars, prefer it.

                    ulong candidateUInt64 = Unsafe.ReadUnaligned<ulong>(pBuffer);
                    if (!AllCharsInUInt64AreAscii(candidateUInt64))
                    {
                        // Clear the low 7 bits (the ASCII bits) of each char, then tzcnt.
                        // Remember to divide by 8 at the end to convert bit count to byte count,
                        // then the & ~1 at the end to treat a match in the high byte of
                        // any char the same as a match in the low byte of that same char.

                        candidateUInt64 &= 0xFF80FF80_FF80FF80ul;
                        pBuffer = (char*)((byte*)pBuffer + ((nuint)(BitOperations.TrailingZeroCount(candidateUInt64) >> 3) & ~(nuint)1));
                        goto Finish;
                    }
                }
                else
                {
                    // If we can't use 64-bit tzcnt, no worries. We'll just do 2x 32-bit reads instead.

                    currentDWord = Unsafe.ReadUnaligned<uint>(pBuffer);
                    uint nextDWord = Unsafe.ReadUnaligned<uint>(pBuffer + 4 / sizeof(char));

                    if (!AllCharsInUInt32AreAscii(currentDWord | nextDWord))
                    {
                        // At least one of the values wasn't all-ASCII.
                        // We need to figure out which one it was and stick it in the currentMask local.

                        if (AllCharsInUInt32AreAscii(currentDWord))
                        {
                            currentDWord = nextDWord; // this one is the culprit
                            pBuffer += 4 / sizeof(char);
                        }

                        goto FoundNonAsciiDataInCurrentDWord;
                    }
                }

                pBuffer += 4; // successfully consumed 4 ASCII chars
            }

            // DWORD drain

            if ((bufferLength & 2) != 0)
            {
                currentDWord = Unsafe.ReadUnaligned<uint>(pBuffer);

                if (!AllCharsInUInt32AreAscii(currentDWord))
                {
                    goto FoundNonAsciiDataInCurrentDWord;
                }

                pBuffer += 2; // successfully consumed 2 ASCII chars
            }

            // WORD drain
            // This is the final drain; there's no need for a BYTE drain since our elemental type is 16-bit char.

            if ((bufferLength & 1) != 0)
            {
                if (*pBuffer <= 0x007F)
                {
                    pBuffer++; // successfully consumed a single char
                }
            }

            goto Finish;
        }

        private static unsafe nuint NarrowUtf16ToAscii_Sse2(char* pUtf16Buffer, byte* pAsciiBuffer, nuint elementCount)
        {
            // This method contains logic optimized for both SSE2 and SSE41. Much of the logic in this method
            // will be elided by JIT once we determine which specific ISAs we support.

            // JIT turns the below into constants

            uint SizeOfVector128 = (uint)Unsafe.SizeOf<Vector128<byte>>();
            nuint MaskOfAllBitsInVector128 = (nuint)(SizeOfVector128 - 1);

            // This method is written such that control generally flows top-to-bottom, avoiding
            // jumps as much as possible in the optimistic case of "all ASCII". If we see non-ASCII
            // data, we jump out of the hot paths to targets at the end of the method.

            Debug.Assert(Sse2.IsSupported);
            Debug.Assert(BitConverter.IsLittleEndian);
            Debug.Assert(elementCount >= 2 * SizeOfVector128);

            Vector128<short> asciiMaskForTestZ = Vector128.Create(unchecked((short)0xFF80)); // used for PTEST on supported hardware
            Vector128<ushort> asciiMaskForAddSaturate = Vector128.Create((ushort)0x7F80); // used for PADDUSW
            const int NonAsciiDataSeenMask = 0b_1010_1010_1010_1010; // used for determining whether the pmovmskb operation saw non-ASCII chars

            // First, perform an unaligned read of the first part of the input buffer.

            Vector128<short> utf16VectorFirst = Sse2.LoadVector128((short*)pUtf16Buffer); // unaligned load

            // If there's non-ASCII data in the first 8 elements of the vector, there's nothing we can do.
            // See comments in GetIndexOfFirstNonAsciiChar_Sse2 for information about how this works.

            if (Sse41.IsSupported)
            {
                if (!Sse41.TestZ(utf16VectorFirst, asciiMaskForTestZ))
                {
                    return 0;
                }
            }
            else
            {
                if ((Sse2.MoveMask(Sse2.AddSaturate(utf16VectorFirst.AsUInt16(), asciiMaskForAddSaturate).AsByte()) & NonAsciiDataSeenMask) != 0)
                {
                    return 0;
                }
            }

            // Turn the 8 ASCII chars we just read into 8 ASCII bytes, then copy it to the destination.

            Vector128<byte> asciiVector = Sse2.PackUnsignedSaturate(utf16VectorFirst, utf16VectorFirst);
            Sse2.StoreScalar((ulong*)pAsciiBuffer, asciiVector.AsUInt64()); // ulong* calculated here is UNALIGNED

            nuint currentOffsetInElements = SizeOfVector128 / 2; // we processed 8 elements so far

            // We're going to get the best performance when we have aligned writes, so we'll take the
            // hit of potentially unaligned reads in order to hit this sweet spot.

            // pAsciiBuffer points to the start of the destination buffer, immediately before where we wrote
            // the 8 bytes previously. If the 0x08 bit is set at the pinned address, then the 8 bytes we wrote
            // previously mean that the 0x08 bit is *not* set at address &pAsciiBuffer[SizeOfVector128 / 2]. In
            // that case we can immediately back up to the previous aligned boundary and start the main loop.
            // If the 0x08 bit is *not* set at the pinned address, then it means the 0x08 bit *is* set at
            // address &pAsciiBuffer[SizeOfVector128 / 2], and we should perform one more 8-byte write to bump
            // just past the next aligned boundary address.

            if (((uint)pAsciiBuffer & (SizeOfVector128 / 2)) == 0)
            {
                // We need to perform one more partial vector write before we can get the alignment we want.

                utf16VectorFirst = Sse2.LoadVector128((short*)pUtf16Buffer + currentOffsetInElements); // unaligned load

                // See comments earlier in this method for information about how this works.
                if (Sse41.IsSupported)
                {
                    if (!Sse41.TestZ(utf16VectorFirst, asciiMaskForTestZ))
                    {
                        goto Finish;
                    }
                }
                else
                {
                    if ((Sse2.MoveMask(Sse2.AddSaturate(utf16VectorFirst.AsUInt16(), asciiMaskForAddSaturate).AsByte()) & NonAsciiDataSeenMask) != 0)
                    {
                        goto Finish;
                    }
                }

                // Turn the 8 ASCII chars we just read into 8 ASCII bytes, then copy it to the destination.
                asciiVector = Sse2.PackUnsignedSaturate(utf16VectorFirst, utf16VectorFirst);
                Sse2.StoreScalar((ulong*)(pAsciiBuffer + currentOffsetInElements), asciiVector.AsUInt64()); // ulong* calculated here is UNALIGNED
            }

            // Calculate how many elements we wrote in order to get pAsciiBuffer to its next alignment
            // point, then use that as the base offset going forward.

            currentOffsetInElements = SizeOfVector128 - ((nuint)pAsciiBuffer & MaskOfAllBitsInVector128);
            Debug.Assert(0 < currentOffsetInElements && currentOffsetInElements <= SizeOfVector128, "We wrote at least 1 byte but no more than a whole vector.");

            Debug.Assert(currentOffsetInElements <= elementCount, "Shouldn't have overrun the destination buffer.");
            Debug.Assert(elementCount - currentOffsetInElements >= SizeOfVector128, "We should be able to run at least one whole vector.");

            nuint finalOffsetWhereCanRunLoop = elementCount - SizeOfVector128;
            do
            {
                // In a loop, perform two unaligned reads, narrow to a single vector, then aligned write one vector.

                utf16VectorFirst = Sse2.LoadVector128((short*)pUtf16Buffer + currentOffsetInElements); // unaligned load
                Vector128<short> utf16VectorSecond = Sse2.LoadVector128((short*)pUtf16Buffer + currentOffsetInElements + SizeOfVector128 / sizeof(short)); // unaligned load
                Vector128<short> combinedVector = Sse2.Or(utf16VectorFirst, utf16VectorSecond);

                // See comments in GetIndexOfFirstNonAsciiChar_Sse2 for information about how this works.
                if (Sse41.IsSupported)
                {
                    if (!Sse41.TestZ(combinedVector, asciiMaskForTestZ))
                    {
                        goto FoundNonAsciiDataInLoop;
                    }
                }
                else
                {
                    if ((Sse2.MoveMask(Sse2.AddSaturate(combinedVector.AsUInt16(), asciiMaskForAddSaturate).AsByte()) & NonAsciiDataSeenMask) != 0)
                    {
                        goto FoundNonAsciiDataInLoop;
                    }
                }

                // Build up the ASCII vector and perform the store.

                asciiVector = Sse2.PackUnsignedSaturate(utf16VectorFirst, utf16VectorSecond);

                Debug.Assert(((nuint)pAsciiBuffer + currentOffsetInElements) % SizeOfVector128 == 0, "Write should be aligned.");
                Sse2.StoreAligned(pAsciiBuffer + currentOffsetInElements, asciiVector); // aligned

                currentOffsetInElements += SizeOfVector128;
            } while (currentOffsetInElements <= finalOffsetWhereCanRunLoop);

        Finish:

            // There might be some ASCII data left over. That's fine - we'll let our caller handle the final drain.
            return currentOffsetInElements;

        FoundNonAsciiDataInLoop:

            // Can we at least narrow the high vector?
            // See comments in GetIndexOfFirstNonAsciiChar_Sse2 for information about how this works.
            if (Sse41.IsSupported)
            {
                if (!Sse41.TestZ(utf16VectorFirst, asciiMaskForTestZ))
                {
                    goto Finish; // found non-ASCII data
                }
            }
            else
            {
                if ((Sse2.MoveMask(Sse2.AddSaturate(utf16VectorFirst.AsUInt16(), asciiMaskForAddSaturate).AsByte()) & NonAsciiDataSeenMask) != 0)
                {
                    goto Finish; // found non-ASCII data
                }
            }

            // First part was all ASCII, narrow and aligned write. Note we're only filling in the low half of the vector.
            asciiVector = Sse2.PackUnsignedSaturate(utf16VectorFirst, utf16VectorFirst);

            Debug.Assert(((nuint)pAsciiBuffer + currentOffsetInElements) % sizeof(ulong) == 0, "Destination should be ulong-aligned.");

            Sse2.StoreScalar((ulong*)(pAsciiBuffer + currentOffsetInElements), asciiVector.AsUInt64()); // ulong* calculated here is aligned
            currentOffsetInElements += SizeOfVector128 / 2;

            goto Finish;
        }

        /// <summary>
        /// Copies as many ASCII bytes (00..7F) as possible from <paramref name="pAsciiBuffer"/>
        /// to <paramref name="pUtf16Buffer"/>, stopping when the first non-ASCII byte is encountered
        /// or once <paramref name="elementCount"/> elements have been converted. Returns the total number
        /// of elements that were able to be converted.
        /// </summary>
        public static unsafe nuint WidenAsciiToUtf16(byte* pAsciiBuffer, char* pUtf16Buffer, nuint elementCount)
        {
            // Intrinsified in mono interpreter
            nuint currentOffset = 0;

            // If SSE2 is supported, use those specific intrinsics instead of the generic vectorized
            // code below. This has two benefits: (a) we can take advantage of specific instructions like
            // pmovmskb which we know are optimized, and (b) we can avoid downclocking the processor while
            // this method is running.

            if (BitConverter.IsLittleEndian && (Sse2.IsSupported || AdvSimd.Arm64.IsSupported))
            {
                if (elementCount >= 2 * (uint)Unsafe.SizeOf<Vector128<byte>>())
                {
                    currentOffset = WidenAsciiToUtf16_Intrinsified(pAsciiBuffer, pUtf16Buffer, elementCount);
                }
            }
            else if (Vector.IsHardwareAccelerated)
            {
                uint SizeOfVector = (uint)Unsafe.SizeOf<Vector<byte>>(); // JIT will make this a const

                // Only bother vectorizing if we have enough data to do so.
                if (elementCount >= SizeOfVector)
                {
                    // Note use of SBYTE instead of BYTE below; we're using the two's-complement
                    // representation of negative integers to act as a surrogate for "is ASCII?".

                    nuint finalOffsetWhereCanLoop = elementCount - SizeOfVector;
                    do
                    {
                        Vector<sbyte> asciiVector = Unsafe.ReadUnaligned<Vector<sbyte>>(pAsciiBuffer + currentOffset);
                        if (Vector.LessThanAny(asciiVector, Vector<sbyte>.Zero))
                        {
                            break; // found non-ASCII data
                        }

                        Vector.Widen(Vector.AsVectorByte(asciiVector), out Vector<ushort> utf16LowVector, out Vector<ushort> utf16HighVector);

                        // TODO: Is the below logic also valid for big-endian platforms?
                        Unsafe.WriteUnaligned<Vector<ushort>>(pUtf16Buffer + currentOffset, utf16LowVector);
                        Unsafe.WriteUnaligned<Vector<ushort>>(pUtf16Buffer + currentOffset + Vector<ushort>.Count, utf16HighVector);

                        currentOffset += SizeOfVector;
                    } while (currentOffset <= finalOffsetWhereCanLoop);
                }
            }

            Debug.Assert(currentOffset <= elementCount);
            nuint remainingElementCount = elementCount - currentOffset;

            // Try to widen 32 bits -> 64 bits at a time.
            // We needn't update remainingElementCount after this point.

            uint asciiData;

            if (remainingElementCount >= 4)
            {
                nuint finalOffsetWhereCanLoop = currentOffset + remainingElementCount - 4;
                do
                {
                    asciiData = Unsafe.ReadUnaligned<uint>(pAsciiBuffer + currentOffset);
                    if (!AllBytesInUInt32AreAscii(asciiData))
                    {
                        goto FoundNonAsciiData;
                    }

                    WidenFourAsciiBytesToUtf16AndWriteToBuffer(ref pUtf16Buffer[currentOffset], asciiData);
                    currentOffset += 4;
                } while (currentOffset <= finalOffsetWhereCanLoop);
            }

            // Try to widen 16 bits -> 32 bits.

            if (((uint)remainingElementCount & 2) != 0)
            {
                asciiData = Unsafe.ReadUnaligned<ushort>(pAsciiBuffer + currentOffset);
                if (!AllBytesInUInt32AreAscii(asciiData))
                {
                    goto FoundNonAsciiData;
                }

                if (BitConverter.IsLittleEndian)
                {
                    pUtf16Buffer[currentOffset] = (char)(byte)asciiData;
                    pUtf16Buffer[currentOffset + 1] = (char)(asciiData >> 8);
                }
                else
                {
                    pUtf16Buffer[currentOffset + 1] = (char)(byte)asciiData;
                    pUtf16Buffer[currentOffset] = (char)(asciiData >> 8);
                }

                currentOffset += 2;
            }

            // Try to widen 8 bits -> 16 bits.

            if (((uint)remainingElementCount & 1) != 0)
            {
                asciiData = pAsciiBuffer[currentOffset];
                if (((byte)asciiData & 0x80) != 0)
                {
                    goto Finish;
                }

                pUtf16Buffer[currentOffset] = (char)asciiData;
                currentOffset++;
            }

        Finish:

            return currentOffset;

        FoundNonAsciiData:

            Debug.Assert(!AllBytesInUInt32AreAscii(asciiData), "Shouldn't have reached this point if we have an all-ASCII input.");

            // Drain ASCII bytes one at a time.

            while (((byte)asciiData & 0x80) == 0)
            {
                pUtf16Buffer[currentOffset] = (char)(byte)asciiData;
                currentOffset++;
                asciiData >>= 8;
            }

            goto Finish;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsNonAsciiByte(Vector128<byte> value)
        {
            if (!AdvSimd.Arm64.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }
            value = AdvSimd.Arm64.MaxPairwise(value, value);
            return (value.AsUInt64().ToScalar() & 0x8080808080808080) != 0;
        }

        private static unsafe nuint WidenAsciiToUtf16_Intrinsified(byte* pAsciiBuffer, char* pUtf16Buffer, nuint elementCount)
        {
            // JIT turns the below into constants

            uint SizeOfVector128 = (uint)Unsafe.SizeOf<Vector128<byte>>();
            nuint MaskOfAllBitsInVector128 = (nuint)(SizeOfVector128 - 1);

            // This method is written such that control generally flows top-to-bottom, avoiding
            // jumps as much as possible in the optimistic case of "all ASCII". If we see non-ASCII
            // data, we jump out of the hot paths to targets at the end of the method.

            Debug.Assert(Sse2.IsSupported || AdvSimd.Arm64.IsSupported);
            Debug.Assert(BitConverter.IsLittleEndian);
            Debug.Assert(elementCount >= 2 * SizeOfVector128);

            // We're going to get the best performance when we have aligned writes, so we'll take the
            // hit of potentially unaligned reads in order to hit this sweet spot.

            Vector128<byte> asciiVector;
            Vector128<byte> utf16FirstHalfVector;
            bool containsNonAsciiBytes;

            // First, perform an unaligned read of the first part of the input buffer.

            if (Sse2.IsSupported)
            {
                asciiVector = Sse2.LoadVector128(pAsciiBuffer); // unaligned load
                containsNonAsciiBytes = (uint)Sse2.MoveMask(asciiVector) != 0;
            }
            else if (AdvSimd.Arm64.IsSupported)
            {
                asciiVector = AdvSimd.LoadVector128(pAsciiBuffer);
                containsNonAsciiBytes = ContainsNonAsciiByte(asciiVector);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            // If there's non-ASCII data in the first 8 elements of the vector, there's nothing we can do.

            if (containsNonAsciiBytes)
            {
                return 0;
            }

            // Then perform an unaligned write of the first part of the input buffer.

            Vector128<byte> zeroVector = Vector128<byte>.Zero;

            if (Sse2.IsSupported)
            {
                utf16FirstHalfVector = Sse2.UnpackLow(asciiVector, zeroVector);
                Sse2.Store((byte*)pUtf16Buffer, utf16FirstHalfVector); // unaligned
            }
            else if (AdvSimd.IsSupported)
            {
                utf16FirstHalfVector = AdvSimd.ZeroExtendWideningLower(asciiVector.GetLower()).AsByte();
                AdvSimd.Store((byte*)pUtf16Buffer, utf16FirstHalfVector); // unaligned
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            // Calculate how many elements we wrote in order to get pOutputBuffer to its next alignment
            // point, then use that as the base offset going forward. Remember the >> 1 to account for
            // that we wrote chars, not bytes. This means we may re-read data in the next iteration of
            // the loop, but this is ok.

            nuint currentOffset = (SizeOfVector128 >> 1) - (((nuint)pUtf16Buffer >> 1) & (MaskOfAllBitsInVector128 >> 1));
            Debug.Assert(0 < currentOffset && currentOffset <= SizeOfVector128 / sizeof(char));

            nuint finalOffsetWhereCanRunLoop = elementCount - SizeOfVector128;

            // Calculating the destination address outside the loop results in significant
            // perf wins vs. relying on the JIT to fold memory addressing logic into the
            // write instructions. See: https://github.com/dotnet/runtime/issues/33002

            char* pCurrentWriteAddress = pUtf16Buffer + currentOffset;

            do
            {
                // In a loop, perform an unaligned read, widen to two vectors, then aligned write the two vectors.

                if (Sse2.IsSupported)
                {
                    asciiVector = Sse2.LoadVector128(pAsciiBuffer + currentOffset); // unaligned load
                    containsNonAsciiBytes = (uint)Sse2.MoveMask(asciiVector) != 0;
                }
                else if (AdvSimd.Arm64.IsSupported)
                {
                    asciiVector = AdvSimd.LoadVector128(pAsciiBuffer + currentOffset);
                    containsNonAsciiBytes = ContainsNonAsciiByte(asciiVector);
                }
                else
                {
                    throw new PlatformNotSupportedException();
                }

                if (containsNonAsciiBytes)
                {
                    // non-ASCII byte somewhere
                    goto NonAsciiDataSeenInInnerLoop;
                }

                if (Sse2.IsSupported)
                {
                    Vector128<byte> low = Sse2.UnpackLow(asciiVector, zeroVector);
                    Sse2.StoreAligned((byte*)pCurrentWriteAddress, low);

                    Vector128<byte> high = Sse2.UnpackHigh(asciiVector, zeroVector);
                    Sse2.StoreAligned((byte*)pCurrentWriteAddress + SizeOfVector128, high);
                }
                else if (AdvSimd.Arm64.IsSupported)
                {
                    Vector128<ushort> low = AdvSimd.ZeroExtendWideningLower(asciiVector.GetLower());
                    Vector128<ushort> high = AdvSimd.ZeroExtendWideningUpper(asciiVector);
                    AdvSimd.Arm64.StorePair((ushort*)pCurrentWriteAddress, low, high);
                }
                else
                {
                    throw new PlatformNotSupportedException();
                }

                currentOffset += SizeOfVector128;
                pCurrentWriteAddress += SizeOfVector128;
            } while (currentOffset <= finalOffsetWhereCanRunLoop);

        Finish:

            return currentOffset;

        NonAsciiDataSeenInInnerLoop:

            // Can we at least widen the first part of the vector?

            if (!containsNonAsciiBytes)
            {
                // First part was all ASCII, widen
                if (Sse2.IsSupported)
                {
                    utf16FirstHalfVector = Sse2.UnpackLow(asciiVector, zeroVector);
                    Sse2.StoreAligned((byte*)(pUtf16Buffer + currentOffset), utf16FirstHalfVector);
                }
                else if (AdvSimd.Arm64.IsSupported)
                {
                    Vector128<ushort> lower = AdvSimd.ZeroExtendWideningLower(asciiVector.GetLower());
                    AdvSimd.Store((ushort*)(pUtf16Buffer + currentOffset), lower);
                }
                else
                {
                    throw new PlatformNotSupportedException();
                }
                currentOffset += SizeOfVector128 / 2;
            }

            goto Finish;
        }

        /// <summary>
        /// Given a DWORD which represents a buffer of 4 bytes, widens the buffer into 4 WORDs and
        /// writes them to the output buffer with machine endianness.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void WidenFourAsciiBytesToUtf16AndWriteToBuffer(ref char outputBuffer, uint value)
        {
            Debug.Assert(AllBytesInUInt32AreAscii(value));

            if (Sse2.X64.IsSupported)
            {
                Debug.Assert(BitConverter.IsLittleEndian, "SSE2 widening assumes little-endian.");
                Vector128<byte> vecNarrow = Sse2.ConvertScalarToVector128UInt32(value).AsByte();
                Vector128<ulong> vecWide = Sse2.UnpackLow(vecNarrow, Vector128<byte>.Zero).AsUInt64();
                Unsafe.WriteUnaligned<ulong>(ref Unsafe.As<char, byte>(ref outputBuffer), Sse2.X64.ConvertToUInt64(vecWide));
            }
            else if (AdvSimd.Arm64.IsSupported)
            {
                Vector128<byte> vecNarrow = AdvSimd.DuplicateToVector128(value).AsByte();
                Vector128<ulong> vecWide = AdvSimd.Arm64.ZipLow(vecNarrow, Vector128<byte>.Zero).AsUInt64();
                Unsafe.WriteUnaligned<ulong>(ref Unsafe.As<char, byte>(ref outputBuffer), vecWide.ToScalar());
            }
            else
            {
                if (BitConverter.IsLittleEndian)
                {
                    outputBuffer = (char)(byte)value;
                    value >>= 8;
                    Unsafe.Add(ref outputBuffer, 1) = (char)(byte)value;
                    value >>= 8;
                    Unsafe.Add(ref outputBuffer, 2) = (char)(byte)value;
                    value >>= 8;
                    Unsafe.Add(ref outputBuffer, 3) = (char)value;
                }
                else
                {
                    Unsafe.Add(ref outputBuffer, 3) = (char)(byte)value;
                    value >>= 8;
                    Unsafe.Add(ref outputBuffer, 2) = (char)(byte)value;
                    value >>= 8;
                    Unsafe.Add(ref outputBuffer, 1) = (char)(byte)value;
                    value >>= 8;
                    outputBuffer = (char)value;
                }
            }
        }
    }
}
#endif
