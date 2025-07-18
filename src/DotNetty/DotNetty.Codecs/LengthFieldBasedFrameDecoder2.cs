/*
 * Copyright 2012 The Netty Project
 *
 * The Netty Project licenses this file to you under the Apache License,
 * version 2.0 (the "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy of the License at:
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * Copyright (c) Microsoft. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com)
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using DotNetty.Buffers;
    using DotNetty.Transport.Channels;

    public class LengthFieldBasedFrameDecoder2 : ByteToMessageDecoder
    {
        readonly int maxFrameLength;
        readonly int lengthFieldOffset;
        readonly int lengthFieldLength;
        readonly int lengthFieldEndOffset;
        readonly int lengthAdjustment;
        readonly int initialBytesToStrip;
        readonly bool failFast;
        bool discardingTooLongFrame;
        long tooLongFrameLength;
        long bytesToDiscard;

        /// <summary>
        ///     Create a new instance.
        /// </summary>
        /// <param name="maxFrameLength">
        ///     The maximum length of the frame.  If the length of the frame is
        ///     greater than this value then <see cref="TooLongFrameException" /> will be thrown.
        /// </param>
        /// <param name="lengthFieldOffset">The offset of the length field.</param>
        /// <param name="lengthFieldLength">The length of the length field.</param>
        public LengthFieldBasedFrameDecoder2(int maxFrameLength, int lengthFieldOffset, int lengthFieldLength)
            : this(maxFrameLength, lengthFieldOffset, lengthFieldLength, 0, 0)
        {
        }

        /// <summary>
        ///     Create a new instance.
        /// </summary>
        /// <param name="maxFrameLength">
        ///     The maximum length of the frame.  If the length of the frame is
        ///     greater than this value then <see cref="TooLongFrameException" /> will be thrown.
        /// </param>
        /// <param name="lengthFieldOffset">The offset of the length field.</param>
        /// <param name="lengthFieldLength">The length of the length field.</param>
        /// <param name="lengthAdjustment">The compensation value to add to the value of the length field.</param>
        /// <param name="initialBytesToStrip">the number of first bytes to strip out from the decoded frame.</param>
        public LengthFieldBasedFrameDecoder2(int maxFrameLength, int lengthFieldOffset, int lengthFieldLength, int lengthAdjustment, int initialBytesToStrip)
            : this(maxFrameLength, lengthFieldOffset, lengthFieldLength, lengthAdjustment, initialBytesToStrip, true)
        {
        }

        /// <summary>
        ///     Create a new instance.
        /// </summary>
        /// <param name="maxFrameLength">
        ///     The maximum length of the frame.  If the length of the frame is
        ///     greater than this value then <see cref="TooLongFrameException" /> will be thrown.
        /// </param>
        /// <param name="lengthFieldOffset">The offset of the length field.</param>
        /// <param name="lengthFieldLength">The length of the length field.</param>
        /// <param name="lengthAdjustment">The compensation value to add to the value of the length field.</param>
        /// <param name="initialBytesToStrip">the number of first bytes to strip out from the decoded frame.</param>
        /// <param name="failFast">
        ///     If <c>true</c>, a <see cref="TooLongFrameException" /> is thrown as soon as the decoder notices the length
        ///     of the frame will exceeed <see cref="maxFrameLength" /> regardless of whether the entire frame has been
        ///     read. If <c>false</c>, a <see cref="TooLongFrameException" /> is thrown after the entire frame that exceeds
        ///     <see cref="maxFrameLength" /> has been read.
        ///     Defaults to <c>true</c> in other overloads.
        /// </param>
        public LengthFieldBasedFrameDecoder2(int maxFrameLength, int lengthFieldOffset, int lengthFieldLength, int lengthAdjustment, int initialBytesToStrip, bool failFast)
        {
            if ((uint)(maxFrameLength - 1) > SharedConstants.TooBigOrNegative) // <= 0
            {
                ThrowHelper.ThrowArgumentException_Positive(maxFrameLength, ExceptionArgument.maxFrameLength);
            }
            if ((uint)lengthFieldOffset > SharedConstants.TooBigOrNegative)
            {
                ThrowHelper.ThrowArgumentException_PositiveOrZero(lengthFieldOffset, ExceptionArgument.lengthFieldOffset);
            }
            if ((uint)initialBytesToStrip > SharedConstants.TooBigOrNegative)
            {
                ThrowHelper.ThrowArgumentException_PositiveOrZero(initialBytesToStrip, ExceptionArgument.initialBytesToStrip);
            }
            if ((uint)(lengthFieldOffset + lengthFieldLength) > (uint)maxFrameLength)
            {
                throw new ArgumentOutOfRangeException(nameof(maxFrameLength), "maxFrameLength (" + maxFrameLength + ") " +
                    "must be equal to or greater than " +
                    "lengthFieldOffset (" + lengthFieldOffset + ") + " +
                    "lengthFieldLength (" + lengthFieldLength + ").");
            }

            this.maxFrameLength = maxFrameLength;
            this.lengthFieldOffset = lengthFieldOffset;
            this.lengthFieldLength = lengthFieldLength;
            this.lengthAdjustment = lengthAdjustment;
            this.lengthFieldEndOffset = lengthFieldOffset + lengthFieldLength;
            this.initialBytesToStrip = initialBytesToStrip;
            this.failFast = failFast;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void DiscardingTooLongFrame(IByteBuffer input)
        {
            long bytesToDiscard = this.bytesToDiscard;
            int localBytesToDiscard = (int)Math.Min(bytesToDiscard, input.ReadableBytes);
            _ = input.SkipBytes(localBytesToDiscard);
            bytesToDiscard -= localBytesToDiscard;
            this.bytesToDiscard = bytesToDiscard;

            this.FailIfNecessary(false);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void FailOnNegativeLengthField(IByteBuffer input, long frameLength, int lengthFieldEndOffset)
        {
            _ = input.SkipBytes(lengthFieldEndOffset);
            CThrowHelper.ThrowCorruptedFrameException_FrameLength(frameLength);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void FailOnFrameLengthLessThanLengthFieldEndOffset(IByteBuffer input,
                                                                          long frameLength,
                                                                          int lengthFieldEndOffset)
        {
            _ = input.SkipBytes(lengthFieldEndOffset);
            CThrowHelper.ThrowCorruptedFrameException_LengthFieldEndOffset(frameLength, lengthFieldEndOffset);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ExceededFrameLength(IByteBuffer input, long frameLength)
        {
            long discard = frameLength - input.ReadableBytes;
            this.tooLongFrameLength = frameLength;

            if (discard < 0)
            {
                // buffer contains more bytes then the frameLength so we can discard all now
                _ = input.SkipBytes((int)frameLength);
            }
            else
            {
                // Enter the discard mode and discard everything received so far.
                this.discardingTooLongFrame = true;
                this.bytesToDiscard = discard;
                _ = input.SkipBytes(input.ReadableBytes);
            }
            this.FailIfNecessary(true);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void FailOnFrameLengthLessThanInitialBytesToStrip(IByteBuffer input,
                                                                         int frameLength,
                                                                         int initialBytesToStrip)
        {
            _ = input.SkipBytes(frameLength);
            CThrowHelper.ThrowCorruptedFrameException_InitialBytesToStrip(frameLength, initialBytesToStrip);
        }

        /// <inheritdoc />
        protected internal override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            if (this.discardingTooLongFrame)
            {
                this.DiscardingTooLongFrame(input);
            }

            var thisLengthFieldEndOffset = this.lengthFieldEndOffset;
            if (input.ReadableBytes < thisLengthFieldEndOffset)
            {
                return;
            }

            int actualLengthFieldOffset = input.ReaderIndex + this.lengthFieldOffset;
            long frameLength = GetUnadjustedFrameLength(input, actualLengthFieldOffset, this.lengthFieldLength);

            if (frameLength < 0)
            {
                FailOnNegativeLengthField(input, frameLength, thisLengthFieldEndOffset);
            }

            frameLength += this.lengthAdjustment + thisLengthFieldEndOffset;

            if (frameLength < thisLengthFieldEndOffset)
            {
                FailOnFrameLengthLessThanLengthFieldEndOffset(input, frameLength, thisLengthFieldEndOffset);
            }

            if (frameLength > this.maxFrameLength)
            {
                this.ExceededFrameLength(input, frameLength);
                return;
            }

            // never overflows because it's less than maxFrameLength
            int frameLengthInt = (int)frameLength;
            if (input.ReadableBytes < frameLengthInt)
            {
                return;
            }

            var thisInitialBytesToStrip = this.initialBytesToStrip;
            if (thisInitialBytesToStrip > frameLengthInt)
            {
                FailOnFrameLengthLessThanInitialBytesToStrip(input, frameLengthInt, thisInitialBytesToStrip);
            }
            _ = input.SkipBytes(thisInitialBytesToStrip); 
            // extract frame
            int readerIndex = input.ReaderIndex;
            int actualFrameLength = frameLengthInt - thisInitialBytesToStrip;
            IByteBuffer frame = this.ExtractFrame(context, input, readerIndex, actualFrameLength);
            _ = input.SetReaderIndex(readerIndex + actualFrameLength);
            output.Add(frame);
        }

        /// <summary>
        ///     Decodes the specified region of the buffer into an unadjusted frame length.  The default implementation is
        ///     capable of decoding the specified region into an unsigned 8/16/24/32/64 bit integer.  Override this method to
        ///     decode the length field encoded differently.
        ///     Note that this method must not modify the state of the specified buffer (e.g.
        ///     <see cref="IByteBuffer.ReaderIndex" />,
        ///     <see cref="IByteBuffer.WriterIndex" />, and the content of the buffer.)
        /// </summary>
        /// <param name="buffer">The buffer we'll be extracting the frame length from.</param>
        /// <param name="offset">The offset from the absolute <see cref="IByteBuffer.ReaderIndex" />.</param>
        /// <param name="length">The length of the framelenght field. Expected: 1, 2, 3, 4, or 8.</param>
        /// <returns>A long integer that represents the unadjusted length of the next frame.</returns>
        protected static long GetUnadjustedFrameLength(IByteBuffer buffer, int offset, int length)
        {
            return length switch
            {
                1 => buffer.GetByte(offset),
                2 => buffer.GetUnsignedShort(offset),
                3 => buffer.GetUnsignedMedium(offset),
                4 => buffer.GetInt(offset),
                8 => buffer.GetLong(offset),
                _ => CThrowHelper.ThrowDecoderException(length),
            };
        }

        /// <summary>
        /// Extract the sub-region of the specified buffer.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected virtual IByteBuffer ExtractFrame(IChannelHandlerContext context, IByteBuffer buffer, int index, int length)
        {
            return buffer.RetainedSlice(index, length);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void FailIfNecessary(bool firstDetectionOfTooLongFrame)
        {
            if (0ul >= (ulong)this.bytesToDiscard)
            {
                // Reset to the initial state and tell the handlers that
                // the frame was too large.
                long tooLongFrameLength = this.tooLongFrameLength;
                this.tooLongFrameLength = 0;
                this.discardingTooLongFrame = false;
                if (!this.failFast || firstDetectionOfTooLongFrame)
                {
                    if (tooLongFrameLength > 0)
                    {
                        Fail(tooLongFrameLength, this.maxFrameLength);
                    }
                    else
                    {
                        Fail(this.maxFrameLength);
                    }
                }
            }
            else
            {
                // Keep discarding and notify handlers if necessary.
                if (this.failFast && firstDetectionOfTooLongFrame)
                {
                    if (tooLongFrameLength > 0)
                    {
                        Fail(tooLongFrameLength, this.maxFrameLength);
                    }
                    else
                    {
                        Fail(this.maxFrameLength);
                    }
                }
            }
        }

        private static void Fail(long frameLength, int maxFrameLength)
        {
            throw GetTooLongFrameException();
            TooLongFrameException GetTooLongFrameException()
            {
                return new TooLongFrameException("Adjusted frame length exceeds " + maxFrameLength +
                    ": " + frameLength + " - discarded");
            }
        }

        private static void Fail(int maxFrameLength)
        {
            throw GetTooLongFrameException();
            TooLongFrameException GetTooLongFrameException()
            {
                return new TooLongFrameException(
                    "Adjusted frame length exceeds " + maxFrameLength +
                        " - discarding");
            }
        }
    }
}