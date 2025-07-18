// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// ReSharper disable ConvertToAutoPropertyWhenPossible
namespace DotNetty.Common.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Common.Utilities;
#if NET
    using System.Runtime.InteropServices;
#endif

    using static PlatformDependent0;

    public static partial class PlatformDependent
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance(typeof(PlatformDependent));

        static readonly bool UseDirectBuffer;

        static PlatformDependent()
        {
            UseDirectBuffer = !SystemPropertyUtil.GetBoolean("io.netty.noPreferDirect", false);

            if (Logger.DebugEnabled)
            {
                Logger.Debug("-Dio.netty.noPreferDirect: {}", !UseDirectBuffer);
            }
        }

        public static readonly bool Is64BitProcess = IntPtr.Size >= 8;

        public static bool DirectBufferPreferred => UseDirectBuffer;

        static int seed = (int)(Stopwatch.GetTimestamp() & 0xFFFFFFFF); //used to safly cast long to int, because the timestamp returned is long and it doesn't fit into an int
        static readonly ThreadLocal<Random> ThreadLocalRandom = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed))); //used to simulate java ThreadLocalRandom
        static readonly bool IsLittleEndian = BitConverter.IsLittleEndian;

        public static IQueue<T> NewFixedMpscQueue<T>(int capacity) where T : class => new MpscArrayQueue<T>(capacity);

        public static IQueue<T> NewMpscQueue<T>() where T : class => new CompatibleConcurrentQueue<T>();

        public static IDeque<T> NewDeque<T>() where T : class => new WorkStealingQueue<T>();

        public static IDictionary<TKey, TValue> NewConcurrentHashMap<TKey, TValue>() => new ConcurrentDictionary<TKey, TValue>();

        public static ILinkedQueue<T> NewSpscLinkedQueue<T>() where T : class => new SpscLinkedQueue<T>();

        public static Random GetThreadLocalRandom() => ThreadLocalRandom.Value;

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static unsafe bool ByteArrayEquals(byte[] bytes1, int startPos1, byte[] bytes2, int startPos2, int length)
        {
            if ((uint)(length - 1) > SharedConstants.TooBigOrNegative) // <= 0
            {
                return true;
            }

#if NET
            return new ReadOnlySpan<byte>(bytes1, startPos1, length).SequenceEqual(new ReadOnlySpan<byte>(bytes2, startPos2, length));
#else
            return SpanHelpers.SequenceEqual(ref bytes1[startPos1], ref bytes2[startPos2], length);
#endif
        }

        public static unsafe int ByteArrayEqualsConstantTime(byte[] bytes1, int startPos1, byte[] bytes2, int startPos2, int length)
        {
            if ((uint)(length - 1) > SharedConstants.TooBigOrNegative) // <= 0
            {
                return 1;
            }

            fixed (byte* array1 = bytes1)
            fixed (byte* array2 = bytes2)
                return PlatformDependent0.ByteArrayEqualsConstantTime(array1, startPos1, array2, startPos2, length);
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static unsafe int HashCodeAscii(byte[] bytes, int startPos, int length)
        {
            if (0u >= (uint)length)
            {
                return HashCodeAsciiSeed;
            }

            fixed (byte* array = &bytes[startPos])
                return PlatformDependent0.HashCodeAscii(array, length);
        }

        public static int HashCodeAscii(ICharSequence bytes)
        {
            int hash = HashCodeAsciiSeed;
            int remainingBytes = bytes.Count & 7;

            // Benchmarking shows that by just naively looping for inputs 8~31 bytes long we incur a relatively large
            // performance penalty (only achieve about 60% performance of loop which iterates over each char). So because
            // of this we take special provisions to unroll the looping for these conditions.
            switch (bytes.Count)
            {
                case 31:
                case 30:
                case 29:
                case 28:
                case 27:
                case 26:
                case 25:
                case 24:
                    hash = HashCodeAsciiCompute(
                        bytes,
                        bytes.Count - 24,
                        HashCodeAsciiCompute(
                            bytes,
                            bytes.Count - 16,
                            HashCodeAsciiCompute(bytes, bytes.Count - 8, hash)));
                    break;
                case 23:
                case 22:
                case 21:
                case 20:
                case 19:
                case 18:
                case 17:
                case 16:
                    hash = HashCodeAsciiCompute(
                        bytes,
                        bytes.Count - 16,
                        HashCodeAsciiCompute(bytes, bytes.Count - 8, hash));
                    break;
                case 15:
                case 14:
                case 13:
                case 12:
                case 11:
                case 10:
                case 9:
                case 8:
                    hash = HashCodeAsciiCompute(bytes, bytes.Count - 8, hash);
                    break;
                case 7:
                case 6:
                case 5:
                case 4:
                case 3:
                case 2:
                case 1:
                case 0:
                    break;
                default:
                    for (int i = bytes.Count - 8; i >= remainingBytes; i -= 8)
                    {
                        hash = HashCodeAsciiCompute(bytes, i, hash);
                    }
                    break;
            }
            switch (remainingBytes)
            {
                case 7:
                    return ((hash
                        * HashCodeC1 + HashCodeAsciiSanitizsByte(bytes[0]))
                        * HashCodeC2 + HashCodeAsciiSanitizeShort(bytes, 1))
                        * HashCodeC1 + HashCodeAsciiSanitizeInt(bytes, 3);
                case 6:
                    return (hash
                        * HashCodeC1 + HashCodeAsciiSanitizeShort(bytes, 0))
                        * HashCodeC2 + HashCodeAsciiSanitizeInt(bytes, 2);
                case 5:
                    return (hash
                        * HashCodeC1 + HashCodeAsciiSanitizsByte(bytes[0]))
                        * HashCodeC2 + HashCodeAsciiSanitizeInt(bytes, 1);
                case 4:
                    return hash
                        * HashCodeC1 + HashCodeAsciiSanitizeInt(bytes, 0);
                case 3:
                    return (hash
                        * HashCodeC1 + HashCodeAsciiSanitizsByte(bytes[0]))
                        * HashCodeC2 + HashCodeAsciiSanitizeShort(bytes, 1);
                case 2:
                    return hash
                        * HashCodeC1 + HashCodeAsciiSanitizeShort(bytes, 0);
                case 1:
                    return hash
                        * HashCodeC1 + HashCodeAsciiSanitizsByte(bytes[0]);
                default:
                    return hash;
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        static int HashCodeAsciiCompute(ICharSequence value, int offset, int hash)
        {
            if (!IsLittleEndian)
            {
                return hash * HashCodeC1 +
                    // Low order int
                    HashCodeAsciiSanitizeInt(value, offset + 4) * HashCodeC2 +
                    // High order int
                    HashCodeAsciiSanitizeInt(value, offset);
            }
            return hash * HashCodeC1 +
                // Low order int
                HashCodeAsciiSanitizeInt(value, offset) * HashCodeC2 +
                // High order int
                HashCodeAsciiSanitizeInt(value, offset + 4);
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        static int HashCodeAsciiSanitizeInt(ICharSequence value, int offset)
        {
            if (!IsLittleEndian)
            {
                // mimic a unsafe.getInt call on a big endian machine
                return (value[offset + 3] & 0x1f)
                    | (value[offset + 2] & 0x1f) << 8
                    | (value[offset + 1] & 0x1f) << 16
                    | (value[offset] & 0x1f) << 24;
            }

            return (value[offset + 3] & 0x1f) << 24
                | (value[offset + 2] & 0x1f) << 16
                | (value[offset + 1] & 0x1f) << 8
                | (value[offset] & 0x1f);
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        static int HashCodeAsciiSanitizeShort(ICharSequence value, int offset)
        {
            if (!IsLittleEndian)
            {
                // mimic a unsafe.getShort call on a big endian machine
                return (value[offset + 1] & 0x1f)
                    | (value[offset] & 0x1f) << 8;
            }

            return (value[offset + 1] & 0x1f) << 8
                | (value[offset] & 0x1f);
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        static int HashCodeAsciiSanitizsByte(char value) => value & 0x1f;

        // https://github.com/Azure/DotNetty/issues/371#issuecomment-372574610

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyMemory(byte[] src, int srcIndex, byte[] dst, int dstIndex, int length)
        {
            uint nlen = unchecked((uint)length);
            if (0u >= nlen) { return; }

#if NET451
            Buffer.BlockCopy(src, srcIndex, dst, dstIndex, length);
#elif NET471
            unsafe
            {
                fixed (byte* source = &src[srcIndex])
                {
                    fixed (byte* destination = &dst[dstIndex])
                    {
                        Buffer.MemoryCopy(source, destination, length, length);
                    }
                }
            }
#elif NET
            Unsafe.CopyBlockUnaligned(
                ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(dst), dstIndex), 
                ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(src), srcIndex), 
                nlen);
#else
            Unsafe.CopyBlockUnaligned(ref dst[dstIndex], ref src[srcIndex], nlen);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CopyMemory(byte* src, byte* dst, int length)
        {
            uint nlen = unchecked((uint)length);
            if (0u >= nlen) { return; }
#if NET471
            Buffer.MemoryCopy(src, dst, length, length);
#else
            Unsafe.CopyBlockUnaligned(dst, src, nlen);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CopyMemory(byte* src, byte[] dst, int dstIndex, int length)
        {
            uint nlen = unchecked((uint)length);
            if (0u >= nlen) { return; }
            fixed (byte* destination = &dst[dstIndex])
            {
#if NET471
                Buffer.MemoryCopy(src, destination, length, length);
#else
                Unsafe.CopyBlockUnaligned(destination, src, nlen);
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CopyMemory(byte[] src, int srcIndex, byte* dst, int length)
        {
            uint nlen = unchecked((uint)length);
            if (0u >= nlen) { return; }
            fixed (byte* source = &src[srcIndex])
            {
#if NET471
                Buffer.MemoryCopy(source, dst, length, length);
#else
                Unsafe.CopyBlockUnaligned(dst, source, nlen);
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear(byte[] src, int srcIndex, int length)
        {
            const byte DefaultValue = default;
            uint nlen = unchecked((uint)length);
            if (0u >= nlen) { return; }
            Unsafe.InitBlockUnaligned(ref src[srcIndex], DefaultValue, nlen);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void SetMemory(byte* src, int length, byte value)
        {
            uint nlen = unchecked((uint)length);
            if (0u >= nlen) { return; }
            Unsafe.InitBlockUnaligned(src, value, nlen);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetMemory(byte[] src, int srcIndex, int length, byte value)
        {
            uint nlen = unchecked((uint)length);
            if (0u >= nlen) { return; }
            Unsafe.InitBlockUnaligned(ref src[srcIndex], value, nlen);
        }
    }
}