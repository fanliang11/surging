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
    using System.Collections.Generic;
    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    /// <summary>
    ///     A decoder that splits the received <see cref="IByteBuffer"/>s on line endings.
    ///     Both {@code "\n"} and {@code "\r\n"} are handled.
    ///     <para>
    ///     The byte stream is expected to be in UTF-8 character encoding or ASCII. The current implementation
    ///     uses direct {@code byte} to {@code char} cast and then compares that {@code char} to a few low range
    ///     ASCII characters like {@code '\n'} or {@code '\r'}. UTF-8 is not using low range [0..0x7F]
    ///     byte values for multibyte codepoint representations therefore fully supported by this implementation.
    ///     </para>
    ///     For a more general delimiter-based decoder, see <see cref="DelimiterBasedFrameDecoder"/>.
    /// </summary>
    public class LineBasedFrameDecoder : ByteToMessageDecoder
    {
        /** Maximum length of a frame we're willing to decode.  */
        readonly int maxLength;
        /** Whether or not to throw an exception as soon as we exceed maxLength. */
        readonly bool failFast;
        readonly bool stripDelimiter;

        /** True if we're discarding input because we're already over maxLength.  */
        bool discarding;
        int discardedBytes;

        /** Last scan position. */
        int offset;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DotNetty.Codecs.LineBasedFrameDecoder" /> class.
        /// </summary>
        /// <param name="maxLength">
        ///     the maximum length of the decoded frame.
        ///     A {@link TooLongFrameException} is thrown if
        ///     the length of the frame exceeds this value.
        /// </param>
        public LineBasedFrameDecoder(int maxLength)
            : this(maxLength, true, false)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DotNetty.Codecs.LineBasedFrameDecoder" /> class.
        /// </summary>
        /// <param name="maxLength">
        ///     the maximum length of the decoded frame.
        ///     A {@link TooLongFrameException} is thrown if
        ///     the length of the frame exceeds this value.
        /// </param>
        /// <param name="stripDelimiter">
        ///     whether the decoded frame should strip out the
        ///     delimiter or not
        /// </param>
        /// <param name="failFast">
        ///     If <tt>true</tt>, a {@link TooLongFrameException} is
        ///     thrown as soon as the decoder notices the length of the
        ///     frame will exceed <tt>maxFrameLength</tt> regardless of
        ///     whether the entire frame has been read.
        ///     If <tt>false</tt>, a {@link TooLongFrameException} is
        ///     thrown after the entire frame that exceeds
        ///     <tt>maxFrameLength</tt> has been read.
        /// </param>
        public LineBasedFrameDecoder(int maxLength, bool stripDelimiter, bool failFast)
        {
            this.maxLength = maxLength;
            this.failFast = failFast;
            this.stripDelimiter = stripDelimiter;
        }

        /// <inheritdoc />
        protected internal override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            object decode = this.Decode(context, input);
            if (decode is object)
            {
                output.Add(decode);
            }
        }

        /// <summary>
        ///     Create a frame out of the {@link ByteBuf} and return it.
        /// </summary>
        /// <param name="ctx">the {@link ChannelHandlerContext} which this {@link ByteToMessageDecoder} belongs to</param>
        /// <param name="buffer">the {@link ByteBuf} from which to read data</param>
        protected virtual internal object Decode(IChannelHandlerContext ctx, IByteBuffer buffer)
        {
            int eol = this.FindEndOfLine(buffer);
            if (!this.discarding)
            {
                if (eol >= 0)
                {
                    IByteBuffer frame;
                    int length = eol - buffer.ReaderIndex;
                    int delimLength = buffer.GetByte(eol) == '\r' ? 2 : 1;

                    if (length > this.maxLength)
                    {
                        _ = buffer.SetReaderIndex(eol + delimLength);
                        this.Fail(ctx, length);
                        return null;
                    }

                    if (this.stripDelimiter)
                    {
                        frame = buffer.ReadRetainedSlice(length);
                        _ = buffer.SkipBytes(delimLength);
                    }
                    else
                    {
                        frame = buffer.ReadRetainedSlice(length + delimLength);
                    }

                    return frame;
                }
                else
                {
                    int length = buffer.ReadableBytes;
                    if (length > this.maxLength)
                    {
                        this.discardedBytes = length;
                        _ = buffer.SetReaderIndex(buffer.WriterIndex);
                        this.discarding = true;
                        this.offset = 0;
                        if (this.failFast)
                        {
                            this.Fail(ctx, "over " + this.discardedBytes);
                        }
                    }
                    return null;
                }
            }
            else
            {
                if (eol >= 0)
                {
                    int length = this.discardedBytes + eol - buffer.ReaderIndex;
                    int delimLength = buffer.GetByte(eol) == '\r' ? 2 : 1;
                    _ = buffer.SetReaderIndex(eol + delimLength);
                    this.discardedBytes = 0;
                    this.discarding = false;
                    if (!this.failFast)
                    {
                        this.Fail(ctx, length);
                    }
                }
                else
                {
                    this.discardedBytes += buffer.ReadableBytes;
                    _ = buffer.SetReaderIndex(buffer.WriterIndex);
                    // We skip everything in the buffer, we need to set the offset to 0 again.
                    this.offset = 0;
                }
                return null;
            }
        }

        void Fail(IChannelHandlerContext ctx, int length) => this.Fail(ctx, length.ToString());

        void Fail(IChannelHandlerContext ctx, string length)
        {
            _ = ctx.FireExceptionCaught(
                new TooLongFrameException(
                    $"frame length ({length}) exceeds the allowed maximum ({this.maxLength})"));
        }

        /// <summary>
        /// Returns the index in the buffer of the end of line found.
        /// Returns <c>-1</c> if no end of line was found in the buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        int FindEndOfLine(IByteBuffer buffer)
        {
            const byte LineFeed = (byte)'\n';
            int i = buffer.IndexOf(buffer.ReaderIndex + offset, buffer.WriterIndex, LineFeed);
            if (i >= 0)
            {
                offset = 0;
                if (i > 0 && buffer.GetByte(i - 1) == '\r')
                {
                    i--;
                }
            }
            else
            {
                offset = buffer.ReadableBytes;
            }
            return i;
        }
    }
}