// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Common.Internal
{
    using System;
    using System.Diagnostics;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using DotNetty.Common.Utilities;
#if NETCOREAPP_3_0_GREATER
    using System.Runtime.Intrinsics;
    using System.Runtime.Intrinsics.X86;
#endif

    public static partial class SpanHelpers
    {
        #region -- FindIndex / FindLastIndex --

        public static unsafe int FindIndex(ref byte searchSpace, Predicate<byte> match, int length)
        {
            Debug.Assert(length >= 0);

            IntPtr offset = (IntPtr)0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr lengthToExamine = (IntPtr)length;

            while ((byte*)lengthToExamine >= (byte*)8)
            {
                lengthToExamine -= 8;

                if (match(Unsafe.AddByteOffset(ref searchSpace, offset)))
                    goto Found;
                if (match(Unsafe.AddByteOffset(ref searchSpace, offset + 1)))
                    goto Found1;
                if (match(Unsafe.AddByteOffset(ref searchSpace, offset + 2)))
                    goto Found2;
                if (match(Unsafe.AddByteOffset(ref searchSpace, offset + 3)))
                    goto Found3;
                if (match(Unsafe.AddByteOffset(ref searchSpace, offset + 4)))
                    goto Found4;
                if (match(Unsafe.AddByteOffset(ref searchSpace, offset + 5)))
                    goto Found5;
                if (match(Unsafe.AddByteOffset(ref searchSpace, offset + 6)))
                    goto Found6;
                if (match(Unsafe.AddByteOffset(ref searchSpace, offset + 7)))
                    goto Found7;

                offset += 8;
            }

            if ((byte*)lengthToExamine >= (byte*)4)
            {
                lengthToExamine -= 4;

                if (match(Unsafe.AddByteOffset(ref searchSpace, offset)))
                    goto Found;
                if (match(Unsafe.AddByteOffset(ref searchSpace, offset + 1)))
                    goto Found1;
                if (match(Unsafe.AddByteOffset(ref searchSpace, offset + 2)))
                    goto Found2;
                if (match(Unsafe.AddByteOffset(ref searchSpace, offset + 3)))
                    goto Found3;

                offset += 4;
            }

            while ((byte*)lengthToExamine > (byte*)0)
            {
                lengthToExamine -= 1;

                if (match(Unsafe.AddByteOffset(ref searchSpace, offset)))
                    goto Found;

                offset += 1;
            }

            return -1;
        Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)offset;
        Found1:
            return (int)(byte*)(offset + 1);
        Found2:
            return (int)(byte*)(offset + 2);
        Found3:
            return (int)(byte*)(offset + 3);
        Found4:
            return (int)(byte*)(offset + 4);
        Found5:
            return (int)(byte*)(offset + 5);
        Found6:
            return (int)(byte*)(offset + 6);
        Found7:
            return (int)(byte*)(offset + 7);

        }

        public static unsafe int FindLastIndex(ref byte searchSpace, Predicate<byte> match, int length)
        {
            Debug.Assert(length >= 0);

            IntPtr offset = (IntPtr)length; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr lengthToExamine = (IntPtr)length;

            while ((byte*)lengthToExamine >= (byte*)8)
            {
                lengthToExamine -= 8;
                offset -= 8;

                if (match(Unsafe.AddByteOffset(ref searchSpace, offset + 7)))
                    goto Found7;
                if (match(Unsafe.AddByteOffset(ref searchSpace, offset + 6)))
                    goto Found6;
                if (match(Unsafe.AddByteOffset(ref searchSpace, offset + 5)))
                    goto Found5;
                if (match(Unsafe.AddByteOffset(ref searchSpace, offset + 4)))
                    goto Found4;
                if (match(Unsafe.AddByteOffset(ref searchSpace, offset + 3)))
                    goto Found3;
                if (match(Unsafe.AddByteOffset(ref searchSpace, offset + 2)))
                    goto Found2;
                if (match(Unsafe.AddByteOffset(ref searchSpace, offset + 1)))
                    goto Found1;
                if (match(Unsafe.AddByteOffset(ref searchSpace, offset)))
                    goto Found;
            }

            if ((byte*)lengthToExamine >= (byte*)4)
            {
                lengthToExamine -= 4;
                offset -= 4;

                if (match(Unsafe.AddByteOffset(ref searchSpace, offset + 3)))
                    goto Found3;
                if (match(Unsafe.AddByteOffset(ref searchSpace, offset + 2)))
                    goto Found2;
                if (match(Unsafe.AddByteOffset(ref searchSpace, offset + 1)))
                    goto Found1;
                if (match(Unsafe.AddByteOffset(ref searchSpace, offset)))
                    goto Found;
            }

            while ((byte*)lengthToExamine > (byte*)0)
            {
                lengthToExamine -= 1;
                offset -= 1;

                if (match(Unsafe.AddByteOffset(ref searchSpace, offset)))
                    goto Found;
            }

            return -1;
        Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)offset;
        Found1:
            return (int)(byte*)(offset + 1);
        Found2:
            return (int)(byte*)(offset + 2);
        Found3:
            return (int)(byte*)(offset + 3);
        Found4:
            return (int)(byte*)(offset + 4);
        Found5:
            return (int)(byte*)(offset + 5);
        Found6:
            return (int)(byte*)(offset + 6);
        Found7:
            return (int)(byte*)(offset + 7);
        }

        #endregion

        #region -- ForEachByte / ForEachByteDesc --

        public static unsafe int ForEachByte(ref byte searchSpace, IByteProcessor processor, int length)
        {
            Debug.Assert(length >= 0);

            IntPtr offset = (IntPtr)0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr lengthToExamine = (IntPtr)length;

            while ((byte*)lengthToExamine >= (byte*)8)
            {
                lengthToExamine -= 8;

                if (!processor.Process(Unsafe.AddByteOffset(ref searchSpace, offset)))
                    goto Found;
                if (!processor.Process(Unsafe.AddByteOffset(ref searchSpace, offset + 1)))
                    goto Found1;
                if (!processor.Process(Unsafe.AddByteOffset(ref searchSpace, offset + 2)))
                    goto Found2;
                if (!processor.Process(Unsafe.AddByteOffset(ref searchSpace, offset + 3)))
                    goto Found3;
                if (!processor.Process(Unsafe.AddByteOffset(ref searchSpace, offset + 4)))
                    goto Found4;
                if (!processor.Process(Unsafe.AddByteOffset(ref searchSpace, offset + 5)))
                    goto Found5;
                if (!processor.Process(Unsafe.AddByteOffset(ref searchSpace, offset + 6)))
                    goto Found6;
                if (!processor.Process(Unsafe.AddByteOffset(ref searchSpace, offset + 7)))
                    goto Found7;

                offset += 8;
            }

            if ((byte*)lengthToExamine >= (byte*)4)
            {
                lengthToExamine -= 4;

                if (!processor.Process(Unsafe.AddByteOffset(ref searchSpace, offset)))
                    goto Found;
                if (!processor.Process(Unsafe.AddByteOffset(ref searchSpace, offset + 1)))
                    goto Found1;
                if (!processor.Process(Unsafe.AddByteOffset(ref searchSpace, offset + 2)))
                    goto Found2;
                if (!processor.Process(Unsafe.AddByteOffset(ref searchSpace, offset + 3)))
                    goto Found3;

                offset += 4;
            }

            while ((byte*)lengthToExamine > (byte*)0)
            {
                lengthToExamine -= 1;

                if (!processor.Process(Unsafe.AddByteOffset(ref searchSpace, offset)))
                    goto Found;

                offset += 1;
            }

            return -1;
        Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)offset;
        Found1:
            return (int)(byte*)(offset + 1);
        Found2:
            return (int)(byte*)(offset + 2);
        Found3:
            return (int)(byte*)(offset + 3);
        Found4:
            return (int)(byte*)(offset + 4);
        Found5:
            return (int)(byte*)(offset + 5);
        Found6:
            return (int)(byte*)(offset + 6);
        Found7:
            return (int)(byte*)(offset + 7);

        }

        public static unsafe int ForEachByteDesc(ref byte searchSpace, IByteProcessor processor, int length)
        {
            Debug.Assert(length >= 0);

            IntPtr offset = (IntPtr)length; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr lengthToExamine = (IntPtr)length;

            while ((byte*)lengthToExamine >= (byte*)8)
            {
                lengthToExamine -= 8;
                offset -= 8;

                if (!processor.Process(Unsafe.AddByteOffset(ref searchSpace, offset + 7)))
                    goto Found7;
                if (!processor.Process(Unsafe.AddByteOffset(ref searchSpace, offset + 6)))
                    goto Found6;
                if (!processor.Process(Unsafe.AddByteOffset(ref searchSpace, offset + 5)))
                    goto Found5;
                if (!processor.Process(Unsafe.AddByteOffset(ref searchSpace, offset + 4)))
                    goto Found4;
                if (!processor.Process(Unsafe.AddByteOffset(ref searchSpace, offset + 3)))
                    goto Found3;
                if (!processor.Process(Unsafe.AddByteOffset(ref searchSpace, offset + 2)))
                    goto Found2;
                if (!processor.Process(Unsafe.AddByteOffset(ref searchSpace, offset + 1)))
                    goto Found1;
                if (!processor.Process(Unsafe.AddByteOffset(ref searchSpace, offset)))
                    goto Found;
            }

            if ((byte*)lengthToExamine >= (byte*)4)
            {
                lengthToExamine -= 4;
                offset -= 4;

                if (!processor.Process(Unsafe.AddByteOffset(ref searchSpace, offset + 3)))
                    goto Found3;
                if (!processor.Process(Unsafe.AddByteOffset(ref searchSpace, offset + 2)))
                    goto Found2;
                if (!processor.Process(Unsafe.AddByteOffset(ref searchSpace, offset + 1)))
                    goto Found1;
                if (!processor.Process(Unsafe.AddByteOffset(ref searchSpace, offset)))
                    goto Found;
            }

            while ((byte*)lengthToExamine > (byte*)0)
            {
                lengthToExamine -= 1;
                offset -= 1;

                if (!processor.Process(Unsafe.AddByteOffset(ref searchSpace, offset)))
                    goto Found;
            }

            return -1;
        Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)offset;
        Found1:
            return (int)(byte*)(offset + 1);
        Found2:
            return (int)(byte*)(offset + 2);
        Found3:
            return (int)(byte*)(offset + 3);
        Found4:
            return (int)(byte*)(offset + 4);
        Found5:
            return (int)(byte*)(offset + 5);
        Found6:
            return (int)(byte*)(offset + 6);
        Found7:
            return (int)(byte*)(offset + 7);
        }

        #endregion

        #region -- Contains --

        // Adapted from IndexOf(...)
        //[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe bool Contains(ref byte searchSpace, byte value, int length)
        {
            Debug.Assert(length >= 0);

#if NETCOREAPP_3_0_GREATER
            return SharedConstants.TooBigOrNegative >= (uint)IndexOf(ref searchSpace, value, length);
#else
            uint uValue = value; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            IntPtr offset = (IntPtr)0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr lengthToExamine = (IntPtr)length;

            if (Vector.IsHardwareAccelerated && length >= Vector<byte>.Count * 2)
            {
                lengthToExamine = UnalignedCountVector(ref searchSpace);
            }

        SequentialScan:
            while ((byte*)lengthToExamine >= (byte*)8)
            {
                lengthToExamine -= 8;

                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 0) ||
                    uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 1) ||
                    uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 2) ||
                    uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 3) ||
                    uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 4) ||
                    uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 5) ||
                    uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 6) ||
                    uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 7))
                {
                    goto Found;
                }

                offset += 8;
            }

            if ((byte*)lengthToExamine >= (byte*)4)
            {
                lengthToExamine -= 4;

                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 0) ||
                    uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 1) ||
                    uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 2) ||
                    uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 3))
                {
                    goto Found;
                }

                offset += 4;
            }

            while ((byte*)lengthToExamine > (byte*)0)
            {
                lengthToExamine -= 1;

                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset))
                    goto Found;

                offset += 1;
            }

            if (Vector.IsHardwareAccelerated && ((int)(byte*)offset < length))
            {
                lengthToExamine = (IntPtr)((length - (int)(byte*)offset) & ~(Vector<byte>.Count - 1));

                Vector<byte> values = GetVector(value);

                while ((byte*)lengthToExamine > (byte*)offset)
                {
                    var matches = Vector.Equals(values, LoadVector(ref searchSpace, offset));
                    if (Vector<byte>.Zero.Equals(matches))
                    {
                        offset += Vector<byte>.Count;
                        continue;
                    }

                    goto Found;
                }

                if ((int)(byte*)offset < length)
                {
                    lengthToExamine = (IntPtr)(length - (int)(byte*)offset);
                    goto SequentialScan;
                }
            }

            return false;

        Found:
            return true;
#endif
        }

        #endregion

        #region -- SequenceEqual --

        // Optimized byte-based SequenceEquals. The "length" parameter for this one is declared a nuint rather than int as we also use it for types other than byte
        // where the length can exceed 2Gb once scaled by sizeof(T).
        //[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe bool SequenceEqual(ref byte first, ref byte second, nint length)
        {
            if (Unsafe.AreSame(ref first, ref second)) { goto Equal; }

            IntPtr offset = (IntPtr)0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr lengthToExamine = (IntPtr)(void*)((nuint)length);

            if (Vector.IsHardwareAccelerated && (byte*)lengthToExamine >= (byte*)Vector<byte>.Count)
            {
                lengthToExamine -= Vector<byte>.Count;
                while ((byte*)lengthToExamine > (byte*)offset)
                {
                    if (LoadVector(ref first, offset) != LoadVector(ref second, offset))
                    {
                        goto NotEqual;
                    }
                    offset += Vector<byte>.Count;
                }
                return LoadVector(ref first, lengthToExamine) == LoadVector(ref second, lengthToExamine);
            }

            if ((byte*)lengthToExamine >= (byte*)sizeof(UIntPtr))
            {
                lengthToExamine -= sizeof(UIntPtr);
                while ((byte*)lengthToExamine > (byte*)offset)
                {
                    if (LoadUIntPtr(ref first, offset) != LoadUIntPtr(ref second, offset))
                    {
                        goto NotEqual;
                    }
                    offset += sizeof(UIntPtr);
                }
                return LoadUIntPtr(ref first, lengthToExamine) == LoadUIntPtr(ref second, lengthToExamine);
            }

            while ((byte*)lengthToExamine > (byte*)offset)
            {
                if (Unsafe.AddByteOffset(ref first, offset) != Unsafe.AddByteOffset(ref second, offset))
                {
                    goto NotEqual;
                }
                offset += 1;
            }

        Equal:
            return true;
        NotEqual: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return false;
        }

        #endregion

        #region -- SequenceCompareTo --

        //[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe int SequenceCompareTo(ref byte first, int firstLength, ref byte second, int secondLength)
        {
            Debug.Assert(firstLength >= 0);
            Debug.Assert(secondLength >= 0);

            if (Unsafe.AreSame(ref first, ref second)) { goto Equal; }

            IntPtr minLength = (IntPtr)((firstLength < secondLength) ? firstLength : secondLength);

            IntPtr offset = (IntPtr)0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr lengthToExamine = (IntPtr)(void*)minLength;

#if NETCOREAPP_3_0_GREATER
            if (Avx2.IsSupported)
            {
                if ((byte*)lengthToExamine >= (byte*)Vector256<byte>.Count)
                {
                    lengthToExamine -= Vector256<byte>.Count;
                    uint matches;
                    while ((byte*)lengthToExamine > (byte*)offset)
                    {
                        matches = (uint)Avx2.MoveMask(Avx2.CompareEqual(LoadVector256(ref first, offset), LoadVector256(ref second, offset)));
                        // Note that MoveMask has converted the equal vector elements into a set of bit flags,
                        // So the bit position in 'matches' corresponds to the element offset.

                        // 32 elements in Vector256<byte> so we compare to uint.MaxValue to check if everything matched
                        if (matches == uint.MaxValue)
                        {
                            // All matched
                            offset += Vector256<byte>.Count;
                            continue;
                        }

                        goto Difference;
                    }
                    // Move to Vector length from end for final compare
                    offset = lengthToExamine;
                    // Same as method as above
                    matches = (uint)Avx2.MoveMask(Avx2.CompareEqual(LoadVector256(ref first, offset), LoadVector256(ref second, offset)));
                    if (matches == uint.MaxValue)
                    {
                        // All matched
                        goto Equal;
                    }
                Difference:
                    // Invert matches to find differences
                    uint differences = ~matches;
                    // Find bitflag offset of first difference and add to current offset
                    offset = (IntPtr)((int)(byte*)offset + BitOperations.TrailingZeroCount((int)differences));

                    int result = Unsafe.AddByteOffset(ref first, offset).CompareTo(Unsafe.AddByteOffset(ref second, offset));
                    Debug.Assert(result != 0);

                    return result;
                }

                if ((byte*)lengthToExamine >= (byte*)Vector128<byte>.Count)
                {
                    lengthToExamine -= Vector128<byte>.Count;
                    uint matches;
                    if ((byte*)lengthToExamine > (byte*)offset)
                    {
                        matches = (uint)Sse2.MoveMask(Sse2.CompareEqual(LoadVector128(ref first, offset), LoadVector128(ref second, offset)));
                        // Note that MoveMask has converted the equal vector elements into a set of bit flags,
                        // So the bit position in 'matches' corresponds to the element offset.

                        // 16 elements in Vector128<byte> so we compare to ushort.MaxValue to check if everything matched
                        if (matches == ushort.MaxValue)
                        {
                            // All matched
                            offset += Vector128<byte>.Count;
                        }
                        else
                        {
                            goto Difference;
                        }
                    }
                    // Move to Vector length from end for final compare
                    offset = lengthToExamine;
                    // Same as method as above
                    matches = (uint)Sse2.MoveMask(Sse2.CompareEqual(LoadVector128(ref first, offset), LoadVector128(ref second, offset)));
                    if (matches == ushort.MaxValue)
                    {
                        // All matched
                        goto Equal;
                    }
                Difference:
                    // Invert matches to find differences
                    uint differences = ~matches;
                    // Find bitflag offset of first difference and add to current offset
                    offset = (IntPtr)((int)(byte*)offset + BitOperations.TrailingZeroCount((int)differences));

                    int result = Unsafe.AddByteOffset(ref first, offset).CompareTo(Unsafe.AddByteOffset(ref second, offset));
                    Debug.Assert(result != 0);

                    return result;
                }
            }
            else if (Sse2.IsSupported)
            {
                if ((byte*)lengthToExamine >= (byte*)Vector128<byte>.Count)
                {
                    lengthToExamine -= Vector128<byte>.Count;
                    uint matches;
                    while ((byte*)lengthToExamine > (byte*)offset)
                    {
                        matches = (uint)Sse2.MoveMask(Sse2.CompareEqual(LoadVector128(ref first, offset), LoadVector128(ref second, offset)));
                        // Note that MoveMask has converted the equal vector elements into a set of bit flags,
                        // So the bit position in 'matches' corresponds to the element offset.

                        // 16 elements in Vector128<byte> so we compare to ushort.MaxValue to check if everything matched
                        if (matches == ushort.MaxValue)
                        {
                            // All matched
                            offset += Vector128<byte>.Count;
                            continue;
                        }

                        goto Difference;
                    }
                    // Move to Vector length from end for final compare
                    offset = lengthToExamine;
                    // Same as method as above
                    matches = (uint)Sse2.MoveMask(Sse2.CompareEqual(LoadVector128(ref first, offset), LoadVector128(ref second, offset)));
                    if (matches == ushort.MaxValue)
                    {
                        // All matched
                        goto Equal;
                    }
                Difference:
                    // Invert matches to find differences
                    uint differences = ~matches;
                    // Find bitflag offset of first difference and add to current offset
                    offset = (IntPtr)((int)(byte*)offset + BitOperations.TrailingZeroCount((int)differences));

                    int result = Unsafe.AddByteOffset(ref first, offset).CompareTo(Unsafe.AddByteOffset(ref second, offset));
                    Debug.Assert(result != 0);

                    return result;
                }
            }
            else
#endif
            if (Vector.IsHardwareAccelerated)
            {
                if ((byte*)lengthToExamine > (byte*)Vector<byte>.Count)
                {
                    lengthToExamine -= Vector<byte>.Count;
                    while ((byte*)lengthToExamine > (byte*)offset)
                    {
                        if (LoadVector(ref first, offset) != LoadVector(ref second, offset))
                        {
                            goto BytewiseCheck;
                        }
                        offset += Vector<byte>.Count;
                    }
                    goto BytewiseCheck;
                }
            }

            if ((byte*)lengthToExamine > (byte*)sizeof(UIntPtr))
            {
                lengthToExamine -= sizeof(UIntPtr);
                while ((byte*)lengthToExamine > (byte*)offset)
                {
                    if (LoadUIntPtr(ref first, offset) != LoadUIntPtr(ref second, offset))
                    {
                        goto BytewiseCheck;
                    }
                    offset += sizeof(UIntPtr);
                }
            }

        BytewiseCheck:  // Workaround for https://github.com/dotnet/coreclr/issues/13549
            while ((byte*)minLength > (byte*)offset)
            {
                int result = Unsafe.AddByteOffset(ref first, offset).CompareTo(Unsafe.AddByteOffset(ref second, offset));
                if (result != 0)
                    return result;
                offset += 1;
            }

        Equal:
            return firstLength - secondLength;
        }

        #endregion

        #region -- IndexOf --

        public static int IndexOf(ref byte searchSpace, int searchSpaceLength, ref byte value, int valueLength)
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

            byte valueHead = value;
            ref byte valueTail = ref Unsafe.Add(ref value, 1);
            int valueTailLength = valueLength - 1;
            int remainingSearchSpaceLength = searchSpaceLength - valueTailLength;

            int offset = 0;
            while (remainingSearchSpaceLength > 0)
            {
                // Do a quick search for the first element of "value".
                int relativeIndex = IndexOf(ref Unsafe.Add(ref searchSpace, offset), valueHead, remainingSearchSpaceLength);
                if (relativeIndex == -1)
                    break;

                remainingSearchSpaceLength -= relativeIndex;
                offset += relativeIndex;

                if ((uint)(remainingSearchSpaceLength - 1) > SharedConstants.TooBigOrNegative) // <= 0
                    break;  // The unsearched portion is now shorter than the sequence we're looking for. So it can't be there.

                // Found the first element of "value". See if the tail matches.
                if (SequenceEqual(ref Unsafe.Add(ref searchSpace, offset + 1), ref valueTail, valueTailLength))
                    return offset;  // The tail matched. Return a successful find.

                remainingSearchSpaceLength--;
                offset++;
            }
            return -1;
        }

        //[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe int IndexOf(ref byte searchSpace, byte value, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue = value; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            IntPtr offset = (IntPtr)0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr lengthToExamine = (IntPtr)length;

#if NETCOREAPP_3_0_GREATER
            if (Avx2.IsSupported || Sse2.IsSupported)
            {
                // Avx2 branch also operates on Sse2 sizes, so check is combined.
                if (length >= Vector128<byte>.Count * 2)
                {
                    lengthToExamine = UnalignedCountVector128(ref searchSpace);
                }
            }
            else
#endif
            if (Vector.IsHardwareAccelerated && length >= Vector<byte>.Count * 2)
            {
                lengthToExamine = UnalignedCountVector(ref searchSpace);
            }
        SequentialScan:
            while ((byte*)lengthToExamine >= (byte*)8)
            {
                lengthToExamine -= 8;

                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset))
                    goto Found;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 1))
                    goto Found1;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 2))
                    goto Found2;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 3))
                    goto Found3;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 4))
                    goto Found4;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 5))
                    goto Found5;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 6))
                    goto Found6;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 7))
                    goto Found7;

                offset += 8;
            }

            if ((byte*)lengthToExamine >= (byte*)4)
            {
                lengthToExamine -= 4;

                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset))
                    goto Found;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 1))
                    goto Found1;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 2))
                    goto Found2;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 3))
                    goto Found3;

                offset += 4;
            }

            while ((byte*)lengthToExamine > (byte*)0)
            {
                lengthToExamine -= 1;

                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset))
                    goto Found;

                offset += 1;
            }

#if NETCOREAPP_3_0_GREATER
            // We get past SequentialScan only if IsHardwareAccelerated or intrinsic .IsSupported is true; and remain length is greater than Vector length.
            // However, we still have the redundant check to allow the JIT to see that the code is unreachable and eliminate it when the platform does not
            // have hardware accelerated. After processing Vector lengths we return to SequentialScan to finish any remaining.
            if (Avx2.IsSupported)
            {
                if ((int)(byte*)offset < length)
                {
                    if ((((nuint)Unsafe.AsPointer(ref searchSpace) + (nuint)(nint)offset) & (nuint)(Vector256<byte>.Count - 1)) != 0)
                    {
                        // Not currently aligned to Vector256 (is aligned to Vector128); this can cause a problem for searches
                        // with no upper bound e.g. String.strlen.
                        // Start with a check on Vector128 to align to Vector256, before moving to processing Vector256.
                        // This ensures we do not fault across memory pages while searching for an end of string.
                        Vector128<byte> values = Vector128.Create(value);
                        Vector128<byte> search = LoadVector128(ref searchSpace, offset);

                        // Same method as below
                        int matches = Sse2.MoveMask(Sse2.CompareEqual(values, search));
                        if (0u >= (uint)matches)
                        {
                            // Zero flags set so no matches
                            offset += Vector128<byte>.Count;
                        }
                        else
                        {
                            // Find bitflag offset of first match and add to current offset
                            return ((int)(byte*)offset) + BitOperations.TrailingZeroCount(matches);
                        }
                    }

                    lengthToExamine = GetByteVector256SpanLength(offset, length);
                    if ((byte*)lengthToExamine > (byte*)offset)
                    {
                        Vector256<byte> values = Vector256.Create(value);
                        do
                        {
                            Vector256<byte> search = LoadVector256(ref searchSpace, offset);
                            int matches = Avx2.MoveMask(Avx2.CompareEqual(values, search));
                            // Note that MoveMask has converted the equal vector elements into a set of bit flags,
                            // So the bit position in 'matches' corresponds to the element offset.
                            if (0u >= (uint)matches)
                            {
                                // Zero flags set so no matches
                                offset += Vector256<byte>.Count;
                                continue;
                            }

                            // Find bitflag offset of first match and add to current offset
                            return ((int)(byte*)offset) + BitOperations.TrailingZeroCount(matches);
                        } while ((byte*)lengthToExamine > (byte*)offset);
                    }

                    lengthToExamine = GetByteVector128SpanLength(offset, length);
                    if ((byte*)lengthToExamine > (byte*)offset)
                    {
                        Vector128<byte> values = Vector128.Create(value);
                        Vector128<byte> search = LoadVector128(ref searchSpace, offset);

                        // Same method as above
                        int matches = Sse2.MoveMask(Sse2.CompareEqual(values, search));
                        if (0u >= (uint)matches)
                        {
                            // Zero flags set so no matches
                            offset += Vector128<byte>.Count;
                        }
                        else
                        {
                            // Find bitflag offset of first match and add to current offset
                            return ((int)(byte*)offset) + BitOperations.TrailingZeroCount(matches);
                        }
                    }

                    if ((int)(byte*)offset < length)
                    {
                        lengthToExamine = (IntPtr)(length - (int)(byte*)offset);
                        goto SequentialScan;
                    }
                }
            }
            else if (Sse2.IsSupported)
            {
                if ((int)(byte*)offset < length)
                {
                    lengthToExamine = GetByteVector128SpanLength(offset, length);

                    Vector128<byte> values = Vector128.Create(value);
                    while ((byte*)lengthToExamine > (byte*)offset)
                    {
                        Vector128<byte> search = LoadVector128(ref searchSpace, offset);

                        // Same method as above
                        int matches = Sse2.MoveMask(Sse2.CompareEqual(values, search));
                        if (0u >= (uint)matches)
                        {
                            // Zero flags set so no matches
                            offset += Vector128<byte>.Count;
                            continue;
                        }

                        // Find bitflag offset of first match and add to current offset
                        return ((int)(byte*)offset) + BitOperations.TrailingZeroCount(matches);
                    }

                    if ((int)(byte*)offset < length)
                    {
                        lengthToExamine = (IntPtr)(length - (int)(byte*)offset);
                        goto SequentialScan;
                    }
                }
            }
            else
#endif
            if (Vector.IsHardwareAccelerated && ((int)(byte*)offset < length))
            {
                lengthToExamine = GetByteVectorSpanLength(offset, length);

                Vector<byte> values = new Vector<byte>(value);

                while ((byte*)lengthToExamine > (byte*)offset)
                {
                    var matches = Vector.Equals(values, LoadVector(ref searchSpace, offset));
                    if (Vector<byte>.Zero.Equals(matches))
                    {
                        offset += Vector<byte>.Count;
                        continue;
                    }

                    // Find offset of first match and add to current offset
                    return (int)(byte*)offset + LocateFirstFoundByte(matches);
                }

                if ((int)(byte*)offset < length)
                {
                    lengthToExamine = (IntPtr)(length - (int)(byte*)offset);
                    goto SequentialScan;
                }
            }
            return -1;
        Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)offset;
        Found1:
            return (int)(byte*)(offset + 1);
        Found2:
            return (int)(byte*)(offset + 2);
        Found3:
            return (int)(byte*)(offset + 3);
        Found4:
            return (int)(byte*)(offset + 4);
        Found5:
            return (int)(byte*)(offset + 5);
        Found6:
            return (int)(byte*)(offset + 6);
        Found7:
            return (int)(byte*)(offset + 7);

        }

        #endregion

        #region -- IndexOfAny --

        public static int IndexOfAny(ref byte searchSpace, int searchSpaceLength, ref byte value, int valueLength)
        {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            switch ((uint)valueLength)
            {
                case 2u:
                    return IndexOfAny(ref searchSpace, value, Unsafe.Add(ref value, 1), searchSpaceLength);

                case 3u:
                    return IndexOfAny(ref searchSpace, value, Unsafe.Add(ref value, 1), Unsafe.Add(ref value, 2), searchSpaceLength);

                case 1u:
                    return IndexOf(ref searchSpace, value, searchSpaceLength);

                case 0u:
                    return -1;  // A zero-length set of values is always treated as "not found".

                default:
                    int offset = -1;
                    for (int i = 0; i < valueLength; i++)
                    {
                        var tempIndex = IndexOf(ref searchSpace, Unsafe.Add(ref value, i), searchSpaceLength);
                        if ((uint)tempIndex < (uint)offset)
                        {
                            offset = tempIndex;
                            // Reduce space for search, cause we don't care if we find the search value after the index of a previously found value
                            searchSpaceLength = tempIndex;

                            if (0u >= (uint)offset) { break; }
                        }
                    }
                    return offset;
            }
        }

        public static unsafe int IndexOfAny(ref byte searchSpace, byte value0, byte value1, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue0 = value0; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            uint uValue1 = value1; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            IntPtr offset = (IntPtr)0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr lengthToExamine = (IntPtr)length;

#if NETCOREAPP_3_0_GREATER
            if (Avx2.IsSupported || Sse2.IsSupported)
            {
                // Avx2 branch also operates on Sse2 sizes, so check is combined.
                if (length >= Vector128<byte>.Count * 2)
                {
                    lengthToExamine = UnalignedCountVector128(ref searchSpace);
                }
            }
            else
#endif
            if (Vector.IsHardwareAccelerated && length >= Vector<byte>.Count * 2)
            {
                lengthToExamine = UnalignedCountVector(ref searchSpace);
            }
        SequentialScan:
            uint lookUp;
            while ((byte*)lengthToExamine >= (byte*)8)
            {
                lengthToExamine -= 8;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 4);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found4;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 5);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found5;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 6);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found6;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 7);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found7;

                offset += 8;
            }

            if ((byte*)lengthToExamine >= (byte*)4)
            {
                lengthToExamine -= 4;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found3;

                offset += 4;
            }

            while ((byte*)lengthToExamine > (byte*)0)
            {
                lengthToExamine -= 1;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found;

                offset += 1;
            }

#if NETCOREAPP_3_0_GREATER
            // We get past SequentialScan only if IsHardwareAccelerated or intrinsic .IsSupported is true. However, we still have the redundant check to allow
            // the JIT to see that the code is unreachable and eliminate it when the platform does not have hardware accelerated.
            if (Avx2.IsSupported)
            {
                if ((int)(byte*)offset < length)
                {
                    lengthToExamine = GetByteVector256SpanLength(offset, length);
                    if ((byte*)lengthToExamine > (byte*)offset)
                    {
                        Vector256<byte> values0 = Vector256.Create(value0);
                        Vector256<byte> values1 = Vector256.Create(value1);
                        do
                        {
                            Vector256<byte> search = LoadVector256(ref searchSpace, offset);
                            // Bitwise Or to combine the matches and MoveMask to convert them to bitflags
                            int matches = Avx2.MoveMask(
                                Avx2.Or(
                                    Avx2.CompareEqual(values0, search),
                                    Avx2.CompareEqual(values1, search)));
                            // Note that MoveMask has converted the equal vector elements into a set of bit flags,
                            // So the bit position in 'matches' corresponds to the element offset.
                            if (0u >= (uint)matches)
                            {
                                // Zero flags set so no matches
                                offset += Vector256<byte>.Count;
                                continue;
                            }

                            // Find bitflag offset of first match and add to current offset
                            return ((int)(byte*)offset) + BitOperations.TrailingZeroCount(matches);
                        } while ((byte*)lengthToExamine > (byte*)offset);
                    }

                    lengthToExamine = GetByteVector128SpanLength(offset, length);
                    if ((byte*)lengthToExamine > (byte*)offset)
                    {
                        Vector128<byte> values0 = Vector128.Create(value0);
                        Vector128<byte> values1 = Vector128.Create(value1);

                        Vector128<byte> search = LoadVector128(ref searchSpace, offset);
                        // Same method as above
                        int matches = Sse2.MoveMask(
                            Sse2.Or(
                                Sse2.CompareEqual(values0, search),
                                Sse2.CompareEqual(values1, search)));
                        if (0u >= (uint)matches)
                        {
                            // Zero flags set so no matches
                            offset += Vector128<byte>.Count;
                        }
                        else
                        {
                            // Find bitflag offset of first match and add to current offset
                            return ((int)(byte*)offset) + BitOperations.TrailingZeroCount(matches);
                        }
                    }

                    if ((int)(byte*)offset < length)
                    {
                        lengthToExamine = (IntPtr)(length - (int)(byte*)offset);
                        goto SequentialScan;
                    }
                }
            }
            else if (Sse2.IsSupported)
            {
                if ((int)(byte*)offset < length)
                {
                    lengthToExamine = GetByteVector128SpanLength(offset, length);

                    Vector128<byte> values0 = Vector128.Create(value0);
                    Vector128<byte> values1 = Vector128.Create(value1);

                    while ((byte*)lengthToExamine > (byte*)offset)
                    {
                        Vector128<byte> search = LoadVector128(ref searchSpace, offset);
                        // Same method as above
                        int matches = Sse2.MoveMask(
                            Sse2.Or(
                                Sse2.CompareEqual(values0, search),
                                Sse2.CompareEqual(values1, search)));
                        if (0u >= (uint)matches)
                        {
                            // Zero flags set so no matches
                            offset += Vector128<byte>.Count;
                            continue;
                        }

                        // Find bitflag offset of first match and add to current offset
                        return ((int)(byte*)offset) + BitOperations.TrailingZeroCount(matches);
                    }

                    if ((int)(byte*)offset < length)
                    {
                        lengthToExamine = (IntPtr)(length - (int)(byte*)offset);
                        goto SequentialScan;
                    }
                }
            }
            else
#endif
            if (Vector.IsHardwareAccelerated && ((int)(byte*)offset < length))
            {
                lengthToExamine = GetByteVectorSpanLength(offset, length);

                Vector<byte> values0 = new Vector<byte>(value0);
                Vector<byte> values1 = new Vector<byte>(value1);

                while ((byte*)lengthToExamine > (byte*)offset)
                {
                    Vector<byte> search = LoadVector(ref searchSpace, offset);
                    var matches = Vector.BitwiseOr(
                                    Vector.Equals(search, values0),
                                    Vector.Equals(search, values1));
                    if (Vector<byte>.Zero.Equals(matches))
                    {
                        offset += Vector<byte>.Count;
                        continue;
                    }

                    // Find offset of first match and add to current offset
                    return (int)(byte*)offset + LocateFirstFoundByte(matches);
                }

                if ((int)(byte*)offset < length)
                {
                    lengthToExamine = (IntPtr)(length - (int)(byte*)offset);
                    goto SequentialScan;
                }
            }
            return -1;
        Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)offset;
        Found1:
            return (int)(byte*)(offset + 1);
        Found2:
            return (int)(byte*)(offset + 2);
        Found3:
            return (int)(byte*)(offset + 3);
        Found4:
            return (int)(byte*)(offset + 4);
        Found5:
            return (int)(byte*)(offset + 5);
        Found6:
            return (int)(byte*)(offset + 6);
        Found7:
            return (int)(byte*)(offset + 7);
        }

        //[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe int IndexOfAny(ref byte searchSpace, byte value0, byte value1, byte value2, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue0 = value0; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            uint uValue1 = value1;
            uint uValue2 = value2;
            IntPtr offset = (IntPtr)0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr lengthToExamine = (IntPtr)length;

#if NETCOREAPP_3_0_GREATER
            if (Avx2.IsSupported || Sse2.IsSupported)
            {
                // Avx2 branch also operates on Sse2 sizes, so check is combined.
                if (length >= Vector128<byte>.Count * 2)
                {
                    lengthToExamine = UnalignedCountVector128(ref searchSpace);
                }
            }
            else
#endif
            if (Vector.IsHardwareAccelerated && length >= Vector<byte>.Count * 2)
            {
                lengthToExamine = UnalignedCountVector(ref searchSpace);
            }
        SequentialScan:
            uint lookUp;
            while ((byte*)lengthToExamine >= (byte*)8)
            {
                lengthToExamine -= 8;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 4);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found4;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 5);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found5;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 6);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found6;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 7);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found7;

                offset += 8;
            }

            if ((byte*)lengthToExamine >= (byte*)4)
            {
                lengthToExamine -= 4;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found3;

                offset += 4;
            }

            while ((byte*)lengthToExamine > (byte*)0)
            {
                lengthToExamine -= 1;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found;

                offset += 1;
            }

#if NETCOREAPP_3_0_GREATER
            if (Avx2.IsSupported)
            {
                if ((int)(byte*)offset < length)
                {
                    lengthToExamine = GetByteVector256SpanLength(offset, length);
                    if ((byte*)lengthToExamine > (byte*)offset)
                    {
                        Vector256<byte> values0 = Vector256.Create(value0);
                        Vector256<byte> values1 = Vector256.Create(value1);
                        Vector256<byte> values2 = Vector256.Create(value2);
                        do
                        {
                            Vector256<byte> search = LoadVector256(ref searchSpace, offset);

                            Vector256<byte> matches0 = Avx2.CompareEqual(values0, search);
                            Vector256<byte> matches1 = Avx2.CompareEqual(values1, search);
                            Vector256<byte> matches2 = Avx2.CompareEqual(values2, search);
                            // Bitwise Or to combine the matches and MoveMask to convert them to bitflags
                            int matches = Avx2.MoveMask(Avx2.Or(Avx2.Or(matches0, matches1), matches2));
                            // Note that MoveMask has converted the equal vector elements into a set of bit flags,
                            // So the bit position in 'matches' corresponds to the element offset.
                            if (0u >= (uint)matches)
                            {
                                // Zero flags set so no matches
                                offset += Vector256<byte>.Count;
                                continue;
                            }

                            // Find bitflag offset of first match and add to current offset
                            return ((int)(byte*)offset) + BitOperations.TrailingZeroCount(matches);
                        } while ((byte*)lengthToExamine > (byte*)offset);
                    }

                    lengthToExamine = GetByteVector128SpanLength(offset, length);
                    if ((byte*)lengthToExamine > (byte*)offset)
                    {
                        Vector128<byte> values0 = Vector128.Create(value0);
                        Vector128<byte> values1 = Vector128.Create(value1);
                        Vector128<byte> values2 = Vector128.Create(value2);

                        Vector128<byte> search = LoadVector128(ref searchSpace, offset);

                        Vector128<byte> matches0 = Sse2.CompareEqual(values0, search);
                        Vector128<byte> matches1 = Sse2.CompareEqual(values1, search);
                        Vector128<byte> matches2 = Sse2.CompareEqual(values2, search);
                        // Same method as above
                        int matches = Sse2.MoveMask(Sse2.Or(Sse2.Or(matches0, matches1), matches2));
                        if (0u >= (uint)matches)
                        {
                            // Zero flags set so no matches
                            offset += Vector128<byte>.Count;
                        }
                        else
                        {
                            // Find bitflag offset of first match and add to current offset
                            return ((int)(byte*)offset) + BitOperations.TrailingZeroCount(matches);
                        }
                    }

                    if ((int)(byte*)offset < length)
                    {
                        lengthToExamine = (IntPtr)(length - (int)(byte*)offset);
                        goto SequentialScan;
                    }
                }
            }
            else if (Sse2.IsSupported)
            {
                if ((int)(byte*)offset < length)
                {
                    lengthToExamine = GetByteVector128SpanLength(offset, length);

                    Vector128<byte> values0 = Vector128.Create(value0);
                    Vector128<byte> values1 = Vector128.Create(value1);
                    Vector128<byte> values2 = Vector128.Create(value2);

                    while ((byte*)lengthToExamine > (byte*)offset)
                    {
                        Vector128<byte> search = LoadVector128(ref searchSpace, offset);

                        Vector128<byte> matches0 = Sse2.CompareEqual(values0, search);
                        Vector128<byte> matches1 = Sse2.CompareEqual(values1, search);
                        Vector128<byte> matches2 = Sse2.CompareEqual(values2, search);
                        // Same method as above
                        int matches = Sse2.MoveMask(Sse2.Or(Sse2.Or(matches0, matches1), matches2));
                        if (0u >= (uint)matches)
                        {
                            // Zero flags set so no matches
                            offset += Vector128<byte>.Count;
                            continue;
                        }

                        // Find bitflag offset of first match and add to current offset
                        return ((int)(byte*)offset) + BitOperations.TrailingZeroCount(matches);
                    }

                    if ((int)(byte*)offset < length)
                    {
                        lengthToExamine = (IntPtr)(length - (int)(byte*)offset);
                        goto SequentialScan;
                    }
                }
            }
            else
#endif
            if (Vector.IsHardwareAccelerated && ((int)(byte*)offset < length))
            {
                lengthToExamine = GetByteVectorSpanLength(offset, length);

                Vector<byte> values0 = new Vector<byte>(value0);
                Vector<byte> values1 = new Vector<byte>(value1);
                Vector<byte> values2 = new Vector<byte>(value2);

                while ((byte*)lengthToExamine > (byte*)offset)
                {
                    Vector<byte> search = LoadVector(ref searchSpace, offset);

                    var matches = Vector.BitwiseOr(
                                    Vector.BitwiseOr(
                                        Vector.Equals(search, values0),
                                        Vector.Equals(search, values1)),
                                    Vector.Equals(search, values2));

                    if (Vector<byte>.Zero.Equals(matches))
                    {
                        offset += Vector<byte>.Count;
                        continue;
                    }

                    // Find offset of first match and add to current offset
                    return (int)(byte*)offset + LocateFirstFoundByte(matches);
                }

                if ((int)(byte*)offset < length)
                {
                    lengthToExamine = (IntPtr)(length - (int)(byte*)offset);
                    goto SequentialScan;
                }
            }
            return -1;
        Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)offset;
        Found1:
            return (int)(byte*)(offset + 1);
        Found2:
            return (int)(byte*)(offset + 2);
        Found3:
            return (int)(byte*)(offset + 3);
        Found4:
            return (int)(byte*)(offset + 4);
        Found5:
            return (int)(byte*)(offset + 5);
        Found6:
            return (int)(byte*)(offset + 6);
        Found7:
            return (int)(byte*)(offset + 7);
        }

        #endregion

        #region -- LastIndexOf --

        public static int LastIndexOf(ref byte searchSpace, int searchSpaceLength, ref byte value, int valueLength)
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

            byte valueHead = value;
            ref byte valueTail = ref Unsafe.Add(ref value, 1);
            int valueTailLength = valueLength - 1;

            int offset = 0;
            for (; ; )
            {
                Debug.Assert(0 <= offset && offset <= searchSpaceLength); // Ensures no deceptive underflows in the computation of "remainingSearchSpaceLength".
                int remainingSearchSpaceLength = searchSpaceLength - offset - valueTailLength;
                if ((uint)(remainingSearchSpaceLength - 1) > SharedConstants.TooBigOrNegative) // <= 0
                    break;  // The unsearched portion is now shorter than the sequence we're looking for. So it can't be there.

                // Do a quick search for the first element of "value".
                int relativeIndex = LastIndexOf(ref searchSpace, valueHead, remainingSearchSpaceLength);
                if (relativeIndex == -1)
                    break;

                // Found the first element of "value". See if the tail matches.
                if (SequenceEqual(ref Unsafe.Add(ref searchSpace, relativeIndex + 1), ref valueTail, valueTailLength))
                    return relativeIndex;  // The tail matched. Return a successful find.

                offset += remainingSearchSpaceLength - relativeIndex;
            }
            return -1;
        }

        //[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe int LastIndexOf(ref byte searchSpace, byte value, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue = value; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            IntPtr offset = (IntPtr)length; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr lengthToExamine = (IntPtr)length;

            if (Vector.IsHardwareAccelerated && length >= Vector<byte>.Count * 2)
            {
                lengthToExamine = UnalignedCountVectorFromEnd(ref searchSpace, length);
            }
        SequentialScan:
            while ((byte*)lengthToExamine >= (byte*)8)
            {
                lengthToExamine -= 8;
                offset -= 8;

                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 7))
                    goto Found7;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 6))
                    goto Found6;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 5))
                    goto Found5;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 4))
                    goto Found4;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 3))
                    goto Found3;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 2))
                    goto Found2;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 1))
                    goto Found1;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset))
                    goto Found;
            }

            if ((byte*)lengthToExamine >= (byte*)4)
            {
                lengthToExamine -= 4;
                offset -= 4;

                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 3))
                    goto Found3;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 2))
                    goto Found2;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 1))
                    goto Found1;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset))
                    goto Found;
            }

            while ((byte*)lengthToExamine > (byte*)0)
            {
                lengthToExamine -= 1;
                offset -= 1;

                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset))
                    goto Found;
            }

            if (Vector.IsHardwareAccelerated && ((byte*)offset > (byte*)0))
            {
                lengthToExamine = (IntPtr)((int)(byte*)offset & ~(Vector<byte>.Count - 1));

                Vector<byte> values = new Vector<byte>(value);

                while ((byte*)lengthToExamine > (byte*)(Vector<byte>.Count - 1))
                {
                    var matches = Vector.Equals(values, LoadVector(ref searchSpace, offset - Vector<byte>.Count));
                    if (Vector<byte>.Zero.Equals(matches))
                    {
                        offset -= Vector<byte>.Count;
                        lengthToExamine -= Vector<byte>.Count;
                        continue;
                    }

                    // Find offset of first match and add to current offset
                    return (int)(offset) - Vector<byte>.Count + LocateLastFoundByte(matches);
                }
                if ((byte*)offset > (byte*)0)
                {
                    lengthToExamine = offset;
                    goto SequentialScan;
                }
            }
            return -1;
        Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)offset;
        Found1:
            return (int)(byte*)(offset + 1);
        Found2:
            return (int)(byte*)(offset + 2);
        Found3:
            return (int)(byte*)(offset + 3);
        Found4:
            return (int)(byte*)(offset + 4);
        Found5:
            return (int)(byte*)(offset + 5);
        Found6:
            return (int)(byte*)(offset + 6);
        Found7:
            return (int)(byte*)(offset + 7);
        }

        #endregion

        #region -- LastIndexOfAny --

        public static int LastIndexOfAny(ref byte searchSpace, int searchSpaceLength, ref byte value, int valueLength)
        {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            if (0u >= (uint)valueLength)
                return -1;  // A zero-length set of values is always treated as "not found".

            int offset = -1;
            for (int i = 0; i < valueLength; i++)
            {
                var tempIndex = LastIndexOf(ref searchSpace, Unsafe.Add(ref value, i), searchSpaceLength);
                if (tempIndex > offset)
                    offset = tempIndex;
            }
            return offset;
        }

        public static unsafe int LastIndexOfAny(ref byte searchSpace, byte value0, byte value1, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue0 = value0; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            uint uValue1 = value1;
            IntPtr offset = (IntPtr)length; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr lengthToExamine = (IntPtr)length;

            if (Vector.IsHardwareAccelerated && length >= Vector<byte>.Count * 2)
            {
                lengthToExamine = UnalignedCountVectorFromEnd(ref searchSpace, length);
            }
        SequentialScan:
            uint lookUp;
            while ((byte*)lengthToExamine >= (byte*)8)
            {
                lengthToExamine -= 8;
                offset -= 8;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 7);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found7;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 6);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found6;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 5);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found5;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 4);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found4;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found;
            }

            if ((byte*)lengthToExamine >= (byte*)4)
            {
                lengthToExamine -= 4;
                offset -= 4;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found;
            }

            while ((byte*)lengthToExamine > (byte*)0)
            {
                lengthToExamine -= 1;
                offset -= 1;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found;
            }

            if (Vector.IsHardwareAccelerated && ((byte*)offset > (byte*)0))
            {
                lengthToExamine = (IntPtr)((int)(byte*)offset & ~(Vector<byte>.Count - 1));

                Vector<byte> values0 = new Vector<byte>(value0);
                Vector<byte> values1 = new Vector<byte>(value1);

                while ((byte*)lengthToExamine > (byte*)(Vector<byte>.Count - 1))
                {
                    Vector<byte> search = LoadVector(ref searchSpace, offset - Vector<byte>.Count);
                    var matches = Vector.BitwiseOr(
                                    Vector.Equals(search, values0),
                                    Vector.Equals(search, values1));
                    if (Vector<byte>.Zero.Equals(matches))
                    {
                        offset -= Vector<byte>.Count;
                        lengthToExamine -= Vector<byte>.Count;
                        continue;
                    }

                    // Find offset of first match and add to current offset
                    return (int)(offset) - Vector<byte>.Count + LocateLastFoundByte(matches);
                }

                if ((byte*)offset > (byte*)0)
                {
                    lengthToExamine = offset;
                    goto SequentialScan;
                }
            }
            return -1;
        Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)offset;
        Found1:
            return (int)(byte*)(offset + 1);
        Found2:
            return (int)(byte*)(offset + 2);
        Found3:
            return (int)(byte*)(offset + 3);
        Found4:
            return (int)(byte*)(offset + 4);
        Found5:
            return (int)(byte*)(offset + 5);
        Found6:
            return (int)(byte*)(offset + 6);
        Found7:
            return (int)(byte*)(offset + 7);
        }

        public static unsafe int LastIndexOfAny(ref byte searchSpace, byte value0, byte value1, byte value2, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue0 = value0; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            uint uValue1 = value1;
            uint uValue2 = value2;
            IntPtr offset = (IntPtr)length; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr lengthToExamine = (IntPtr)length;

            if (Vector.IsHardwareAccelerated && length >= Vector<byte>.Count * 2)
            {
                lengthToExamine = UnalignedCountVectorFromEnd(ref searchSpace, length);
            }
        SequentialScan:
            uint lookUp;
            while ((byte*)lengthToExamine >= (byte*)8)
            {
                lengthToExamine -= 8;
                offset -= 8;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 7);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found7;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 6);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found6;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 5);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found5;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 4);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found4;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found;
            }

            if ((byte*)lengthToExamine >= (byte*)4)
            {
                lengthToExamine -= 4;
                offset -= 4;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found;
            }

            while ((byte*)lengthToExamine > (byte*)0)
            {
                lengthToExamine -= 1;
                offset -= 1;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found;
            }

            if (Vector.IsHardwareAccelerated && ((byte*)offset > (byte*)0))
            {
                lengthToExamine = (IntPtr)((int)(byte*)offset & ~(Vector<byte>.Count - 1));

                Vector<byte> values0 = new Vector<byte>(value0);
                Vector<byte> values1 = new Vector<byte>(value1);
                Vector<byte> values2 = new Vector<byte>(value2);

                while ((byte*)lengthToExamine > (byte*)(Vector<byte>.Count - 1))
                {
                    Vector<byte> search = LoadVector(ref searchSpace, offset - Vector<byte>.Count);

                    var matches = Vector.BitwiseOr(
                                    Vector.BitwiseOr(
                                        Vector.Equals(search, values0),
                                        Vector.Equals(search, values1)),
                                    Vector.Equals(search, values2));

                    if (Vector<byte>.Zero.Equals(matches))
                    {
                        offset -= Vector<byte>.Count;
                        lengthToExamine -= Vector<byte>.Count;
                        continue;
                    }

                    // Find offset of first match and add to current offset
                    return (int)(offset) - Vector<byte>.Count + LocateLastFoundByte(matches);
                }

                if ((byte*)offset > (byte*)0)
                {
                    lengthToExamine = offset;
                    goto SequentialScan;
                }
            }
            return -1;
        Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)offset;
        Found1:
            return (int)(byte*)(offset + 1);
        Found2:
            return (int)(byte*)(offset + 2);
        Found3:
            return (int)(byte*)(offset + 3);
        Found4:
            return (int)(byte*)(offset + 4);
        Found5:
            return (int)(byte*)(offset + 5);
        Found6:
            return (int)(byte*)(offset + 6);
        Found7:
            return (int)(byte*)(offset + 7);
        }

        #endregion

        #region -- IndexOfOrLessThan --

        public static unsafe int IndexOfOrLessThan(ref byte searchSpace, byte value, byte lessThan, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue = value; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            uint uLessThan = lessThan;
            IntPtr offset = (IntPtr)0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr lengthToExamine = (IntPtr)length;

            if (Vector.IsHardwareAccelerated && length >= Vector<byte>.Count * 2)
            {
                lengthToExamine = UnalignedCountVector(ref searchSpace);
            }
        SequentialScan:
            uint lookUp;
            while ((byte*)lengthToExamine >= (byte*)8)
            {
                lengthToExamine -= 8;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue == lookUp || uLessThan > lookUp)
                    goto Found;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue == lookUp || uLessThan > lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue == lookUp || uLessThan > lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue == lookUp || uLessThan > lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 4);
                if (uValue == lookUp || uLessThan > lookUp)
                    goto Found4;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 5);
                if (uValue == lookUp || uLessThan > lookUp)
                    goto Found5;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 6);
                if (uValue == lookUp || uLessThan > lookUp)
                    goto Found6;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 7);
                if (uValue == lookUp || uLessThan > lookUp)
                    goto Found7;

                offset += 8;
            }

            if ((byte*)lengthToExamine >= (byte*)4)
            {
                lengthToExamine -= 4;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue == lookUp || uLessThan > lookUp)
                    goto Found;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue == lookUp || uLessThan > lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue == lookUp || uLessThan > lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue == lookUp || uLessThan > lookUp)
                    goto Found3;

                offset += 4;
            }

            while ((byte*)lengthToExamine > (byte*)0)
            {
                lengthToExamine -= 1;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue == lookUp || uLessThan > lookUp)
                    goto Found;

                offset += 1;
            }

            if (Vector.IsHardwareAccelerated && ((int)(byte*)offset < length))
            {
                lengthToExamine = GetByteVectorSpanLength(offset, length);

                // Get comparison Vector
                Vector<byte> values = new Vector<byte>(value);
                Vector<byte> valuesLessThan = new Vector<byte>(lessThan);

                while ((byte*)lengthToExamine > (byte*)offset)
                {
                    Vector<byte> search = LoadVector(ref searchSpace, offset);
                    var matches = Vector.BitwiseOr(
                                    Vector.Equals(search, values),
                                    Vector.LessThan(search, valuesLessThan));
                    if (Vector<byte>.Zero.Equals(matches))
                    {
                        offset += Vector<byte>.Count;
                        continue;
                    }

                    // Find offset of first match
                    return (int)(byte*)offset + LocateFirstFoundByte(matches);
                }

                if ((int)(byte*)offset < length)
                {
                    lengthToExamine = (IntPtr)(length - (int)(byte*)offset);
                    goto SequentialScan;
                }
            }
            return -1;
        Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)offset;
        Found1:
            return (int)(byte*)(offset + 1);
        Found2:
            return (int)(byte*)(offset + 2);
        Found3:
            return (int)(byte*)(offset + 3);
        Found4:
            return (int)(byte*)(offset + 4);
        Found5:
            return (int)(byte*)(offset + 5);
        Found6:
            return (int)(byte*)(offset + 6);
        Found7:
            return (int)(byte*)(offset + 7);
        }

        #endregion

        #region -- IndexOfAnyOrLessThan --

        public static unsafe int IndexOfAnyOrLessThan(ref byte searchSpace, byte value0, byte value1, byte lessThan, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue0 = value0; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            uint uValue1 = value1;
            uint uLessThan = lessThan;
            IntPtr offset = (IntPtr)0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr lengthToExamine = (IntPtr)length;

            if (Vector.IsHardwareAccelerated && length >= Vector<byte>.Count * 2)
            {
                lengthToExamine = UnalignedCountVector(ref searchSpace);
            }
        SequentialScan:
            uint lookUp;
            while ((byte*)lengthToExamine >= (byte*)8)
            {
                lengthToExamine -= 8;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp || uLessThan > lookUp)
                    goto Found;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 == lookUp || uValue1 == lookUp || uLessThan > lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 == lookUp || uValue1 == lookUp || uLessThan > lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 == lookUp || uValue1 == lookUp || uLessThan > lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 4);
                if (uValue0 == lookUp || uValue1 == lookUp || uLessThan > lookUp)
                    goto Found4;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 5);
                if (uValue0 == lookUp || uValue1 == lookUp || uLessThan > lookUp)
                    goto Found5;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 6);
                if (uValue0 == lookUp || uValue1 == lookUp || uLessThan > lookUp)
                    goto Found6;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 7);
                if (uValue0 == lookUp || uValue1 == lookUp || uLessThan > lookUp)
                    goto Found7;

                offset += 8;
            }

            if ((byte*)lengthToExamine >= (byte*)4)
            {
                lengthToExamine -= 4;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp || uLessThan > lookUp)
                    goto Found;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 == lookUp || uValue1 == lookUp || uLessThan > lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 == lookUp || uValue1 == lookUp || uLessThan > lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 == lookUp || uValue1 == lookUp || uLessThan > lookUp)
                    goto Found3;

                offset += 4;
            }

            while ((byte*)lengthToExamine > (byte*)0)
            {
                lengthToExamine -= 1;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp || uLessThan > lookUp)
                    goto Found;

                offset += 1;
            }

            if (Vector.IsHardwareAccelerated && ((int)(byte*)offset < length))
            {
                lengthToExamine = GetByteVectorSpanLength(offset, length);

                // Get comparison Vector
                Vector<byte> values0 = new Vector<byte>(value0);
                Vector<byte> values1 = new Vector<byte>(value1);
                Vector<byte> valuesLessThan = new Vector<byte>(lessThan);

                while ((byte*)lengthToExamine > (byte*)offset)
                {
                    Vector<byte> search = LoadVector(ref searchSpace, offset);
                    var matches = Vector.BitwiseOr(
                                    Vector.BitwiseOr(
                                        Vector.Equals(search, values0),
                                        Vector.Equals(search, values1)),
                                    Vector.LessThan(search, valuesLessThan));

                    if (Vector<byte>.Zero.Equals(matches))
                    {
                        offset += Vector<byte>.Count;
                        continue;
                    }

                    // Find offset of first match
                    return (int)(byte*)offset + LocateFirstFoundByte(matches);
                }

                if ((int)(byte*)offset < length)
                {
                    lengthToExamine = (IntPtr)(length - (int)(byte*)offset);
                    goto SequentialScan;
                }
            }
            return -1;
        Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)offset;
        Found1:
            return (int)(byte*)(offset + 1);
        Found2:
            return (int)(byte*)(offset + 2);
        Found3:
            return (int)(byte*)(offset + 3);
        Found4:
            return (int)(byte*)(offset + 4);
        Found5:
            return (int)(byte*)(offset + 5);
        Found6:
            return (int)(byte*)(offset + 6);
        Found7:
            return (int)(byte*)(offset + 7);
        }

        public static unsafe int IndexOfAnyOrLessThan(ref byte searchSpace, byte value0, byte value1, byte value2, byte lessThan, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue0 = value0; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            uint uValue1 = value1;
            uint uValue2 = value2;
            uint uLessThan = lessThan; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            IntPtr offset = (IntPtr)0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr lengthToExamine = (IntPtr)length;

            if (Vector.IsHardwareAccelerated && length >= Vector<byte>.Count * 2)
            {
                lengthToExamine = UnalignedCountVector(ref searchSpace);
            }
        SequentialScan:
            uint lookUp;
            while ((byte*)lengthToExamine >= (byte*)8)
            {
                lengthToExamine -= 8;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp || uLessThan > lookUp)
                    goto Found;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp || uLessThan > lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp || uLessThan > lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp || uLessThan > lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 4);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp || uLessThan > lookUp)
                    goto Found4;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 5);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp || uLessThan > lookUp)
                    goto Found5;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 6);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp || uLessThan > lookUp)
                    goto Found6;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 7);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp || uLessThan > lookUp)
                    goto Found7;

                offset += 8;
            }

            if ((byte*)lengthToExamine >= (byte*)4)
            {
                lengthToExamine -= 4;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp || uLessThan > lookUp)
                    goto Found;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp || uLessThan > lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp || uLessThan > lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp || uLessThan > lookUp)
                    goto Found3;

                offset += 4;
            }

            while ((byte*)lengthToExamine > (byte*)0)
            {
                lengthToExamine -= 1;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp || uLessThan > lookUp)
                    goto Found;

                offset += 1;
            }

            if (Vector.IsHardwareAccelerated && ((int)(byte*)offset < length))
            {
                lengthToExamine = GetByteVectorSpanLength(offset, length);

                // Get comparison Vector
                Vector<byte> values0 = new Vector<byte>(value0);
                Vector<byte> values1 = new Vector<byte>(value1);
                Vector<byte> values2 = new Vector<byte>(value2);
                Vector<byte> valuesLessThan = new Vector<byte>(lessThan);

                while ((byte*)lengthToExamine > (byte*)offset)
                {
                    Vector<byte> search = LoadVector(ref searchSpace, offset);
                    var matches = Vector.BitwiseOr(
                                    Vector.BitwiseOr(
                                        Vector.BitwiseOr(
                                            Vector.Equals(search, values0),
                                            Vector.Equals(search, values1)),
                                        Vector.Equals(search, values2)),
                                    Vector.LessThan(search, valuesLessThan));

                    if (Vector<byte>.Zero.Equals(matches))
                    {
                        offset += Vector<byte>.Count;
                        continue;
                    }

                    // Find offset of first match
                    return (int)(byte*)offset + LocateFirstFoundByte(matches);
                }

                if ((int)(byte*)offset < length)
                {
                    lengthToExamine = (IntPtr)(length - (int)(byte*)offset);
                    goto SequentialScan;
                }
            }
            return -1;
        Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)offset;
        Found1:
            return (int)(byte*)(offset + 1);
        Found2:
            return (int)(byte*)(offset + 2);
        Found3:
            return (int)(byte*)(offset + 3);
        Found4:
            return (int)(byte*)(offset + 4);
        Found5:
            return (int)(byte*)(offset + 5);
        Found6:
            return (int)(byte*)(offset + 6);
        Found7:
            return (int)(byte*)(offset + 7);
        }

        #endregion

        #region -- IndexNotOf --

        public static unsafe int IndexNotOf(ref byte searchSpace, byte value, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue = value; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            IntPtr offset = (IntPtr)0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr lengthToExamine = (IntPtr)length;

            while ((byte*)lengthToExamine >= (byte*)8)
            {
                lengthToExamine -= 8;

                if (uValue != Unsafe.AddByteOffset(ref searchSpace, offset))
                    goto Found;
                if (uValue != Unsafe.AddByteOffset(ref searchSpace, offset + 1))
                    goto Found1;
                if (uValue != Unsafe.AddByteOffset(ref searchSpace, offset + 2))
                    goto Found2;
                if (uValue != Unsafe.AddByteOffset(ref searchSpace, offset + 3))
                    goto Found3;
                if (uValue != Unsafe.AddByteOffset(ref searchSpace, offset + 4))
                    goto Found4;
                if (uValue != Unsafe.AddByteOffset(ref searchSpace, offset + 5))
                    goto Found5;
                if (uValue != Unsafe.AddByteOffset(ref searchSpace, offset + 6))
                    goto Found6;
                if (uValue != Unsafe.AddByteOffset(ref searchSpace, offset + 7))
                    goto Found7;

                offset += 8;
            }

            if ((byte*)lengthToExamine >= (byte*)4)
            {
                lengthToExamine -= 4;

                if (uValue != Unsafe.AddByteOffset(ref searchSpace, offset))
                    goto Found;
                if (uValue != Unsafe.AddByteOffset(ref searchSpace, offset + 1))
                    goto Found1;
                if (uValue != Unsafe.AddByteOffset(ref searchSpace, offset + 2))
                    goto Found2;
                if (uValue != Unsafe.AddByteOffset(ref searchSpace, offset + 3))
                    goto Found3;

                offset += 4;
            }

            while ((byte*)lengthToExamine > (byte*)0)
            {
                lengthToExamine -= 1;

                if (uValue != Unsafe.AddByteOffset(ref searchSpace, offset))
                    goto Found;

                offset += 1;
            }

            return -1;
        Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)offset;
        Found1:
            return (int)(byte*)(offset + 1);
        Found2:
            return (int)(byte*)(offset + 2);
        Found3:
            return (int)(byte*)(offset + 3);
        Found4:
            return (int)(byte*)(offset + 4);
        Found5:
            return (int)(byte*)(offset + 5);
        Found6:
            return (int)(byte*)(offset + 6);
        Found7:
            return (int)(byte*)(offset + 7);

        }

        #endregion

        #region -- IndexNotOfAny --

        public static unsafe int IndexNotOfAny(ref byte searchSpace, int searchSpaceLength, ref byte value, int valueLength)
        {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            if (0u >= (uint)valueLength)
                return -1;  // A zero-length set of values is always treated as "not found".

            IntPtr offset = (IntPtr)0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr lengthToExamine = (IntPtr)searchSpaceLength;

            byte lookUp;
            while ((byte*)lengthToExamine >= (byte*)8)
            {
                lengthToExamine -= 8;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if ((uint)IndexOf(ref value, lookUp, valueLength) > SharedConstants.TooBigOrNegative)
                    goto Found;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if ((uint)IndexOf(ref value, lookUp, valueLength) > SharedConstants.TooBigOrNegative)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if ((uint)IndexOf(ref value, lookUp, valueLength) > SharedConstants.TooBigOrNegative)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if ((uint)IndexOf(ref value, lookUp, valueLength) > SharedConstants.TooBigOrNegative)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 4);
                if ((uint)IndexOf(ref value, lookUp, valueLength) > SharedConstants.TooBigOrNegative)
                    goto Found4;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 5);
                if ((uint)IndexOf(ref value, lookUp, valueLength) > SharedConstants.TooBigOrNegative)
                    goto Found5;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 6);
                if ((uint)IndexOf(ref value, lookUp, valueLength) > SharedConstants.TooBigOrNegative)
                    goto Found6;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 7);
                if ((uint)IndexOf(ref value, lookUp, valueLength) > SharedConstants.TooBigOrNegative)
                    goto Found7;

                offset += 8;
            }

            if ((byte*)lengthToExamine >= (byte*)4)
            {
                lengthToExamine -= 4;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if ((uint)IndexOf(ref value, lookUp, valueLength) > SharedConstants.TooBigOrNegative)
                    goto Found;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if ((uint)IndexOf(ref value, lookUp, valueLength) > SharedConstants.TooBigOrNegative)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if ((uint)IndexOf(ref value, lookUp, valueLength) > SharedConstants.TooBigOrNegative)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if ((uint)IndexOf(ref value, lookUp, valueLength) > SharedConstants.TooBigOrNegative)
                    goto Found3;

                offset += 4;
            }

            while ((byte*)lengthToExamine > (byte*)0)
            {
                lengthToExamine -= 1;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if ((uint)IndexOf(ref value, lookUp, valueLength) > SharedConstants.TooBigOrNegative)
                    goto Found;

                offset += 1;
            }

            return -1;
        Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)offset;
        Found1:
            return (int)(byte*)(offset + 1);
        Found2:
            return (int)(byte*)(offset + 2);
        Found3:
            return (int)(byte*)(offset + 3);
        Found4:
            return (int)(byte*)(offset + 4);
        Found5:
            return (int)(byte*)(offset + 5);
        Found6:
            return (int)(byte*)(offset + 6);
        Found7:
            return (int)(byte*)(offset + 7);

        }

        public static unsafe int IndexNotOfAny(ref byte searchSpace, byte value0, byte value1, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue0 = value0; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            uint uValue1 = value1; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            IntPtr offset = (IntPtr)0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr lengthToExamine = (IntPtr)length;

            uint lookUp;
            while ((byte*)lengthToExamine >= (byte*)8)
            {
                lengthToExamine -= 8;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 != lookUp && uValue1 != lookUp)
                    goto Found;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 != lookUp && uValue1 != lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 != lookUp && uValue1 != lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 != lookUp && uValue1 != lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 4);
                if (uValue0 != lookUp && uValue1 != lookUp)
                    goto Found4;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 5);
                if (uValue0 != lookUp && uValue1 != lookUp)
                    goto Found5;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 6);
                if (uValue0 != lookUp && uValue1 != lookUp)
                    goto Found6;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 7);
                if (uValue0 != lookUp && uValue1 != lookUp)
                    goto Found7;

                offset += 8;
            }

            if ((byte*)lengthToExamine >= (byte*)4)
            {
                lengthToExamine -= 4;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 != lookUp && uValue1 != lookUp)
                    goto Found;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 != lookUp && uValue1 != lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 != lookUp && uValue1 != lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 != lookUp && uValue1 != lookUp)
                    goto Found3;

                offset += 4;
            }

            while ((byte*)lengthToExamine > (byte*)0)
            {
                lengthToExamine -= 1;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 != lookUp && uValue1 != lookUp)
                    goto Found;

                offset += 1;
            }

            return -1;
        Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)offset;
        Found1:
            return (int)(byte*)(offset + 1);
        Found2:
            return (int)(byte*)(offset + 2);
        Found3:
            return (int)(byte*)(offset + 3);
        Found4:
            return (int)(byte*)(offset + 4);
        Found5:
            return (int)(byte*)(offset + 5);
        Found6:
            return (int)(byte*)(offset + 6);
        Found7:
            return (int)(byte*)(offset + 7);
        }

        public static unsafe int IndexNotOfAny(ref byte searchSpace, byte value0, byte value1, byte value2, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue0 = value0; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            uint uValue1 = value1;
            uint uValue2 = value2;
            IntPtr offset = (IntPtr)0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr lengthToExamine = (IntPtr)length;

            uint lookUp;
            while ((byte*)lengthToExamine >= (byte*)8)
            {
                lengthToExamine -= 8;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp)
                    goto Found;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 4);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp)
                    goto Found4;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 5);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp)
                    goto Found5;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 6);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp)
                    goto Found6;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 7);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp)
                    goto Found7;

                offset += 8;
            }

            if ((byte*)lengthToExamine >= (byte*)4)
            {
                lengthToExamine -= 4;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp)
                    goto Found;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp)
                    goto Found3;

                offset += 4;
            }

            while ((byte*)lengthToExamine > (byte*)0)
            {
                lengthToExamine -= 1;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp)
                    goto Found;

                offset += 1;
            }

            return -1;
        Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)offset;
        Found1:
            return (int)(byte*)(offset + 1);
        Found2:
            return (int)(byte*)(offset + 2);
        Found3:
            return (int)(byte*)(offset + 3);
        Found4:
            return (int)(byte*)(offset + 4);
        Found5:
            return (int)(byte*)(offset + 5);
        Found6:
            return (int)(byte*)(offset + 6);
        Found7:
            return (int)(byte*)(offset + 7);
        }

        public static unsafe int IndexNotOfAny(ref byte searchSpace, byte value0, byte value1, byte value2, byte value3, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue0 = value0; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            uint uValue1 = value1;
            uint uValue2 = value2;
            uint uValue3 = value3;
            IntPtr offset = (IntPtr)0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr lengthToExamine = (IntPtr)length;

            uint lookUp;
            while ((byte*)lengthToExamine >= (byte*)8)
            {
                lengthToExamine -= 8;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp && uValue3 != lookUp)
                    goto Found;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp && uValue3 != lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp && uValue3 != lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp && uValue3 != lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 4);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp && uValue3 != lookUp)
                    goto Found4;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 5);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp && uValue3 != lookUp)
                    goto Found5;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 6);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp && uValue3 != lookUp)
                    goto Found6;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 7);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp && uValue3 != lookUp)
                    goto Found7;

                offset += 8;
            }

            if ((byte*)lengthToExamine >= (byte*)4)
            {
                lengthToExamine -= 4;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp && uValue3 != lookUp)
                    goto Found;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp && uValue3 != lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp && uValue3 != lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp && uValue3 != lookUp)
                    goto Found3;

                offset += 4;
            }

            while ((byte*)lengthToExamine > (byte*)0)
            {
                lengthToExamine -= 1;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp && uValue3 != lookUp)
                    goto Found;

                offset += 1;
            }

            return -1;
        Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)offset;
        Found1:
            return (int)(byte*)(offset + 1);
        Found2:
            return (int)(byte*)(offset + 2);
        Found3:
            return (int)(byte*)(offset + 3);
        Found4:
            return (int)(byte*)(offset + 4);
        Found5:
            return (int)(byte*)(offset + 5);
        Found6:
            return (int)(byte*)(offset + 6);
        Found7:
            return (int)(byte*)(offset + 7);
        }

        #endregion

        #region -- LastIndexNotOf --

        public static unsafe int LastIndexNotOf(ref byte searchSpace, byte value, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue = value; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            IntPtr offset = (IntPtr)length; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr lengthToExamine = (IntPtr)length;

            while ((byte*)lengthToExamine >= (byte*)8)
            {
                lengthToExamine -= 8;
                offset -= 8;

                if (uValue != Unsafe.AddByteOffset(ref searchSpace, offset + 7))
                    goto Found7;
                if (uValue != Unsafe.AddByteOffset(ref searchSpace, offset + 6))
                    goto Found6;
                if (uValue != Unsafe.AddByteOffset(ref searchSpace, offset + 5))
                    goto Found5;
                if (uValue != Unsafe.AddByteOffset(ref searchSpace, offset + 4))
                    goto Found4;
                if (uValue != Unsafe.AddByteOffset(ref searchSpace, offset + 3))
                    goto Found3;
                if (uValue != Unsafe.AddByteOffset(ref searchSpace, offset + 2))
                    goto Found2;
                if (uValue != Unsafe.AddByteOffset(ref searchSpace, offset + 1))
                    goto Found1;
                if (uValue != Unsafe.AddByteOffset(ref searchSpace, offset))
                    goto Found;
            }

            if ((byte*)lengthToExamine >= (byte*)4)
            {
                lengthToExamine -= 4;
                offset -= 4;

                if (uValue != Unsafe.AddByteOffset(ref searchSpace, offset + 3))
                    goto Found3;
                if (uValue != Unsafe.AddByteOffset(ref searchSpace, offset + 2))
                    goto Found2;
                if (uValue != Unsafe.AddByteOffset(ref searchSpace, offset + 1))
                    goto Found1;
                if (uValue != Unsafe.AddByteOffset(ref searchSpace, offset))
                    goto Found;
            }

            while ((byte*)lengthToExamine > (byte*)0)
            {
                lengthToExamine -= 1;
                offset -= 1;

                if (uValue != Unsafe.AddByteOffset(ref searchSpace, offset))
                    goto Found;
            }

            return -1;
        Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)offset;
        Found1:
            return (int)(byte*)(offset + 1);
        Found2:
            return (int)(byte*)(offset + 2);
        Found3:
            return (int)(byte*)(offset + 3);
        Found4:
            return (int)(byte*)(offset + 4);
        Found5:
            return (int)(byte*)(offset + 5);
        Found6:
            return (int)(byte*)(offset + 6);
        Found7:
            return (int)(byte*)(offset + 7);
        }

        #endregion

        #region -- LastIndexNotOfAny--

        public static unsafe int LastIndexNotOfAny(ref byte searchSpace, int searchSpaceLength, ref byte value, int valueLength)
        {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            if (0u >= (uint)valueLength)
                return -1;  // A zero-length set of values is always treated as "not found".

            IntPtr offset = (IntPtr)0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr lengthToExamine = (IntPtr)searchSpaceLength;

            byte lookUp;
            while ((byte*)lengthToExamine >= (byte*)8)
            {
                lengthToExamine -= 8;
                offset -= 8;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 7);
                if ((uint)IndexOf(ref value, lookUp, valueLength) > SharedConstants.TooBigOrNegative)
                    goto Found7;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 6);
                if ((uint)IndexOf(ref value, lookUp, valueLength) > SharedConstants.TooBigOrNegative)
                    goto Found6;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 5);
                if ((uint)IndexOf(ref value, lookUp, valueLength) > SharedConstants.TooBigOrNegative)
                    goto Found5;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 4);
                if ((uint)IndexOf(ref value, lookUp, valueLength) > SharedConstants.TooBigOrNegative)
                    goto Found4;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if ((uint)IndexOf(ref value, lookUp, valueLength) > SharedConstants.TooBigOrNegative)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if ((uint)IndexOf(ref value, lookUp, valueLength) > SharedConstants.TooBigOrNegative)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if ((uint)IndexOf(ref value, lookUp, valueLength) > SharedConstants.TooBigOrNegative)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if ((uint)IndexOf(ref value, lookUp, valueLength) > SharedConstants.TooBigOrNegative)
                    goto Found;
            }

            if ((byte*)lengthToExamine >= (byte*)4)
            {
                lengthToExamine -= 4;
                offset -= 4;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if ((uint)IndexOf(ref value, lookUp, valueLength) > SharedConstants.TooBigOrNegative)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if ((uint)IndexOf(ref value, lookUp, valueLength) > SharedConstants.TooBigOrNegative)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if ((uint)IndexOf(ref value, lookUp, valueLength) > SharedConstants.TooBigOrNegative)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if ((uint)IndexOf(ref value, lookUp, valueLength) > SharedConstants.TooBigOrNegative)
                    goto Found;
            }

            while ((byte*)lengthToExamine > (byte*)0)
            {
                lengthToExamine -= 1;
                offset -= 1;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if ((uint)IndexOf(ref value, lookUp, valueLength) > SharedConstants.TooBigOrNegative)
                    goto Found;
            }

            return -1;
        Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)offset;
        Found1:
            return (int)(byte*)(offset + 1);
        Found2:
            return (int)(byte*)(offset + 2);
        Found3:
            return (int)(byte*)(offset + 3);
        Found4:
            return (int)(byte*)(offset + 4);
        Found5:
            return (int)(byte*)(offset + 5);
        Found6:
            return (int)(byte*)(offset + 6);
        Found7:
            return (int)(byte*)(offset + 7);
        }

        public static unsafe int LastIndexNotOfAny(ref byte searchSpace, byte value0, byte value1, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue0 = value0; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            uint uValue1 = value1;
            IntPtr offset = (IntPtr)length; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr lengthToExamine = (IntPtr)length;

            uint lookUp;
            while ((byte*)lengthToExamine >= (byte*)8)
            {
                lengthToExamine -= 8;
                offset -= 8;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 7);
                if (uValue0 != lookUp && uValue1 != lookUp)
                    goto Found7;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 6);
                if (uValue0 != lookUp && uValue1 != lookUp)
                    goto Found6;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 5);
                if (uValue0 != lookUp && uValue1 != lookUp)
                    goto Found5;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 4);
                if (uValue0 != lookUp && uValue1 != lookUp)
                    goto Found4;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 != lookUp && uValue1 != lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 != lookUp && uValue1 != lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 != lookUp && uValue1 != lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 != lookUp && uValue1 != lookUp)
                    goto Found;
            }

            if ((byte*)lengthToExamine >= (byte*)4)
            {
                lengthToExamine -= 4;
                offset -= 4;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 != lookUp && uValue1 != lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 != lookUp && uValue1 != lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 != lookUp && uValue1 != lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 != lookUp && uValue1 != lookUp)
                    goto Found;
            }

            while ((byte*)lengthToExamine > (byte*)0)
            {
                lengthToExamine -= 1;
                offset -= 1;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 != lookUp && uValue1 != lookUp)
                    goto Found;
            }

            return -1;
        Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)offset;
        Found1:
            return (int)(byte*)(offset + 1);
        Found2:
            return (int)(byte*)(offset + 2);
        Found3:
            return (int)(byte*)(offset + 3);
        Found4:
            return (int)(byte*)(offset + 4);
        Found5:
            return (int)(byte*)(offset + 5);
        Found6:
            return (int)(byte*)(offset + 6);
        Found7:
            return (int)(byte*)(offset + 7);
        }

        public static unsafe int LastIndexNotOfAny(ref byte searchSpace, byte value0, byte value1, byte value2, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue0 = value0; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            uint uValue1 = value1;
            uint uValue2 = value2;
            IntPtr offset = (IntPtr)length; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr lengthToExamine = (IntPtr)length;

            uint lookUp;
            while ((byte*)lengthToExamine >= (byte*)8)
            {
                lengthToExamine -= 8;
                offset -= 8;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 7);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp)
                    goto Found7;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 6);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp)
                    goto Found6;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 5);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp)
                    goto Found5;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 4);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp)
                    goto Found4;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp)
                    goto Found;
            }

            if ((byte*)lengthToExamine >= (byte*)4)
            {
                lengthToExamine -= 4;
                offset -= 4;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp)
                    goto Found;
            }

            while ((byte*)lengthToExamine > (byte*)0)
            {
                lengthToExamine -= 1;
                offset -= 1;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp)
                    goto Found;
            }

            return -1;
        Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)offset;
        Found1:
            return (int)(byte*)(offset + 1);
        Found2:
            return (int)(byte*)(offset + 2);
        Found3:
            return (int)(byte*)(offset + 3);
        Found4:
            return (int)(byte*)(offset + 4);
        Found5:
            return (int)(byte*)(offset + 5);
        Found6:
            return (int)(byte*)(offset + 6);
        Found7:
            return (int)(byte*)(offset + 7);
        }

        public static unsafe int LastIndexNotOfAny(ref byte searchSpace, byte value0, byte value1, byte value2, byte value3, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue0 = value0; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            uint uValue1 = value1;
            uint uValue2 = value2;
            uint uValue3 = value3;
            IntPtr offset = (IntPtr)length; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr lengthToExamine = (IntPtr)length;

            uint lookUp;
            while ((byte*)lengthToExamine >= (byte*)8)
            {
                lengthToExamine -= 8;
                offset -= 8;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 7);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp && uValue3 != lookUp)
                    goto Found7;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 6);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp && uValue3 != lookUp)
                    goto Found6;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 5);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp && uValue3 != lookUp)
                    goto Found5;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 4);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp && uValue3 != lookUp)
                    goto Found4;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp && uValue3 != lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp && uValue3 != lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp && uValue3 != lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp && uValue3 != lookUp)
                    goto Found;
            }

            if ((byte*)lengthToExamine >= (byte*)4)
            {
                lengthToExamine -= 4;
                offset -= 4;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp && uValue3 != lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp && uValue3 != lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp && uValue3 != lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp && uValue3 != lookUp)
                    goto Found;
            }

            while ((byte*)lengthToExamine > (byte*)0)
            {
                lengthToExamine -= 1;
                offset -= 1;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 != lookUp && uValue1 != lookUp && uValue2 != lookUp && uValue3 != lookUp)
                    goto Found;
            }

            return -1;
        Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)offset;
        Found1:
            return (int)(byte*)(offset + 1);
        Found2:
            return (int)(byte*)(offset + 2);
        Found3:
            return (int)(byte*)(offset + 3);
        Found4:
            return (int)(byte*)(offset + 4);
        Found5:
            return (int)(byte*)(offset + 5);
        Found6:
            return (int)(byte*)(offset + 6);
        Found7:
            return (int)(byte*)(offset + 7);
        }

        #endregion

        #region ** Helper **

        // Vector sub-search adapted from https://github.com/aspnet/KestrelHttpServer/pull/1138
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateFirstFoundByte(Vector<byte> match)
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
            return i * 8 + LocateFirstFoundByte(candidate);
        }

        // Vector sub-search adapted from https://github.com/aspnet/KestrelHttpServer/pull/1138
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateLastFoundByte(Vector<byte> match)
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
            return i * 8 + LocateLastFoundByte(candidate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateFirstFoundByte(ulong match)
        {
#if NETCOREAPP_3_0_GREATER
            if (Bmi1.X64.IsSupported)
            {
                return (int)(Bmi1.X64.TrailingZeroCount(match) >> 3);
            }
            else
            {
#endif
            // Flag least significant power of two bit
            var powerOfTwoFlag = match ^ (match - 1);
            // Shift all powers of two into the high byte and extract
            return (int)((powerOfTwoFlag * XorPowerOfTwoToHighByte) >> 57);
#if NETCOREAPP_3_0_GREATER
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateLastFoundByte(ulong match)
        {
#if NETCOREAPP_3_0_GREATER
            return 7 - (BitOperations.LeadingZeroCount(match) >> 3);
#else
            // Find the most significant byte that has its highest bit set
            int index = 7;
            while ((long)match > 0)
            {
                match = match << 8;
                index--;
            }
            return index;
#endif
        }

        private const ulong XorPowerOfTwoToHighByte = (0x07ul |
                                                       0x06ul << 8 |
                                                       0x05ul << 16 |
                                                       0x04ul << 24 |
                                                       0x03ul << 32 |
                                                       0x02ul << 40 |
                                                       0x01ul << 48) + 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector<byte> GetVector(byte vectorByte)
        {
#if NETSTANDARD2_0 || NET471 || NET451
            // Vector<byte> .ctor doesn't become an intrinsic due to detection issue
            // However this does cause it to become an intrinsic (with additional multiply and reg->reg copy)
            // https://github.com/dotnet/coreclr/issues/7459#issuecomment-253965670
            return Vector.AsVectorByte(new Vector<uint>(vectorByte * 0x01010101u));
#else
            return new Vector<byte>(vectorByte);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe UIntPtr LoadUIntPtr(ref byte start, IntPtr offset)
            => Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset(ref start, offset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Vector<byte> LoadVector(ref byte start, IntPtr offset)
            => Unsafe.ReadUnaligned<Vector<byte>>(ref Unsafe.AddByteOffset(ref start, offset));

#if NETCOREAPP_3_0_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Vector128<byte> LoadVector128(ref byte start, IntPtr offset)
            => Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.AddByteOffset(ref start, offset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Vector256<byte> LoadVector256(ref byte start, IntPtr offset)
            => Unsafe.ReadUnaligned<Vector256<byte>>(ref Unsafe.AddByteOffset(ref start, offset));
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe IntPtr GetByteVectorSpanLength(IntPtr offset, int length)
            => (IntPtr)((length - (int)(byte*)offset) & ~(Vector<byte>.Count - 1));

#if NETCOREAPP_3_0_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe IntPtr GetByteVector128SpanLength(IntPtr offset, int length)
            => (IntPtr)((length - (int)(byte*)offset) & ~(Vector128<byte>.Count - 1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe IntPtr GetByteVector256SpanLength(IntPtr offset, int length)
            => (IntPtr)((length - (int)(byte*)offset) & ~(Vector256<byte>.Count - 1));
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe IntPtr UnalignedCountVector(ref byte searchSpace)
        {
            int unaligned = (int)Unsafe.AsPointer(ref searchSpace) & (Vector<byte>.Count - 1);
            return (IntPtr)((Vector<byte>.Count - unaligned) & (Vector<byte>.Count - 1));
        }

#if NETCOREAPP_3_0_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe IntPtr UnalignedCountVector128(ref byte searchSpace)
        {
            int unaligned = (int)Unsafe.AsPointer(ref searchSpace) & (Vector128<byte>.Count - 1);
            return (IntPtr)((Vector128<byte>.Count - unaligned) & (Vector128<byte>.Count - 1));
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe IntPtr UnalignedCountVectorFromEnd(ref byte searchSpace, int length)
        {
            int unaligned = (int)Unsafe.AsPointer(ref searchSpace) & (Vector<byte>.Count - 1);
            return (IntPtr)(((length & (Vector<byte>.Count - 1)) + unaligned) & (Vector<byte>.Count - 1));
        }

        #endregion
    }
}
