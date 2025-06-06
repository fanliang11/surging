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
 * Copyright (c) The DotNetty Project (Microsoft). All rights reserved.
 *
 *   https://github.com/azure/dotnetty
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs.Http
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using DotNetty.Buffers;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// Decodes <see cref="IByteBuffer"/>s into <see cref="IHttpMessage"/>s and
    /// <see cref="IHttpContent"/>s.
    ///
    /// <h3>Parameters that prevents excessive memory consumption</h3>
    /// <table border="1">
    /// <tr>
    /// <th>Name</th><th>Default value</th><th>Meaning</th>
    /// </tr>
    /// <tr>
    /// <td>{@code maxInitialLineLength}</td>
    /// <td>{@value #DEFAULT_MAX_INITIAL_LINE_LENGTH}</td>
    /// <td>The maximum length of the initial line
    ///     (e.g. {@code "GET / HTTP/1.0"} or {@code "HTTP/1.0 200 OK"})
    ///     If the length of the initial line exceeds this value, a
    ///     <see cref="TooLongFrameException"/> will be raised.</td>
    /// </tr>
    /// <tr>
    /// <td>{@code maxHeaderSize}</td>
    /// <td>{@value #DEFAULT_MAX_HEADER_SIZE}</td>
    /// <td>The maximum length of all headers.  If the sum of the length of each
    ///     header exceeds this value, a <see cref="TooLongFrameException"/> will be raised.</td>
    /// </tr>
    /// <tr>
    /// <td><see cref="_maxChunkSize"/></td>
    /// <td>{@value #DEFAULT_MAX_CHUNK_SIZE}</td>
    /// <td>The maximum length of the content or each chunk.  If the content length
    ///     (or the length of each chunk) exceeds this value, the content or chunk
    ///     will be split into multiple <see cref="IHttpContent"/>s whose length is
    ///     <see cref="_maxChunkSize"/> at maximum.</td>
    /// </tr>
    /// </table>
    ///
    /// <h3>Parameters that control parsing behavior</h3>
    /// <table border="1">
    /// <tr>
    /// <th>Name</th><th>Default value</th><th>Meaning</th>
    /// </tr>
    /// <tr>
    /// <td>{@code allowDuplicateContentLengths}</td>
    /// <td>{@value #DEFAULT_ALLOW_DUPLICATE_CONTENT_LENGTHS}</td>
    /// <td>When set to {@code false}, will reject any messages that contain multiple Content-Length header fields.
    ///     When set to {@code true}, will allow multiple Content-Length headers only if they are all the same decimal value.
    ///     The duplicated field-values will be replaced with a single valid Content-Length field.
    ///     See <a href="https://tools.ietf.org/html/rfc7230#section-3.3.2">RFC 7230, Section 3.3.2</a>.</td>
    /// </tr>
    /// </table>
    ///
    /// <h3>Chunked Content</h3>
    ///
    /// If the content of an HTTP message is greater than <see cref="_maxChunkSize"/> or
    /// the transfer encoding of the HTTP message is 'chunked', this decoder
    /// generates one <see cref="IHttpMessage"/> instance and its following
    /// <see cref="IHttpContent"/>s per single HTTP message to avoid excessive memory
    /// consumption. For example, the following HTTP message:
    /// <pre>
    /// GET / HTTP/1.1
    /// Transfer-Encoding: chunked
    ///
    /// 1a
    /// abcdefghijklmnopqrstuvwxyz
    /// 10
    /// 1234567890abcdef
    /// 0
    /// Content-MD5: ...
    /// <i>[blank line]</i>
    /// </pre>
    /// triggers <see cref="HttpRequestDecoder"/> to generate 3 objects:
    /// <ol>
    /// <li>An <see cref="IHttpRequest"/>,</li>
    /// <li>The first <see cref="IHttpContent"/> whose content is {@code 'abcdefghijklmnopqrstuvwxyz'},</li>
    /// <li>The second <see cref="ILastHttpContent"/> whose content is {@code '1234567890abcdef'}, which marks
    /// the end of the content.</li>
    /// </ol>
    ///
    /// If you prefer not to handle <see cref="IHttpContent"/>s by yourself for your
    /// convenience, insert <see cref="HttpObjectAggregator"/> after this decoder in the
    /// <see cref="IChannelPipeline"/>.  However, please note that your server might not
    /// be as memory efficient as without the aggregator.
    ///
    /// <h3>Extensibility</h3>
    ///
    /// Please note that this decoder is designed to be extended to implement
    /// a protocol derived from HTTP, such as
    /// <a href="http://en.wikipedia.org/wiki/Real_Time_Streaming_Protocol">RTSP</a> and
    /// <a href="http://en.wikipedia.org/wiki/Internet_Content_Adaptation_Protocol">ICAP</a>.
    /// To implement the decoder of such a derived protocol, extend this class and
    /// implement all abstract methods properly.
    /// </summary>
    public abstract class HttpObjectDecoder : ByteToMessageDecoder
    {
        private const byte c_space = (byte)' ';
        private const byte c_tab = (byte)'\t';

        public const int DefaultMaxInitialLineLength = 4096;
        public const int DefaultMaxHeaderSize = 8192;
        public const bool DefaultChunkedSupported = true;
        public const int DefaultMaxChunkSize = 8192;
        public const bool DefaultValidateHeaders = true;
        public const int DefaultInitialBufferSize = 128;
        public const bool DefaultAllowDuplicateContentLengths = false;

        private static readonly char[] s_commaSeparator = new char[] { ',' };

        protected readonly bool ValidateHeaders;

        private readonly int _maxChunkSize;
        private readonly bool _chunkedSupported;
        private readonly bool _allowDuplicateContentLengths;
        private readonly HeaderParser _headerParser;
        private readonly LineParser _lineParser;

        private IHttpMessage _message;
        private long _chunkSize;
        private long _contentLength = long.MinValue;
        private int _resetRequested;

        // These will be updated by splitHeader(...)
        private AsciiString _name;
        private AsciiString _value;

        private ILastHttpContent _trailer;

        private enum State
        {
            SkipControlChars,
            ReadInitial,
            ReadHeader,
            ReadVariableLengthContent,
            ReadFixedLengthContent,
            ReadChunkSize,
            ReadChunkedContent,
            ReadChunkDelimiter,
            ReadChunkFooter,
            BadMessage,
            Upgraded
        }

        private State _currentState = State.SkipControlChars;

        /// <summary>
        /// Creates a new instance with the default
        /// </summary>
        protected HttpObjectDecoder()
            : this(DefaultMaxInitialLineLength, DefaultMaxHeaderSize, DefaultMaxChunkSize, DefaultChunkedSupported)
        {
        }

        /// <summary>
        /// Creates a new instance with the specified parameters.
        /// </summary>
        /// <param name="maxInitialLineLength"></param>
        /// <param name="maxHeaderSize"></param>
        /// <param name="maxChunkSize"></param>
        /// <param name="chunkedSupported"></param>
        protected HttpObjectDecoder(
            int maxInitialLineLength, int maxHeaderSize, int maxChunkSize, bool chunkedSupported)
            : this(maxInitialLineLength, maxHeaderSize, maxChunkSize, chunkedSupported, DefaultValidateHeaders)
        {
        }

        /// <summary>
        /// Creates a new instance with the specified parameters.
        /// </summary>
        /// <param name="maxInitialLineLength"></param>
        /// <param name="maxHeaderSize"></param>
        /// <param name="maxChunkSize"></param>
        /// <param name="chunkedSupported"></param>
        /// <param name="validateHeaders"></param>
        protected HttpObjectDecoder(
            int maxInitialLineLength, int maxHeaderSize, int maxChunkSize,
            bool chunkedSupported, bool validateHeaders)
            : this(maxInitialLineLength, maxHeaderSize, maxChunkSize, chunkedSupported, validateHeaders,
                  DefaultInitialBufferSize)
        {
        }

        /// <summary>
        /// Creates a new instance with the specified parameters.
        /// </summary>
        /// <param name="maxInitialLineLength"></param>
        /// <param name="maxHeaderSize"></param>
        /// <param name="maxChunkSize"></param>
        /// <param name="chunkedSupported"></param>
        /// <param name="validateHeaders"></param>
        /// <param name="initialBufferSize"></param>
        protected HttpObjectDecoder(
            int maxInitialLineLength, int maxHeaderSize, int maxChunkSize,
            bool chunkedSupported, bool validateHeaders, int initialBufferSize)
            : this(maxInitialLineLength, maxHeaderSize, maxChunkSize, chunkedSupported, validateHeaders, initialBufferSize,
                DefaultAllowDuplicateContentLengths)
        {
        }

        /// <summary>
        /// Creates a new instance with the specified parameters.
        /// </summary>
        /// <param name="maxInitialLineLength"></param>
        /// <param name="maxHeaderSize"></param>
        /// <param name="maxChunkSize"></param>
        /// <param name="chunkedSupported"></param>
        /// <param name="validateHeaders"></param>
        /// <param name="initialBufferSize"></param>
        /// <param name="allowDuplicateContentLengths"></param>
        protected HttpObjectDecoder(
            int maxInitialLineLength, int maxHeaderSize, int maxChunkSize,
            bool chunkedSupported, bool validateHeaders, int initialBufferSize,
            bool allowDuplicateContentLengths)
        {
            if ((uint)(maxInitialLineLength - 1) > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_Positive(maxInitialLineLength, ExceptionArgument.maxInitialLineLength); }
            if ((uint)(maxHeaderSize - 1) > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_Positive(maxHeaderSize, ExceptionArgument.maxHeaderSize); }
            if ((uint)(maxChunkSize - 1) > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_Positive(maxChunkSize, ExceptionArgument.maxChunkSize); }

            var seq = new AppendableCharSequence(initialBufferSize);
            _lineParser = new LineParser(this, seq, maxInitialLineLength);
            _headerParser = new HeaderParser(seq, maxHeaderSize);
            _maxChunkSize = maxChunkSize;
            _chunkedSupported = chunkedSupported;
            ValidateHeaders = validateHeaders;
            _allowDuplicateContentLengths = allowDuplicateContentLengths;
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer buffer, List<object> output)
        {
            if (SharedConstants.False < (uint)Volatile.Read(ref _resetRequested))
            {
                ResetNow();
            }

            switch (_currentState)
            {
                case State.SkipControlChars:
                // Fall through 
                case State.ReadInitial:
                    {
                        try
                        {
                            AppendableCharSequence line = _lineParser.Parse(buffer);
                            if (line is null)
                            {
                                return;
                            }
                            AsciiString[] initialLine = SplitInitialLine(line);
                            if ((uint)initialLine.Length < 3u)
                            {
                                // Invalid initial line - ignore.
                                _currentState = State.SkipControlChars;
                                return;
                            }

                            _message = CreateMessage(initialLine);
                            _currentState = State.ReadHeader;
                            goto case State.ReadHeader; // Fall through
                        }
                        catch (Exception e)
                        {
                            output.Add(InvalidMessage(buffer, e));
                            return;
                        }
                    }
                case State.ReadHeader:
                    {
                        try
                        {
                            State? nextState = ReadHeaders(buffer);
                            if (nextState is null)
                            {
                                return;
                            }
                            _currentState = nextState.Value;
                            switch (nextState.Value)
                            {
                                case State.SkipControlChars:
                                    {
                                        // fast-path
                                        // No content is expected.
                                        output.Add(_message);
                                        output.Add(EmptyLastHttpContent.Default);
                                        ResetNow();
                                        return;
                                    }
                                case State.ReadChunkSize:
                                    {
                                        if (!_chunkedSupported)
                                        {
                                            ThrowHelper.ThrowArgumentException_ChunkedMsgNotSupported();
                                        }
                                        // Chunked encoding - generate HttpMessage first.  HttpChunks will follow.
                                        output.Add(_message);
                                        return;
                                    }
                                default:
                                    {
                                        // <a href="https://tools.ietf.org/html/rfc7230#section-3.3.3">RFC 7230, 3.3.3</a> states that if a
                                        // request does not have either a transfer-encoding or a content-length header then the message body
                                        // length is 0. However for a response the body length is the number of octets received prior to the
                                        // server closing the connection. So we treat this as variable length chunked encoding.
                                        long length = ContentLength();
                                        if (0u >= (uint)length || length == -1 && IsDecodingRequest())
                                        {
                                            output.Add(_message);
                                            output.Add(EmptyLastHttpContent.Default);
                                            ResetNow();
                                            return;
                                        }

                                        Debug.Assert(nextState.Value == State.ReadFixedLengthContent
                                            || nextState.Value == State.ReadVariableLengthContent);

                                        output.Add(_message);

                                        if (nextState == State.ReadFixedLengthContent)
                                        {
                                            // chunkSize will be decreased as the READ_FIXED_LENGTH_CONTENT state reads data chunk by chunk.
                                            _chunkSize = length;
                                        }

                                        // We return here, this forces decode to be called again where we will decode the content
                                        return;
                                    }
                            }
                        }
                        catch (Exception exception)
                        {
                            output.Add(InvalidMessage(buffer, exception));
                            return;
                        }
                    }
                case State.ReadVariableLengthContent:
                    {
                        // Keep reading data as a chunk until the end of connection is reached.
                        int toRead = Math.Min(buffer.ReadableBytes, _maxChunkSize);
                        if (toRead > 0)
                        {
                            IByteBuffer content = buffer.ReadRetainedSlice(toRead);
                            output.Add(new DefaultHttpContent(content));
                        }
                        return;
                    }
                case State.ReadFixedLengthContent:
                    {
                        int readLimit = buffer.ReadableBytes;

                        // Check if the buffer is readable first as we use the readable byte count
                        // to create the HttpChunk. This is needed as otherwise we may end up with
                        // create an HttpChunk instance that contains an empty buffer and so is
                        // handled like it is the last HttpChunk.
                        //
                        // See https://github.com/netty/netty/issues/433
                        if (0u >= (uint)readLimit)
                        {
                            return;
                        }

                        int toRead = Math.Min(readLimit, _maxChunkSize);
                        if (toRead > _chunkSize)
                        {
                            toRead = (int)_chunkSize;
                        }
                        IByteBuffer content = buffer.ReadRetainedSlice(toRead);
                        _chunkSize -= toRead;

                        if (0ul >= (ulong)_chunkSize)
                        {
                            // Read all content.
                            output.Add(new DefaultLastHttpContent(content, ValidateHeaders));
                            ResetNow();
                        }
                        else
                        {
                            output.Add(new DefaultHttpContent(content));
                        }
                        return;
                    }
                //  everything else after this point takes care of reading chunked content. basically, read chunk size,
                //  read chunk, read and ignore the CRLF and repeat until 0
                case State.ReadChunkSize:
                    {
                        try
                        {
                            AppendableCharSequence line = _lineParser.Parse(buffer);
                            if (line is null)
                            {
                                return;
                            }
                            int size = GetChunkSize(line.ToAsciiString());
                            _chunkSize = size;
                            if (0u >= (uint)size)
                            {
                                _currentState = State.ReadChunkFooter;
                                return;
                            }
                            _currentState = State.ReadChunkedContent;
                            goto case State.ReadChunkedContent; // fall-through
                        }
                        catch (Exception e)
                        {
                            output.Add(InvalidChunk(buffer, e));
                            return;
                        }
                    }
                case State.ReadChunkedContent:
                    {
                        Debug.Assert(_chunkSize <= int.MaxValue);

                        int toRead = Math.Min((int)_chunkSize, _maxChunkSize);
                        toRead = Math.Min(toRead, buffer.ReadableBytes);
                        if (0u >= (uint)toRead)
                        {
                            return;
                        }
                        IHttpContent chunk = new DefaultHttpContent(buffer.ReadRetainedSlice(toRead));
                        _chunkSize -= toRead;

                        output.Add(chunk);

                        if (_chunkSize != 0)
                        {
                            return;
                        }
                        _currentState = State.ReadChunkDelimiter;
                        goto case State.ReadChunkDelimiter; // fall-through
                    }
                case State.ReadChunkDelimiter:
                    {
                        int wIdx = buffer.WriterIndex;
                        int rIdx = buffer.ReaderIndex;
                        // TODO ForEachByte
                        while (wIdx > rIdx)
                        {
                            byte next = buffer.GetByte(rIdx++);
                            if (next == HttpConstants.LineFeed)
                            {
                                _currentState = State.ReadChunkSize;
                                break;
                            }
                        }
                        _ = buffer.SetReaderIndex(rIdx);
                        return;
                    }
                case State.ReadChunkFooter:
                    {
                        try
                        {
                            ILastHttpContent lastTrialer = ReadTrailingHeaders(buffer);
                            if (lastTrialer is null)
                            {
                                return;
                            }
                            output.Add(lastTrialer);
                            ResetNow();
                            return;
                        }
                        catch (Exception exception)
                        {
                            output.Add(InvalidChunk(buffer, exception));
                            return;
                        }
                    }
                case State.BadMessage:
                    {
                        // Keep discarding until disconnection.
                        _ = buffer.SkipBytes(buffer.ReadableBytes);
                        break;
                    }
                case State.Upgraded:
                    {
                        int readableBytes = buffer.ReadableBytes;
                        if (readableBytes > 0)
                        {
                            // Keep on consuming as otherwise we may trigger an DecoderException,
                            // other handler will replace this codec with the upgraded protocol codec to
                            // take the traffic over at some point then.
                            // See https://github.com/netty/netty/issues/2173
                            output.Add(buffer.ReadBytes(readableBytes));
                        }
                        break;
                    }
            }
        }

        protected override void DecodeLast(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            base.DecodeLast(context, input, output);

            if (SharedConstants.False < (uint)Volatile.Read(ref _resetRequested))
            {
                // If a reset was requested by decodeLast() we need to do it now otherwise we may produce a
                // LastHttpContent while there was already one.
                ResetNow();
            }

            // Handle the last unfinished message.
            if (_message is object)
            {
                bool chunked = HttpUtil.IsTransferEncodingChunked(_message);
                if (_currentState == State.ReadVariableLengthContent
                    && !input.IsReadable() && !chunked)
                {
                    // End of connection.
                    output.Add(EmptyLastHttpContent.Default);
                    ResetNow();
                    return;
                }

                if (_currentState == State.ReadHeader)
                {
                    // If we are still in the state of reading headers we need to create a new invalid message that
                    // signals that the connection was closed before we received the headers.
                    output.Add(InvalidMessage(Unpooled.Empty,
                        new PrematureChannelClosureException("Connection closed before received headers")));
                    ResetNow();
                    return;
                }

                // Check if the closure of the connection signifies the end of the content.
                bool prematureClosure;
                if (IsDecodingRequest() || chunked)
                {
                    // The last request did not wait for a response.
                    prematureClosure = true;
                }
                else
                {
                    // Compare the length of the received content and the 'Content-Length' header.
                    // If the 'Content-Length' header is absent, the length of the content is determined by the end of the
                    // connection, so it is perfectly fine.
                    prematureClosure = ContentLength() > 0;
                }

                if (!prematureClosure)
                {
                    output.Add(EmptyLastHttpContent.Default);
                }
                ResetNow();
            }
        }

        public override void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            if (evt is HttpExpectationFailedEvent)
            {
                switch (_currentState)
                {
                    case State.ReadFixedLengthContent:
                    case State.ReadVariableLengthContent:
                    case State.ReadChunkSize:
                        Reset();
                        break;
                }
            }
            base.UserEventTriggered(context, evt);
        }

        protected virtual bool IsContentAlwaysEmpty(IHttpMessage msg)
        {
            if (msg is IHttpResponse res)
            {
                int code = res.Status.Code;

                // Correctly handle return codes of 1xx.
                //
                // See:
                //     - http://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html Section 4.4
                //     - https://github.com/netty/netty/issues/222
                if (code >= 100 && code < 200)
                {
                    // One exception: Hixie 76 websocket handshake response
                    return !(code == 101 && !res.Headers.Contains(HttpHeaderNames.SecWebsocketAccept)
                        && res.Headers.Contains(HttpHeaderNames.Upgrade, HttpHeaderValues.Websocket, true));
                }
                switch (code)
                {
                    case 204:
                    case 304:
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if the server switched to a different protocol than HTTP/1.0 or HTTP/1.1, e.g. HTTP/2 or Websocket.
        /// Returns false if the upgrade happened in a different layer, e.g. upgrade from HTTP/1.1 to HTTP/1.1 over TLS.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        protected bool IsSwitchingToNonHttp1Protocol(IHttpResponse msg)
        {
            if (msg.Status.Code != StatusCodes.Status101SwitchingProtocols)
            {
                return false;
            }

            return !msg.Headers.TryGet(HttpHeaderNames.Upgrade, out ICharSequence newProtocol)
                || !AsciiString.Contains(newProtocol, HttpVersion.Http10String)
                   && !AsciiString.Contains(newProtocol, HttpVersion.Http11String);
        }

        /// <summary>
        /// Resets the state of the decoder so that it is ready to decode a new message.
        /// This method is useful for handling a rejected request with {@code Expect: 100-continue} header.
        /// </summary>
        public void Reset() => Interlocked.Exchange(ref _resetRequested, SharedConstants.True);

        void ResetNow()
        {
            IHttpMessage msg = _message;
            _message = null;
            _name = null;
            _value = null;
            _contentLength = long.MinValue;
            _lineParser.Reset();
            _headerParser.Reset();
            _trailer = null;
            if (!IsDecodingRequest())
            {
                if (msg is IHttpResponse res && IsSwitchingToNonHttp1Protocol(res))
                {
                    _currentState = State.Upgraded;
                    return;
                }
            }

            _ = Interlocked.Exchange(ref _resetRequested, SharedConstants.False);
            _currentState = State.SkipControlChars;
        }

        IHttpMessage InvalidMessage(IByteBuffer buf, Exception cause)
        {
            _currentState = State.BadMessage;

            // Advance the readerIndex so that ByteToMessageDecoder does not complain
            // when we produced an invalid message without consuming anything.
            _ = buf.SkipBytes(buf.ReadableBytes);

            if (_message is null)
            {
                _message = CreateInvalidMessage();
            }
            _message.Result = DecoderResult.Failure(cause);

            IHttpMessage ret = _message;
            _message = null;
            return ret;
        }

        IHttpContent InvalidChunk(IByteBuffer buf, Exception cause)
        {
            _currentState = State.BadMessage;

            // Advance the readerIndex so that ByteToMessageDecoder does not complain
            // when we produced an invalid message without consuming anything.
            _ = buf.SkipBytes(buf.ReadableBytes);

            IHttpContent chunk = new DefaultLastHttpContent(Unpooled.Empty)
            {
                Result = DecoderResult.Failure(cause)
            };
            _message = null;
            _trailer = null;
            return chunk;
        }

        State? ReadHeaders(IByteBuffer buffer)
        {
            IHttpMessage httpMessage = _message;
            HttpHeaders headers = httpMessage.Headers;

            AppendableCharSequence line = _headerParser.Parse(buffer);
            if (line is null)
            {
                return null;
            }
            // ReSharper disable once ConvertIfDoToWhile
            if ((uint)line.Count > 0u)
            {
                do
                {
                    byte firstChar = line.Bytes[0];
                    if (_name is object && (firstChar == c_space || firstChar == c_tab))
                    {
                        //please do not make one line from below code
                        //as it breaks +XX:OptimizeStringConcat optimization
                        ICharSequence trimmedLine = CharUtil.Trim(line);
                        _value = new AsciiString($"{_value} {trimmedLine}");
                    }
                    else
                    {
                        if (_name is object)
                        {
                            _ = headers.Add(_name, _value);
                        }
                        SplitHeader(line);
                    }

                    line = _headerParser.Parse(buffer);
                    if (line is null)
                    {
                        return null;
                    }
                } while ((uint)line.Count > 0u);
            }

            // Add the last header.
            if (_name is object)
            {
                _ = headers.Add(_name, _value);
            }

            // reset name and value fields
            _name = null;
            _value = null;

            var contentLengthFields = headers.GetAll(HttpHeaderNames.ContentLength);
            uint contentLengthValuesCount = (uint)contentLengthFields.Count;

            if (contentLengthValuesCount > 0u)
            {
                // Guard against multiple Content-Length headers as stated in
                // https://tools.ietf.org/html/rfc7230#section-3.3.2:
                //
                // If a message is received that has multiple Content-Length header
                //   fields with field-values consisting of the same decimal value, or a
                //   single Content-Length header field with a field value containing a
                //   list of identical decimal values (e.g., "Content-Length: 42, 42"),
                //   indicating that duplicate Content-Length header fields have been
                //   generated or combined by an upstream message processor, then the
                //   recipient MUST either reject the message as invalid or replace the
                //   duplicated field-values with a single valid Content-Length field
                //   containing that decimal value prior to determining the message body
                //   length or forwarding the message.
                bool multipleContentLengths =
                    contentLengthValuesCount > 1u || SharedConstants.TooBigOrNegative >= (uint)contentLengthFields[0].IndexOf(StringUtil.Comma); // >= 0
                if (multipleContentLengths && httpMessage.ProtocolVersion == HttpVersion.Http11)
                {
                    if (_allowDuplicateContentLengths)
                    {
                        // Find and enforce that all Content-Length values are the same
                        string firstValue = null;
                        for (int idx = 0; idx < contentLengthFields.Count; idx++)
                        {
                            string[] tokens = contentLengthFields[idx].ToString().Split(
#if NETCOREAPP_3_0_GREATER || NETSTANDARD_2_0_GREATER
                                StringUtil.Comma
#else
                                s_commaSeparator
#endif
                                );
                            for (int tokenIdx = 0; tokenIdx < tokens.Length; tokenIdx++)
                            {
                                string trimmed = tokens[tokenIdx].Trim();
                                if (firstValue is null)
                                {
                                    firstValue = trimmed;
                                }
                                else if (!string.Equals(trimmed, firstValue))
                                {
                                    ThrowHelper.ThrowArgumentException_Multiple_Content_Length_Headers_Found(contentLengthFields);
                                }
                            }
                        }
                        // Replace the duplicated field-values with a single valid Content-Length field
                        headers.Set(HttpHeaderNames.ContentLength, firstValue);
                        if (!long.TryParse(firstValue, out _contentLength))
                        {
                            ThrowHelper.ThrowArgumentException_Invalid_Content_Length();
                        }
                    }
                    else
                    {
                        // Reject the message as invalid
                        ThrowHelper.ThrowArgumentException_Multiple_Content_Length_Headers_Found(contentLengthFields);
                    }
                }
                else
                {
                    if (!long.TryParse(contentLengthFields[0].ToString(), out _contentLength))
                    {
                        ThrowHelper.ThrowArgumentException_Invalid_Content_Length();
                    }
                }
            }

            if (IsContentAlwaysEmpty(httpMessage))
            {
                HttpUtil.SetTransferEncodingChunked(httpMessage, false);
                return State.SkipControlChars;
            }
            else if (HttpUtil.IsTransferEncodingChunked(httpMessage))
            {
                if (contentLengthValuesCount > 0u && httpMessage.ProtocolVersion == HttpVersion.Http11)
                {
                    HandleTransferEncodingChunkedWithContentLength(httpMessage);
                }

                return State.ReadChunkSize;
            }
            else if (ContentLength() >= 0L)
            {
                return State.ReadFixedLengthContent;
            }
            else
            {
                return State.ReadVariableLengthContent;
            }
        }

        /// <summary>
        /// Invoked when a message with both a "Transfer-Encoding: chunked" and a "Content-Length" header field is detected.
        /// The default behavior is to <i>remove</i> the Content-Length field, but this method could be overridden
        /// to change the behavior (to, e.g., throw an exception and produce an invalid message).
        /// 
        /// <para>See: https://tools.ietf.org/html/rfc7230#section-3.3.3 </para>
        /// <para>
        ///     If a message is received with both a Transfer-Encoding and a
        ///     Content-Length header field, the Transfer-Encoding overrides the
        ///     Content-Length.  Such a message might indicate an attempt to
        ///     perform request smuggling (Section 9.5) or response splitting
        ///     (Section 9.4) and ought to be handled as an error.  A sender MUST
        ///     remove the received Content-Length field prior to forwarding such
        ///     a message downstream.
        /// </para>
        /// Also see:
        /// https://github.com/apache/tomcat/blob/b693d7c1981fa7f51e58bc8c8e72e3fe80b7b773/
        /// java/org/apache/coyote/http11/Http11Processor.java#L747-L755
        /// https://github.com/nginx/nginx/blob/0ad4393e30c119d250415cb769e3d8bc8dce5186/
        /// src/http/ngx_http_request.c#L1946-L1953
        /// </summary>
        /// <param name="message"></param>
        protected void HandleTransferEncodingChunkedWithContentLength(IHttpMessage message)
        {
            _ = message.Headers.Remove(HttpHeaderNames.ContentLength);
            _contentLength = long.MinValue;
        }

        private long ContentLength()
        {
            if (_contentLength == long.MinValue)
            {
                _contentLength = HttpUtil.GetContentLength(_message, -1L);
            }
            return _contentLength;
        }

        private ILastHttpContent ReadTrailingHeaders(IByteBuffer buffer)
        {
            AppendableCharSequence line = _headerParser.Parse(buffer);
            if (line is null)
            {
                return null;
            }
            ILastHttpContent trailingHeaders = _trailer;
            if (0u >= (uint)line.Count && trailingHeaders is null)
            {
                // We have received the empty line which signals the trailer is complete and did not parse any trailers
                // before. Just return an empty last content to reduce allocations.
                return EmptyLastHttpContent.Default;
            }

            AsciiString lastHeader = null;
            if (trailingHeaders is null)
            {
                trailingHeaders = new DefaultLastHttpContent(Unpooled.Empty, ValidateHeaders);
                _trailer = trailingHeaders;
            }
            while ((uint)line.Count > 0u)
            {
                byte firstChar = line.Bytes[0];
                if (lastHeader is object && (firstChar == c_space || firstChar == c_tab))
                {
                    IList<ICharSequence> current = trailingHeaders.TrailingHeaders.GetAll(lastHeader);
                    if ((uint)current.Count > 0u)
                    {
                        int lastPos = current.Count - 1;
                        //please do not make one line from below code
                        //as it breaks +XX:OptimizeStringConcat optimization
                        ICharSequence lineTrimmed = CharUtil.Trim(line);
                        current[lastPos] = new AsciiString($"{current[lastPos]}{lineTrimmed}");
                    }
                }
                else
                {
                    SplitHeader(line);
                    AsciiString headerName = _name;
                    if (!HttpHeaderNames.ContentLength.ContentEqualsIgnoreCase(headerName)
                        && !HttpHeaderNames.TransferEncoding.ContentEqualsIgnoreCase(headerName)
                        && !HttpHeaderNames.Trailer.ContentEqualsIgnoreCase(headerName))
                    {
                        _ = trailingHeaders.TrailingHeaders.Add(headerName, _value);
                    }
                    lastHeader = _name;
                    // reset name and value fields
                    _name = null;
                    _value = null;
                }

                line = _headerParser.Parse(buffer);
                if (line is null)
                {
                    return null;
                }
            }

            _trailer = null;
            return trailingHeaders;
        }

        protected abstract bool IsDecodingRequest();

        protected abstract IHttpMessage CreateMessage(AsciiString[] initialLine);

        protected abstract IHttpMessage CreateInvalidMessage();

        private static int GetChunkSize(AsciiString hex)
        {
            hex = hex.Trim();
            for (int i = hex.Offset; i < hex.Count; i++)
            {
                byte c = hex.Array[i];
                if (IsWhiteSpaceOrSemicolonOrISOControl(c))
                {
                    hex = (AsciiString)hex.SubSequence(0, i);
                    break;
                }
            }

            return hex.ParseInt(16);
        }

        private static AsciiString[] SplitInitialLine(AppendableCharSequence sb)
        {
            byte[] chars = sb.Bytes;
            int length = sb.Count;

            int aStart = FindNonSPLenient(chars, 0, length);
            int aEnd = FindSPLenient(chars, aStart, length);

            int bStart = FindNonSPLenient(chars, aEnd, length);
            int bEnd = FindSPLenient(chars, bStart, length);

            int cStart = FindNonSPLenient(chars, bEnd, length);
            int cEnd = FindEndOfString(chars, length);

            return new[]
            {
                sb.SubStringUnsafe(aStart, aEnd),
                sb.SubStringUnsafe(bStart, bEnd),
                cStart < cEnd ? sb.SubStringUnsafe(cStart, cEnd) : AsciiString.Empty
            };
        }

        private void SplitHeader(AppendableCharSequence sb)
        {
            byte[] chars = sb.Bytes;
            int length = sb.Count;
            int nameEnd;
            int colonEnd;

            int nameStart = FindNonWhitespace(chars, 0, length, false);
            for (nameEnd = nameStart; nameEnd < length; nameEnd++)
            {
                byte ch = chars[nameEnd];
                // https://tools.ietf.org/html/rfc7230#section-3.2.4
                //
                // No whitespace is allowed between the header field-name and colon. In
                // the past, differences in the handling of such whitespace have led to
                // security vulnerabilities in request routing and response handling. A
                // server MUST reject any received request message that contains
                // whitespace between a header field-name and colon with a response code
                // of 400 (Bad Request). A proxy MUST remove any such whitespace from a
                // response message before forwarding the message downstream.
                if (ch == ':' ||
                        // In case of decoding a request we will just continue processing and header validation
                        // is done in the DefaultHttpHeaders implementation.
                        //
                        // In the case of decoding a response we will "skip" the whitespace.
                        (!IsDecodingRequest() && IsOWS(ch)))
                {
                    break;
                }
            }

            if (0u >= (uint)(nameEnd - length))
            {
                // There was no colon present at all.
                ThrowHelper.ThrowArgumentException_No_colon_found();
            }

            for (colonEnd = nameEnd; colonEnd < length; colonEnd++)
            {
                if (chars[colonEnd] == HttpConstants.Colon)
                {
                    colonEnd++;
                    break;
                }
            }

            _name = sb.SubStringUnsafe(nameStart, nameEnd);
            int valueStart = FindNonWhitespace(chars, colonEnd, length, true);
            if (valueStart == length)
            {
                _value = AsciiString.Empty;
            }
            else
            {
                int valueEnd = FindEndOfString(chars, length);
                _value = sb.SubStringUnsafe(valueStart, valueEnd);
            }
        }

        private static int FindNonSPLenient(byte[] sb, int offset, int length)
        {
            for (int result = offset; result < length; ++result)
            {
                var c = sb[result];

                // See https://tools.ietf.org/html/rfc7230#section-3.5
                if (IsSPLenient(c)) { continue; }

                if (IsWhiteSpace(c))
                {
                    // Any other whitespace delimiter is invalid
                    ThrowHelper.ThrowArgumentException_Invalid_separator();
                }
                return result;
            }
            return length;
        }

        private static int FindSPLenient(byte[] sb, int offset, int length)
        {
            for (int result = offset; result < length; ++result)
            {
                if (IsSPLenient(sb[result]))
                {
                    return result;
                }
            }
            return length;
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        private static bool IsSPLenient(byte c)
        {
            // See https://tools.ietf.org/html/rfc7230#section-3.5
            switch (c)
            {
                case HttpConstants.Space:
                case 0x09:
                case 0x0B:
                case 0x0C:
                case 0x0D:
                    return true;

                default:
                    return false;
            }
        }

        private static int FindNonWhitespace(byte[] sb, int offset, int length, bool validateOWS)
        {
            for (int result = offset; result < length; ++result)
            {
                var c = sb[result];
                if (!IsWhiteSpace(c))
                {
                    return result;
                }
                else if (validateOWS && !IsOWS(c))
                {
                    // Only OWS is supported for whitespace
                    ThrowHelper.ThrowArgumentException_Invalid_separator_only_a_single_space_or_horizontal_tab_allowed(c);
                }
            }
            return length;
        }

        private static int FindEndOfString(byte[] sb, int length)
        {
            for (int result = length - 1; result > 0; --result)
            {
                if (!IsWhiteSpace(sb[result]))
                {
                    return result + 1;
                }
            }
            return 0;
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        private static bool IsOWS(byte ch)
        {
            switch (ch)
            {
                case HttpConstants.Space:
                case 0x09:
                    return true;
                default:
                    return false;
            }
        }

        class HeaderParser : IByteProcessor
        {
            readonly AppendableCharSequence _seq;
            readonly int _maxLength;
            int _size;

            internal HeaderParser(AppendableCharSequence seq, int maxLength)
            {
                _seq = seq;
                _maxLength = maxLength;
            }

            public virtual AppendableCharSequence Parse(IByteBuffer buffer)
            {
                int oldSize = _size;
                _seq.Reset();
                int i = buffer.ForEachByte(this);
                if (i == -1)
                {
                    _size = oldSize;
                    return null;
                }
                _ = buffer.SetReaderIndex(i + 1);
                return _seq;
            }

            public void Reset() => _size = 0;

            [MethodImpl(InlineMethod.AggressiveInlining)]
            public virtual bool Process(byte value)
            {
                if (0u >= (uint)(HttpConstants.LineFeed - value))
                {
                    int len = _seq.Count;
                    // Drop CR if we had a CRLF pair
                    if ((uint)len >= 1u && 0u >= (uint)(_seq[len - 1] - HttpConstants.CarriageReturn))
                    {
                        --_size;
                        _seq.SetCount(len - 1);
                    }
                    return false;
                }

                IncreaseCount();

                _ = _seq.Append(value);
                return true;
            }

            [MethodImpl(InlineMethod.AggressiveOptimization)]
            protected void IncreaseCount()
            {
                if (++_size > _maxLength)
                {
                    // TODO: Respond with Bad Request and discard the traffic
                    //    or close the connection.
                    //       No need to notify the upstream handlers - just log.
                    //       If decoding a response, just throw an exception.
                    ThrowTooLongFrameException(this, _maxLength);
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void ThrowTooLongFrameException(HeaderParser parser, int length)
            {
                throw GetTooLongFrameException();

                TooLongFrameException GetTooLongFrameException()
                {
                    return new TooLongFrameException(parser.NewExceptionMessage(length));
                }
            }

            protected virtual string NewExceptionMessage(int length) => $"HTTP header is larger than {length} bytes.";
        }

        sealed class LineParser : HeaderParser
        {
            private readonly HttpObjectDecoder _owner;

            internal LineParser(HttpObjectDecoder owner, AppendableCharSequence seq, int maxLength)
                : base(seq, maxLength)
            {
                _owner = owner;
            }

            public override AppendableCharSequence Parse(IByteBuffer buffer)
            {
                Reset();
                return base.Parse(buffer);
            }

            public override bool Process(byte value)
            {
                if (_owner._currentState == State.SkipControlChars)
                {
                    //char c = (char)(value & 0xFF); // Java byte = .net sbyte
                    if (IsWhiteSpaceOrISOControl(value)) // Character.isISOControl(c) || Character.isWhitespace(c)
                    {
                        IncreaseCount();
                        return true;
                    }
                    _owner._currentState = State.ReadInitial;
                }
                return base.Process(value);
            }

            protected override string NewExceptionMessage(int maxLength) => $"An HTTP line is larger than {maxLength} bytes.";
        }

        // Similar to char.IsWhiteSpace for ascii
        [MethodImpl(InlineMethod.AggressiveInlining)]
        private static bool IsWhiteSpace(byte c)
        {
            switch (c)
            {
                case HttpConstants.HorizontalSpace:
                case HttpConstants.HorizontalTab:
                case HttpConstants.CarriageReturn:
                    return true;
                default:
                    return false;
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        private static bool IsWhiteSpaceOrISOControl(byte c)
        {
            switch (c)
            {
                case HttpConstants.HorizontalSpace:
                case HttpConstants.HorizontalTab:
                case HttpConstants.CarriageReturn:
                    return true;
                default:
                    return CharUtil.IsISOControl(c);
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        private static bool IsWhiteSpaceOrSemicolonOrISOControl(byte c)
        {
            switch (c)
            {
                case HttpConstants.Semicolon:
                case HttpConstants.HorizontalSpace:
                case HttpConstants.HorizontalTab:
                case HttpConstants.CarriageReturn:
                    return true;
                default:
                    return CharUtil.IsISOControl(c);
            }
        }
    }
}
