// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Common.Internal
{
    using System;
    using System.Diagnostics;
    using System.Numerics;
    using System.Runtime.CompilerServices;
#if NETCOREAPP_3_0_GREATER
    using System.Runtime.Intrinsics;
    using System.Runtime.Intrinsics.X86;
#endif

    public static partial class SpanHelpers
    {
        #region -- Contains --

        public static unsafe bool Contains(ref char searchSpace, char value, int length)
        {
            Debug.Assert(length >= 0);

#if NETCOREAPP_3_0_GREATER
            int index = IndexOf(ref searchSpace, value, length);
            return SharedConstants.TooBigOrNegative >= (uint)index;
#else
            fixed (char* pChars = &searchSpace)
            {
                char* pCh = pChars;
                char* pEndCh = pCh + length;

                if (Vector.IsHardwareAccelerated && length >= Vector<ushort>.Count * 2)
                {
                    // Figure out how many characters to read sequentially until we are vector aligned
                    // This is equivalent to:
                    //         unaligned = ((int)pCh % Unsafe.SizeOf<Vector<ushort>>()) / elementsPerByte
                    //         length = (Vector<ushort>.Count - unaligned) % Vector<ushort>.Count
                    const int elementsPerByte = sizeof(ushort) / sizeof(byte);
                    int unaligned = ((int)pCh & (Unsafe.SizeOf<Vector<ushort>>() - 1)) / elementsPerByte;
                    length = (Vector<ushort>.Count - unaligned) & (Vector<ushort>.Count - 1);
                }

            SequentialScan:
                while (length >= 4)
                {
                    length -= 4;

                    if (value == *pCh ||
                        value == *(pCh + 1) ||
                        value == *(pCh + 2) ||
                        value == *(pCh + 3))
                    {
                        goto Found;
                    }

                    pCh += 4;
                }

                while (length > 0)
                {
                    length -= 1;

                    if (value == *pCh)
                        goto Found;

                    pCh += 1;
                }

                // We get past SequentialScan only if IsHardwareAccelerated is true. However, we still have the redundant check to allow
                // the JIT to see that the code is unreachable and eliminate it when the platform does not have hardware accelerated.
                if (Vector.IsHardwareAccelerated && pCh < pEndCh)
                {
                    // Get the highest multiple of Vector<ushort>.Count that is within the search space.
                    // That will be how many times we iterate in the loop below.
                    // This is equivalent to: length = Vector<ushort>.Count * ((int)(pEndCh - pCh) / Vector<ushort>.Count)
                    length = (int)((pEndCh - pCh) & ~(Vector<ushort>.Count - 1));

                    // Get comparison Vector
                    Vector<ushort> vComparison = new Vector<ushort>(value);

                    while (length > 0)
                    {
                        // Using Unsafe.Read instead of ReadUnaligned since the search space is pinned and pCh is always vector aligned
                        Debug.Assert(((int)pCh & (Unsafe.SizeOf<Vector<ushort>>() - 1)) == 0);
                        Vector<ushort> vMatches = Vector.Equals(vComparison, Unsafe.Read<Vector<ushort>>(pCh));
                        if (Vector<ushort>.Zero.Equals(vMatches))
                        {
                            pCh += Vector<ushort>.Count;
                            length -= Vector<ushort>.Count;
                            continue;
                        }

                        goto Found;
                    }

                    if (pCh < pEndCh)
                    {
                        length = (int)(pEndCh - pCh);
                        goto SequentialScan;
                    }
                }

                return false;

            Found:
                return true;
            }
#endif
        }

        #endregion

        #region -- SequenceEqual --

        public static bool SequenceEqual(ref char first, ref char second, int length)
        {
            Debug.Assert(length >= 0);

            if (Unsafe.AreSame(ref first, ref second)) { goto Equal; }

            IntPtr index = (IntPtr)0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            char lookUp0;
            char lookUp1;
            while (length >= 8)
            {
                length -= 8;

                lookUp0 = Unsafe.Add(ref first, index);
                lookUp1 = Unsafe.Add(ref second, index);
                if (lookUp0 != lookUp1) { goto NotEqual; }
                lookUp0 = Unsafe.Add(ref first, index + 1);
                lookUp1 = Unsafe.Add(ref second, index + 1);
                if (lookUp0 != lookUp1) { goto NotEqual; }
                lookUp0 = Unsafe.Add(ref first, index + 2);
                lookUp1 = Unsafe.Add(ref second, index + 2);
                if (lookUp0 != lookUp1) { goto NotEqual; }
                lookUp0 = Unsafe.Add(ref first, index + 3);
                lookUp1 = Unsafe.Add(ref second, index + 3);
                if (lookUp0 != lookUp1) { goto NotEqual; }
                lookUp0 = Unsafe.Add(ref first, index + 4);
                lookUp1 = Unsafe.Add(ref second, index + 4);
                if (lookUp0 != lookUp1) { goto NotEqual; }
                lookUp0 = Unsafe.Add(ref first, index + 5);
                lookUp1 = Unsafe.Add(ref second, index + 5);
                if (lookUp0 != lookUp1) { goto NotEqual; }
                lookUp0 = Unsafe.Add(ref first, index + 6);
                lookUp1 = Unsafe.Add(ref second, index + 6);
                if (lookUp0 != lookUp1) { goto NotEqual; }
                lookUp0 = Unsafe.Add(ref first, index + 7);
                lookUp1 = Unsafe.Add(ref second, index + 7);
                if (lookUp0 != lookUp1) { goto NotEqual; }

                index += 8;
            }

            if (length >= 4)
            {
                length -= 4;

                lookUp0 = Unsafe.Add(ref first, index);
                lookUp1 = Unsafe.Add(ref second, index);
                if (lookUp0 != lookUp1) { goto NotEqual; }
                lookUp0 = Unsafe.Add(ref first, index + 1);
                lookUp1 = Unsafe.Add(ref second, index + 1);
                if (lookUp0 != lookUp1) { goto NotEqual; }
                lookUp0 = Unsafe.Add(ref first, index + 2);
                lookUp1 = Unsafe.Add(ref second, index + 2);
                if (lookUp0 != lookUp1) { goto NotEqual; }
                lookUp0 = Unsafe.Add(ref first, index + 3);
                lookUp1 = Unsafe.Add(ref second, index + 3);
                if (lookUp0 != lookUp1) { goto NotEqual; }

                index += 4;
            }

            while (length > 0)
            {
                lookUp0 = Unsafe.Add(ref first, index);
                lookUp1 = Unsafe.Add(ref second, index);
                if (lookUp0 != lookUp1) { goto NotEqual; }
                index += 1;
                length--;
            }

        Equal:
            return true;

        NotEqual: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return false;
        }

        #endregion

        #region -- SequenceCompareTo --

        //[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe int SequenceCompareTo(ref char first, int firstLength, ref char second, int secondLength)
        {
            Debug.Assert(firstLength >= 0);
            Debug.Assert(secondLength >= 0);

            int lengthDelta = firstLength - secondLength;

            if (Unsafe.AreSame(ref first, ref second))
                goto Equal;

            IntPtr minLength = (IntPtr)((firstLength < secondLength) ? firstLength : secondLength);
            IntPtr i = (IntPtr)0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations

            if ((byte*)minLength >= (byte*)(sizeof(UIntPtr) / sizeof(char)))
            {
                if (Vector.IsHardwareAccelerated && (byte*)minLength >= (byte*)Vector<ushort>.Count)
                {
                    IntPtr nLength = minLength - Vector<ushort>.Count;
                    do
                    {
                        if (Unsafe.ReadUnaligned<Vector<ushort>>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref first, i))) !=
                            Unsafe.ReadUnaligned<Vector<ushort>>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref second, i))))
                        {
                            break;
                        }
                        i += Vector<ushort>.Count;
                    }
                    while ((byte*)nLength >= (byte*)i);
                }

                while ((byte*)minLength >= (byte*)(i + sizeof(UIntPtr) / sizeof(char)))
                {
                    if (Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref first, i))) !=
                        Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref second, i))))
                    {
                        break;
                    }
                    i += sizeof(UIntPtr) / sizeof(char);
                }
            }

            if (sizeof(UIntPtr) > sizeof(int) && (byte*)minLength >= (byte*)(i + sizeof(int) / sizeof(char)))
            {
                if (Unsafe.ReadUnaligned<int>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref first, i))) ==
                    Unsafe.ReadUnaligned<int>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref second, i))))
                {
                    i += sizeof(int) / sizeof(char);
                }
            }

            while ((byte*)i < (byte*)minLength)
            {
                int result = Unsafe.Add(ref first, i).CompareTo(Unsafe.Add(ref second, i));
                if (result != 0)
                    return result;
                i += 1;
            }

        Equal:
            return lengthDelta;
        }

        #endregion

        #region -- IndexOf --

        public static int IndexOf(ref char searchSpace, int searchSpaceLength, ref char value, int valueLength)
        {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            uint uValueLength = (uint)valueLength;
            if (0u >= uValueLength)
            {
                return 0;  // A zero-length sequence is always treated as "found" at the start of the search space.
            }
            if (1u >= uValueLength)
            {
                return IndexOf(ref searchSpace, value, searchSpaceLength);
            }

            char valueHead = value;
            ref char valueTail = ref Unsafe.Add(ref value, 1);
            int valueTailLength = valueLength - 1;
            int remainingSearchSpaceLength = searchSpaceLength - valueTailLength;

            int index = 0;
            while (remainingSearchSpaceLength > 0)
            {
                // Do a quick search for the first element of "value".
                int relativeIndex = IndexOf(ref Unsafe.Add(ref searchSpace, index), valueHead, remainingSearchSpaceLength);
                if (relativeIndex == -1)
                    break;

                remainingSearchSpaceLength -= relativeIndex;
                index += relativeIndex;

                if ((uint)(remainingSearchSpaceLength - 1) > SharedConstants.TooBigOrNegative) // <= 0
                    break;  // The unsearched portion is now shorter than the sequence we're looking for. So it can't be there.

                // Found the first element of "value". See if the tail matches.
                if (SequenceEqual(
                    ref Unsafe.As<char, byte>(ref Unsafe.Add(ref searchSpace, index + 1)),
                    ref Unsafe.As<char, byte>(ref valueTail),
                    (nint)valueTailLength * 2)) // nuint
                {
                    return index;  // The tail matched. Return a successful find.
                }

                remainingSearchSpaceLength--;
                index++;
            }
            return -1;
        }

        public static unsafe int IndexOf(ref char searchSpace, char value, int length)
        {
            Debug.Assert(length >= 0);

#if NETCOREAPP_3_0_GREATER
            nint offset = 0;
            nint lengthToExamine = length;

            if (((int)Unsafe.AsPointer(ref searchSpace) & 1) != 0)
            {
                // Input isn't char aligned, we won't be able to align it to a Vector
            }
            else if (Sse2.IsSupported)
            {
                // Avx2 branch also operates on Sse2 sizes, so check is combined.
                // Needs to be double length to allow us to align the data first.
                if (length >= Vector128<ushort>.Count * 2)
                {
                    lengthToExamine = UnalignedCountVector128(ref searchSpace);
                }
            }
            else if (Vector.IsHardwareAccelerated)
            {
                // Needs to be double length to allow us to align the data first.
                if (length >= Vector<ushort>.Count * 2)
                {
                    lengthToExamine = UnalignedCountVector(ref searchSpace);
                }
            }

        SequentialScan:
            // In the non-vector case lengthToExamine is the total length.
            // In the vector case lengthToExamine first aligns to Vector,
            // then in a second pass after the Vector lengths is the 
            // remaining data that is shorter than a Vector length.
            while (lengthToExamine >= 4)
            {
                ref char current = ref Add(ref searchSpace, offset);

                if (value == current)
                    goto Found;
                if (value == Add(ref current, 1))
                    goto Found1;
                if (value == Add(ref current, 2))
                    goto Found2;
                if (value == Add(ref current, 3))
                    goto Found3;

                offset += 4;
                lengthToExamine -= 4;
            }

            while (lengthToExamine > 0)
            {
                if (value == Add(ref searchSpace, offset))
                    goto Found;

                offset += 1;
                lengthToExamine -= 1;
            }

            // We get past SequentialScan only if IsHardwareAccelerated or intrinsic .IsSupported is true. However, we still have the redundant check to allow
            // the JIT to see that the code is unreachable and eliminate it when the platform does not have hardware accelerated.
            if (Avx2.IsSupported)
            {
                if (offset < length)
                {
                    Debug.Assert(length - offset >= Vector128<ushort>.Count);
                    if (((nint)Unsafe.AsPointer(ref Unsafe.Add(ref searchSpace, (IntPtr)offset)) & (nint)(Vector256<byte>.Count - 1)) != 0)
                    {
                        // Not currently aligned to Vector256 (is aligned to Vector128); this can cause a problem for searches
                        // with no upper bound e.g. String.wcslen. Start with a check on Vector128 to align to Vector256, 
                        // before moving to processing Vector256.

                        // If the input searchSpan has been fixed or pinned, this ensures we do not fault across memory pages 
                        // while searching for an end of string. Specifically that this assumes that the length is either correct 
                        // or that the data is pinned otherwise it may cause an AccessViolation from crossing a page boundary into an 
                        // unowned page. If the search is unbounded (e.g. null terminator in wcslen) and the search value is not found,
                        // again this will likely cause an AccessViolation. However, correctly bounded searches will return -1 rather 
                        // than ever causing an AV.

                        // If the searchSpan has not been fixed or pinned the GC can relocate it during the execution of this 
                        // method, so the alignment only acts as best endeavour. The GC cost is likely to dominate over
                        // the misalignment that may occur after; to we default to giving the GC a free hand to relocate and 
                        // its up to the caller whether they are operating over fixed data.
                        Vector128<ushort> values = Vector128.Create((ushort)value);
                        Vector128<ushort> search = LoadVector128(ref searchSpace, offset);

                        // Same method as below
                        int matches = Sse2.MoveMask(Sse2.CompareEqual(values, search).AsByte());
                        if (0u >= (uint)matches)
                        {
                            // Zero flags set so no matches
                            offset += Vector128<ushort>.Count;
                        }
                        else
                        {
                            // Find bitflag offset of first match and add to current offset
                            return (int)(offset + (BitOperations.TrailingZeroCount(matches) / sizeof(char)));
                        }
                    }

                    lengthToExamine = GetCharVector256SpanLength(offset, length);
                    if (lengthToExamine > 0)
                    {
                        Vector256<ushort> values = Vector256.Create((ushort)value);
                        do
                        {
                            Debug.Assert(lengthToExamine >= Vector256<ushort>.Count);

                            Vector256<ushort> search = LoadVector256(ref searchSpace, offset);
                            int matches = Avx2.MoveMask(Avx2.CompareEqual(values, search).AsByte());
                            // Note that MoveMask has converted the equal vector elements into a set of bit flags,
                            // So the bit position in 'matches' corresponds to the element offset.
                            if (0u >= (uint)matches)
                            {
                                // Zero flags set so no matches
                                offset += Vector256<ushort>.Count;
                                lengthToExamine -= Vector256<ushort>.Count;
                                continue;
                            }

                            // Find bitflag offset of first match and add to current offset, 
                            // flags are in bytes so divide for chars
                            return (int)(offset + (BitOperations.TrailingZeroCount(matches) / sizeof(char)));
                        } while (lengthToExamine > 0);
                    }

                    lengthToExamine = GetCharVector128SpanLength(offset, length);
                    if (lengthToExamine > 0)
                    {
                        Debug.Assert(lengthToExamine >= Vector128<ushort>.Count);

                        Vector128<ushort> values = Vector128.Create((ushort)value);
                        Vector128<ushort> search = LoadVector128(ref searchSpace, offset);

                        // Same method as above
                        int matches = Sse2.MoveMask(Sse2.CompareEqual(values, search).AsByte());
                        if (0u >= (uint)matches)
                        {
                            // Zero flags set so no matches
                            offset += Vector128<ushort>.Count;
                            // Don't need to change lengthToExamine here as we don't use its current value again.
                        }
                        else
                        {
                            // Find bitflag offset of first match and add to current offset, 
                            // flags are in bytes so divide for chars
                            return (int)(offset + (BitOperations.TrailingZeroCount(matches) / sizeof(char)));
                        }
                    }

                    if (offset < length)
                    {
                        lengthToExamine = length - offset;
                        goto SequentialScan;
                    }
                }
            }
            else if (Sse2.IsSupported)
            {
                if (offset < length)
                {
                    Debug.Assert(length - offset >= Vector128<ushort>.Count);

                    lengthToExamine = GetCharVector128SpanLength(offset, length);
                    if (lengthToExamine > 0)
                    {
                        Vector128<ushort> values = Vector128.Create((ushort)value);
                        do
                        {
                            Debug.Assert(lengthToExamine >= Vector128<ushort>.Count);

                            Vector128<ushort> search = LoadVector128(ref searchSpace, offset);

                            // Same method as above
                            int matches = Sse2.MoveMask(Sse2.CompareEqual(values, search).AsByte());
                            if (0u >= (uint)matches)
                            {
                                // Zero flags set so no matches
                                offset += Vector128<ushort>.Count;
                                lengthToExamine -= Vector128<ushort>.Count;
                                continue;
                            }

                            // Find bitflag offset of first match and add to current offset, 
                            // flags are in bytes so divide for chars
                            return (int)(offset + (BitOperations.TrailingZeroCount(matches) / sizeof(char)));
                        } while (lengthToExamine > 0);
                    }

                    if (offset < length)
                    {
                        lengthToExamine = length - offset;
                        goto SequentialScan;
                    }
                }
            }
            else if (Vector.IsHardwareAccelerated && offset < length)
            {
                Debug.Assert(length - offset >= Vector<ushort>.Count);

                lengthToExamine = GetCharVectorSpanLength(offset, length);

                if (lengthToExamine > 0)
                {
                    Vector<ushort> values = new Vector<ushort>((ushort)value);
                    do
                    {
                        Debug.Assert(lengthToExamine >= Vector<ushort>.Count);

                        var matches = Vector.Equals(values, LoadVector(ref searchSpace, offset));
                        if (Vector<ushort>.Zero.Equals(matches))
                        {
                            offset += Vector<ushort>.Count;
                            lengthToExamine -= Vector<ushort>.Count;
                            continue;
                        }

                        // Find offset of first match
                        return (int)(offset + LocateFirstFoundChar(matches));
                    } while (lengthToExamine > 0);
                }

                if (offset < length)
                {
                    lengthToExamine = length - offset;
                    goto SequentialScan;
                }
            }
            return -1;
        Found3:
            return (int)(offset + 3);
        Found2:
            return (int)(offset + 2);
        Found1:
            return (int)(offset + 1);
        Found:
            return (int)(offset);
#else
            fixed (char* pChars = &searchSpace)
            {
                char* pCh = pChars;
                char* pEndCh = pCh + length;

                if (Vector.IsHardwareAccelerated && length >= Vector<ushort>.Count * 2)
                {
                    // Figure out how many characters to read sequentially until we are vector aligned
                    // This is equivalent to:
                    //         unaligned = ((int)pCh % Unsafe.SizeOf<Vector<ushort>>()) / elementsPerByte
                    //         length = (Vector<ushort>.Count - unaligned) % Vector<ushort>.Count
                    const int elementsPerByte = sizeof(ushort) / sizeof(byte);
                    int unaligned = ((int)pCh & (Unsafe.SizeOf<Vector<ushort>>() - 1)) / elementsPerByte;
                    length = (Vector<ushort>.Count - unaligned) & (Vector<ushort>.Count - 1);
                }

            SequentialScan:
                while (length >= 4)
                {
                    length -= 4;

                    if (pCh[0] == value)
                        goto Found;
                    if (pCh[1] == value)
                        goto Found1;
                    if (pCh[2] == value)
                        goto Found2;
                    if (pCh[3] == value)
                        goto Found3;

                    pCh += 4;
                }

                while (length > 0)
                {
                    length--;

                    if (pCh[0] == value)
                        goto Found;

                    pCh++;
                }

                // We get past SequentialScan only if IsHardwareAccelerated is true. However, we still have the redundant check to allow
                // the JIT to see that the code is unreachable and eliminate it when the platform does not have hardware accelerated.
                if (Vector.IsHardwareAccelerated && pCh < pEndCh)
                {
                    // Get the highest multiple of Vector<ushort>.Count that is within the search space.
                    // That will be how many times we iterate in the loop below.
                    // This is equivalent to: length = Vector<ushort>.Count * ((int)(pEndCh - pCh) / Vector<ushort>.Count)
                    length = (int)((pEndCh - pCh) & ~(Vector<ushort>.Count - 1));

                    // Get comparison Vector
                    Vector<ushort> vComparison = new Vector<ushort>(value);

                    while (length > 0)
                    {
                        // Using Unsafe.Read instead of ReadUnaligned since the search space is pinned and pCh is always vector aligned
                        Debug.Assert(((int)pCh & (Unsafe.SizeOf<Vector<ushort>>() - 1)) == 0);
                        Vector<ushort> vMatches = Vector.Equals(vComparison, Unsafe.Read<Vector<ushort>>(pCh));
                        if (Vector<ushort>.Zero.Equals(vMatches))
                        {
                            pCh += Vector<ushort>.Count;
                            length -= Vector<ushort>.Count;
                            continue;
                        }
                        // Find offset of first match
                        return (int)(pCh - pChars) + LocateFirstFoundChar(vMatches);
                    }

                    if (pCh < pEndCh)
                    {
                        length = (int)(pEndCh - pCh);
                        goto SequentialScan;
                    }
                }

                return -1;
            Found3:
                pCh++;
            Found2:
                pCh++;
            Found1:
                pCh++;
            Found:
                return (int)(pCh - pChars);
            }
#endif
        }

        #endregion

        #region -- IndexOfAny --

        public static int IndexOfAny(ref char searchSpace, int searchSpaceLength, ref char value, int valueLength)
        {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            switch ((uint)valueLength)
            {
                case 5u: // Length 5 is a common length for FileSystemName expression (", <, >, *, ?) and in preference to 2 as it has an explicit overload
                    return IndexOfAny(ref searchSpace, value,
                        Unsafe.Add(ref value, 1),
                        Unsafe.Add(ref value, 2),
                        Unsafe.Add(ref value, 3),
                        Unsafe.Add(ref value, 4),
                        searchSpaceLength);

                case 2u: // Length 2 is a common length for simple wildcards (*, ?),  directory separators (/, \), quotes (", '), brackets, etc
                    return IndexOfAny(ref searchSpace, value,
                        Unsafe.Add(ref value, 1),
                        searchSpaceLength);

                case 4u: // Length 4 before 3 as 3 has an explicit overload
                    return IndexOfAny(ref searchSpace, value,
                        Unsafe.Add(ref value, 1),
                        Unsafe.Add(ref value, 2),
                        Unsafe.Add(ref value, 3),
                        searchSpaceLength);

                case 3u:
                    return IndexOfAny(ref searchSpace, value,
                        Unsafe.Add(ref value, 1),
                        Unsafe.Add(ref value, 2),
                        searchSpaceLength);

                case 1u:
                    return IndexOf(ref searchSpace, value, searchSpaceLength);

                case 0u:
                    return -1;  // A zero-length set of values is always treated as "not found".

                default:
                    int index = -1;
                    for (int i = 0; i < valueLength; i++)
                    {
                        var tempIndex = IndexOf(ref searchSpace, Unsafe.Add(ref value, i), searchSpaceLength);
                        if ((uint)tempIndex < (uint)index)
                        {
                            index = tempIndex;
                            // Reduce space for search, cause we don't care if we find the search value after the index of a previously found value
                            searchSpaceLength = tempIndex;

                            if (0u >= (uint)index) { break; }
                        }
                    }
                    return index;
            }
        }

        public static unsafe int IndexOfAny(ref char searchSpace, char value0, char value1, int length)
        {
            Debug.Assert(length >= 0);

            fixed (char* pChars = &searchSpace)
            {
                char* pCh = pChars;
                char* pEndCh = pCh + length;

                if (Vector.IsHardwareAccelerated && length >= Vector<ushort>.Count * 2)
                {
                    // Figure out how many characters to read sequentially until we are vector aligned
                    // This is equivalent to:
                    //         unaligned = ((int)pCh % Unsafe.SizeOf<Vector<ushort>>()) / elementsPerByte
                    //         length = (Vector<ushort>.Count - unaligned) % Vector<ushort>.Count
                    const int elementsPerByte = sizeof(ushort) / sizeof(byte);
                    int unaligned = ((int)pCh & (Unsafe.SizeOf<Vector<ushort>>() - 1)) / elementsPerByte;
                    length = (Vector<ushort>.Count - unaligned) & (Vector<ushort>.Count - 1);
                }

            SequentialScan:
                while (length >= 4)
                {
                    length -= 4;

                    if (pCh[0] == value0 || pCh[0] == value1)
                        goto Found;
                    if (pCh[1] == value0 || pCh[1] == value1)
                        goto Found1;
                    if (pCh[2] == value0 || pCh[2] == value1)
                        goto Found2;
                    if (pCh[3] == value0 || pCh[3] == value1)
                        goto Found3;

                    pCh += 4;
                }

                while (length > 0)
                {
                    length--;

                    if (pCh[0] == value0 || pCh[0] == value1)
                        goto Found;

                    pCh++;
                }

                // We get past SequentialScan only if IsHardwareAccelerated is true. However, we still have the redundant check to allow
                // the JIT to see that the code is unreachable and eliminate it when the platform does not have hardware accelerated.
                if (Vector.IsHardwareAccelerated && pCh < pEndCh)
                {
                    // Get the highest multiple of Vector<ushort>.Count that is within the search space.
                    // That will be how many times we iterate in the loop below.
                    // This is equivalent to: length = Vector<ushort>.Count * ((int)(pEndCh - pCh) / Vector<ushort>.Count)
                    length = (int)((pEndCh - pCh) & ~(Vector<ushort>.Count - 1));

                    // Get comparison Vector
                    Vector<ushort> values0 = new Vector<ushort>(value0);
                    Vector<ushort> values1 = new Vector<ushort>(value1);

                    while (length > 0)
                    {
                        // Using Unsafe.Read instead of ReadUnaligned since the search space is pinned and pCh is always vector aligned
                        Debug.Assert(((int)pCh & (Unsafe.SizeOf<Vector<ushort>>() - 1)) == 0);
                        Vector<ushort> vData = Unsafe.Read<Vector<ushort>>(pCh);
                        var vMatches = Vector.BitwiseOr(
                                        Vector.Equals(vData, values0),
                                        Vector.Equals(vData, values1));
                        if (Vector<ushort>.Zero.Equals(vMatches))
                        {
                            pCh += Vector<ushort>.Count;
                            length -= Vector<ushort>.Count;
                            continue;
                        }
                        // Find offset of first match
                        return (int)(pCh - pChars) + LocateFirstFoundChar(vMatches);
                    }

                    if (pCh < pEndCh)
                    {
                        length = (int)(pEndCh - pCh);
                        goto SequentialScan;
                    }
                }

                return -1;
            Found3:
                pCh++;
            Found2:
                pCh++;
            Found1:
                pCh++;
            Found:
                return (int)(pCh - pChars);
            }
        }

        public static unsafe int IndexOfAny(ref char searchSpace, char value0, char value1, char value2, int length)
        {
            Debug.Assert(length >= 0);

            fixed (char* pChars = &searchSpace)
            {
                char* pCh = pChars;
                char* pEndCh = pCh + length;

                if (Vector.IsHardwareAccelerated && length >= Vector<ushort>.Count * 2)
                {
                    // Figure out how many characters to read sequentially until we are vector aligned
                    // This is equivalent to:
                    //         unaligned = ((int)pCh % Unsafe.SizeOf<Vector<ushort>>()) / elementsPerByte
                    //         length = (Vector<ushort>.Count - unaligned) % Vector<ushort>.Count
                    const int elementsPerByte = sizeof(ushort) / sizeof(byte);
                    int unaligned = ((int)pCh & (Unsafe.SizeOf<Vector<ushort>>() - 1)) / elementsPerByte;
                    length = (Vector<ushort>.Count - unaligned) & (Vector<ushort>.Count - 1);
                }

            SequentialScan:
                while (length >= 4)
                {
                    length -= 4;

                    if (pCh[0] == value0 || pCh[0] == value1 || pCh[0] == value2)
                        goto Found;
                    if (pCh[1] == value0 || pCh[1] == value1 || pCh[1] == value2)
                        goto Found1;
                    if (pCh[2] == value0 || pCh[2] == value1 || pCh[2] == value2)
                        goto Found2;
                    if (pCh[3] == value0 || pCh[3] == value1 || pCh[3] == value2)
                        goto Found3;

                    pCh += 4;
                }

                while (length > 0)
                {
                    length--;

                    if (pCh[0] == value0 || pCh[0] == value1 || pCh[0] == value2)
                        goto Found;

                    pCh++;
                }

                // We get past SequentialScan only if IsHardwareAccelerated is true. However, we still have the redundant check to allow
                // the JIT to see that the code is unreachable and eliminate it when the platform does not have hardware accelerated.
                if (Vector.IsHardwareAccelerated && pCh < pEndCh)
                {
                    // Get the highest multiple of Vector<ushort>.Count that is within the search space.
                    // That will be how many times we iterate in the loop below.
                    // This is equivalent to: length = Vector<ushort>.Count * ((int)(pEndCh - pCh) / Vector<ushort>.Count)
                    length = (int)((pEndCh - pCh) & ~(Vector<ushort>.Count - 1));

                    // Get comparison Vector
                    Vector<ushort> values0 = new Vector<ushort>(value0);
                    Vector<ushort> values1 = new Vector<ushort>(value1);
                    Vector<ushort> values2 = new Vector<ushort>(value2);

                    while (length > 0)
                    {
                        // Using Unsafe.Read instead of ReadUnaligned since the search space is pinned and pCh is always vector aligned
                        Debug.Assert(((int)pCh & (Unsafe.SizeOf<Vector<ushort>>() - 1)) == 0);
                        Vector<ushort> vData = Unsafe.Read<Vector<ushort>>(pCh);
                        var vMatches = Vector.BitwiseOr(
                                        Vector.BitwiseOr(
                                            Vector.Equals(vData, values0),
                                            Vector.Equals(vData, values1)),
                                        Vector.Equals(vData, values2));

                        if (Vector<ushort>.Zero.Equals(vMatches))
                        {
                            pCh += Vector<ushort>.Count;
                            length -= Vector<ushort>.Count;
                            continue;
                        }
                        // Find offset of first match
                        return (int)(pCh - pChars) + LocateFirstFoundChar(vMatches);
                    }

                    if (pCh < pEndCh)
                    {
                        length = (int)(pEndCh - pCh);
                        goto SequentialScan;
                    }
                }
                return -1;
            Found3:
                pCh++;
            Found2:
                pCh++;
            Found1:
                pCh++;
            Found:
                return (int)(pCh - pChars);
            }
        }

        public static unsafe int IndexOfAny(ref char searchSpace, char value0, char value1, char value2, char value3, int length)
        {
            Debug.Assert(length >= 0);

            fixed (char* pChars = &searchSpace)
            {
                char* pCh = pChars;
                char* pEndCh = pCh + length;

                if (Vector.IsHardwareAccelerated && length >= Vector<ushort>.Count * 2)
                {
                    // Figure out how many characters to read sequentially until we are vector aligned
                    // This is equivalent to:
                    //         unaligned = ((int)pCh % Unsafe.SizeOf<Vector<ushort>>()) / elementsPerByte
                    //         length = (Vector<ushort>.Count - unaligned) % Vector<ushort>.Count
                    const int elementsPerByte = sizeof(ushort) / sizeof(byte);
                    int unaligned = ((int)pCh & (Unsafe.SizeOf<Vector<ushort>>() - 1)) / elementsPerByte;
                    length = (Vector<ushort>.Count - unaligned) & (Vector<ushort>.Count - 1);
                }

            SequentialScan:
                while (length >= 4)
                {
                    length -= 4;

                    if (pCh[0] == value0 || pCh[0] == value1 || pCh[0] == value2 || pCh[0] == value3)
                        goto Found;
                    if (pCh[1] == value0 || pCh[1] == value1 || pCh[1] == value2 || pCh[1] == value3)
                        goto Found1;
                    if (pCh[2] == value0 || pCh[2] == value1 || pCh[2] == value2 || pCh[2] == value3)
                        goto Found2;
                    if (pCh[3] == value0 || pCh[3] == value1 || pCh[3] == value2 || pCh[3] == value3)
                        goto Found3;

                    pCh += 4;
                }

                while (length > 0)
                {
                    length--;

                    if (pCh[0] == value0 || pCh[0] == value1 || pCh[0] == value2 || pCh[0] == value3)
                        goto Found;

                    pCh++;
                }

                // We get past SequentialScan only if IsHardwareAccelerated is true. However, we still have the redundant check to allow
                // the JIT to see that the code is unreachable and eliminate it when the platform does not have hardware accelerated.
                if (Vector.IsHardwareAccelerated && pCh < pEndCh)
                {
                    // Get the highest multiple of Vector<ushort>.Count that is within the search space.
                    // That will be how many times we iterate in the loop below.
                    // This is equivalent to: length = Vector<ushort>.Count * ((int)(pEndCh - pCh) / Vector<ushort>.Count)
                    length = (int)((pEndCh - pCh) & ~(Vector<ushort>.Count - 1));

                    // Get comparison Vector
                    Vector<ushort> values0 = new Vector<ushort>(value0);
                    Vector<ushort> values1 = new Vector<ushort>(value1);
                    Vector<ushort> values2 = new Vector<ushort>(value2);
                    Vector<ushort> values3 = new Vector<ushort>(value3);

                    while (length > 0)
                    {
                        // Using Unsafe.Read instead of ReadUnaligned since the search space is pinned and pCh is always vector aligned
                        Debug.Assert(((int)pCh & (Unsafe.SizeOf<Vector<ushort>>() - 1)) == 0);
                        Vector<ushort> vData = Unsafe.Read<Vector<ushort>>(pCh);
                        var vMatches = Vector.BitwiseOr(
                                            Vector.BitwiseOr(
                                                Vector.BitwiseOr(Vector.Equals(vData, values0), Vector.Equals(vData, values1)),
                                                Vector.Equals(vData, values2)),
                                            Vector.Equals(vData, values3));

                        if (Vector<ushort>.Zero.Equals(vMatches))
                        {
                            pCh += Vector<ushort>.Count;
                            length -= Vector<ushort>.Count;
                            continue;
                        }
                        // Find offset of first match
                        return (int)(pCh - pChars) + LocateFirstFoundChar(vMatches);
                    }

                    if (pCh < pEndCh)
                    {
                        length = (int)(pEndCh - pCh);
                        goto SequentialScan;
                    }
                }

                return -1;
            Found3:
                pCh++;
            Found2:
                pCh++;
            Found1:
                pCh++;
            Found:
                return (int)(pCh - pChars);
            }
        }

        public static unsafe int IndexOfAny(ref char searchSpace, char value0, char value1, char value2, char value3, char value4, int length)
        {
            Debug.Assert(length >= 0);

            fixed (char* pChars = &searchSpace)
            {
                char* pCh = pChars;
                char* pEndCh = pCh + length;

                if (Vector.IsHardwareAccelerated && length >= Vector<ushort>.Count * 2)
                {
                    // Figure out how many characters to read sequentially until we are vector aligned
                    // This is equivalent to:
                    //         unaligned = ((int)pCh % Unsafe.SizeOf<Vector<ushort>>()) / elementsPerByte
                    //         length = (Vector<ushort>.Count - unaligned) % Vector<ushort>.Count
                    const int elementsPerByte = sizeof(ushort) / sizeof(byte);
                    int unaligned = ((int)pCh & (Unsafe.SizeOf<Vector<ushort>>() - 1)) / elementsPerByte;
                    length = (Vector<ushort>.Count - unaligned) & (Vector<ushort>.Count - 1);
                }

            SequentialScan:
                while (length >= 4)
                {
                    length -= 4;

                    if (pCh[0] == value0 || pCh[0] == value1 || pCh[0] == value2 || pCh[0] == value3 || pCh[0] == value4)
                        goto Found;
                    if (pCh[1] == value0 || pCh[1] == value1 || pCh[1] == value2 || pCh[1] == value3 || pCh[1] == value4)
                        goto Found1;
                    if (pCh[2] == value0 || pCh[2] == value1 || pCh[2] == value2 || pCh[2] == value3 || pCh[2] == value4)
                        goto Found2;
                    if (pCh[3] == value0 || pCh[3] == value1 || pCh[3] == value2 || pCh[3] == value3 || pCh[3] == value4)
                        goto Found3;

                    pCh += 4;
                }

                while (length > 0)
                {
                    length--;

                    if (pCh[0] == value0 || pCh[0] == value1 || pCh[0] == value2 || pCh[0] == value3 || pCh[0] == value4)
                        goto Found;

                    pCh++;
                }

                // We get past SequentialScan only if IsHardwareAccelerated is true. However, we still have the redundant check to allow
                // the JIT to see that the code is unreachable and eliminate it when the platform does not have hardware accelerated.
                if (Vector.IsHardwareAccelerated && pCh < pEndCh)
                {
                    // Get the highest multiple of Vector<ushort>.Count that is within the search space.
                    // That will be how many times we iterate in the loop below.
                    // This is equivalent to: length = Vector<ushort>.Count * ((int)(pEndCh - pCh) / Vector<ushort>.Count)
                    length = (int)((pEndCh - pCh) & ~(Vector<ushort>.Count - 1));

                    // Get comparison Vector
                    Vector<ushort> values0 = new Vector<ushort>(value0);
                    Vector<ushort> values1 = new Vector<ushort>(value1);
                    Vector<ushort> values2 = new Vector<ushort>(value2);
                    Vector<ushort> values3 = new Vector<ushort>(value3);
                    Vector<ushort> values4 = new Vector<ushort>(value4);

                    while (length > 0)
                    {
                        // Using Unsafe.Read instead of ReadUnaligned since the search space is pinned and pCh is always vector aligned
                        Debug.Assert(((int)pCh & (Unsafe.SizeOf<Vector<ushort>>() - 1)) == 0);
                        Vector<ushort> vData = Unsafe.Read<Vector<ushort>>(pCh);
                        var vMatches = Vector.BitwiseOr(
                                            Vector.BitwiseOr(
                                                Vector.BitwiseOr(
                                                    Vector.BitwiseOr(Vector.Equals(vData, values0), Vector.Equals(vData, values1)),
                                                    Vector.Equals(vData, values2)),
                                                Vector.Equals(vData, values3)),
                                            Vector.Equals(vData, values4));

                        if (Vector<ushort>.Zero.Equals(vMatches))
                        {
                            pCh += Vector<ushort>.Count;
                            length -= Vector<ushort>.Count;
                            continue;
                        }
                        // Find offset of first match
                        return (int)(pCh - pChars) + LocateFirstFoundChar(vMatches);
                    }

                    if (pCh < pEndCh)
                    {
                        length = (int)(pEndCh - pCh);
                        goto SequentialScan;
                    }
                }

                return -1;
            Found3:
                pCh++;
            Found2:
                pCh++;
            Found1:
                pCh++;
            Found:
                return (int)(pCh - pChars);
            }
        }

        #endregion

        #region -- LastIndexOf --

        public static int LastIndexOf(ref char searchSpace, int searchSpaceLength, ref char value, int valueLength)
        {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            uint uValueLength = (uint)valueLength;
            if (0u >= uValueLength)
            {
                return 0;  // A zero-length sequence is always treated as "found" at the start of the search space.
            }
            if (1u >= uValueLength)
            {
                return LastIndexOf(ref searchSpace, value, searchSpaceLength);
            }

            char valueHead = value;
            ref char valueTail = ref Unsafe.Add(ref value, 1);
            int valueTailLength = valueLength - 1;

            int index = 0;
            for (; ; )
            {
                Debug.Assert(0 <= index && index <= searchSpaceLength); // Ensures no deceptive underflows in the computation of "remainingSearchSpaceLength".
                int remainingSearchSpaceLength = searchSpaceLength - index - valueTailLength;
                if ((uint)(remainingSearchSpaceLength - 1) > SharedConstants.TooBigOrNegative) // <= 0
                    break;  // The unsearched portion is now shorter than the sequence we're looking for. So it can't be there.

                // Do a quick search for the first element of "value".
                int relativeIndex = LastIndexOf(ref searchSpace, valueHead, remainingSearchSpaceLength);
                if (relativeIndex == -1)
                    break;

                // Found the first element of "value". See if the tail matches.
                if (SequenceEqual(ref Unsafe.Add(ref searchSpace, relativeIndex + 1), ref valueTail, valueTailLength))
                    return relativeIndex;  // The tail matched. Return a successful find.

                index += remainingSearchSpaceLength - relativeIndex;
            }
            return -1;
        }

        //[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe int LastIndexOf(ref char searchSpace, char value, int length)
        {
            Debug.Assert(length >= 0);

            fixed (char* pChars = &searchSpace)
            {
                char* pCh = pChars + length;
                char* pEndCh = pChars;

                if (Vector.IsHardwareAccelerated && length >= Vector<ushort>.Count * 2)
                {
                    // Figure out how many characters to read sequentially from the end until we are vector aligned
                    // This is equivalent to: length = ((int)pCh % Unsafe.SizeOf<Vector<ushort>>()) / elementsPerByte
                    const int elementsPerByte = sizeof(ushort) / sizeof(byte);
                    length = ((int)pCh & (Unsafe.SizeOf<Vector<ushort>>() - 1)) / elementsPerByte;
                }

            SequentialScan:
                while (length >= 4)
                {
                    length -= 4;
                    pCh -= 4;

                    if (*(pCh + 3) == value)
                        goto Found3;
                    if (*(pCh + 2) == value)
                        goto Found2;
                    if (*(pCh + 1) == value)
                        goto Found1;
                    if (*pCh == value)
                        goto Found;
                }

                while (length > 0)
                {
                    length -= 1;
                    pCh -= 1;

                    if (*pCh == value)
                        goto Found;
                }

                // We get past SequentialScan only if IsHardwareAccelerated is true. However, we still have the redundant check to allow
                // the JIT to see that the code is unreachable and eliminate it when the platform does not have hardware accelerated.
                if (Vector.IsHardwareAccelerated && pCh > pEndCh)
                {
                    // Get the highest multiple of Vector<ushort>.Count that is within the search space.
                    // That will be how many times we iterate in the loop below.
                    // This is equivalent to: length = Vector<ushort>.Count * ((int)(pCh - pEndCh) / Vector<ushort>.Count)
                    length = (int)((pCh - pEndCh) & ~(Vector<ushort>.Count - 1));

                    // Get comparison Vector
                    Vector<ushort> vComparison = new Vector<ushort>(value);

                    while (length > 0)
                    {
                        char* pStart = pCh - Vector<ushort>.Count;
                        // Using Unsafe.Read instead of ReadUnaligned since the search space is pinned and pCh (and hence pSart) is always vector aligned
                        Debug.Assert(((int)pStart & (Unsafe.SizeOf<Vector<ushort>>() - 1)) == 0);
                        Vector<ushort> vMatches = Vector.Equals(vComparison, Unsafe.Read<Vector<ushort>>(pStart));
                        if (Vector<ushort>.Zero.Equals(vMatches))
                        {
                            pCh -= Vector<ushort>.Count;
                            length -= Vector<ushort>.Count;
                            continue;
                        }
                        // Find offset of last match
                        return (int)(pStart - pEndCh) + LocateLastFoundChar(vMatches);
                    }

                    if (pCh > pEndCh)
                    {
                        length = (int)(pCh - pEndCh);
                        goto SequentialScan;
                    }
                }

                return -1;
            Found:
                return (int)(pCh - pEndCh);
            Found1:
                return (int)(pCh - pEndCh) + 1;
            Found2:
                return (int)(pCh - pEndCh) + 2;
            Found3:
                return (int)(pCh - pEndCh) + 3;
            }
        }

        #endregion

        #region -- LastIndexOfAny --

        public static int LastIndexOfAny(ref char searchSpace, int searchSpaceLength, ref char value, int valueLength)
        {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            if (0u >= (uint)valueLength)
                return -1;  // A zero-length set of values is always treated as "not found".

            int index = -1;
            for (int i = 0; i < valueLength; i++)
            {
                var tempIndex = LastIndexOf(ref searchSpace, Unsafe.Add(ref value, i), searchSpaceLength);
                if (tempIndex > index)
                    index = tempIndex;
            }
            return index;
        }

        public static int LastIndexOfAny(ref char searchSpace, char value0, char value1, int length)
        {
            Debug.Assert(length >= 0);

            char lookUp;
            while (length >= 8)
            {
                length -= 8;

                lookUp = Unsafe.Add(ref searchSpace, length + 7);
                if (value0 == lookUp || value1 == lookUp)
                    goto Found7;
                lookUp = Unsafe.Add(ref searchSpace, length + 6);
                if (value0 == lookUp || value1 == lookUp)
                    goto Found6;
                lookUp = Unsafe.Add(ref searchSpace, length + 5);
                if (value0 == lookUp || value1 == lookUp)
                    goto Found5;
                lookUp = Unsafe.Add(ref searchSpace, length + 4);
                if (value0 == lookUp || value1 == lookUp)
                    goto Found4;
                lookUp = Unsafe.Add(ref searchSpace, length + 3);
                if (value0 == lookUp || value1 == lookUp)
                    goto Found3;
                lookUp = Unsafe.Add(ref searchSpace, length + 2);
                if (value0 == lookUp || value1 == lookUp)
                    goto Found2;
                lookUp = Unsafe.Add(ref searchSpace, length + 1);
                if (value0 == lookUp || value1 == lookUp)
                    goto Found1;
                lookUp = Unsafe.Add(ref searchSpace, length);
                if (value0 == lookUp || value1 == lookUp)
                    goto Found;
            }

            if (length >= 4)
            {
                length -= 4;

                lookUp = Unsafe.Add(ref searchSpace, length + 3);
                if (value0 == lookUp || value1 == lookUp)
                    goto Found3;
                lookUp = Unsafe.Add(ref searchSpace, length + 2);
                if (value0 == lookUp || value1 == lookUp)
                    goto Found2;
                lookUp = Unsafe.Add(ref searchSpace, length + 1);
                if (value0 == lookUp || value1 == lookUp)
                    goto Found1;
                lookUp = Unsafe.Add(ref searchSpace, length);
                if (value0 == lookUp || value1 == lookUp)
                    goto Found;
            }

            while (length > 0)
            {
                length--;

                lookUp = Unsafe.Add(ref searchSpace, length);
                if (value0 == lookUp || value1 == lookUp)
                    goto Found;
            }

            return -1;

        Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return length;
        Found1:
            return length + 1;
        Found2:
            return length + 2;
        Found3:
            return length + 3;
        Found4:
            return length + 4;
        Found5:
            return length + 5;
        Found6:
            return length + 6;
        Found7:
            return length + 7;
        }

        public static int LastIndexOfAny(ref char searchSpace, char value0, char value1, char value2, int length)
        {
            Debug.Assert(length >= 0);

            char lookUp;
            while (length >= 8)
            {
                length -= 8;

                lookUp = Unsafe.Add(ref searchSpace, length + 7);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp)
                    goto Found7;
                lookUp = Unsafe.Add(ref searchSpace, length + 6);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp)
                    goto Found6;
                lookUp = Unsafe.Add(ref searchSpace, length + 5);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp)
                    goto Found5;
                lookUp = Unsafe.Add(ref searchSpace, length + 4);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp)
                    goto Found4;
                lookUp = Unsafe.Add(ref searchSpace, length + 3);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp)
                    goto Found3;
                lookUp = Unsafe.Add(ref searchSpace, length + 2);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp)
                    goto Found2;
                lookUp = Unsafe.Add(ref searchSpace, length + 1);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp)
                    goto Found1;
                lookUp = Unsafe.Add(ref searchSpace, length);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp)
                    goto Found;
            }

            if (length >= 4)
            {
                length -= 4;

                lookUp = Unsafe.Add(ref searchSpace, length + 3);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp)
                    goto Found3;
                lookUp = Unsafe.Add(ref searchSpace, length + 2);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp)
                    goto Found2;
                lookUp = Unsafe.Add(ref searchSpace, length + 1);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp)
                    goto Found1;
                lookUp = Unsafe.Add(ref searchSpace, length);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp)
                    goto Found;
            }

            while (length > 0)
            {
                length--;

                lookUp = Unsafe.Add(ref searchSpace, length);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp)
                    goto Found;
            }

            return -1;

        Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return length;
        Found1:
            return length + 1;
        Found2:
            return length + 2;
        Found3:
            return length + 3;
        Found4:
            return length + 4;
        Found5:
            return length + 5;
        Found6:
            return length + 6;
        Found7:
            return length + 7;
        }

        #endregion

        #region ** Helper **

        // Vector sub-search adapted from https://github.com/aspnet/KestrelHttpServer/pull/1138
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateFirstFoundChar(Vector<ushort> match)
        {
            var vector64 = Vector.AsVectorUInt64(match);
            ulong candidate = 0;
            int i = 0;
            // Pattern unrolled by jit https://github.com/dotnet/coreclr/pull/8001
            for (; i < Vector<ulong>.Count; i++)
            {
                candidate = vector64[i];
                if (candidate != 0)
                {
                    break;
                }
            }

            // Single LEA instruction with jitted const (using function result)
            return i * 4 + LocateFirstFoundChar(candidate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateFirstFoundChar(ulong match)
        {
#if NETCOREAPP_3_0_GREATER
            // TODO: Arm variants
            if (Bmi1.X64.IsSupported)
            {
                return (int)(Bmi1.X64.TrailingZeroCount(match) >> 4);
            }
            else
            {
#endif
            unchecked
            {
                // Flag least significant power of two bit
                var powerOfTwoFlag = match ^ (match - 1);
                // Shift all powers of two into the high byte and extract
                return (int)((powerOfTwoFlag * XorPowerOfTwoToHighChar) >> 49);
            }
#if NETCOREAPP_3_0_GREATER
            }
#endif
        }

        private const ulong XorPowerOfTwoToHighChar = (0x03ul |
                                                       0x02ul << 16 |
                                                       0x01ul << 32) + 1;

        // Vector sub-search adapted from https://github.com/aspnet/KestrelHttpServer/pull/1138
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateLastFoundChar(Vector<ushort> match)
        {
            var vector64 = Vector.AsVectorUInt64(match);
            ulong candidate = 0;
            int i = Vector<ulong>.Count - 1;
            // Pattern unrolled by jit https://github.com/dotnet/coreclr/pull/8001
            for (; i >= 0; i--)
            {
                candidate = vector64[i];
                if (candidate != 0)
                {
                    break;
                }
            }

            // Single LEA instruction with jitted const (using function result)
            return i * 4 + LocateLastFoundChar(candidate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateLastFoundChar(ulong match)
        {
#if NETCOREAPP_3_0_GREATER
            return 3 - (BitOperations.LeadingZeroCount(match) >> 4);
#else
            // Find the most significant char that has its highest bit set
            int index = 3;
            while ((long)match > 0)
            {
                match = match << 16;
                index--;
            }
            return index;
#endif
        }

#if NETCOREAPP_3_0_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref char Add(ref char source, nint elementOffset)
            => ref Unsafe.Add(ref source, (IntPtr)elementOffset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Vector<ushort> LoadVector(ref char start, nint offset)
            => Unsafe.ReadUnaligned<Vector<ushort>>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref start, (IntPtr)offset)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Vector128<ushort> LoadVector128(ref char start, nint offset)
            => Unsafe.ReadUnaligned<Vector128<ushort>>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref start, (IntPtr)offset)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Vector256<ushort> LoadVector256(ref char start, nint offset)
            => Unsafe.ReadUnaligned<Vector256<ushort>>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref start, (IntPtr)offset)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe UIntPtr LoadUIntPtr(ref char start, nint offset)
            => Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref start, (IntPtr)offset)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe nint GetCharVectorSpanLength(nint offset, nint length)
            => ((length - offset) & ~(Vector<ushort>.Count - 1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe nint GetCharVector128SpanLength(nint offset, nint length)
            => ((length - offset) & ~(Vector128<ushort>.Count - 1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static nint GetCharVector256SpanLength(nint offset, nint length)
            => ((length - offset) & ~(Vector256<ushort>.Count - 1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe nint UnalignedCountVector(ref char searchSpace)
        {
            const int ElementsPerByte = sizeof(ushort) / sizeof(byte);
            // Figure out how many characters to read sequentially until we are vector aligned
            // This is equivalent to:
            //         unaligned = ((int)pCh % Unsafe.SizeOf<Vector<ushort>>()) / ElementsPerByte 
            //         length = (Vector<ushort>.Count - unaligned) % Vector<ushort>.Count

            // This alignment is only valid if the GC does not relocate; so we use ReadUnaligned to get the data.
            // If a GC does occur and alignment is lost, the GC cost will outweigh any gains from alignment so it
            // isn't too important to pin to maintain the alignment.
            return (nint)(uint)(-(int)Unsafe.AsPointer(ref searchSpace) / ElementsPerByte) & (Vector<ushort>.Count - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe nint UnalignedCountVector128(ref char searchSpace)
        {
            const int ElementsPerByte = sizeof(ushort) / sizeof(byte);
            // This alignment is only valid if the GC does not relocate; so we use ReadUnaligned to get the data.
            // If a GC does occur and alignment is lost, the GC cost will outweigh any gains from alignment so it
            // isn't too important to pin to maintain the alignment.
            return (nint)(uint)(-(int)Unsafe.AsPointer(ref searchSpace) / ElementsPerByte) & (Vector128<ushort>.Count - 1);
        }
#endif

        #endregion
    }
}
