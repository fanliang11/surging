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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com)
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Codecs.Compression;
using DotNetty.Codecs.Http.Cookies;
using DotNetty.Codecs.Http.Multipart;
using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;

namespace DotNetty.Codecs.Http
{
    #region -- ExceptionArgument --

    /// <summary>The convention for this enum is using the argument name as the enum name</summary>
    internal enum ExceptionArgument
    {
        s,

        ts,
        pi,
        fi,

        asm,
        func,
        key,
        obj,
        str,
        uri,

        data,
        text,
        name,
        item,
        type,
        list,
        pool,
        path,

        input,
        query,
        array,
        inner,
        other,
        count,
        types,
        value,
        match,
        index,

        header,
        cookie,
        output,
        status,
        target,
        member,
        policy,
        offset,
        values,
        buffer,
        source,
        method,
        length,

        feature,
        manager,
        options,
        channel,
        newSize,
        invoker,
        content,
        version,
        headers,
        cookies,
        request,
        charset,
        factory,

        fileName,
        filename,
        encoding,
        typeName,
        assembly,
        fullName,
        typeInfo,
        capacity,

        maxParams,
        fieldInfo,
        predicate,
        defaultFn,

        memberInfo,
        collection,
        expression,
        startIndex,
        returnType,
        parameters,
        fileStream,
        configList,

        closeStatus,
        stringValue,
        queryString,
        inputStream,
        contentType,
        sourceCodec,
        directories,
        dirEnumArgs,
        destination,

        clientConfig,
        protocolName,
        reasonPhrase,
        majorVersion,
        minorVersion,
        upgradeCodec,
        maxChunkSize,
        propertyInfo,
        valueFactory,
        instanceType,
        serverConfig,

        ReadDelimiter,
        decoderConfig,
        maxHeaderSize,
        attributeType,

        contentEncoder,
        trailingHeader,
        parameterTypes,

        trailingHeaders,

        contentTypeValue,

        qualifiedTypeName,
        assemblyPredicate,
        includedAssemblies,

        upgradeCodecFactory,

        maxInitialLineLength,
        extensionHandshakers,
        contentSizeThreshold,

        targetContentEncoding,
        ReadDelimiterStandard,

        extensionDecoderFilter,
        extensionEncoderFilter,
        handshakeTimeoutMillis,

        extensionFilterProvider,

        preferredClientWindowSize,
        requestedServerWindowSize,

        HttpPostMultipartRequestDecoder,
        HttpPostStandardRequestDecoder,
    }

    #endregion

    #region -- ExceptionResource --

    /// <summary>The convention for this enum is using the resource name as the enum name</summary>
    internal enum ExceptionResource
    {
    }

    #endregion

    partial class ThrowHelper
    {
        #region -- Exception --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowException_FrameDecoder()
        {
            throw GetException();

            static Exception GetException()
            {
                return new Exception("Shouldn't reach here.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static WebSocketFrame ThrowException_UnkonwFrameType()
        {
            throw GetException();

            static Exception GetException()
            {
                return new Exception("Unkonw WebSocketFrame type, must be either TextWebSocketFrame or BinaryWebSocketFrame");
            }
        }

        #endregion

        #region -- ArgumentException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_Positive(int value, ExceptionArgument argument)
        {
            throw GetException();
            ArgumentOutOfRangeException GetException()
            {
                return new ArgumentOutOfRangeException($"{GetArgumentName(argument)}: {value} (expected: > 0)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_Positive(long value, ExceptionArgument argument)
        {
            throw GetException();
            ArgumentOutOfRangeException GetException()
            {
                return new ArgumentOutOfRangeException($"{GetArgumentName(argument)}: {value} (expected: > 0)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_PositiveOrZero(int value, ExceptionArgument argument)
        {
            throw GetException();
            ArgumentOutOfRangeException GetException()
            {
                return new ArgumentOutOfRangeException($"{GetArgumentName(argument)}: {value} (expected: >= 0)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_PositiveOrZero(long value, ExceptionArgument argument)
        {
            throw GetException();
            ArgumentOutOfRangeException GetException()
            {
                return new ArgumentOutOfRangeException($"{GetArgumentName(argument)}: {value} (expected: >= 0)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_Positive(ExceptionArgument argument)
        {
            throw GetArgumentException();
            ArgumentOutOfRangeException GetArgumentException()
            {
                return new ArgumentOutOfRangeException($"{GetArgumentName(argument)} (expected: > 0)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_Empty(ExceptionArgument argument)
        {
            throw GetArgumentException();
            ArgumentException GetArgumentException()
            {
                return new ArgumentException("empty " + GetArgumentName(argument));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_No_colon_found()
        {
            throw GetException();
            static ArgumentException GetException()
            {
                return new ArgumentException("No colon found");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_Multiple_Content_Length_Headers_Found(IList<ICharSequence> contentLengthFields)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException("Multiple Content-Length values found: " + string.Join(",", contentLengthFields));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_Invalid_Content_Length()
        {
            throw GetException();
            static ArgumentException GetException()
            {
                return new ArgumentException("Invalid Content-Length");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_NullText()
        {
            throw GetArgumentException();

            static ArgumentException GetArgumentException()
            {
                return new ArgumentException("text");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_EmptyText()
        {
            throw GetArgumentException();

            static ArgumentException GetArgumentException()
            {
                return new ArgumentException("text is empty (possibly HTTP/0.9)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_HeaderName()
        {
            throw GetArgumentException();

            static ArgumentException GetArgumentException()
            {
                return new ArgumentException("empty headers are not allowed", "name");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_TrailingHeaderName(ICharSequence name)
        {
            throw GetArgumentException();

            ArgumentException GetArgumentException()
            {
                return new ArgumentException(string.Format("prohibited trailing header: {0}", name));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_HeaderValue(uint value)
        {
            throw GetArgumentException();

            ArgumentException GetArgumentException()
            {
                return new ArgumentException(string.Format("a header name cannot contain the following prohibited characters: =,;: \\t\\r\\n\\v\\f: {0}", value));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_HeaderValue(char value)
        {
            throw GetArgumentException();

            ArgumentException GetArgumentException()
            {
                return new ArgumentException(string.Format("a header name cannot contain the following prohibited characters: =,;: \\t\\r\\n\\v\\f: {0}", value));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_HeaderValueNonAscii(uint value)
        {
            throw GetArgumentException();

            ArgumentException GetArgumentException()
            {
                return new ArgumentException(string.Format("a header name cannot contain non-ASCII character: {0}", value));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_HeaderValueNonAscii(char value)
        {
            throw GetArgumentException();

            ArgumentException GetArgumentException()
            {
                return new ArgumentException(string.Format("a header name cannot contain non-ASCII character: {0}", value));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_HeaderValueEnd(ICharSequence seq)
        {
            throw GetArgumentException();

            ArgumentException GetArgumentException()
            {
                return new ArgumentException(string.Format("a header value must not end with '\\r' or '\\n':{0}", seq));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_HeaderValueNullChar()
        {
            throw GetArgumentException();

            static ArgumentException GetArgumentException()
            {
                return new ArgumentException("a header value contains a prohibited character '\0'");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_HeaderValueVerticalTabChar()
        {
            throw GetArgumentException();

            static ArgumentException GetArgumentException()
            {
                return new ArgumentException("a header value contains a prohibited character '\\v'");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_HeaderValueFormFeed()
        {
            throw GetArgumentException();

            static ArgumentException GetArgumentException()
            {
                return new ArgumentException("a header value contains a prohibited character '\\f'");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_NewLineAfterLineFeed()
        {
            throw GetArgumentException();

            static ArgumentException GetArgumentException()
            {
                return new ArgumentException("only '\\n' is allowed after '\\r'");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_TabAndSpaceAfterLineFeed()
        {
            throw GetArgumentException();

            static ArgumentException GetArgumentException()
            {
                return new ArgumentException("only ' ' and '\\t' are allowed after '\\n'");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_DiffArrayLen()
        {
            throw GetException();

            static ArgumentException GetException()
            {
                return new ArgumentException("Different array length");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_BufferNoBacking()
        {
            throw GetException();

            static ArgumentException GetException()
            {
                return new ArgumentException("buffer hasn't backing byte array");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_FileTooBig()
        {
            throw GetException();

            static ArgumentException GetException()
            {
                return new ArgumentException("File too big to be loaded in memory");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_CookieName(string name, int pos)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"Cookie name contains an invalid char: {name[pos]}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_CookieValue(string value)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"Cookie value wrapping quotes are not balanced: {value}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_CookieValue(ICharSequence unwrappedValue, int pos)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"Cookie value contains an invalid char: {unwrappedValue[pos]}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_ValidateAttrValue(string name, string value, int index)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"{name} contains the prohibited characters: ${value[index]}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static int FromArgumentException_CompareToCookie()
        {
            throw GetException();

            static ArgumentException GetException()
            {
                return new ArgumentException($"obj must be of {nameof(ICookie)} type");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static int FromArgumentException_CompareToHttpVersion()
        {
            throw GetException();

            static ArgumentException GetException()
            {
                return new ArgumentException($"obj must be of {nameof(HttpVersion)} type");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static int FromArgumentException_CompareToHttpData(HttpDataType x, HttpDataType y)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"Cannot compare {x} with {y}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_Stream_NotReadable()
        {
            throw GetException();

            static ArgumentException GetException()
            {
                return new ArgumentException($"inputStream is not readable");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_Stream_NotWritable()
        {
            throw GetException();

            static ArgumentException GetException()
            {
                return new ArgumentException($"destination is not writable");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_AttrBigger()
        {
            throw GetException();

            static ArgumentException GetException()
            {
                return new ArgumentException("Attribute bigger than maxSize allowed");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_TextFrame()
        {
            throw GetException();

            static ArgumentException GetException()
            {
                return new ArgumentException("a text frame should not contain 0xFF.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_HeadCantAddSelf()
        {
            throw GetException();

            static ArgumentException GetException()
            {
                return new ArgumentException("can't add to itself.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_ChunkedMsgNotSupported()
        {
            throw GetException();

            static ArgumentException GetException()
            {
                return new ArgumentException("Chunked messages not supported");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_InvalidMethodName(char c, string name)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"Invalid character '{c}' in {name}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_CompressionLevel(int compressionLevel)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"compressionLevel: {compressionLevel} (expected: 0-9)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_WindowBits(int windowBits)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException("windowBits: " + windowBits + " (expected: 9-15)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_MemLevel(int memLevel)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException("memLevel: " + memLevel + " (expected: 1-9)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_WindowSize(ExceptionArgument argument, int windowSize)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"{GetArgumentName(argument)}: {windowSize} (expected: 8-15)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_InvalidResponseCode(int code)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"code: {code} (expected: 0+)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static HttpResponseStatus FromArgumentException_ParseLine<T>(T line, Exception e)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"malformed status line: {line}", e);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_ReasonPhrase(AsciiString reasonPhrase)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"reasonPhrase contains one of the following prohibited characters: \\r\\n: {reasonPhrase}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_InvalidVersion(string text)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"invalid version format: {text}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_InvalidProtocolName(char c)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"invalid character {c} in protocolName");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_UnterminatedEscapeSeq(int index, string s)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"unterminated escape sequence at index {index} of: {s}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_WebSocket_close_status_code_does_NOT_comply(int statusCode)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException("WebSocket close status code does NOT comply with RFC-6455: " + statusCode);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_Invalid_separator()
        {
            throw GetException();

            static ArgumentException GetException()
            {
                return new ArgumentException("Invalid separator.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_Invalid_separator_only_a_single_space_or_horizontal_tab_allowed(int c)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException("Invalid separator, only a single space or horizontal tab allowed," +
                            " but received a '" + c + "'");
            }
        }

        #endregion

        #region -- IOException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIOException_CheckSize()
        {
            throw GetException();

            static IOException GetException()
            {
                return new IOException("Size exceed allowed maximum capacity");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIOException_CheckSize(HttpDataType dataType)
        {
            throw GetException();
            IOException GetException()
            {
                return new IOException($"{dataType} Size exceed allowed maximum capacity");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIOException_CheckSize(long maxSize)
        {
            throw GetException();
            IOException GetException()
            {
                return new IOException($"Size exceed allowed maximum capacity of {maxSize}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIOException_OutOfSize(long size, long definedSize)
        {
            throw GetException();
            IOException GetException()
            {
                return new IOException($"Out of size: {size} > {definedSize}");
            }
        }

        #endregion

        #region -- InvalidOperationException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_HttpRequestEncoder()
        {
            throw GetInvalidOperationException<HttpRequestEncoder>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Task FromInvalidOperationException_HttpResponseDecoder()
        {
            return TaskUtil.FromException(GetInvalidOperationException<HttpResponseDecoder>());
        }

        internal static InvalidOperationException GetInvalidOperationException<T>()
        {
            return new InvalidOperationException($"ChannelPipeline does not contain an {typeof(T).Name} or HttpClientCodec");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Task FromInvalidOperationException_NoHttpDecoderAndServerCodec()
        {
            return TaskUtil.FromException(GetInvalidOperationException_NoHttpDecoderAndServerCodec());
        }

        internal static InvalidOperationException GetInvalidOperationException_NoHttpDecoderAndServerCodec()
        {
            return new InvalidOperationException("No HttpDecoder and no HttpServerCodec in the pipeline");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static InvalidOperationException GetInvalidOperationException_Attempting()
        {
            return new InvalidOperationException("Attempting to write HTTP request with upgrade in progress");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static object FromInvalidOperationException_Cqrs(object callable, Exception exception)
        {
            throw GetException();
            InvalidOperationException GetException()
            {
                return new InvalidOperationException($"Could not generate value for callable [{callable}]", exception);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_NoMoreElement()
        {
            throw GetException();

            static InvalidOperationException GetException()
            {
                return new InvalidOperationException("No more element to iterate");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_WebSocketClientHandshaker()
        {
            throw GetException();

            static InvalidOperationException GetException()
            {
                return new InvalidOperationException("WebSocketClientHandshaker should have been finished yet");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_UnknownWebSocketVersion()
        {
            throw GetException();

            static InvalidOperationException GetException()
            {
                return new InvalidOperationException("Unknown web socket version");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_OnlyHaveOneValue()
        {
            throw GetException();

            static InvalidOperationException GetException()
            {
                return new InvalidOperationException($"{nameof(CombinedHttpHeaders)} should only have one value");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_NoFileDefined()
        {
            throw GetException();

            static InvalidOperationException GetException()
            {
                return new InvalidOperationException("No file defined so cannot be renamed");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_CannotSendMore()
        {
            throw GetException();

            static InvalidOperationException GetException()
            {
                return new InvalidOperationException("cannot send more responses than requests");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_ReadHttpResponse()
        {
            throw GetException();

            static InvalidOperationException GetException()
            {
                return new InvalidOperationException("Read HTTP response without requesting protocol switch");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_CheckDestroyed<T>()
        {
            throw GetException();

            static InvalidOperationException GetException()
            {
                return new InvalidOperationException($"{StringUtil.SimpleClassName<T>()} was destroyed already");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_UnexpectedMsg(object message)
        {
            throw GetException();
            InvalidOperationException GetException()
            {
                return new InvalidOperationException($"unexpected message type: {StringUtil.SimpleClassName(message)}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_UnexpectedMsg(object message, int state)
        {
            throw GetException();
            InvalidOperationException GetException()
            {
                return new InvalidOperationException($"unexpected message type: {StringUtil.SimpleClassName(message)}, state: {state}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_InvalidType(IHttpMessage oversized)
        {
            throw GetException();
            InvalidOperationException GetException()
            {
                return new InvalidOperationException($"Invalid type {StringUtil.SimpleClassName(oversized)}, expecting {nameof(IHttpRequest)} or {nameof(IHttpResponse)}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_UnexpectedUpgradeProtocol(ICharSequence upgradeHeader)
        {
            throw GetException();
            InvalidOperationException GetException()
            {
                return new InvalidOperationException($"Switching Protocols response with unexpected UPGRADE protocol: {upgradeHeader}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_Cannot_skip_per_message_deflate_decoder()
        {
            throw GetException();
            static InvalidOperationException GetException()
            {
                return new InvalidOperationException("Cannot skip per message deflate decoder, compression in progress");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_Cannot_skip_per_message_deflate_encoder()
        {
            throw GetException();
            static InvalidOperationException GetException()
            {
                return new InvalidOperationException("Cannot skip per message deflate encoder, compression in progress");
            }
        }

        #endregion

        #region -- ChannelException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowChannelException_IO(IOException e)
        {
            throw GetException();
            ChannelException GetException()
            {
                return new ChannelException(e);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static T FromChannelException_IO<T>(IOException e)
        {
            throw GetException();
            ChannelException GetException()
            {
                return new ChannelException(e);
            }
        }

        #endregion

        #region -- EncoderException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowEncoderException_UnexpectedState(int state, object message)
        {
            throw GetException();
            EncoderException GetException()
            {
                return new EncoderException($"unexpected state {state}: {StringUtil.SimpleClassName(message)}");
            }
        }

        #endregion

        #region -- FormatException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static long FromFormatException_HeaderNotFound()
        {
            throw GetException();

            static FormatException GetException()
            {
                return new FormatException($"header not found: {HttpHeaderNames.ContentLength}");
            }
        }

        #endregion

        #region -- TooLongFrameException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowTooLongFrameException_WebSocket00FrameDecoder()
        {
            throw GetException();

            static TooLongFrameException GetException()
            {
                return new TooLongFrameException(nameof(WebSocket00FrameDecoder));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowTooLongFrameException_ResponseTooLarge(IHttpMessage oversized)
        {
            throw GetException();
            TooLongFrameException GetException()
            {
                return new TooLongFrameException($"Response entity too large: {oversized}");
            }
        }

        #endregion

        #region -- NotEnoughDataDecoderException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowNotEnoughDataDecoderException(ExceptionArgument argument)
        {
            throw GetException();
            NotEnoughDataDecoderException GetException()
            {
                return new NotEnoughDataDecoderException(GetArgumentName(argument));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowNotEnoughDataDecoderException(Exception e)
        {
            throw GetException();
            NotEnoughDataDecoderException GetException()
            {
                return new NotEnoughDataDecoderException(e);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowNotEnoughDataDecoderException_AccessOutOfBounds()
        {
            throw GetException();

            static NotEnoughDataDecoderException GetException()
            {
                return new NotEnoughDataDecoderException("Access out of bounds");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static StringCharSequence FromNotEnoughDataDecoderException_ReadLineStandard()
        {
            throw GetException();

            static NotEnoughDataDecoderException GetException()
            {
                return new NotEnoughDataDecoderException("ReadLineStandard");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static StringCharSequence FromNotEnoughDataDecoderException_ReadLine()
        {
            throw GetException();

            static NotEnoughDataDecoderException GetException()
            {
                return new NotEnoughDataDecoderException("ReadLine");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static StringBuilderCharSequence FromNotEnoughDataDecoderException_ReadDelimiterStandard()
        {
            throw GetException();

            static NotEnoughDataDecoderException GetException()
            {
                return new NotEnoughDataDecoderException("ReadDelimiterStandard");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static StringBuilderCharSequence FromNotEnoughDataDecoderException_ReadDelimiter()
        {
            throw GetException();

            static NotEnoughDataDecoderException GetException()
            {
                return new NotEnoughDataDecoderException("ReadDelimiter");
            }
        }

        #endregion

        #region -- ErrorDataDecoderException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowErrorDataDecoderException(Exception e)
        {
            throw GetErrorDataDecoderException();
            ErrorDataDecoderException GetErrorDataDecoderException()
            {
                return new ErrorDataDecoderException(e);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowErrorDataDecoderException_GetStatus()
        {
            throw GetErrorDataDecoderException();

            static ErrorDataDecoderException GetErrorDataDecoderException()
            {
                return new ErrorDataDecoderException("Should not be called with the current getStatus");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowErrorDataDecoderException_Attr()
        {
            throw GetErrorDataDecoderException();

            static ErrorDataDecoderException GetErrorDataDecoderException()
            {
                return new ErrorDataDecoderException($"{HttpHeaderValues.Name} attribute cannot be null.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowErrorDataDecoderException_NameAttr()
        {
            throw GetErrorDataDecoderException();

            static ErrorDataDecoderException GetErrorDataDecoderException()
            {
                return new ErrorDataDecoderException($"{HttpHeaderValues.Name} attribute cannot be null for file upload");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowErrorDataDecoderException_FileNameAttr()
        {
            throw GetErrorDataDecoderException();

            static ErrorDataDecoderException GetErrorDataDecoderException()
            {
                return new ErrorDataDecoderException($"{HttpHeaderValues.FileName} attribute cannot be null for file upload");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowErrorDataDecoderException_ReachHere()
        {
            throw GetErrorDataDecoderException();

            static ErrorDataDecoderException GetErrorDataDecoderException()
            {
                return new ErrorDataDecoderException("Shouldn't reach here.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowErrorDataDecoderException_NoMultipartDelimiterFound()
        {
            throw GetErrorDataDecoderException();

            static ErrorDataDecoderException GetErrorDataDecoderException()
            {
                return new ErrorDataDecoderException("No Multipart delimiter found");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowErrorDataDecoderException_MixedMultipartFound()
        {
            throw GetErrorDataDecoderException();

            static ErrorDataDecoderException GetErrorDataDecoderException()
            {
                return new ErrorDataDecoderException("Mixed Multipart found in a previous Mixed Multipart");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowErrorDataDecoderException_FileName()
        {
            throw GetErrorDataDecoderException();

            static ErrorDataDecoderException GetErrorDataDecoderException()
            {
                return new ErrorDataDecoderException("Filename not found");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowErrorDataDecoderException_NeedBoundaryValue()
        {
            throw GetErrorDataDecoderException();

            static ErrorDataDecoderException GetErrorDataDecoderException()
            {
                return new ErrorDataDecoderException("Needs a boundary value");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowErrorDataDecoderException_BadEndOfLine()
        {
            throw GetErrorDataDecoderException();

            static ErrorDataDecoderException GetErrorDataDecoderException()
            {
                return new ErrorDataDecoderException("Bad end of line");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static string FromErrorDataDecoderException_BadString(string s, Exception e)
        {
            throw GetErrorDataDecoderException();
            ErrorDataDecoderException GetErrorDataDecoderException()
            {
                return new ErrorDataDecoderException($"Bad string: '{s}'", e);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowErrorDataDecoderException_TransferEncoding(string code)
        {
            throw GetErrorDataDecoderException();
            ErrorDataDecoderException GetErrorDataDecoderException()
            {
                return new ErrorDataDecoderException("TransferEncoding Unknown: " + code);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowErrorDataDecoderException_Invalid_hex_byte_at_index(int idx, IByteBuffer b, Encoding charset)
        {
            throw GetErrorDataDecoderException();
            ErrorDataDecoderException GetErrorDataDecoderException()
            {
                return new ErrorDataDecoderException(
                    string.Format("Invalid hex byte at index '{0}' in string: '{1}'", idx, b.ToString(charset)));
            }
        }

        #endregion

        #region -- ErrorDataEncoderException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowErrorDataEncoderException(Exception e)
        {
            throw GetErrorDataEncoderException();
            ErrorDataEncoderException GetErrorDataEncoderException()
            {
                return new ErrorDataEncoderException(e);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowErrorDataEncoderException_HeaderAlreadyEncoded()
        {
            throw GetErrorDataEncoderException();

            static ErrorDataEncoderException GetErrorDataEncoderException()
            {
                return new ErrorDataEncoderException("Header already encoded");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowErrorDataEncoderException_CannotAddValue()
        {
            throw GetErrorDataEncoderException();

            static ErrorDataEncoderException GetErrorDataEncoderException()
            {
                return new ErrorDataEncoderException("Cannot add value once finalized");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowErrorDataEncoderException_CannotCreate()
        {
            throw GetErrorDataEncoderException();

            static ErrorDataEncoderException GetErrorDataEncoderException()
            {
                return new ErrorDataEncoderException("Cannot create a Encoder if request is a TRACE");
            }
        }

        #endregion

        #region -- CodecException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static IFullHttpMessage FromCodecException_InvalidType(IHttpMessage start)
        {
            throw GetException();
            CodecException GetException()
            {
                return new CodecException($"Invalid type {StringUtil.SimpleClassName(start)} expecting {nameof(IHttpRequest)} or {nameof(IHttpResponse)}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowCodecException_EnsureContent(IHttpObject msg)
        {
            throw GetException();
            CodecException GetException()
            {
                return new CodecException($"unexpected message type: {msg.GetType().Name} (expected: {StringUtil.SimpleClassName<IHttpContent>()})");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowCodecException_EnsureHeaders(IHttpObject msg)
        {
            throw GetException();
            CodecException GetException()
            {
                return new CodecException($"unexpected message type: {msg.GetType().Name} (expected: {StringUtil.SimpleClassName<IHttpResponse>()})");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowCodecException_InvalidHttpMsg(IHttpMessage httpMessage)
        {
            throw GetException();
            CodecException GetException()
            {
                return new CodecException($"Object of class {StringUtil.SimpleClassName(httpMessage.GetType())} is not an HttpRequest or HttpResponse");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowCodecException_InvalidCompression(ZlibWrapper mode)
        {
            throw GetException();
            CodecException GetException()
            {
                return new CodecException($"{mode} not supported, only Gzip and Zlib are allowed.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowCodecException_InvalidWSExHandshake(string extensionsHeader)
        {
            throw GetException();
            CodecException GetException()
            {
                return new CodecException($"invalid WebSocket Extension handshake for \"{extensionsHeader}\"");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowCodecException_UnexpectedFrameType(WebSocketFrame msg)
        {
            throw GetException();
            CodecException GetException()
            {
                return new CodecException($"unexpected frame type: {msg.GetType().Name}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowCodecException_CannotReadCompressedBuf()
        {
            throw GetException();

            static CodecException GetException()
            {
                return new CodecException("cannot read compressed buffer");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowCodecException_CannotReadUncompressedBuf()
        {
            throw GetException();

            static CodecException GetException()
            {
                return new CodecException("cannot read uncompressed buffer");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowCodecException_CannotCompressContentBuffer()
        {
            throw GetException();

            static CodecException GetException()
            {
                return new CodecException("cannot compress content buffer");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowCodecException_UnexpectedInitialFrameType(WebSocketFrame msg)
        {
            throw GetException();
            CodecException GetException()
            {
                return new CodecException($"unexpected initial frame type: {msg.GetType().Name}");
            }
        }

        #endregion

        #region -- WebSocketHandshakeException --

        internal static WebSocketHandshakeException GetWebSocketHandshakeException_SendCloseFrameTimedOut()
        {
            return new WebSocketHandshakeException("send close frame timed out");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static WebSocketClientHandshaker FromWebSocketHandshakeException_InvalidVersion(WebSocketVersion version)
        {
            throw GetException();
            WebSocketHandshakeException GetException()
            {
                return new WebSocketHandshakeException($"Protocol version {version}not supported.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowWebSocketHandshakeException_InvalidSubprotocol(string receivedProtocol, string expectedSubprotocol)
        {
            throw GetException();
            WebSocketHandshakeException GetException()
            {
                return new WebSocketHandshakeException($"Invalid subprotocol. Actual: {receivedProtocol}. Expected one of: {expectedSubprotocol}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowWebSocketHandshakeException_InvalidHandshakeResponseGS(IFullHttpResponse response)
        {
            throw GetException();
            WebSocketHandshakeException GetException()
            {
                return new WebSocketHandshakeException($"Invalid handshake response getStatus: {response.Status}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowWebSocketHandshakeException_InvalidHandshakeResponseU(ICharSequence upgrade)
        {
            throw GetException();
            WebSocketHandshakeException GetException()
            {
                return new WebSocketHandshakeException($"Invalid handshake response upgrade: {upgrade}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowWebSocketHandshakeException_InvalidHandshakeResponseConn(ICharSequence upgrade)
        {
            throw GetException();
            WebSocketHandshakeException GetException()
            {
                return new WebSocketHandshakeException($"Invalid handshake response connection: {upgrade}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowWebSocketHandshakeException_InvalidChallenge()
        {
            throw GetException();

            static WebSocketHandshakeException GetException()
            {
                return new WebSocketHandshakeException("Invalid challenge");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowWebSocketHandshakeException_InvalidChallenge(ICharSequence accept, AsciiString expectedChallengeResponseString)
        {
            throw GetException();
            WebSocketHandshakeException GetException()
            {
                return new WebSocketHandshakeException($"Invalid challenge. Actual: {accept}. Expected: {expectedChallengeResponseString}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowWebSocketHandshakeException_MissingUpgrade()
        {
            throw GetException();

            static WebSocketHandshakeException GetException()
            {
                return new WebSocketHandshakeException("not a WebSocket handshake request: missing upgrade");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowWebSocketHandshakeException_MissingKey()
        {
            throw GetException();

            static WebSocketHandshakeException GetException()
            {
                return new WebSocketHandshakeException("not a WebSocket request: missing key");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowWebSocketHandshakeException_Missing_origin_header(IFullHttpRequest req)
        {
            throw GetException();

            WebSocketHandshakeException GetException()
            {
                return new WebSocketHandshakeException("Missing origin header, got only [" + string.Join(", ", req.Headers.Names()) + "]");
            }
        }

        #endregion

        #region -- EndOfDataDecoderException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowEndOfDataDecoderException_HttpPostStandardRequestDecoder()
        {
            throw GetException();

            static EndOfDataDecoderException GetException()
            {
                return new EndOfDataDecoderException(nameof(HttpPostStandardRequestDecoder));
            }
        }

        #endregion

        #region -- MessageAggregationException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowMessageAggregationException_StartMessage()
        {
            throw GetException();

            static MessageAggregationException GetException()
            {
                return new MessageAggregationException("Start message should not have any current content.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowMessageAggregationException_UnknownAggregationState()
        {
            throw GetException();

            static MessageAggregationException GetException()
            {
                return new MessageAggregationException("Unknown aggregation state.");
            }
        }

        #endregion

        #region -- ClosedChannelException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static ClosedChannelException GetClosedChannelException()
        {
            return new ClosedChannelException();
        }

        #endregion
    }
}
