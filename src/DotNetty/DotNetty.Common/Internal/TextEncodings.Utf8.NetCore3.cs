#if NETCOREAPP_3_0_GREATER
namespace DotNetty.Common.Internal
{
    using System;
    using System.Buffers;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    public static partial class TextEncodings
    {
        public static partial class Utf8
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static unsafe bool TryGetCharCountFast(in ReadOnlySpan<byte> utf8Bytes, out int totalCharCount)
            {
                fixed (byte* bytesPtr = &MemoryMarshal.GetReference(utf8Bytes))
                {
                    var byteCount = utf8Bytes.Length;
                    totalCharCount = GetCharCountFastInternal(bytesPtr, byteCount, out int bytesConsumed);
                    if (bytesConsumed == byteCount) { return true; }
                }
                totalCharCount = 0; return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)] // called directly by TryGetCharCountFast
            private static unsafe int GetCharCountFastInternal(byte* pBytes, int bytesLength, out int bytesConsumed)
            {
                // The number of UTF-16 code units will never exceed the number of UTF-8 code units,
                // so the addition at the end of this method will not overflow.

                byte* ptrToFirstInvalidByte = Utf8Utility.GetPointerToFirstInvalidByte(pBytes, bytesLength, out int utf16CodeUnitCountAdjustment, out _);

                int tempBytesConsumed = (int)(ptrToFirstInvalidByte - pBytes);
                bytesConsumed = tempBytesConsumed;

                return tempBytesConsumed + utf16CodeUnitCountAdjustment;
            }

            //[MethodImpl(MethodImplOptions.AggressiveOptimization)]
            public static unsafe int GetChars(in ReadOnlySpan<byte> utf8Bytes, Span<char> chars)
            {
                var byteCount = utf8Bytes.Length;
                if (0u >= (uint)byteCount) { return 0; }

                fixed (byte* bytesPtr = &MemoryMarshal.GetReference(utf8Bytes))
                fixed (char* charsPtr = &MemoryMarshal.GetReference(chars))
                {
                    // First call into the fast path.
                    var charsWritten = GetCharsFastInternal(bytesPtr, byteCount, charsPtr, chars.Length, out int bytesConsumed);

                    if (bytesConsumed == byteCount)
                    {
                        // All elements converted - return immediately.

                        return charsWritten;
                    }
                }
                return UTF8NoBOM.GetChars(utf8Bytes, chars);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)] // called directly by GetChars
            private static unsafe int GetCharsFastInternal(byte* pBytes, int bytesLength, char* pChars, int charsLength, out int bytesConsumed)
            {
                // We don't care about the exact OperationStatus value returned by the workhorse routine; we only
                // care if the workhorse was able to consume the entire input payload. If we're unable to do so,
                // we'll handle the remainder in the fallback routine.

                byte* pInputBufferRemaining;
                char* pOutputBufferRemaining;

                _ = Utf8Utility.TranscodeToUtf16(pBytes, bytesLength, pChars, charsLength, out pInputBufferRemaining, out pOutputBufferRemaining);

                bytesConsumed = (int)(pInputBufferRemaining - pBytes);
                return (int)(pOutputBufferRemaining - pChars);
            }

            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            public static string GetString(in ReadOnlySpan<byte> utf8Bytes)
            {
                return UTF8NoBOM.GetString(utf8Bytes);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static unsafe bool TryGetByteCountFast(in ReadOnlySpan<char> chars, out int bytesNeeded)
            {
                // It's ok for us to pass null pointers down to the workhorse below.
                fixed (char* charsPtr = &MemoryMarshal.GetReference(chars))
                {
                    var charCount = chars.Length;
                    // First call into the fast path.
                    bytesNeeded = GetByteCountFastInternal(charsPtr, charCount, out int charsConsumed);
                    if (charsConsumed == charCount) { return true; }
                }
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)] // called directly by TryGetByteCountFast
            private static unsafe int GetByteCountFastInternal(char* pChars, int charsLength, out int charsConsumed)
            {
                // The number of UTF-8 code units may exceed the number of UTF-16 code units,
                // so we'll need to check for overflow before casting to Int32.

                char* ptrToFirstInvalidChar = Utf16Utility.GetPointerToFirstInvalidChar(pChars, charsLength, out long utf8CodeUnitCountAdjustment, out _);

                int tempCharsConsumed = (int)(ptrToFirstInvalidChar - pChars);
                charsConsumed = tempCharsConsumed;

                long totalUtf8Bytes = tempCharsConsumed + utf8CodeUnitCountAdjustment;
                if ((ulong)totalUtf8Bytes > int.MaxValue)
                {
                    ThrowConversionOverflow();
                }

                return (int)totalUtf8Bytes;
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void ThrowConversionOverflow()
            {
                throw GetArgumentException();
                static ArgumentException GetArgumentException()
                {
                    return new ArgumentException("Argument_ConversionOverflow");
                }
            }

            /// <summary>
            /// Transcodes the UTF-8 <paramref name="source"/> buffer to <paramref name="destination"/> as UTF-16.
            /// </summary>
            /// <remarks>
            /// If <paramref name="replaceInvalidSequences"/> is <see langword="true"/>, invalid UTF-8 sequences
            /// in <paramref name="source"/> will be replaced with U+FFFD in <paramref name="destination"/>, and
            /// this method will not return <see cref="OperationStatus.InvalidData"/>.
            /// </remarks>
            public static unsafe OperationStatus ToUtf16(ReadOnlySpan<byte> source, Span<char> destination, out int bytesRead, out int charsWritten, bool replaceInvalidSequences = true, bool isFinalBlock = true)
            {
                // Throwaway span accesses - workaround for https://github.com/dotnet/coreclr/issues/12332

                _ = source.Length;
                _ = destination.Length;

                // We'll be mutating these values throughout our loop.

                fixed (byte* pOriginalSource = &MemoryMarshal.GetReference(source))
                fixed (char* pOriginalDestination = &MemoryMarshal.GetReference(destination))
                {
                    // We're going to bulk transcode as much as we can in a loop, iterating
                    // every time we see bad data that requires replacement.

                    OperationStatus operationStatus = OperationStatus.Done;
                    byte* pInputBufferRemaining = pOriginalSource;
                    char* pOutputBufferRemaining = pOriginalDestination;

                    while (!source.IsEmpty)
                    {
                        // We've pinned the spans at the entry point to this method.
                        // It's safe for us to use Unsafe.AsPointer on them during this loop.

                        operationStatus = Utf8Utility.TranscodeToUtf16(
                            pInputBuffer: (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(source)),
                            inputLength: source.Length,
                            pOutputBuffer: (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(destination)),
                            outputCharsRemaining: destination.Length,
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

                        destination = destination.Slice((int)(pOutputBufferRemaining - (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(destination))));

                        if (destination.IsEmpty)
                        {
                            operationStatus = OperationStatus.DestinationTooSmall;
                            break;
                        }

                        destination[0] = (char)UnicodeUtility.ReplacementChar;
                        destination = destination.Slice(1);

                        // Now figure out how many bytes of the source we must skip over before we should retry
                        // the operation. This might be more than 1 byte.

                        source = source.Slice((int)(pInputBufferRemaining - (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(source))));
                        Debug.Assert(!source.IsEmpty, "Expected 'Done' if source is fully consumed.");

                        Rune.DecodeFromUtf8(source, out _, out int bytesConsumedJustNow);
                        source = source.Slice(bytesConsumedJustNow);

                        operationStatus = OperationStatus.Done; // we patched the error - if we're about to break out of the loop this is a success case
                        pInputBufferRemaining = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(source));
                        pOutputBufferRemaining = (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(destination));
                    }

                    // Not possible to make any further progress - report to our caller how far we got.

                    bytesRead = (int)(pInputBufferRemaining - pOriginalSource);
                    charsWritten = (int)(pOutputBufferRemaining - pOriginalDestination);
                    return operationStatus;
                }
            }
        }
    }
}
#endif
