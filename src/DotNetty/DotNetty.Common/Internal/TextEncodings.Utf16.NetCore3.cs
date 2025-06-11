#if NETCOREAPP_3_0_GREATER
namespace DotNetty.Common.Internal
{
    using System;
    using System.Buffers;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using SysUtf8 = System.Text.Unicode.Utf8;

    public static partial class TextEncodings
    {
        public static partial class Utf16
        {
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static unsafe OperationStatus ToUtf8(in ReadOnlySpan<char> chars, Span<byte> utf8Destination, out int charsConsumed, out int bytesWritten)
            {
                // It's ok for us to operate on null / empty spans.

                fixed (char* charsPtr = &MemoryMarshal.GetReference(chars))
                fixed (byte* bytesPtr = &MemoryMarshal.GetReference(utf8Destination))
                {
                    if (TryGetBytesFast(charsPtr, chars.Length, bytesPtr, utf8Destination.Length, out charsConsumed, out bytesWritten))
                    {
                        return OperationStatus.Done;
                    }
                }

                return SysUtf8.FromUtf16(chars, utf8Destination, out charsConsumed, out bytesWritten);
            }

            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static unsafe OperationStatus ToUtf8(in ReadOnlySpan<char> chars, ref byte utf8Destination, int utf8Length, out int charsConsumed, out int bytesWritten)
            {
                // It's ok for us to operate on null / empty spans.

                fixed (char* charsPtr = &MemoryMarshal.GetReference(chars))
                fixed (byte* bytesPtr = &utf8Destination)
                {
                    if (TryGetBytesFast(charsPtr, chars.Length, bytesPtr, utf8Length, out charsConsumed, out bytesWritten))
                    {
                        return OperationStatus.Done;
                    }
                }

                return SysUtf8.FromUtf16(chars, MemoryMarshal.CreateSpan(ref utf8Destination, utf8Length), out charsConsumed, out bytesWritten);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static unsafe bool TryGetBytesFast(char* pChars, int charCount, byte* pBytes, int byteCount, out int charsConsumed, out int bytesWritten)
            {
                // First call into the fast path.

                bytesWritten = GetBytesFastInternal(pChars, charCount, pBytes, byteCount, out charsConsumed);

                return (charsConsumed == charCount); // All elements converted - return immediately.
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)] // called directly by TryGetBytesFast
            private static unsafe int GetBytesFastInternal(char* pChars, int charsLength, byte* pBytes, int bytesLength, out int charsConsumed)
            {
                // We don't care about the exact OperationStatus value returned by the workhorse routine; we only
                // care if the workhorse was able to consume the entire input payload. If we're unable to do so,
                // we'll handle the remainder in the fallback routine.

                char* pInputBufferRemaining;
                byte* pOutputBufferRemaining;

                _ = Utf8Utility.TranscodeToUtf8(pChars, charsLength, pBytes, bytesLength, out pInputBufferRemaining, out pOutputBufferRemaining);

                charsConsumed = (int)(pInputBufferRemaining - pChars);
                return (int)(pOutputBufferRemaining - pBytes);
            }


            /// <summary>
            /// Transcodes the UTF-16 <paramref name="source"/> buffer to <paramref name="destination"/> as UTF-8.
            /// </summary>
            /// <remarks>
            /// If <paramref name="replaceInvalidSequences"/> is <see langword="true"/>, invalid UTF-16 sequences
            /// in <paramref name="source"/> will be replaced with U+FFFD in <paramref name="destination"/>, and
            /// this method will not return <see cref="OperationStatus.InvalidData"/>.
            /// </remarks>
            public static unsafe OperationStatus ToUtf8(ReadOnlySpan<char> source, Span<byte> destination, out int charsRead, out int bytesWritten, bool replaceInvalidSequences = true, bool isFinalBlock = true)
            {
                // Throwaway span accesses - workaround for https://github.com/dotnet/coreclr/issues/12332

                _ = source.Length;
                _ = destination.Length;

                fixed (char* pOriginalSource = &MemoryMarshal.GetReference(source))
                fixed (byte* pOriginalDestination = &MemoryMarshal.GetReference(destination))
                {
                    // We're going to bulk transcode as much as we can in a loop, iterating
                    // every time we see bad data that requires replacement.

                    OperationStatus operationStatus = OperationStatus.Done;
                    char* pInputBufferRemaining = pOriginalSource;
                    byte* pOutputBufferRemaining = pOriginalDestination;

                    while (!source.IsEmpty)
                    {
                        // We've pinned the spans at the entry point to this method.
                        // It's safe for us to use Unsafe.AsPointer on them during this loop.

                        operationStatus = Utf8Utility.TranscodeToUtf8(
                            pInputBuffer: (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(source)),
                            inputLength: source.Length,
                            pOutputBuffer: (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(destination)),
                            outputBytesRemaining: destination.Length,
                            pInputBufferRemaining: out pInputBufferRemaining,
                            pOutputBufferRemaining: out pOutputBufferRemaining);

                        // If we finished the operation entirely or we ran out of space in the destination buffer,
                        // or if we need more input data and the caller told us that there's possibly more data
                        // coming, return immediately.

                        if (operationStatus <= OperationStatus.DestinationTooSmall
                            || (operationStatus == OperationStatus.NeedMoreData && !isFinalBlock))
                        {
                            break;
                        }

                        // We encountered invalid data, or we need more data but the caller told us we're
                        // at the end of the stream. In either case treat this as truly invalid.
                        // If the caller didn't tell us to replace invalid sequences, return immediately.

                        if (!replaceInvalidSequences)
                        {
                            operationStatus = OperationStatus.InvalidData; // status code may have been NeedMoreData - force to be error
                            break;
                        }

                        // We're going to attempt to write U+FFFD to the destination buffer.
                        // Do we even have enough space to do so?

                        destination = destination.Slice((int)(pOutputBufferRemaining - (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(destination))));

                        if (2 >= (uint)destination.Length)
                        {
                            operationStatus = OperationStatus.DestinationTooSmall;
                            break;
                        }

                        destination[0] = 0xEF; // U+FFFD = [ EF BF BD ] in UTF-8
                        destination[1] = 0xBF;
                        destination[2] = 0xBD;
                        destination = destination.Slice(3);

                        // Invalid UTF-16 sequences are always of length 1. Just skip the next character.

                        source = source.Slice((int)(pInputBufferRemaining - (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(source))) + 1);

                        operationStatus = OperationStatus.Done; // we patched the error - if we're about to break out of the loop this is a success case
                        pInputBufferRemaining = (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(source));
                        pOutputBufferRemaining = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(destination));
                    }

                    // Not possible to make any further progress - report to our caller how far we got.

                    charsRead = (int)(pInputBufferRemaining - pOriginalSource);
                    bytesWritten = (int)(pOutputBufferRemaining - pOriginalDestination);
                    return operationStatus;
                }
            }
        }
    }
}
#endif