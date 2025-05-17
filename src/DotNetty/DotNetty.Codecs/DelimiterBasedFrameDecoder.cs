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
    using System.Runtime.InteropServices;
    using DotNetty.Buffers;
    using DotNetty.Common.Internal;
    using DotNetty.Transport.Channels;

    /// <summary>
    ///     A decoder that splits the received <see cref="DotNetty.Buffers.IByteBuffer" /> by one or more
    ///     delimiters.It is particularly useful for decoding the frames which ends
    ///     with a delimiter such as <see cref="DotNetty.Codecs.Delimiters.NullDelimiter" /> or
    ///     <see cref="DotNetty.Codecs.Delimiters.LineDelimiter" />
    ///     <h3>Specifying more than one delimiter </h3>
    ///     <see cref="DotNetty.Codecs.Delimiters.NullDelimiter" /> allows you to specify more than one
    ///     delimiter.  If more than one delimiter is found in the buffer, it chooses
    ///     the delimiter which produces the shortest frame.  For example, if you have
    ///     the following data in the buffer:
    ///     +--------------+
    ///     | ABC\nDEF\r\n |
    ///     +--------------+
    ///     a <see cref="DotNetty.Codecs.Delimiters.LineDelimiter" /> will choose '\n' as the first delimiter and produce two
    ///     frames:
    ///     +-----+-----+
    ///     | ABC | DEF |
    ///     +-----+-----+
    ///     rather than incorrectly choosing '\r\n' as the first delimiter:
    ///     +----------+
    ///     | ABC\nDEF |
    ///     +----------+
    /// </summary>
    public class DelimiterBasedFrameDecoder : ByteToMessageDecoder
    {
        private readonly IByteBuffer[] _delimiters;
        private readonly int _maxFrameLength;
        private readonly bool _stripDelimiter;
        private readonly bool _failFast;
        private readonly LineBasedFrameDecoder _lineBasedDecoder; // Set only when decoding with "\n" and "\r\n" as the delimiter.

        private bool _discardingTooLongFrame;
        private int _tooLongFrameLength;

        /// <summary>Common constructor</summary>
        /// <param name="maxFrameLength">
        ///     The maximum length of the decoded frame
        ///     NOTE: A see <see cref="DotNetty.Codecs.TooLongFrameException" /> is thrown if the length of the frame exceeds this
        ///     value.
        /// </param>
        /// <param name="stripDelimiter">whether the decoded frame should strip out the delimiter or not</param>
        /// <param name="failFast">
        ///     If true, a <see cref="DotNetty.Codecs.TooLongFrameException" /> is
        ///     thrown as soon as the decoder notices the length of the
        ///     frame will exceed<tt>maxFrameLength</tt> regardless of
        ///     whether the entire frame has been read.
        ///     If false, a <see cref="DotNetty.Codecs.TooLongFrameException" /> is
        ///     thrown after the entire frame that exceeds maxFrameLength has been read.
        /// </param>
        /// <param name="delimiters">delimiters</param>
        public DelimiterBasedFrameDecoder(int maxFrameLength, bool stripDelimiter, bool failFast, params IByteBuffer[] delimiters)
        {
            ValidateMaxFrameLength(maxFrameLength);
            if (delimiters is null)
                CThrowHelper.ThrowNullReferenceException(CExceptionArgument.delimiters);

            if (0u >= (uint)delimiters.Length)
                CThrowHelper.ThrowArgumentException_EmptyDelimiters();

            if (IsLineBased(delimiters) && !IsSubclass())
            {
                _lineBasedDecoder = new LineBasedFrameDecoder(maxFrameLength, stripDelimiter, failFast);
                _delimiters = null;
            }
            else
            {
                _delimiters = new IByteBuffer[delimiters.Length];
                for (int i = 0; i < delimiters.Length; i++)
                {
                    IByteBuffer d = delimiters[i];
                    ValidateDelimiter(d);
                    _delimiters[i] = d.Slice(d.ReaderIndex, d.ReadableBytes);
                }
                _lineBasedDecoder = null;
            }
            _maxFrameLength = maxFrameLength;
            _stripDelimiter = stripDelimiter;
            _failFast = failFast;
        }

        /// <summary>Creates a new instance.</summary>
        /// <param name="maxFrameLength">the maximum length of the decoded frame.
        /// A <see cref="TooLongFrameException"/> is thrown if
        /// the length of the frame exceeds this value.</param>
        /// <param name="delimiter">the delimiter</param>
        public DelimiterBasedFrameDecoder(int maxFrameLength, IByteBuffer delimiter)
            : this(maxFrameLength, true, true, new[] { delimiter })
        {
        }

        /// <summary>Creates a new instance.</summary>
        /// <param name="maxFrameLength">the maximum length of the decoded frame.
        /// A <see cref="TooLongFrameException"/> is thrown if
        /// the length of the frame exceeds this value.</param>
        /// <param name="stripDelimiter">whether the decoded frame should strip out the
        /// delimiter or not</param>
        /// <param name="delimiter">the delimiter</param>
        public DelimiterBasedFrameDecoder(int maxFrameLength, bool stripDelimiter, IByteBuffer delimiter)
            : this(maxFrameLength, stripDelimiter, true, new[] { delimiter })
        {
        }

        /// <summary>Creates a new instance.</summary>
        /// <param name="maxFrameLength">the maximum length of the decoded frame.
        /// A <see cref="TooLongFrameException"/> is thrown if
        /// the length of the frame exceeds this value.</param>
        /// <param name="stripDelimiter">whether the decoded frame should strip out the
        /// delimiter or not</param>
        /// <param name="failFast">If <c>true</c>, a <see cref="TooLongFrameException"/> is
        /// thrown as soon as the decoder notices the length of the
        /// frame will exceed <paramref name="maxFrameLength"/> regardless of
        /// whether the entire frame has been read.
        /// If <c>false</c>, a <see cref="TooLongFrameException"/> is
        /// thrown after the entire frame that exceeds
        /// <paramref name="maxFrameLength"/> has been read.</param>
        /// <param name="delimiter">the delimiter</param>
        public DelimiterBasedFrameDecoder(int maxFrameLength, bool stripDelimiter, bool failFast, IByteBuffer delimiter)
            : this(maxFrameLength, stripDelimiter, failFast, new[] { delimiter })
        {
        }

        /// <summary>Creates a new instance.</summary>
        /// <param name="maxFrameLength">the maximum length of the decoded frame.
        /// A <see cref="TooLongFrameException"/> is thrown if
        /// the length of the frame exceeds this value.</param>
        /// <param name="delimiters">the delimiters</param>
        public DelimiterBasedFrameDecoder(int maxFrameLength, params IByteBuffer[] delimiters)
            : this(maxFrameLength, true, true, delimiters)
        {
        }

        /// <summary>Creates a new instance.</summary>
        /// <param name="maxFrameLength">the maximum length of the decoded frame.
        /// A <see cref="TooLongFrameException"/> is thrown if
        /// the length of the frame exceeds this value.</param>
        /// <param name="stripDelimiter">whether the decoded frame should strip out the
        /// delimiter or not</param>
        /// <param name="delimiters">the delimiters</param>
        public DelimiterBasedFrameDecoder(int maxFrameLength, bool stripDelimiter, params IByteBuffer[] delimiters)
            : this(maxFrameLength, stripDelimiter, true, delimiters)
        {
        }

        /// <summary>Returns <c>true</c> if the delimiters are "\n" and "\r\n"</summary>
        static bool IsLineBased(IByteBuffer[] delimiters)
        {
            if (delimiters.Length != 2)
            {
                return false;
            }

            IByteBuffer a = delimiters[0];
            IByteBuffer b = delimiters[1];
            if (a.Capacity < b.Capacity)
            {
                a = delimiters[1];
                b = delimiters[0];
            }
            return a.Capacity == 2 && b.Capacity == 1 && a.GetByte(0) == '\r' && a.GetByte(1) == '\n' && b.GetByte(0) == '\n';
        }

        /// <summary>Returns <c>true</c> if the current instance is a subclass of DelimiterBasedFrameDecoder</summary>
        bool IsSubclass() => GetType() != typeof(DelimiterBasedFrameDecoder);

        /// <inheritdoc />
        protected internal override void Decode(IChannelHandlerContext ctx, IByteBuffer input, List<object> output)
        {
            object decoded = Decode(ctx, input);
            if (decoded is object)
                output.Add(decoded);
        }

        /// <summary>Create a frame out of the <see cref="DotNetty.Buffers.IByteBuffer" /> and return it</summary>
        /// <param name="ctx">
        ///     the <see cref="DotNetty.Transport.Channels.IChannelHandlerContext" /> which this
        ///     <see cref="DotNetty.Codecs.ByteToMessageDecoder" /> belongs to
        /// </param>
        /// <param name="buffer">the <see cref="DotNetty.Buffers.IByteBuffer" /> from which to read data</param>
        /// <returns>
        ///     the <see cref="DotNetty.Buffers.IByteBuffer" /> which represent the frame or null if no frame could be
        ///     created.
        /// </returns>
        protected virtual object Decode(IChannelHandlerContext ctx, IByteBuffer buffer)
        {
            if (_lineBasedDecoder is object)
            {
                return _lineBasedDecoder.Decode(ctx, buffer);
            }

            // Try all delimiters and choose the delimiter which yields the shortest frame.
            int minFrameLength = int.MaxValue;
            IByteBuffer minDelim = null;
            for (int idx = 0; idx < _delimiters.Length; idx++)
            {
                var delim = _delimiters[idx];
                int frameLength = IndexOf(buffer, delim);
                if (/*frameLength >= 0 && */(uint)frameLength < (uint)minFrameLength)
                {
                    minFrameLength = frameLength;
                    minDelim = delim;
                }
            }

            if (minDelim is object)
            {
                int minDelimLength = minDelim.Capacity;
                IByteBuffer frame;

                if (_discardingTooLongFrame)
                {
                    // We've just finished discarding a very large frame.
                    // Go back to the initial state.
                    _discardingTooLongFrame = false;
                    _ = buffer.SkipBytes(minFrameLength + minDelimLength);

                    int tooLongFrameLength = _tooLongFrameLength;
                    _tooLongFrameLength = 0;
                    if (!_failFast)
                    {
                        Fail(tooLongFrameLength);
                    }
                    return null;
                }

                if ((uint)minFrameLength > (uint)_maxFrameLength)
                {
                    // Discard read frame.
                    _ = buffer.SkipBytes(minFrameLength + minDelimLength);
                    Fail(minFrameLength);
                    return null;
                }

                if (_stripDelimiter)
                {
                    frame = buffer.ReadRetainedSlice(minFrameLength);
                    _ = buffer.SkipBytes(minDelimLength);
                }
                else
                {
                    frame = buffer.ReadRetainedSlice(minFrameLength + minDelimLength);
                }

                return frame;
            }
            else
            {
                if (!_discardingTooLongFrame)
                {
                    if ((uint)buffer.ReadableBytes > (uint)_maxFrameLength)
                    {
                        // Discard the content of the buffer until a delimiter is found.
                        _tooLongFrameLength = buffer.ReadableBytes;
                        _ = buffer.SkipBytes(buffer.ReadableBytes);
                        _discardingTooLongFrame = true;
                        if (_failFast)
                        {
                            Fail(_tooLongFrameLength);
                        }
                    }
                }
                else
                {
                    // Still discarding the buffer since a delimiter is not found.
                    _tooLongFrameLength += buffer.ReadableBytes;
                    _ = buffer.SkipBytes(buffer.ReadableBytes);
                }
                return null;
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        void Fail(long frameLength)
        {
            if (frameLength > 0)
                CThrowHelper.ThrowTooLongFrameException(_maxFrameLength, frameLength);
            else
                CThrowHelper.ThrowTooLongFrameException(_maxFrameLength);
        }

        /// <summary>
        /// Returns the number of bytes between the readerIndex of the haystack and
        /// the first needle found in the haystack.  <c>-1</c> is returned if no needle is
        /// found in the haystack.
        /// </summary>
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        static int IndexOf(IByteBuffer haystack, IByteBuffer needle)
        {
            var haystackSpan = haystack.GetReadableSpan();
            var needleSpan = needle.GetReadableSpan(0, needle.Capacity);
            return SpanHelpers.IndexOf(
                ref MemoryMarshal.GetReference(haystackSpan), haystackSpan.Length,
                ref MemoryMarshal.GetReference(needleSpan), needleSpan.Length);
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        static void ValidateDelimiter(IByteBuffer delimiter)
        {
            if (delimiter is null)
                CThrowHelper.ThrowNullReferenceException(CExceptionArgument.delimiter);

            if (!delimiter.IsReadable())
                CThrowHelper.ThrowArgumentException_EmptyDelimiter();
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        static void ValidateMaxFrameLength(int maxFrameLength)
        {
            if ((uint)(maxFrameLength - 1) > SharedConstants.TooBigOrNegative) // <= 0
                CThrowHelper.ThrowArgumentException_MaxFrameLengthMustBe(maxFrameLength);
        }
    }
}