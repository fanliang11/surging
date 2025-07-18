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
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs.Http.Multipart
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Common;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;

    /// <summary>
    /// This decoder will decode Body and can handle POST BODY.
    /// You <c>MUST</c> call <see cref="Destroy"/> after completion to release all resources.
    /// </summary>
    public class HttpPostMultipartRequestDecoder : IInterfaceHttpPostRequestDecoder
    {
        // Factory used to create InterfaceHttpData
        readonly IHttpDataFactory _factory;

        // Request to decode
        readonly IHttpRequest _request;

        // Default charset to use
        Encoding _charset;

        // Does the last chunk already received
        bool _isLastChunk;

        // HttpDatas from Body
        readonly List<IInterfaceHttpData> _bodyListHttpData;

        // HttpDatas as Map from Body
        readonly Dictionary<string, List<IInterfaceHttpData>> _bodyMapHttpData;

        // The current channelBuffer
        IByteBuffer _undecodedChunk;

        // Body HttpDatas current position
        int _bodyListHttpDataRank;

        // If multipart, this is the boundary for the global multipart
        ICharSequence _multipartDataBoundary;

        // If multipart, there could be internal multiparts (mixed) to the global
        // multipart. Only one level is allowed.
        ICharSequence _multipartMixedBoundary;

        // Current getStatus
        MultiPartStatus _currentStatus;

        // Used in Multipart
        Dictionary<AsciiString, IAttribute> _currentFieldAttributes;

        // The current FileUpload that is currently in decode process
        IFileUpload _currentFileUpload;

        // The current Attribute that is currently in decode process
        IAttribute _currentAttribute;

        bool _destroyed;

        int _discardThreshold;

        public HttpPostMultipartRequestDecoder(IHttpRequest request)
            : this(new DefaultHttpDataFactory(DefaultHttpDataFactory.MinSize), request, HttpConstants.DefaultEncoding)
        {
        }

        public HttpPostMultipartRequestDecoder(IHttpDataFactory factory, IHttpRequest request)
            : this(factory, request, HttpConstants.DefaultEncoding)
        {
        }

        public HttpPostMultipartRequestDecoder(IHttpDataFactory factory, IHttpRequest request, Encoding charset)
        {
            if (request is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.request); }
            if (charset is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.charset); }
            if (factory is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.factory); }

            _currentStatus = MultiPartStatus.Notstarted;
            _discardThreshold = HttpPostRequestDecoder.DefaultDiscardThreshold;
            _bodyListHttpData = new List<IInterfaceHttpData>();
            _bodyMapHttpData = new Dictionary<string, List<IInterfaceHttpData>>(StringComparer.OrdinalIgnoreCase);

            _factory = factory;
            _request = request;
            _charset = charset;

            // Fill default values
            SetMultipart(_request.Headers.Get(HttpHeaderNames.ContentType, null));
            if (request is IHttpContent content)
            {
                // Offer automatically if the given request is als type of HttpContent
                // See #1089
                _ = Offer(content);
            }
            else
            {
                ParseBody();
            }
        }

        void SetMultipart(ICharSequence contentType)
        {
            ICharSequence[] dataBoundary = HttpPostRequestDecoder.GetMultipartDataBoundary(contentType);
            if (dataBoundary is object)
            {
                _multipartDataBoundary = new AsciiString(dataBoundary[0]);
                if ((uint)dataBoundary.Length > 1u && dataBoundary[1] is object)
                {
                    _charset = Encoding.GetEncoding(dataBoundary[1].ToString());
                }
            }
            else
            {
                _multipartDataBoundary = null;
            }
            _currentStatus = MultiPartStatus.HeaderDelimiter;
        }

        void CheckDestroyed()
        {
            if (_destroyed)
            {
                ThrowHelper.ThrowInvalidOperationException_CheckDestroyed<HttpPostMultipartRequestDecoder>();
            }
        }

        public bool IsMultipart
        {
            get
            {
                CheckDestroyed();
                return true;
            }
        }

        public int DiscardThreshold
        {
            get => _discardThreshold;
            set
            {
                if ((uint)value > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_PositiveOrZero(value, ExceptionArgument.value); }
                _discardThreshold = value;
            }
        }

        public List<IInterfaceHttpData> GetBodyHttpDatas()
        {
            CheckDestroyed();

            if (!_isLastChunk)
            {
                ThrowHelper.ThrowNotEnoughDataDecoderException(ExceptionArgument.HttpPostMultipartRequestDecoder);
            }
            return _bodyListHttpData;
        }

        public List<IInterfaceHttpData> GetBodyHttpDatas(string name)
        {
            CheckDestroyed();

            if (!_isLastChunk)
            {
                ThrowHelper.ThrowNotEnoughDataDecoderException(ExceptionArgument.HttpPostMultipartRequestDecoder);
            }
            return _bodyMapHttpData[name];
        }

        public IInterfaceHttpData GetBodyHttpData(string name)
        {
            CheckDestroyed();

            if (!_isLastChunk)
            {
                ThrowHelper.ThrowNotEnoughDataDecoderException(ExceptionArgument.HttpPostMultipartRequestDecoder);
            }
            if (_bodyMapHttpData.TryGetValue(name, out List<IInterfaceHttpData> list))
            {
                return list[0];
            }
            return null;
        }

        public IInterfaceHttpPostRequestDecoder Offer(IHttpContent content)
        {
            CheckDestroyed();

            if (content is ILastHttpContent)
            {
                _isLastChunk = true;
            }

            IByteBuffer buf = content.Content;
            if (_undecodedChunk is null)
            {
                _undecodedChunk = _isLastChunk ?
                        // Take a slice instead of copying when the first chunk is also the last
                        // as undecodedChunk.writeBytes will never be called.
                        buf.RetainedSlice() :
                        // Maybe we should better not copy here for performance reasons but this will need
                        // more care by the caller to release the content in a correct manner later
                        // So maybe something to optimize on a later stage
                        //
                        // We are explicit allocate a buffer and NOT calling copy() as otherwise it may set a maxCapacity
                        // which is not really usable for us as we may exceed it once we add more bytes.
                        buf.Allocator.Buffer(buf.ReadableBytes).WriteBytes(buf);
            }
            else
            {
                _ = _undecodedChunk.WriteBytes(buf);
            }
            ParseBody();
            if (_undecodedChunk is object
                && _undecodedChunk.WriterIndex > _discardThreshold)
            {
                _ = _undecodedChunk.DiscardReadBytes();
            }
            return this;
        }

        public bool HasNext
        {
            get
            {
                CheckDestroyed();

                if (_currentStatus == MultiPartStatus.Epilogue)
                {
                    // OK except if end of list
                    if (_bodyListHttpDataRank >= _bodyListHttpData.Count)
                    {
                        ThrowHelper.ThrowNotEnoughDataDecoderException(ExceptionArgument.HttpPostMultipartRequestDecoder);
                    }
                }
                return (uint)_bodyListHttpData.Count > 0u && _bodyListHttpDataRank < _bodyListHttpData.Count;
            }
        }

        public IInterfaceHttpData Next()
        {
            CheckDestroyed();

            return HasNext
                ? _bodyListHttpData[_bodyListHttpDataRank++]
                : null;
        }

        public IInterfaceHttpData CurrentPartialHttpData
        {
            get
            {
                if (_currentFileUpload is object)
                {
                    return _currentFileUpload;
                }
                else
                {
                    return _currentAttribute;
                }
            }
        }

        void ParseBody()
        {
            if (_currentStatus == MultiPartStatus.PreEpilogue
                || _currentStatus == MultiPartStatus.Epilogue)
            {
                if (_isLastChunk)
                {
                    _currentStatus = MultiPartStatus.Epilogue;
                }
                return;
            }

            ParseBodyMultipart();
        }

        protected void AddHttpData(IInterfaceHttpData data)
        {
            if (data is null)
            {
                return;
            }
            var name = data.Name;
            if (!_bodyMapHttpData.TryGetValue(name, out List<IInterfaceHttpData> datas))
            {
                datas = new List<IInterfaceHttpData>(1);
                _bodyMapHttpData.Add(name, datas);
            }
            datas.Add(data);
            _bodyListHttpData.Add(data);
        }

        void ParseBodyMultipart()
        {
            if (_undecodedChunk is null
                || 0u >= (uint)_undecodedChunk.ReadableBytes)
            {
                // nothing to decode
                return;
            }

            IInterfaceHttpData data = DecodeMultipart(_currentStatus);
            while (data is object)
            {
                AddHttpData(data);
                if (_currentStatus == MultiPartStatus.PreEpilogue
                    || _currentStatus == MultiPartStatus.Epilogue)
                {
                    break;
                }

                data = DecodeMultipart(_currentStatus);
            }
        }

        IInterfaceHttpData DecodeMultipart(MultiPartStatus state)
        {
            switch (state)
            {
                case MultiPartStatus.Notstarted:
                    ThrowHelper.ThrowErrorDataDecoderException_GetStatus(); return null;
                case MultiPartStatus.Preamble:
                    // Content-type: multipart/form-data, boundary=AaB03x
                    ThrowHelper.ThrowErrorDataDecoderException_GetStatus(); return null;
                case MultiPartStatus.HeaderDelimiter:
                    {
                        // --AaB03x or --AaB03x--
                        return FindMultipartDelimiter(_multipartDataBoundary, MultiPartStatus.Disposition,
                            MultiPartStatus.PreEpilogue);
                    }
                case MultiPartStatus.Disposition:
                    {
                        // content-disposition: form-data; name="field1"
                        // content-disposition: form-data; name="pics"; filename="file1.txt"
                        // and other immediate values like
                        // Content-type: image/gif
                        // Content-Type: text/plain
                        // Content-Type: text/plain; charset=ISO-8859-1
                        // Content-Transfer-Encoding: binary
                        // The following line implies a change of mode (mixed mode)
                        // Content-type: multipart/mixed, boundary=BbC04y
                        return FindMultipartDisposition();
                    }
                case MultiPartStatus.Field:
                    {
                        // Now get value according to Content-Type and Charset
                        Encoding localCharset = null;
                        if (_currentFieldAttributes.TryGetValue(HttpHeaderValues.Charset, out IAttribute charsetAttribute))
                        {
                            try
                            {
                                localCharset = Encoding.GetEncoding(charsetAttribute.Value);
                            }
                            catch (IOException e)
                            {
                                ThrowHelper.ThrowErrorDataDecoderException(e);
                            }
                            catch (ArgumentException e)
                            {
                                ThrowHelper.ThrowErrorDataDecoderException(e);
                            }
                        }
                        _ = _currentFieldAttributes.TryGetValue(HttpHeaderValues.Name, out IAttribute nameAttribute);
                        if (_currentAttribute is null)
                        {
                            _ = _currentFieldAttributes.TryGetValue(HttpHeaderNames.ContentLength, out IAttribute lengthAttribute);
                            long size;
                            try
                            {
                                size = lengthAttribute is object ? long.Parse(lengthAttribute.Value) : 0L;
                            }
                            catch (IOException e)
                            {
                                ThrowHelper.ThrowErrorDataDecoderException(e); size = 0L;
                            }
                            catch (FormatException)
                            {
                                size = 0L;
                            }
                            try
                            {
                                if (nameAttribute is null)
                                {
                                    ThrowHelper.ThrowErrorDataDecoderException_Attr();
                                }
                                if (size > 0)
                                {
                                    _currentAttribute = _factory.CreateAttribute(_request,
                                        CleanString0(nameAttribute.Value), size);
                                }
                                else
                                {
                                    _currentAttribute = _factory.CreateAttribute(_request,
                                        CleanString0(nameAttribute.Value));
                                }
                            }
                            catch (ArgumentException e)
                            {
                                ThrowHelper.ThrowErrorDataDecoderException(e);
                            }
                            catch (IOException e)
                            {
                                ThrowHelper.ThrowErrorDataDecoderException(e);
                            }
                            if (localCharset is object)
                            {
                                _currentAttribute.Charset = localCharset;
                            }
                        }
                        // load data
                        if (!LoadDataMultipart(_undecodedChunk, _multipartDataBoundary, _currentAttribute))
                        {
                            // Delimiter is not found. Need more chunks.
                            return null;
                        }
                        IAttribute finalAttribute = _currentAttribute;
                        _currentAttribute = null;
                        _currentFieldAttributes = null;
                        // ready to load the next one
                        _currentStatus = MultiPartStatus.HeaderDelimiter;
                        return finalAttribute;
                    }
                case MultiPartStatus.Fileupload:
                    {
                        // eventually restart from existing FileUpload
                        return GetFileUpload(_multipartDataBoundary);
                    }
                case MultiPartStatus.MixedDelimiter:
                    {
                        // --AaB03x or --AaB03x--
                        // Note that currentFieldAttributes exists
                        return FindMultipartDelimiter(_multipartMixedBoundary, MultiPartStatus.MixedDisposition,
                            MultiPartStatus.HeaderDelimiter);
                    }
                case MultiPartStatus.MixedDisposition:
                    {
                        return FindMultipartDisposition();
                    }
                case MultiPartStatus.MixedFileUpload:
                    {
                        // eventually restart from existing FileUpload
                        return GetFileUpload(_multipartMixedBoundary);
                    }
                case MultiPartStatus.PreEpilogue:
                case MultiPartStatus.Epilogue:
                    return null;
                default:
                    ThrowHelper.ThrowErrorDataDecoderException_ReachHere(); return null;
            }
        }

        static void SkipControlCharacters(IByteBuffer undecodedChunk)
        {
            if (!undecodedChunk.HasArray)
            {
                try
                {
                    SkipControlCharactersStandard(undecodedChunk);
                }
                catch (IndexOutOfRangeException e)
                {
                    ThrowHelper.ThrowNotEnoughDataDecoderException(e);
                }
                return;
            }
            var sao = new HttpPostBodyUtil.SeekAheadOptimize(undecodedChunk);
            while (sao.Pos < sao.Limit)
            {
                char c = (char)sao.Bytes[sao.Pos++];
                if (!CharUtil.IsISOControl(c) && !char.IsWhiteSpace(c))
                {
                    sao.SetReadPosition(1);
                    return;
                }
            }
            ThrowHelper.ThrowNotEnoughDataDecoderException_AccessOutOfBounds();
        }

        static void SkipControlCharactersStandard(IByteBuffer undecodedChunk)
        {
            while (true)
            {
                char c = (char)undecodedChunk.ReadByte();
                if (!CharUtil.IsISOControl(c) && !char.IsWhiteSpace(c))
                {
                    _ = undecodedChunk.SetReaderIndex(undecodedChunk.ReaderIndex - 1);
                    break;
                }
            }
        }

        IInterfaceHttpData FindMultipartDelimiter(ICharSequence delimiter, MultiPartStatus dispositionStatus,
            MultiPartStatus closeDelimiterStatus)
        {
            // --AaB03x or --AaB03x--
            int readerIndex = _undecodedChunk.ReaderIndex;
            try
            {
                SkipControlCharacters(_undecodedChunk);
            }
            catch (NotEnoughDataDecoderException)
            {
                _ = _undecodedChunk.SetReaderIndex(readerIndex);
                return null;
            }
            _ = SkipOneLine();
            StringBuilderCharSequence newline;
            try
            {
                newline = ReadDelimiter(_undecodedChunk, delimiter);
            }
            catch (NotEnoughDataDecoderException)
            {
                _ = _undecodedChunk.SetReaderIndex(readerIndex);
                return null;
            }
            if (newline.Equals(delimiter))
            {
                _currentStatus = dispositionStatus;
                return DecodeMultipart(dispositionStatus);
            }
            if (AsciiString.ContentEquals(newline, new StringCharSequence(delimiter.ToString() + "--")))
            {
                // CloseDelimiter or MIXED CloseDelimiter found
                _currentStatus = closeDelimiterStatus;
                if (_currentStatus == MultiPartStatus.HeaderDelimiter)
                {
                    // MixedCloseDelimiter
                    // end of the Mixed part
                    _currentFieldAttributes = null;
                    return DecodeMultipart(MultiPartStatus.HeaderDelimiter);
                }
                return null;
            }
            _ = _undecodedChunk.SetReaderIndex(readerIndex);
            ThrowHelper.ThrowErrorDataDecoderException_NoMultipartDelimiterFound(); return null;
        }

        IInterfaceHttpData FindMultipartDisposition()
        {
            int readerIndex = _undecodedChunk.ReaderIndex;
            if (_currentStatus == MultiPartStatus.Disposition)
            {
                _currentFieldAttributes = new Dictionary<AsciiString, IAttribute>(AsciiStringComparer.IgnoreCase);
            }
            // read many lines until empty line with newline found! Store all data
            while (!SkipOneLine())
            {
                StringCharSequence newline;
                try
                {
                    SkipControlCharacters(_undecodedChunk);
                    newline = ReadLine(_undecodedChunk, _charset);
                }
                catch (NotEnoughDataDecoderException)
                {
                    _ = _undecodedChunk.SetReaderIndex(readerIndex);
                    return null;
                }
                ICharSequence[] contents = SplitMultipartHeader(newline);
                if (HttpHeaderNames.ContentDisposition.ContentEqualsIgnoreCase(contents[0]))
                {
                    bool checkSecondArg;
                    if (_currentStatus == MultiPartStatus.Disposition)
                    {
                        checkSecondArg = HttpHeaderValues.FormData.ContentEqualsIgnoreCase(contents[1]);
                    }
                    else
                    {
                        checkSecondArg = HttpHeaderValues.Attachment.ContentEqualsIgnoreCase(contents[1])
                            || HttpHeaderValues.File.ContentEqualsIgnoreCase(contents[1]);
                    }
                    if (checkSecondArg)
                    {
                        // read next values and store them in the map as Attribute
                        for (int i = 2; i < contents.Length; i++)
                        {
                            ICharSequence[] values = CharUtil.Split(contents[i], '=');
                            IAttribute attribute = null;
                            try
                            {
                                attribute = GetContentDispositionAttribute(values);
                            }
                            catch (ArgumentNullException e)
                            {
                                ThrowHelper.ThrowErrorDataDecoderException(e);
                            }
                            catch (ArgumentException e)
                            {
                                ThrowHelper.ThrowErrorDataDecoderException(e);
                            }
                            _currentFieldAttributes.Add(new AsciiString(attribute.Name), attribute);
                        }
                    }
                }
                else if (HttpHeaderNames.ContentTransferEncoding.ContentEqualsIgnoreCase(contents[0]))
                {
                    IAttribute attribute = null;
                    try
                    {
                        attribute = _factory.CreateAttribute(_request, HttpHeaderNames.ContentTransferEncoding.ToString(),
                            CleanString0(contents[1]));
                    }
                    catch (ArgumentNullException e)
                    {
                        ThrowHelper.ThrowErrorDataDecoderException(e);
                    }
                    catch (ArgumentException e)
                    {
                        ThrowHelper.ThrowErrorDataDecoderException(e);
                    }

                    _currentFieldAttributes.Add(HttpHeaderNames.ContentTransferEncoding, attribute);
                }
                else if (HttpHeaderNames.ContentLength.ContentEqualsIgnoreCase(contents[0]))
                {
                    IAttribute attribute = null;
                    try
                    {
                        attribute = _factory.CreateAttribute(_request, HttpHeaderNames.ContentLength.ToString(),
                            CleanString0(contents[1]));
                    }
                    catch (ArgumentNullException e)
                    {
                        ThrowHelper.ThrowErrorDataDecoderException(e);
                    }
                    catch (ArgumentException e)
                    {
                        ThrowHelper.ThrowErrorDataDecoderException(e);
                    }

                    _currentFieldAttributes.Add(HttpHeaderNames.ContentLength, attribute);
                }
                else if (HttpHeaderNames.ContentType.ContentEqualsIgnoreCase(contents[0]))
                {
                    // Take care of possible "multipart/mixed"
                    if (HttpHeaderValues.MultipartMixed.ContentEqualsIgnoreCase(contents[1]))
                    {
                        if (_currentStatus == MultiPartStatus.Disposition)
                        {
                            ICharSequence values = contents[2].SubstringAfter('=');
                            _multipartMixedBoundary = new StringCharSequence("--" + values.ToString());
                            _currentStatus = MultiPartStatus.MixedDelimiter;
                            return DecodeMultipart(MultiPartStatus.MixedDelimiter);
                        }
                        else
                        {
                            ThrowHelper.ThrowErrorDataDecoderException_MixedMultipartFound();
                        }
                    }
                    else
                    {
                        for (int i = 1; i < contents.Length; i++)
                        {
                            ICharSequence charsetHeader = HttpHeaderValues.Charset;
                            if (contents[i].RegionMatchesIgnoreCase(0, charsetHeader, 0, charsetHeader.Count))
                            {
                                ICharSequence values = contents[i].SubstringAfter('=');
                                IAttribute attribute = null;
                                try
                                {
                                    attribute = _factory.CreateAttribute(_request, charsetHeader.ToString(), CleanString0(values));
                                }
                                catch (ArgumentNullException e)
                                {
                                    ThrowHelper.ThrowErrorDataDecoderException(e);
                                }
                                catch (ArgumentException e)
                                {
                                    ThrowHelper.ThrowErrorDataDecoderException(e);
                                }
                                _currentFieldAttributes.Add(HttpHeaderValues.Charset, attribute);
                            }
                            else
                            {
                                IAttribute attribute = null;
                                string name = null;
                                try
                                {
                                    name = CleanString0(contents[0]);
                                    attribute = _factory.CreateAttribute(_request,
                                        name.ToString(), contents[i].ToString());
                                }
                                catch (ArgumentNullException e)
                                {
                                    ThrowHelper.ThrowErrorDataDecoderException(e);
                                }
                                catch (ArgumentException e)
                                {
                                    ThrowHelper.ThrowErrorDataDecoderException(e);
                                }
                                _currentFieldAttributes.Add(new AsciiString(name), attribute);
                            }
                        }
                    }
                }
            }
            // Is it a FileUpload
            _ = _currentFieldAttributes.TryGetValue(HttpHeaderValues.FileName, out IAttribute filenameAttribute);
            if (_currentStatus == MultiPartStatus.Disposition)
            {
                if (filenameAttribute is object)
                {
                    // FileUpload
                    _currentStatus = MultiPartStatus.Fileupload;
                    // do not change the buffer position
                    return DecodeMultipart(MultiPartStatus.Fileupload);
                }
                else
                {
                    // Field
                    _currentStatus = MultiPartStatus.Field;
                    // do not change the buffer position
                    return DecodeMultipart(MultiPartStatus.Field);
                }
            }
            else
            {
                if (filenameAttribute is object)
                {
                    // FileUpload
                    _currentStatus = MultiPartStatus.MixedFileUpload;
                    // do not change the buffer position
                    return DecodeMultipart(MultiPartStatus.MixedFileUpload);
                }
                else
                {
                    // Field is not supported in MIXED mode
                    ThrowHelper.ThrowErrorDataDecoderException_FileName(); return null;
                }
            }
        }

        static readonly AsciiString FilenameEncoded = AsciiString.Cached(HttpHeaderValues.FileName.ToString() + '*');

        IAttribute GetContentDispositionAttribute(params ICharSequence[] values)
        {
            ICharSequence name = CleanString(values[0]);
            ICharSequence value = values[1];

            // Filename can be token, quoted or encoded. See https://tools.ietf.org/html/rfc5987
            if (HttpHeaderValues.FileName.ContentEquals(name))
            {
                // Value is quoted or token. Strip if quoted:
                int last = value.Count - 1;
                if (last > 0
                    && value[0] == HttpConstants.DoubleQuote
                    && value[last] == HttpConstants.DoubleQuote)
                {
                    value = value.SubSequence(1, last);
                }
            }
            else if (FilenameEncoded.ContentEquals(name))
            {
                try
                {
                    name = HttpHeaderValues.FileName;
                    string[] split = CleanString0(value).Split(new[] { '\'' }, 3);
                    value = new StringCharSequence(
                        QueryStringDecoder.DecodeComponent(split[2], Encoding.GetEncoding(split[0])));
                }
                catch (IndexOutOfRangeException e)
                {
                    ThrowHelper.ThrowErrorDataDecoderException(e);
                }
                catch (ArgumentException e) // Invalid encoding
                {
                    ThrowHelper.ThrowErrorDataDecoderException(e);
                }
            }
            else
            {
                // otherwise we need to clean the value
                value = CleanString(value);
            }
            return _factory.CreateAttribute(_request, name.ToString(), value.ToString());
        }

        protected IInterfaceHttpData GetFileUpload(ICharSequence delimiter)
        {
            // eventually restart from existing FileUpload
            // Now get value according to Content-Type and Charset
            _currentFieldAttributes.TryGetValue(HttpHeaderNames.ContentTransferEncoding, out IAttribute encodingAttribute);
            Encoding localCharset = _charset;
            // Default
            HttpPostBodyUtil.TransferEncodingMechanism mechanism = HttpPostBodyUtil.TransferEncodingMechanism.Bit7;
            if (encodingAttribute is object)
            {
                string code = null;
                try
                {
                    code = encodingAttribute.Value.ToLowerInvariant();
                }
                catch (IOException e)
                {
                    ThrowHelper.ThrowErrorDataDecoderException(e);
                }
                if (string.Equals(code, HttpPostBodyUtil.TransferEncodingMechanism.Bit7.Value
#if NETCOREAPP_3_0_GREATER || NETSTANDARD_2_0_GREATER
                    ))
#else
                    , StringComparison.Ordinal))
#endif
                {
                    localCharset = Encoding.ASCII;
                }
                else if (string.Equals(code, HttpPostBodyUtil.TransferEncodingMechanism.Bit8.Value
#if NETCOREAPP_3_0_GREATER || NETSTANDARD_2_0_GREATER
                    ))
#else
                    , StringComparison.Ordinal))
#endif
                {
                    localCharset = Encoding.UTF8;
                    mechanism = HttpPostBodyUtil.TransferEncodingMechanism.Bit8;
                }
                else if (string.Equals(code, HttpPostBodyUtil.TransferEncodingMechanism.Binary.Value
#if NETCOREAPP_3_0_GREATER || NETSTANDARD_2_0_GREATER
                    ))
#else
                    , StringComparison.Ordinal))
#endif
                {
                    // no real charset, so let the default
                    mechanism = HttpPostBodyUtil.TransferEncodingMechanism.Binary;
                }
                else
                {
                    ThrowHelper.ThrowErrorDataDecoderException_TransferEncoding(code);
                }
            }
            if (_currentFieldAttributes.TryGetValue(HttpHeaderValues.Charset, out IAttribute charsetAttribute))
            {
                try
                {
                    localCharset = Encoding.GetEncoding(charsetAttribute.Value);
                }
                catch (IOException e)
                {
                    ThrowHelper.ThrowErrorDataDecoderException(e);
                }
                catch (ArgumentException e)
                {
                    ThrowHelper.ThrowErrorDataDecoderException(e);
                }
            }
            if (_currentFileUpload is null)
            {
                _currentFieldAttributes.TryGetValue(HttpHeaderValues.FileName, out IAttribute filenameAttribute);
                _currentFieldAttributes.TryGetValue(HttpHeaderValues.Name, out IAttribute nameAttribute);
                _currentFieldAttributes.TryGetValue(HttpHeaderNames.ContentType, out IAttribute contentTypeAttribute);
                _currentFieldAttributes.TryGetValue(HttpHeaderNames.ContentLength, out IAttribute lengthAttribute);
                long size;
                try
                {
                    size = lengthAttribute is object ? long.Parse(lengthAttribute.Value) : 0L;
                }
                catch (IOException e)
                {
                    ThrowHelper.ThrowErrorDataDecoderException(e); size = 0L;
                }
                catch (FormatException)
                {
                    size = 0L;
                }
                try
                {
                    string contentType;
                    if (contentTypeAttribute is object)
                    {
                        contentType = contentTypeAttribute.Value;
                    }
                    else
                    {
                        contentType = HttpPostBodyUtil.DefaultBinaryContentType;
                    }
                    if (nameAttribute is null)
                    {
                        ThrowHelper.ThrowErrorDataDecoderException_NameAttr();
                    }
                    if (filenameAttribute is null)
                    {
                        ThrowHelper.ThrowErrorDataDecoderException_FileNameAttr();
                    }
                    _currentFileUpload = _factory.CreateFileUpload(_request,
                        CleanString0(nameAttribute.Value), CleanString0(filenameAttribute.Value),
                        contentType, mechanism.Value, localCharset,
                        size);
                }
                catch (ArgumentNullException e)
                {
                    ThrowHelper.ThrowErrorDataDecoderException(e);
                }
                catch (ArgumentException e)
                {
                    ThrowHelper.ThrowErrorDataDecoderException(e);
                }
                catch (IOException e)
                {
                    ThrowHelper.ThrowErrorDataDecoderException(e);
                }
            }
            // load data as much as possible
            if (!LoadDataMultipart(_undecodedChunk, delimiter, _currentFileUpload))
            {
                // Delimiter is not found. Need more chunks.
                return null;
            }
            if (_currentFileUpload.IsCompleted)
            {
                // ready to load the next one
                if (_currentStatus == MultiPartStatus.Fileupload)
                {
                    _currentStatus = MultiPartStatus.HeaderDelimiter;
                    _currentFieldAttributes = null;
                }
                else
                {
                    _currentStatus = MultiPartStatus.MixedDelimiter;
                    CleanMixedAttributes();
                }
                IFileUpload fileUpload = _currentFileUpload;
                _currentFileUpload = null;
                return fileUpload;
            }

            // do not change the buffer position
            // since some can be already saved into FileUpload
            // So do not change the currentStatus
            return null;
        }

        public void Destroy()
        {
            // Release all data items, including those not yet pulled
            CleanFiles();
            _destroyed = true;

            if (_undecodedChunk is object && _undecodedChunk.ReferenceCount > 0)
            {
                _ = _undecodedChunk.Release();
                _undecodedChunk = null;
            }
        }

        public void CleanFiles()
        {
            CheckDestroyed();
            _factory.CleanRequestHttpData(_request);
        }

        public void RemoveHttpDataFromClean(IInterfaceHttpData data)
        {
            CheckDestroyed();

            _factory.RemoveHttpDataFromClean(_request, data);
        }


        // Remove all Attributes that should be cleaned between two FileUpload in
        // Mixed mode
        void CleanMixedAttributes()
        {
            _ = _currentFieldAttributes.Remove(HttpHeaderValues.Charset);
            _ = _currentFieldAttributes.Remove(HttpHeaderNames.ContentLength);
            _ = _currentFieldAttributes.Remove(HttpHeaderNames.ContentTransferEncoding);
            _ = _currentFieldAttributes.Remove(HttpHeaderNames.ContentType);
            _ = _currentFieldAttributes.Remove(HttpHeaderValues.FileName);
        }

        static StringCharSequence ReadLineStandard(IByteBuffer undecodedChunk, Encoding charset)
        {
            int readerIndex = undecodedChunk.ReaderIndex;
            IByteBuffer line = undecodedChunk.Allocator.HeapBuffer(64);
            try
            {
                while (undecodedChunk.IsReadable())
                {
                    byte nextByte = undecodedChunk.ReadByte();
                    if (nextByte == HttpConstants.CarriageReturn)
                    {
                        // check but do not changed readerIndex
                        nextByte = undecodedChunk.GetByte(undecodedChunk.ReaderIndex);
                        if (nextByte == HttpConstants.LineFeed)
                        {
                            // force read
                            _ = undecodedChunk.ReadByte();
                            return new StringCharSequence(line.ToString(charset));
                        }
                        else
                        {
                            // Write CR (not followed by LF)
                            _ = line.WriteByte(HttpConstants.CarriageReturn);
                        }
                    }
                    else if (nextByte == HttpConstants.LineFeed)
                    {
                        return new StringCharSequence(line.ToString(charset));
                    }
                    else
                    {
                        _ = line.WriteByte(nextByte);
                    }
                }
            }
            catch (IndexOutOfRangeException e)
            {
                _ = undecodedChunk.SetReaderIndex(readerIndex);
                ThrowHelper.ThrowNotEnoughDataDecoderException(e);
            }
            finally { _ = line.Release(); }
            _ = undecodedChunk.SetReaderIndex(readerIndex);
            return ThrowHelper.FromNotEnoughDataDecoderException_ReadLineStandard();
        }

        static StringCharSequence ReadLine(IByteBuffer undecodedChunk, Encoding charset)
        {
            if (!undecodedChunk.HasArray)
            {
                return ReadLineStandard(undecodedChunk, charset);
            }
            var sao = new HttpPostBodyUtil.SeekAheadOptimize(undecodedChunk);
            int readerIndex = undecodedChunk.ReaderIndex;
            IByteBuffer line = undecodedChunk.Allocator.HeapBuffer(64);
            try
            {
                while (sao.Pos < sao.Limit)
                {
                    byte nextByte = sao.Bytes[sao.Pos++];
                    if (nextByte == HttpConstants.CarriageReturn)
                    {
                        if (sao.Pos < sao.Limit)
                        {
                            nextByte = sao.Bytes[sao.Pos++];
                            if (nextByte == HttpConstants.LineFeed)
                            {
                                sao.SetReadPosition(0);
                                return new StringCharSequence(line.ToString(charset));
                            }
                            else
                            {
                                // Write CR (not followed by LF)
                                sao.Pos--;
                                _ = line.WriteByte(HttpConstants.CarriageReturn);
                            }
                        }
                        else
                        {
                            _ = line.WriteByte(nextByte);
                        }
                    }
                    else if (nextByte == HttpConstants.LineFeed)
                    {
                        sao.SetReadPosition(0);
                        return new StringCharSequence(line.ToString(charset));
                    }
                    else
                    {
                        _ = line.WriteByte(nextByte);
                    }
                }
            }
            catch (IndexOutOfRangeException e)
            {
                _ = undecodedChunk.SetReaderIndex(readerIndex);
                ThrowHelper.ThrowNotEnoughDataDecoderException(e);
            }
            finally
            {
                _ = line.Release();
            }
            _ = undecodedChunk.SetReaderIndex(readerIndex);
            return ThrowHelper.FromNotEnoughDataDecoderException_ReadLine();
        }

        static StringBuilderCharSequence ReadDelimiterStandard(IByteBuffer undecodedChunk, ICharSequence delimiter)
        {
            int readerIndex = undecodedChunk.ReaderIndex;
            try
            {
                var sb = new StringBuilderCharSequence(64);
                int delimiterPos = 0;
                int len = delimiter.Count;
                while (undecodedChunk.IsReadable() && delimiterPos < len)
                {
                    byte nextByte = undecodedChunk.ReadByte();
                    if (nextByte == delimiter[delimiterPos])
                    {
                        delimiterPos++;
                        sb.Append((char)nextByte);
                    }
                    else
                    {
                        // delimiter not found so break here !
                        _ = undecodedChunk.SetReaderIndex(readerIndex);
                        ThrowHelper.ThrowNotEnoughDataDecoderException(ExceptionArgument.ReadDelimiterStandard);
                    }
                }
                // Now check if either opening delimiter or closing delimiter
                if (undecodedChunk.IsReadable())
                {
                    byte nextByte = undecodedChunk.ReadByte();
                    switch (nextByte)
                    {
                        // first check for opening delimiter
                        case HttpConstants.CarriageReturn:
                            nextByte = undecodedChunk.ReadByte();
                            if (nextByte == HttpConstants.LineFeed)
                            {
                                return sb;
                            }
                            else
                            {
                                // error since CR must be followed by LF
                                // delimiter not found so break here !
                                _ = undecodedChunk.SetReaderIndex(readerIndex);
                                ThrowHelper.ThrowNotEnoughDataDecoderException(ExceptionArgument.ReadDelimiterStandard);
                            }
                            break;

                        case HttpConstants.LineFeed:
                            return sb;

                        case HttpConstants.MinusSign:
                            sb.Append(HttpConstants.MinusSignChar);
                            // second check for closing delimiter
                            nextByte = undecodedChunk.ReadByte();
                            if (nextByte == HttpConstants.MinusSignChar)
                            {
                                sb.Append(HttpConstants.MinusSignChar);
                                // now try to find if CRLF or LF there
                                if (undecodedChunk.IsReadable())
                                {
                                    nextByte = undecodedChunk.ReadByte();
                                    if (nextByte == HttpConstants.CarriageReturn)
                                    {
                                        nextByte = undecodedChunk.ReadByte();
                                        if (nextByte == HttpConstants.LineFeed)
                                        {
                                            return sb;
                                        }
                                        else
                                        {
                                            // error CR without LF
                                            // delimiter not found so break here !
                                            _ = undecodedChunk.SetReaderIndex(readerIndex);
                                            ThrowHelper.ThrowNotEnoughDataDecoderException(ExceptionArgument.ReadDelimiterStandard);
                                        }
                                    }
                                    else if (nextByte == HttpConstants.LineFeed)
                                    {
                                        return sb;
                                    }
                                    else
                                    {
                                        // No CRLF but ok however (Adobe Flash uploader)
                                        // minus 1 since we read one char ahead but
                                        // should not
                                        _ = undecodedChunk.SetReaderIndex(undecodedChunk.ReaderIndex - 1);
                                        return sb;
                                    }
                                }
                                // FIXME what do we do here?
                                // either considering it is fine, either waiting for
                                // more data to come?
                                // lets try considering it is fine...
                                return sb;
                            }
                            // only one '-' => not enough
                            // whatever now => error since incomplete
                            break;
                    }
                }
            }
            catch (IndexOutOfRangeException e)
            {
                _ = undecodedChunk.SetReaderIndex(readerIndex);
                ThrowHelper.ThrowNotEnoughDataDecoderException(e);
            }
            _ = undecodedChunk.SetReaderIndex(readerIndex);
            return ThrowHelper.FromNotEnoughDataDecoderException_ReadDelimiterStandard();
        }

        static StringBuilderCharSequence ReadDelimiter(IByteBuffer undecodedChunk, ICharSequence delimiter)
        {
            if (!undecodedChunk.HasArray)
            {
                return ReadDelimiterStandard(undecodedChunk, delimiter);
            }
            var sao = new HttpPostBodyUtil.SeekAheadOptimize(undecodedChunk);
            int readerIndex = undecodedChunk.ReaderIndex;
            int delimiterPos = 0;
            int len = delimiter.Count;
            try
            {
                var sb = new StringBuilderCharSequence(64);
                // check conformity with delimiter
                while (sao.Pos < sao.Limit && delimiterPos < len)
                {
                    byte nextByte = sao.Bytes[sao.Pos++];
                    if (nextByte == delimiter[delimiterPos])
                    {
                        delimiterPos++;
                        sb.Append((char)nextByte);
                    }
                    else
                    {
                        // delimiter not found so break here !
                        _ = undecodedChunk.SetReaderIndex(readerIndex);
                        ThrowHelper.ThrowNotEnoughDataDecoderException(ExceptionArgument.ReadDelimiter);
                    }
                }
                // Now check if either opening delimiter or closing delimiter
                if (sao.Pos < sao.Limit)
                {
                    byte nextByte = sao.Bytes[sao.Pos++];
                    switch (nextByte)
                    {
                        case HttpConstants.CarriageReturn:
                            // first check for opening delimiter
                            if (sao.Pos < sao.Limit)
                            {
                                nextByte = sao.Bytes[sao.Pos++];
                                if (nextByte == HttpConstants.LineFeed)
                                {
                                    sao.SetReadPosition(0);
                                    return sb;
                                }
                                else
                                {
                                    // error CR without LF
                                    // delimiter not found so break here !
                                    _ = undecodedChunk.SetReaderIndex(readerIndex);
                                    ThrowHelper.ThrowNotEnoughDataDecoderException(ExceptionArgument.ReadDelimiter);
                                }
                            }
                            else
                            {
                                // error since CR must be followed by LF
                                // delimiter not found so break here !
                                _ = undecodedChunk.SetReaderIndex(readerIndex);
                                ThrowHelper.ThrowNotEnoughDataDecoderException(ExceptionArgument.ReadDelimiter);
                            }
                            break;

                        case HttpConstants.LineFeed:
                            // same first check for opening delimiter where LF used with
                            // no CR
                            sao.SetReadPosition(0);
                            return sb;

                        case HttpConstants.MinusSign:
                            sb.Append(HttpConstants.MinusSignChar);
                            // second check for closing delimiter
                            if (sao.Pos < sao.Limit)
                            {
                                nextByte = sao.Bytes[sao.Pos++];
                                if (nextByte == HttpConstants.MinusSignChar)
                                {
                                    sb.Append(HttpConstants.MinusSignChar);
                                    // now try to find if CRLF or LF there
                                    if (sao.Pos < sao.Limit)
                                    {
                                        nextByte = sao.Bytes[sao.Pos++];
                                        if (nextByte == HttpConstants.CarriageReturn)
                                        {
                                            if (sao.Pos < sao.Limit)
                                            {
                                                nextByte = sao.Bytes[sao.Pos++];
                                                if (nextByte == HttpConstants.LineFeed)
                                                {
                                                    sao.SetReadPosition(0);
                                                    return sb;
                                                }
                                                else
                                                {
                                                    // error CR without LF
                                                    // delimiter not found so break here !
                                                    _ = undecodedChunk.SetReaderIndex(readerIndex);
                                                    ThrowHelper.ThrowNotEnoughDataDecoderException(ExceptionArgument.ReadDelimiter);
                                                }
                                            }
                                            else
                                            {
                                                // error CR without LF
                                                // delimiter not found so break here !
                                                _ = undecodedChunk.SetReaderIndex(readerIndex);
                                                ThrowHelper.ThrowNotEnoughDataDecoderException(ExceptionArgument.ReadDelimiter);
                                            }
                                        }
                                        else if (nextByte == HttpConstants.LineFeed)
                                        {
                                            sao.SetReadPosition(0);
                                            return sb;
                                        }
                                        else
                                        {
                                            // No CRLF but ok however (Adobe Flash
                                            // uploader)
                                            // minus 1 since we read one char ahead but
                                            // should not
                                            sao.SetReadPosition(1);
                                            return sb;
                                        }
                                    }
                                    // FIXME what do we do here?
                                    // either considering it is fine, either waiting for
                                    // more data to come?
                                    // lets try considering it is fine...
                                    sao.SetReadPosition(0);
                                    return sb;
                                }
                                // whatever now => error since incomplete
                                // only one '-' => not enough or whatever not enough
                                // element
                            }
                            break;
                    }
                }
            }
            catch (IndexOutOfRangeException e)
            {
                _ = undecodedChunk.SetReaderIndex(readerIndex);
                ThrowHelper.ThrowNotEnoughDataDecoderException(e);
            }
            _ = undecodedChunk.SetReaderIndex(readerIndex);
            return ThrowHelper.FromNotEnoughDataDecoderException_ReadDelimiter();
        }

        static bool LoadDataMultipartStandard(IByteBuffer undecodedChunk, ICharSequence delimiter, IHttpData httpData)
        {
            int startReaderIndex = undecodedChunk.ReaderIndex;
            int delimeterLength = delimiter.Count;
            int index = 0;
            int lastPosition = startReaderIndex;
            byte prevByte = HttpConstants.LineFeed;
            bool delimiterFound = false;
            while (undecodedChunk.IsReadable())
            {
                byte nextByte = undecodedChunk.ReadByte();
                // Check the delimiter
                if (prevByte == HttpConstants.LineFeed && nextByte == CharUtil.CodePointAt(delimiter, index))
                {
                    index++;
                    if (delimeterLength == index)
                    {
                        delimiterFound = true;
                        break;
                    }
                    continue;
                }
                lastPosition = undecodedChunk.ReaderIndex;
                if (nextByte == HttpConstants.LineFeed)
                {
                    index = 0;
                    lastPosition -= (prevByte == HttpConstants.CarriageReturn) ? 2 : 1;
                }
                prevByte = nextByte;
            }
            if (prevByte == HttpConstants.CarriageReturn)
            {
                lastPosition--;
            }
            IByteBuffer content = undecodedChunk.RetainedSlice(startReaderIndex, lastPosition - startReaderIndex);
            try
            {
                httpData.AddContent(content, delimiterFound);
            }
            catch (IOException e)
            {
                ThrowHelper.ThrowErrorDataDecoderException(e);
            }
            _ = undecodedChunk.SetReaderIndex(lastPosition);
            return delimiterFound;
        }

        static bool LoadDataMultipart(IByteBuffer undecodedChunk, ICharSequence delimiter, IHttpData httpData)
        {
            if (!undecodedChunk.HasArray)
            {
                return LoadDataMultipartStandard(undecodedChunk, delimiter, httpData);
            }
            var sao = new HttpPostBodyUtil.SeekAheadOptimize(undecodedChunk);
            int startReaderIndex = undecodedChunk.ReaderIndex;
            int delimeterLength = delimiter.Count;
            int index = 0;
            int lastRealPos = sao.Pos;
            byte prevByte = HttpConstants.LineFeed;
            bool delimiterFound = false;
            while (sao.Pos < sao.Limit)
            {
                byte nextByte = sao.Bytes[sao.Pos++];
                // Check the delimiter
                if (prevByte == HttpConstants.LineFeed && nextByte == CharUtil.CodePointAt(delimiter, index))
                {
                    index++;
                    if (delimeterLength == index)
                    {
                        delimiterFound = true;
                        break;
                    }
                    continue;
                }
                lastRealPos = sao.Pos;
                if (nextByte == HttpConstants.LineFeed)
                {
                    index = 0;
                    lastRealPos -= (prevByte == HttpConstants.CarriageReturn) ? 2 : 1;
                }
                prevByte = nextByte;
            }
            if (prevByte == HttpConstants.CarriageReturn)
            {
                lastRealPos--;
            }
            int lastPosition = sao.GetReadPosition(lastRealPos);
            IByteBuffer content = undecodedChunk.RetainedSlice(startReaderIndex, lastPosition - startReaderIndex);
            try
            {
                httpData.AddContent(content, delimiterFound);
            }
            catch (IOException e)
            {
                ThrowHelper.ThrowErrorDataDecoderException(e);
            }
            _ = undecodedChunk.SetReaderIndex(lastPosition);
            return delimiterFound;
        }

        static ICharSequence CleanString(string field) => new StringCharSequence(CleanString0(field));

        static ICharSequence CleanString(ICharSequence field)
        {
            return new StringCharSequence(CleanString0(field));
        }

        static string CleanString0(ICharSequence field)
        {
            return CleanString0(field.ToString());
        }

        static string CleanString0(string field)
        {
            int size = field.Length;
            var sb = StringBuilderCache.Acquire();
            for (int i = 0; i < size; i++)
            {
                char nextChar = field[i];
                switch (nextChar)
                {
                    case HttpConstants.ColonChar:  // Colon
                    case HttpConstants.CommaChar:  // Comma
                    case HttpConstants.EqualsSignChar:  // EqualsSign
                    case HttpConstants.SemicolonChar:  // Semicolon
                    case HttpConstants.HorizontalTabChar: // HorizontalTab
                        sb.Append(HttpConstants.HorizontalSpaceChar);
                        break;
                    case HttpConstants.DoubleQuoteChar:  // DoubleQuote
                        // nothing added, just removes it
                        break;
                    default:
                        sb.Append(nextChar);
                        break;
                }
            }
            return sb.ToString().Trim();
        }

        bool SkipOneLine()
        {
            if (!_undecodedChunk.IsReadable())
            {
                return false;
            }
            byte nextByte = _undecodedChunk.ReadByte();
            if (nextByte == HttpConstants.CarriageReturn)
            {
                if (!_undecodedChunk.IsReadable())
                {
                    _ = _undecodedChunk.SetReaderIndex(_undecodedChunk.ReaderIndex - 1);
                    return false;
                }

                nextByte = _undecodedChunk.ReadByte();
                if (nextByte == HttpConstants.LineFeed)
                {
                    return true;
                }

                _ = _undecodedChunk.SetReaderIndex(_undecodedChunk.ReaderIndex - 2);
                return false;
            }

            if (nextByte == HttpConstants.LineFeed)
            {
                return true;
            }
            _ = _undecodedChunk.SetReaderIndex(_undecodedChunk.ReaderIndex - 1);
            return false;
        }


        static ICharSequence[] SplitMultipartHeader(ICharSequence sb)
        {
            var headers = new List<ICharSequence>(1);
            int nameEnd;
            int colonEnd;
            int nameStart = HttpPostBodyUtil.FindNonWhitespace(sb, 0);
            for (nameEnd = nameStart; nameEnd < sb.Count; nameEnd++)
            {
                char ch = sb[nameEnd];
                if (ch == HttpConstants.ColonChar || char.IsWhiteSpace(ch))
                {
                    break;
                }
            }
            for (colonEnd = nameEnd; colonEnd < sb.Count; colonEnd++)
            {
                if (sb[colonEnd] == HttpConstants.ColonChar)
                {
                    colonEnd++;
                    break;
                }
            }
            int valueStart = HttpPostBodyUtil.FindNonWhitespace(sb, colonEnd);
            int valueEnd = HttpPostBodyUtil.FindEndOfString(sb);
            headers.Add(sb.SubSequence(nameStart, nameEnd));
            ICharSequence svalue = (valueStart >= valueEnd) ? AsciiString.Empty : sb.SubSequence(valueStart, valueEnd);
            ICharSequence[] values;
            if (svalue.IndexOf(HttpConstants.SemicolonChar) >= 0)
            {
                values = SplitMultipartHeaderValues(svalue);
            }
            else
            {
                values = CharUtil.Split(svalue, HttpConstants.CommaChar);
            }
            foreach (ICharSequence value in values)
            {
                headers.Add(CharUtil.Trim(value));
            }
            var array = new ICharSequence[headers.Count];
            for (int i = 0; i < headers.Count; i++)
            {
                array[i] = headers[i];
            }
            return array;
        }

        static ICharSequence[] SplitMultipartHeaderValues(ICharSequence svalue)
        {
            List<ICharSequence> values = InternalThreadLocalMap.Get().CharSequenceList(1);
            bool inQuote = false;
            bool escapeNext = false;
            int start = 0;
            for (int i = 0; i < svalue.Count; i++)
            {
                char c = svalue[i];
                if (inQuote)
                {
                    if (escapeNext)
                    {
                        escapeNext = false;
                    }
                    else
                    {
                        switch (c)
                        {
                            case HttpConstants.BackSlashChar:
                                escapeNext = true;
                                break;

                            case HttpConstants.DoubleQuoteChar:
                                inQuote = false;
                                break;
                        }
                    }
                }
                else
                {
                    switch (c)
                    {
                        case HttpConstants.DoubleQuoteChar:
                            inQuote = true;
                            break;
                        case HttpConstants.SemicolonChar:
                            values.Add(svalue.SubSequence(start, i));
                            start = i + 1;
                            break;
                    }
                }
            }
            values.Add(svalue.SubSequence(start));
            return values.ToArray();
        }
    }
}
