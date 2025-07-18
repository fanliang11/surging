// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// borrowed from https://github.com/dotnet/corefxlab/tree/master/src/System.Text.Primitives/System/Text/Encoders

#if !NETCOREAPP_3_0_GREATER
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

//
// This file contains workhorse methods for performing validation of UTF-8 byte sequences.
//

namespace DotNetty.Common.Internal
{
    internal static partial class Utf8Util
    {
        // This method will consume as many ASCII bytes as it can using fast vectorized processing, returning the number of
        // consumed (ASCII) bytes. It's possible that the method exits early, perhaps because there is some non-ASCII byte
        // later in the sequence or because we're running out of input to search. The intent is that the caller *skips over*
        // the number of bytes returned by this method, then it continues data processing from the next byte.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static unsafe IntPtr ConsumeAsciiBytesVectorized(ref byte buffer, int length)
        {
            // Only allow vectorization if vectors are hardware-accelerated and we have enough
            // data to allow a vectorized search.

            if (!Vector.IsHardwareAccelerated || length < 3 * Vector<byte>.Count)
            {
                return IntPtr.Zero;
            }

            // JITter will generate VMOVUPD instructions, which performs better when the memory to read
            // is aligned. The GC may move the buffer around in memory, and while it will never cause
            // moved data to be misaligned with respect to the natural word size, no such guarantee is
            // made with respect to SIMD vector alignment. We'll pin the buffer so that we can enforce
            // alignment manually.

            fixed (byte* pbBuffer = &buffer)
            {
                // [Potentially unaligned] single SIMD read and comparison, quick check for non-ASCII data.

                Vector<byte> mask = new Vector<byte>((byte)0x80);
                if ((Unsafe.ReadUnaligned<Vector<byte>>(pbBuffer) & mask) != Vector<byte>.Zero)
                {
                    return IntPtr.Zero;
                }

                // Round 'pbBuffer' up to the *next* aligned address. If 'pbBuffer' was already aligned, this
                // just bumps the address up to the next vector. The read above guaranteed that we read all
                // data between 'pbBuffer' and 'pbAlignedBuffer' and checked it for non-ASCII bytes. It's
                // possible we'll duplicate a little bit of work if 'pbBuffer' wasn't already aligned since
                // its tail end may overlap with the immediate upcoming aligned read, but it's faster just to
                // perform the extra work and not worry about checking for this condition.

                // 'pbAlignedBuffer' will be somewhere between 1 and Vector<byte>.Count bytes ahead of 'pbBuffer',
                // hence the check for a length of >= 3 * Vector<byte>.Count at the beginning of this method.

                byte* pbAlignedBuffer;
                if (PlatformDependent.Is64BitProcess)
                {
                    pbAlignedBuffer = (byte*)(((long)pbBuffer + Vector<byte>.Count) & ~((long)Vector<byte>.Count - 1));
                }
                else
                {
                    pbAlignedBuffer = (byte*)(((int)pbBuffer + Vector<byte>.Count) & ~((int)Vector<byte>.Count - 1));
                }

                // Now iterate and read two aligned SIMD vectors at a time. We can skip the first length check on the
                // first iteration of the loop since we already performed a length check at the very beginning of this
                // method.

                byte* pbFinalPosAtWhichCanReadTwoVectors = &pbBuffer[length - 2 * Vector<byte>.Count];
                Debug.Assert(pbAlignedBuffer <= pbFinalPosAtWhichCanReadTwoVectors);

                do
                {
                    if (((Unsafe.Read<Vector<byte>>(pbAlignedBuffer) | Unsafe.Read<Vector<byte>>(pbAlignedBuffer + Vector<byte>.Count)) & mask) != Vector<byte>.Zero)
                    {
                        break; // non-ASCII data incoming
                    }
                } while ((pbAlignedBuffer += 2 * Vector<byte>.Count) <= pbFinalPosAtWhichCanReadTwoVectors);

                // We consumed all data up to 'pbAlignedBuffer' and know it to be non-ASCII.
                return (IntPtr)(pbAlignedBuffer - pbBuffer);
            }
        }

        /// <summary>
        /// Returns the offset in <paramref name="input"/> of where the first invalid UTF-8 sequence appears,
        /// or -1 if the input is valid UTF-8 text. (Empty inputs are considered valid.) On method return the
        /// <paramref name="scalarCount"/> parameter will contain the total number of Unicode scalar values seen
        /// up to (but not including) the first invalid sequence, and <paramref name="surrogatePairCount"/> will
        /// contain the number of surrogate pairs present if this text up to (but not including) the first
        /// invalid sequence were represented as UTF-16. To get the total UTF-16 code unit count, add
        /// <paramref name="surrogatePairCount"/> to <paramref name="scalarCount"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetIndexOfFirstInvalidUtf8Sequence(in ReadOnlySpan<byte> input, out int scalarCount, out int surrogatePairCount)
            => GetIndexOfFirstInvalidUtf8Sequence(ref MemoryMarshal.GetReference(input), input.Length, out scalarCount, out surrogatePairCount);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int GetIndexOfFirstInvalidUtf8Sequence(ref byte inputBuffer, int inputLength, out int scalarCount, out int surrogatePairCount)
        {
            // The fields below control where we read from the buffer.

            IntPtr inputBufferCurrentOffset = IntPtr.Zero;
            int tempScalarCount = inputLength;
            int tempSurrogatePairCount = 0;

            // If the sequence is long enough, try running vectorized "is this sequence ASCII?"
            // logic. We perform a small test of the first few bytes to make sure they're all
            // ASCII before we incur the cost of invoking the vectorized code path.

            if (Vector.IsHardwareAccelerated)
            {
                if (PlatformDependent.Is64BitProcess)
                {
                    // Test first 16 bytes and check for all-ASCII.
                    if ((inputLength >= 2 * sizeof(ulong) + 3 * Vector<byte>.Count) && QWordAllBytesAreAscii(ReadAndFoldTwoQWordsUnaligned(ref inputBuffer)))
                    {
                        inputBufferCurrentOffset = ConsumeAsciiBytesVectorized(ref Unsafe.Add(ref inputBuffer, 2 * sizeof(ulong)), inputLength - 2 * sizeof(ulong)) + 2 * sizeof(ulong);
                    }
                }
                else
                {
                    // Test first 8 bytes and check for all-ASCII.
                    if ((inputLength >= 2 * sizeof(uint) + 3 * Vector<byte>.Count) && DWordAllBytesAreAscii(ReadAndFoldTwoDWordsUnaligned(ref inputBuffer)))
                    {
                        inputBufferCurrentOffset = ConsumeAsciiBytesVectorized(ref Unsafe.Add(ref inputBuffer, 2 * sizeof(uint)), inputLength - 2 * sizeof(uint)) + 2 * sizeof(uint);
                    }
                }
            }

            int inputBufferRemainingBytes = inputLength - ConvertIntPtrToInt32WithoutOverflowCheck(inputBufferCurrentOffset);

            // Begin the main loop.

#if DEBUG
            long lastOffsetProcessed = -1; // used for invariant checking in debug builds
#endif

            while (inputBufferRemainingBytes >= sizeof(uint))
            {
                // Read 32 bits at a time. This is enough to hold any possible UTF8-encoded scalar.

                Debug.Assert(inputLength - (int)inputBufferCurrentOffset >= sizeof(uint));
                uint thisDWord = Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref inputBuffer, inputBufferCurrentOffset));

                AfterReadDWord:

#if DEBUG
                Debug.Assert(lastOffsetProcessed < (long)inputBufferCurrentOffset, "Algorithm should've made forward progress since last read.");
                lastOffsetProcessed = (long)inputBufferCurrentOffset;
#endif

                // First, check for the common case of all-ASCII bytes.

                if (DWordAllBytesAreAscii(thisDWord))
                {
                    // We read an all-ASCII sequence.

                    inputBufferCurrentOffset += 4;
                    inputBufferRemainingBytes -= 4;

                    // If we saw a sequence of all ASCII, there's a good chance a significant amount of following data is also ASCII.
                    // Below is basically unrolled loops with poor man's vectorization.

                    if (inputBufferRemainingBytes >= 5 * sizeof(uint))
                    {
                        // The JIT produces better codegen for aligned reads than it does for
                        // unaligned reads, and we want the processor to operate at maximum
                        // efficiency in the loop that follows, so we'll align the references
                        // now. It's OK to do this without pinning because the GC will never
                        // move a heap-allocated object in a manner that messes with its
                        // alignment.

                        {
                            ref byte refToCurrentDWord = ref Unsafe.Add(ref inputBuffer, inputBufferCurrentOffset);
                            thisDWord = Unsafe.ReadUnaligned<uint>(ref refToCurrentDWord);
                            if (!DWordAllBytesAreAscii(thisDWord))
                            {
                                goto AfterReadDWordSkipAllBytesAsciiCheck;
                            }

                            int adjustment = GetNumberOfBytesToNextDWordAlignment(ref refToCurrentDWord);
                            inputBufferCurrentOffset += adjustment;
                            // will adjust 'bytes remaining' value after below loop
                        }

                        // At this point, the input buffer offset points to an aligned DWORD.
                        // We also know that there's enough room to read at least four DWORDs from the stream.

                        IntPtr inputBufferFinalOffsetAtWhichCanSafelyLoop = (IntPtr)(inputLength - 4 * sizeof(uint));
                        do
                        {
                            ref uint currentReadPosition = ref Unsafe.As<byte, uint>(ref Unsafe.Add(ref inputBuffer, inputBufferCurrentOffset));

                            if (!DWordAllBytesAreAscii(currentReadPosition | Unsafe.Add(ref currentReadPosition, 1)))
                            {
                                goto LoopTerminatedEarlyDueToNonAsciiData;
                            }

                            if (!DWordAllBytesAreAscii(Unsafe.Add(ref currentReadPosition, 2) | Unsafe.Add(ref currentReadPosition, 3)))
                            {
                                inputBufferCurrentOffset += 2 * sizeof(uint);
                                goto LoopTerminatedEarlyDueToNonAsciiData;
                            }

                            inputBufferCurrentOffset += 4 * sizeof(uint);
                        } while (IntPtrIsLessThanOrEqualTo(inputBufferCurrentOffset, inputBufferFinalOffsetAtWhichCanSafelyLoop));

                        inputBufferRemainingBytes = inputLength - ConvertIntPtrToInt32WithoutOverflowCheck(inputBufferCurrentOffset);
                        continue; // need to perform a bounds check because we might be running out of data

                        LoopTerminatedEarlyDueToNonAsciiData:

                        // We know that there's *at least* two DWORDs of data remaining in the buffer.
                        // We also know that one of them (or both of them) contains non-ASCII data somewhere.
                        // Let's perform a quick check here to bypass the logic at the beginning of the main loop.

                        thisDWord = Unsafe.As<byte, uint>(ref Unsafe.Add(ref inputBuffer, inputBufferCurrentOffset));
                        if (DWordAllBytesAreAscii(thisDWord))
                        {
                            inputBufferCurrentOffset += 4;
                            thisDWord = Unsafe.As<byte, uint>(ref Unsafe.Add(ref inputBuffer, inputBufferCurrentOffset));
                        }

                        inputBufferRemainingBytes = inputLength - ConvertIntPtrToInt32WithoutOverflowCheck(inputBufferCurrentOffset);
                        goto AfterReadDWordSkipAllBytesAsciiCheck;
                    }

                    continue;
                }

                AfterReadDWordSkipAllBytesAsciiCheck:

                Debug.Assert(!DWordAllBytesAreAscii(thisDWord)); // this should have been handled earlier

                // Next, try stripping off ASCII bytes one at a time.
                // We only handle up to three ASCII bytes here since we handled the four ASCII byte case above.

                {
                    uint numLeadingAsciiBytes = CountNumberOfLeadingAsciiBytesFrom24BitInteger(thisDWord);
                    inputBufferCurrentOffset += (int)numLeadingAsciiBytes;
                    inputBufferRemainingBytes -= (int)numLeadingAsciiBytes;

                    if (inputBufferRemainingBytes < sizeof(uint))
                    {
                        goto ProcessRemainingBytesSlow; // Input buffer doesn't contain enough data to read a DWORD
                    }
                    else
                    {
                        // The input buffer at the current offset contains a non-ASCII byte.
                        // Read an entire DWORD and fall through to multi-byte consumption logic.
                        thisDWord = Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref inputBuffer, inputBufferCurrentOffset));
                    }
                }

                // At this point, we know we're working with a multi-byte code unit,
                // but we haven't yet validated it.

                // The masks and comparands are derived from the Unicode Standard, Table 3-6.
                // Additionally, we need to check for valid byte sequences per Table 3-7.

                // Check the 2-byte case.

                BeforeProcessTwoByteSequence:

                if (DWordBeginsWithUtf8TwoByteMask(thisDWord))
                {
                    // Per Table 3-7, valid sequences are:
                    // [ C2..DF ] [ 80..BF ]

                    if (DWordBeginsWithOverlongUtf8TwoByteSequence(thisDWord)) { goto Error; }

                    ProcessTwoByteSequenceSkipOverlongFormCheck:

                    // Optimization: If this is a two-byte-per-character language like Cyrillic or Hebrew,
                    // there's a good chance that if we see one two-byte run then there's another two-byte
                    // run immediately after. Let's check that now.

                    // On little-endian platforms, we can check for the two-byte UTF8 mask *and* validate that
                    // the value isn't overlong using a single comparison. On big-endian platforms, we'll need
                    // to validate the mask and validate that the sequence isn't overlong as two separate comparisons.

                    if ((BitConverter.IsLittleEndian && DWordEndsWithValidUtf8TwoByteSequenceLittleEndian(thisDWord))
                        || (!BitConverter.IsLittleEndian && (DWordEndsWithUtf8TwoByteMask(thisDWord) && !DWordEndsWithOverlongUtf8TwoByteSequence(thisDWord))))
                    {
                        ConsumeTwoAdjacentKnownGoodTwoByteSequences:

                        // We have two runs of two bytes each.
                        inputBufferCurrentOffset += 4;
                        inputBufferRemainingBytes -= 4;
                        tempScalarCount -= 2; // 4 bytes -> 2 scalars

                        if (inputBufferRemainingBytes >= sizeof(uint))
                        {
                            // Optimization: If we read a long run of two-byte sequences, the next sequence is probably
                            // also two bytes. Check for that first before going back to the beginning of the loop.

                            thisDWord = Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref inputBuffer, inputBufferCurrentOffset));

                            if (BitConverter.IsLittleEndian)
                            {
                                if (DWordBeginsWithValidUtf8TwoByteSequenceLittleEndian(thisDWord))
                                {
                                    // The next sequence is a valid two-byte sequence.
                                    goto ProcessTwoByteSequenceSkipOverlongFormCheck;
                                }
                            }
                            else
                            {
                                if (DWordBeginsAndEndsWithUtf8TwoByteMask(thisDWord))
                                {
                                    if (DWordBeginsWithOverlongUtf8TwoByteSequence(thisDWord) || DWordEndsWithOverlongUtf8TwoByteSequence(thisDWord))
                                    {
                                        // Mask said it was 2x 2-byte sequences but validation failed, go to beginning of loop for error handling
                                        goto AfterReadDWord;
                                    }
                                    else
                                    {
                                        // Validated next bytes are 2x 2-byte sequences
                                        goto ConsumeTwoAdjacentKnownGoodTwoByteSequences;
                                    }
                                }
                                else if (DWordBeginsWithUtf8TwoByteMask(thisDWord))
                                {
                                    if (DWordBeginsWithOverlongUtf8TwoByteSequence(thisDWord))
                                    {
                                        // Mask said it was a 2-byte sequence but validation failed
                                        goto Error;
                                    }
                                    else
                                    {
                                        // Validated next bytes are a single 2-byte sequence with no valid 2-byte sequence following
                                        goto ConsumeSingleKnownGoodTwoByteSequence;
                                    }
                                }
                            }

                            // If we reached this point, the next sequence is something other than a valid
                            // two-byte sequence, so go back to the beginning of the loop.
                            goto AfterReadDWord;
                        }
                        else
                        {
                            goto ProcessRemainingBytesSlow; // Running out of data - go down slow path
                        }
                    }

                    ConsumeSingleKnownGoodTwoByteSequence:

                    // The buffer contains a 2-byte sequence followed by 2 bytes that aren't a 2-byte sequence.
                    // Unlikely that a 3-byte sequence would follow a 2-byte sequence, so perhaps remaining
                    // bytes are ASCII?

                    if (DWordThirdByteIsAscii(thisDWord))
                    {
                        if (DWordFourthByteIsAscii(thisDWord))
                        {
                            inputBufferCurrentOffset += 4; // a 2-byte sequence + 2 ASCII bytes
                            inputBufferRemainingBytes -= 4; // a 2-byte sequence + 2 ASCII bytes
                            tempScalarCount--; // 2-byte sequence + 2 ASCII bytes -> 3 scalars
                        }
                        else
                        {
                            inputBufferCurrentOffset += 3; // a 2-byte sequence + 1 ASCII byte
                            inputBufferRemainingBytes -= 3; // a 2-byte sequence + 1 ASCII byte
                            tempScalarCount--; // 2-byte sequence + 1 ASCII bytes -> 2 scalars

                            // A two-byte sequence followed by an ASCII byte followed by a non-ASCII byte.
                            // Read in the next DWORD and jump directly to the start of the multi-byte processing block.

                            if (inputBufferRemainingBytes >= sizeof(uint))
                            {
                                thisDWord = Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref inputBuffer, inputBufferCurrentOffset));
                                goto BeforeProcessTwoByteSequence;
                            }
                        }
                    }
                    else
                    {
                        inputBufferCurrentOffset += 2;
                        inputBufferRemainingBytes -= 2;
                        tempScalarCount--; // 2-byte sequence -> 1 scalar
                    }

                    continue;
                }

                // Check the 3-byte case.

                if (DWordBeginsWithUtf8ThreeByteMask(thisDWord))
                {
                    ProcessThreeByteSequenceWithCheck:

                    // We need to check for overlong or surrogate three-byte sequences.
                    //
                    // Per Table 3-7, valid sequences are:
                    // [   E0   ] [ A0..BF ] [ 80..BF ]
                    // [ E1..EC ] [ 80..BF ] [ 80..BF ]
                    // [   ED   ] [ 80..9F ] [ 80..BF ]
                    // [ EE..EF ] [ 80..BF ] [ 80..BF ]
                    //
                    // Big-endian examples of using the above validation table:
                    // E0A0 = 1110 0000 1010 0000 => invalid (overlong ) patterns are 1110 0000 100# ####
                    // ED9F = 1110 1101 1001 1111 => invalid (surrogate) patterns are 1110 1101 101# ####
                    // If using the bitmask ......................................... 0000 1111 0010 0000 (=0F20),
                    // Then invalid (overlong) patterns match the comparand ......... 0000 0000 0000 0000 (=0000),
                    // And invalid (surrogate) patterns match the comparand ......... 0000 1101 0010 0000 (=0D20).

                    if (BitConverter.IsLittleEndian)
                    {
                        // The "overlong or surrogate" check can be implemented using a single jump, but there's
                        // some overhead to moving the bits into the correct locations in order to perform the
                        // correct comparison, and in practice the processor's branch prediction capability is
                        // good enough that we shouldn't bother. So we'll use two jumps instead.

                        // Can't extract this check into its own helper method because JITter produces suboptimal
                        // assembly, even with aggressive inlining.

                        uint comparand = thisDWord & 0x0000200FU;
                        if ((comparand == 0U) || (comparand == 0x0000200DU)) { goto Error; }
                    }
                    else
                    {
                        uint comparand = thisDWord & 0x0F200000U;
                        if ((comparand == 0U) || (comparand == 0x0D200000U)) { goto Error; }
                    }

                    ProcessSingleThreeByteSequenceSkipOverlongAndSurrogateChecks:

                    inputBufferCurrentOffset += 3;
                    inputBufferRemainingBytes -= 3;
                    tempScalarCount -= 2; // 3 bytes -> 1 scalar

                    // Occasionally one-off ASCII characters like spaces, periods, or newlines will make their way
                    // in to the text. If this happens strip it off now before seeing if the next character
                    // consists of three code units.

                    if (DWordFourthByteIsAscii(thisDWord))
                    {
                        inputBufferCurrentOffset += 1;
                        inputBufferRemainingBytes--;
                    }

                    SuccessfullyProcessedThreeByteSequence:

                    // Optimization: A three-byte character could indicate CJK text, which makes it likely
                    // that the character following this one is also CJK. We'll try to process several
                    // three-byte sequences at a time.

                    if (PlatformDependent.Is64BitProcess && BitConverter.IsLittleEndian && inputBufferRemainingBytes >= (sizeof(ulong) + 1))
                    {
                        ulong thisQWord = Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref inputBuffer, inputBufferCurrentOffset));

                        // Is this three 3-byte sequences in a row?
                        // thisQWord = [ 10yyyyyy 1110zzzz | 10xxxxxx 10yyyyyy 1110zzzz | 10xxxxxx 10yyyyyy 1110zzzz ] [ 10xxxxxx ]
                        //               ---- CHAR 3  ----   --------- CHAR 2 ---------   --------- CHAR 1 ---------     -CHAR 3-
                        if ((thisQWord & 0xC0F0C0C0F0C0C0F0UL) == 0x80E08080E08080E0UL && IsUtf8ContinuationByte(Unsafe.Add(ref inputBuffer, inputBufferCurrentOffset + sizeof(ulong))))
                        {
                            // Saw a proper bitmask for three incoming 3-byte sequences, perform the
                            // overlong and surrogate sequence checking now.

                            // Check the first character.
                            // If the first character is overlong or a surrogate, fail immediately.

                            uint comparand = (uint)thisQWord & 0x200FU;
                            if ((comparand == 0UL) || (comparand == 0x200DU))
                            {
                                goto Error;
                            }

                            // Check the second character.
                            // If this character is overlong or a surrogate, process the first character (which we
                            // know to be good because the first check passed) before reporting an error.

                            comparand = (uint)(thisQWord >> 24) & 0x200FU;
                            if ((comparand == 0U) || (comparand == 0x200DU))
                            {
                                thisDWord = (uint)thisQWord;
                                goto ProcessSingleThreeByteSequenceSkipOverlongAndSurrogateChecks;
                            }

                            // Check the third character (we already checked that it's followed by a continuation byte).
                            // If this character is overlong or a surrogate, process the first character (which we
                            // know to be good because the first check passed) before reporting an error.

                            comparand = (uint)(thisQWord >> 48) & 0x200FU;
                            if ((comparand == 0U) || (comparand == 0x200DU))
                            {
                                thisDWord = (uint)thisQWord;
                                goto ProcessSingleThreeByteSequenceSkipOverlongAndSurrogateChecks;
                            }

                            inputBufferCurrentOffset += 9;
                            inputBufferRemainingBytes -= 9;
                            tempScalarCount -= 6; // 9 bytes -> 3 scalars
                            goto SuccessfullyProcessedThreeByteSequence;
                        }

                        // Is this two 3-byte sequences in a row?
                        // thisQWord = [ ######## ######## | 10xxxxxx 10yyyyyy 1110zzzz | 10xxxxxx 10yyyyyy 1110zzzz ]
                        //                                   --------- CHAR 2 ---------   --------- CHAR 1 ---------
                        if ((thisQWord & 0xC0C0F0C0C0F0UL) == 0x8080E08080E0UL)
                        {
                            // Saw a proper bitmask for two incoming 3-byte sequences, perform the
                            // overlong and surrogate sequence checking now.

                            // Check the first character.
                            // If the first character is overlong or a surrogate, fail immediately.

                            uint comparand = (uint)thisQWord & 0x200FU;
                            if ((comparand == 0UL) || (comparand == 0x200DU))
                            {
                                goto Error;
                            }

                            // Check the second character.
                            // If this character is overlong or a surrogate, process the first character (which we
                            // know to be good because the first check passed) before reporting an error.

                            comparand = (uint)(thisQWord >> 24) & 0x200FU;
                            if ((comparand == 0U) || (comparand == 0x200DU))
                            {
                                thisDWord = (uint)thisQWord;
                                goto ProcessSingleThreeByteSequenceSkipOverlongAndSurrogateChecks;
                            }

                            inputBufferCurrentOffset += 6;
                            inputBufferRemainingBytes -= 6;
                            tempScalarCount -= 4; // 6 bytes -> 2 scalars

                            // The next char in the sequence didn't have a 3-byte marker, so it's probably
                            // an ASCII character. Jump back to the beginning of loop processing.
                            continue;
                        }

                        thisDWord = (uint)thisQWord;
                        if (DWordBeginsWithUtf8ThreeByteMask(thisDWord))
                        {
                            // A single three-byte sequence.
                            goto ProcessThreeByteSequenceWithCheck;
                        }
                        else
                        {
                            // Not a three-byte sequence; perhaps ASCII?
                            goto AfterReadDWord;
                        }
                    }

                    if (inputBufferRemainingBytes >= sizeof(uint))
                    {
                        thisDWord = Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref inputBuffer, inputBufferCurrentOffset));

                        // Optimization: A three-byte character could indicate CJK text, which makes it likely
                        // that the character following this one is also CJK. We'll check for a three-byte sequence
                        // marker now and jump directly to three-byte sequence processing if we see one, skipping
                        // all of the logic at the beginning of the loop.

                        if (DWordBeginsWithUtf8ThreeByteMask(thisDWord))
                        {
                            goto ProcessThreeByteSequenceWithCheck; // Found another [not yet validated] three-byte sequence; process
                        }
                        else
                        {
                            goto AfterReadDWord; // Probably ASCII punctuation or whitespace; go back to start of loop
                        }
                    }
                    else
                    {
                        goto ProcessRemainingBytesSlow; // Running out of data
                    }
                }

                // Assume the 4-byte case, but we need to validate.

                {
                    // We need to check for overlong or invalid (over U+10FFFF) four-byte sequences.
                    //
                    // Per Table 3-7, valid sequences are:
                    // [   F0   ] [ 90..BF ] [ 80..BF ] [ 80..BF ]
                    // [ F1..F3 ] [ 80..BF ] [ 80..BF ] [ 80..BF ]
                    // [   F4   ] [ 80..8F ] [ 80..BF ] [ 80..BF ]

                    if (!DWordBeginsWithUtf8FourByteMask(thisDWord)) { goto Error; }

                    // Now check for overlong / out-of-range sequences.

                    if (BitConverter.IsLittleEndian)
                    {
                        // The DWORD we read is [ 10xxxxxx 10yyyyyy 10zzzzzz 11110www ].
                        // We want to get the 'w' byte in front of the 'z' byte so that we can perform
                        // a single range comparison. We'll take advantage of the fact that the JITter
                        // can detect a ROR / ROL operation, then we'll just zero out the bytes that
                        // aren't involved in the range check.

                        uint toCheck = (ushort)thisDWord;

                        // At this point, toCheck = [ 00000000 00000000 10zzzzzz 11110www ].

                        toCheck = (toCheck << 24) | (toCheck >> 8); // ROR 8 / ROL 24

                        // At this point, toCheck = [ 11110www 00000000 00000000 10zzzzzz ].

                        if (!IsInRangeInclusive(toCheck, 0xF0000090U, 0xF400008FU)) { goto Error; }
                    }
                    else
                    {
                        if (!IsInRangeInclusive(thisDWord, 0xF0900000U, 0xF48FFFFFU)) { goto Error; }
                    }

                    // Validation complete.

                    inputBufferCurrentOffset += 4;
                    inputBufferRemainingBytes -= 4;
                    tempScalarCount -= 3; // 4 bytes -> 1 scalar
                    tempSurrogatePairCount++; // 4 bytes implies UTF16 surrogate pair

                    continue; // go back to beginning of loop for processing
                }
            }

            ProcessRemainingBytesSlow:

            Debug.Assert(inputBufferRemainingBytes < 4);
            while (inputBufferRemainingBytes > 0)
            {
                uint firstByte = Unsafe.Add(ref inputBuffer, inputBufferCurrentOffset);

                if (firstByte < 0x80U)
                {
                    // 1-byte (ASCII) case
                    inputBufferCurrentOffset += 1;
                    inputBufferRemainingBytes -= 1;
                    continue;
                }
                else if (inputBufferRemainingBytes >= 2)
                {
                    uint secondByte = Unsafe.Add(ref inputBuffer, inputBufferCurrentOffset + 1);
                    if (firstByte < 0xE0U)
                    {
                        // 2-byte case
                        if (firstByte >= 0xC2U && IsUtf8ContinuationByte(secondByte))
                        {
                            inputBufferCurrentOffset += 2;
                            inputBufferRemainingBytes -= 2;
                            tempScalarCount--; // 2 bytes -> 1 scalar
                            continue;
                        }
                    }
                    else if (inputBufferRemainingBytes >= 3)
                    {
                        uint thirdByte = Unsafe.Add(ref inputBuffer, inputBufferCurrentOffset + 2);
                        if (firstByte <= 0xF0U)
                        {
                            if (firstByte == 0xE0U)
                            {
                                if (!IsInRangeInclusive(secondByte, 0xA0U, 0xBFU)) { goto Error; }
                            }
                            else if (firstByte == 0xEDU)
                            {
                                if (!IsInRangeInclusive(secondByte, 0x80U, 0x9FU)) { goto Error; }
                            }
                            else
                            {
                                if (!IsUtf8ContinuationByte(secondByte)) { goto Error; }
                            }

                            if (IsUtf8ContinuationByte(thirdByte))
                            {
                                inputBufferCurrentOffset += 3;
                                inputBufferRemainingBytes -= 3;
                                tempScalarCount -= 2; // 3 bytes -> 1 scalar
                                continue;
                            }
                        }
                    }
                }

                // Error - no match.

                goto Error;
            }

            // If we reached this point, we're out of data, and we saw no bad UTF8 sequence.

            scalarCount = tempScalarCount;
            surrogatePairCount = tempSurrogatePairCount;
            return -1;

            // Error handling logic.

            Error:

            scalarCount = tempScalarCount - inputBufferRemainingBytes; // we assumed earlier each byte corresponded to a single scalar, perform fixup now to account for unread bytes
            surrogatePairCount = tempSurrogatePairCount;
            return ConvertIntPtrToInt32WithoutOverflowCheck(inputBufferCurrentOffset);
        }
    }
}
#endif
