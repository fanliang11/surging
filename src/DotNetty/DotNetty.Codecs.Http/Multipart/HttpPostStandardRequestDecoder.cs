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
    using System.Runtime.CompilerServices;
    using System.Runtime.ExceptionServices;
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;

    public class HttpPostStandardRequestDecoder : IInterfaceHttpPostRequestDecoder
    {
        // Factory used to create InterfaceHttpData
        private readonly IHttpDataFactory _factory;

        // Request to decode
        private readonly IHttpRequest _request;

        // Default charset to use
        private readonly Encoding _charset;

        // Does the last chunk already received
        private bool _isLastChunk;

        // HttpDatas from Body
        private readonly List<IInterfaceHttpData> _bodyListHttpData;

        //  HttpDatas as Map from Body
        private readonly Dictionary<string, List<IInterfaceHttpData>> _bodyMapHttpData;

        // The current channelBuffer
        private IByteBuffer _undecodedChunk;

        // Body HttpDatas current position
        private int _bodyListHttpDataRank;

        // Current getStatus
        private MultiPartStatus _currentStatus;

        // The current Attribute that is currently in decode process
        private IAttribute _currentAttribute;

        private bool _destroyed;

        private int _discardThreshold;

        public HttpPostStandardRequestDecoder(IHttpRequest request)
            : this(new DefaultHttpDataFactory(DefaultHttpDataFactory.MinSize), request, HttpConstants.DefaultEncoding)
        {
        }

        public HttpPostStandardRequestDecoder(IHttpDataFactory factory, IHttpRequest request)
            : this(factory, request, HttpConstants.DefaultEncoding)
        {
        }

        public HttpPostStandardRequestDecoder(IHttpDataFactory factory, IHttpRequest request, Encoding charset)
        {
            if (request is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.request); }
            if (charset is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.charset); }
            if (factory is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.factory); }

            _bodyListHttpData = new List<IInterfaceHttpData>();
            _bodyMapHttpData = new Dictionary<string, List<IInterfaceHttpData>>(StringComparer.OrdinalIgnoreCase);
            _currentStatus = MultiPartStatus.Notstarted;
            _discardThreshold = HttpPostRequestDecoder.DefaultDiscardThreshold;

            _factory = factory;
            _request = request;
            _charset = charset;
            try
            {
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
            catch (Exception exc)
            {
                Destroy();
                ExceptionDispatchInfo.Capture(exc).Throw();
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        private void CheckDestroyed()
        {
            if (_destroyed)
            {
                ThrowHelper.ThrowInvalidOperationException_CheckDestroyed<HttpPostStandardRequestDecoder>();
            }
        }

        public bool IsMultipart
        {
            get
            {
                CheckDestroyed();
                return false;
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
                ThrowHelper.ThrowNotEnoughDataDecoderException(ExceptionArgument.HttpPostStandardRequestDecoder);
            }
            return _bodyListHttpData;
        }

        public List<IInterfaceHttpData> GetBodyHttpDatas(string name)
        {
            CheckDestroyed();

            if (!_isLastChunk)
            {
                ThrowHelper.ThrowNotEnoughDataDecoderException(ExceptionArgument.HttpPostStandardRequestDecoder);
            }
            return _bodyMapHttpData[name];
        }

        public IInterfaceHttpData GetBodyHttpData(string name)
        {
            CheckDestroyed();

            if (!_isLastChunk)
            {
                ThrowHelper.ThrowNotEnoughDataDecoderException(ExceptionArgument.HttpPostStandardRequestDecoder);
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
                        // So maybe something to optimize on a later stage.
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
            if (_undecodedChunk is object && _undecodedChunk.WriterIndex > _discardThreshold)
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
                        ThrowHelper.ThrowEndOfDataDecoderException_HttpPostStandardRequestDecoder();
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

        public IInterfaceHttpData CurrentPartialHttpData => _currentAttribute;

        void ParseBody()
        {
            if (_currentStatus == MultiPartStatus.PreEpilogue || _currentStatus == MultiPartStatus.Epilogue)
            {
                if (_isLastChunk)
                {
                    _currentStatus = MultiPartStatus.Epilogue;
                }

                return;
            }
            ParseBodyAttributes();
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

        void ParseBodyAttributesStandard()
        {
            int firstpos = _undecodedChunk.ReaderIndex;
            int currentpos = firstpos;
            if (_currentStatus == MultiPartStatus.Notstarted)
            {
                _currentStatus = MultiPartStatus.Disposition;
            }
            bool contRead = true;
            try
            {
                int ampersandpos;
                while (_undecodedChunk.IsReadable() && contRead)
                {
                    char read = (char)_undecodedChunk.ReadByte();
                    currentpos++;
                    switch (_currentStatus)
                    {
                        case MultiPartStatus.Disposition:// search '='
                            switch (read)
                            {
                                case HttpConstants.EqualsSignChar:
                                    _currentStatus = MultiPartStatus.Field;
                                    int equalpos = currentpos - 1;
                                    string key = DecodeAttribute(_undecodedChunk.ToString(firstpos, equalpos - firstpos, _charset), _charset);
                                    _currentAttribute = _factory.CreateAttribute(_request, key);
                                    firstpos = currentpos;
                                    break;

                                case HttpConstants.AmpersandChar:
                                    // special empty FIELD
                                    _currentStatus = MultiPartStatus.Disposition;
                                    ampersandpos = currentpos - 1;
                                    string key0 = DecodeAttribute(_undecodedChunk.ToString(firstpos, ampersandpos - firstpos, _charset), _charset);
                                    _currentAttribute = _factory.CreateAttribute(_request, key0);
                                    _currentAttribute.Value = ""; // empty
                                    AddHttpData(_currentAttribute);
                                    _currentAttribute = null;
                                    firstpos = currentpos;
                                    contRead = true;
                                    break;
                            }
                            break;
                        case MultiPartStatus.Field:// search '&' or end of line
                            switch (read)
                            {
                                case HttpConstants.AmpersandChar:
                                    _currentStatus = MultiPartStatus.Disposition;
                                    ampersandpos = currentpos - 1;
                                    SetFinalBuffer(_undecodedChunk.RetainedSlice(firstpos, ampersandpos - firstpos));
                                    firstpos = currentpos;
                                    contRead = true;
                                    break;

                                case HttpConstants.CarriageReturnChar:
                                    if (_undecodedChunk.IsReadable())
                                    {
                                        read = (char)_undecodedChunk.ReadByte();
                                        currentpos++;
                                        if (read == HttpConstants.LineFeed)
                                        {
                                            _currentStatus = MultiPartStatus.PreEpilogue;
                                            ampersandpos = currentpos - 2;
                                            SetFinalBuffer(_undecodedChunk.RetainedSlice(firstpos, ampersandpos - firstpos));
                                            firstpos = currentpos;
                                            contRead = false;
                                        }
                                        else
                                        {
                                            // Error
                                            ThrowHelper.ThrowErrorDataDecoderException_BadEndOfLine();
                                        }
                                    }
                                    else
                                    {
                                        currentpos--;
                                    }
                                    break;

                                case HttpConstants.LineFeedChar:
                                    _currentStatus = MultiPartStatus.PreEpilogue;
                                    ampersandpos = currentpos - 1;
                                    SetFinalBuffer(_undecodedChunk.RetainedSlice(firstpos, ampersandpos - firstpos));
                                    firstpos = currentpos;
                                    contRead = false;
                                    break;
                            }
                            break;
                        default:
                            // just stop
                            contRead = false;
                            break;
                    }
                }
                if (_isLastChunk && _currentAttribute is object)
                {
                    // special case
                    ampersandpos = currentpos;
                    if (ampersandpos > firstpos)
                    {
                        SetFinalBuffer(_undecodedChunk.RetainedSlice(firstpos, ampersandpos - firstpos));
                    }
                    else if (!_currentAttribute.IsCompleted)
                    {
                        SetFinalBuffer(Unpooled.Empty);
                    }
                    firstpos = currentpos;
                    _currentStatus = MultiPartStatus.Epilogue;
                }
                else if (contRead && _currentAttribute is object && _currentStatus == MultiPartStatus.Field)
                {
                    // reset index except if to continue in case of FIELD getStatus
                    _currentAttribute.AddContent(_undecodedChunk.RetainedSlice(firstpos, currentpos - firstpos), false);
                    firstpos = currentpos;
                }
                _ = _undecodedChunk.SetReaderIndex(firstpos);
            }
            catch (ErrorDataDecoderException)
            {
                // error while decoding
                _ = _undecodedChunk.SetReaderIndex(firstpos);
                throw;
            }
            catch (IOException e)
            {
                // error while decoding
                _ = _undecodedChunk.SetReaderIndex(firstpos);
                ThrowHelper.ThrowErrorDataDecoderException(e);
            }
            catch (ArgumentException exc)
            {
                // error while decoding
                _ = _undecodedChunk.SetReaderIndex(firstpos);
                ThrowHelper.ThrowErrorDataDecoderException(exc);
            }
        }

        void ParseBodyAttributes()
        {
            if (_undecodedChunk is null) { return; }
            if (!_undecodedChunk.HasArray)
            {
                ParseBodyAttributesStandard();
                return;
            }
            var sao = new HttpPostBodyUtil.SeekAheadOptimize(_undecodedChunk);
            int firstpos = _undecodedChunk.ReaderIndex;
            int currentpos = firstpos;
            if (_currentStatus == MultiPartStatus.Notstarted)
            {
                _currentStatus = MultiPartStatus.Disposition;
            }
            bool contRead = true;
            try
            {
                //loop:
                int ampersandpos;
                while (sao.Pos < sao.Limit)
                {
                    char read = (char)(sao.Bytes[sao.Pos++]);
                    currentpos++;
                    switch (_currentStatus)
                    {
                        case MultiPartStatus.Disposition:// search '='
                            switch (read)
                            {
                                case HttpConstants.EqualsSignChar:
                                    _currentStatus = MultiPartStatus.Field;
                                    int equalpos = currentpos - 1;
                                    string key = DecodeAttribute(_undecodedChunk.ToString(firstpos, equalpos - firstpos, _charset), _charset);
                                    _currentAttribute = _factory.CreateAttribute(_request, key);
                                    firstpos = currentpos;
                                    break;

                                case HttpConstants.AmpersandChar:
                                    // special empty FIELD
                                    _currentStatus = MultiPartStatus.Disposition;
                                    ampersandpos = currentpos - 1;
                                    string key0 = DecodeAttribute(_undecodedChunk.ToString(firstpos, ampersandpos - firstpos, _charset), _charset);
                                    _currentAttribute = _factory.CreateAttribute(_request, key0);
                                    _currentAttribute.Value = ""; // empty
                                    AddHttpData(_currentAttribute);
                                    _currentAttribute = null;
                                    firstpos = currentpos;
                                    contRead = true;
                                    break;
                            }
                            break;
                        case MultiPartStatus.Field:// search '&' or end of line
                            switch (read)
                            {
                                case HttpConstants.AmpersandChar:
                                    _currentStatus = MultiPartStatus.Disposition;
                                    ampersandpos = currentpos - 1;
                                    SetFinalBuffer(_undecodedChunk.RetainedSlice(firstpos, ampersandpos - firstpos));
                                    firstpos = currentpos;
                                    contRead = true;
                                    break;

                                case HttpConstants.CarriageReturnChar:
                                    if (sao.Pos < sao.Limit)
                                    {
                                        read = (char)(sao.Bytes[sao.Pos++]);
                                        currentpos++;
                                        if (read == HttpConstants.LineFeed)
                                        {
                                            _currentStatus = MultiPartStatus.PreEpilogue;
                                            ampersandpos = currentpos - 2;
                                            sao.SetReadPosition(0);
                                            SetFinalBuffer(_undecodedChunk.RetainedSlice(firstpos, ampersandpos - firstpos));
                                            firstpos = currentpos;
                                            contRead = false;
                                            goto loop;
                                        }
                                        else
                                        {
                                            // Error
                                            sao.SetReadPosition(0);
                                            ThrowHelper.ThrowErrorDataDecoderException_BadEndOfLine();
                                        }
                                    }
                                    else
                                    {
                                        if (sao.Limit > 0)
                                        {
                                            currentpos--;
                                        }
                                    }
                                    break;

                                case HttpConstants.LineFeedChar:
                                    _currentStatus = MultiPartStatus.PreEpilogue;
                                    ampersandpos = currentpos - 1;
                                    sao.SetReadPosition(0);
                                    SetFinalBuffer(_undecodedChunk.RetainedSlice(firstpos, ampersandpos - firstpos));
                                    firstpos = currentpos;
                                    contRead = false;
                                    goto loop;
                            }
                            break;
                        default:
                            // just stop
                            sao.SetReadPosition(0);
                            contRead = false;
                            goto loop;
                    }
                }
            loop:
                if (_isLastChunk && _currentAttribute is object)
                {
                    // special case
                    ampersandpos = currentpos;
                    if (ampersandpos > firstpos)
                    {
                        SetFinalBuffer(_undecodedChunk.RetainedSlice(firstpos, ampersandpos - firstpos));
                    }
                    else if (!_currentAttribute.IsCompleted)
                    {
                        SetFinalBuffer(Unpooled.Empty);
                    }
                    firstpos = currentpos;
                    _currentStatus = MultiPartStatus.Epilogue;
                }
                else if (contRead && _currentAttribute is object && _currentStatus == MultiPartStatus.Field)
                {
                    // reset index except if to continue in case of FIELD getStatus
                    _currentAttribute.AddContent(_undecodedChunk.RetainedSlice(firstpos, currentpos - firstpos), false);
                    firstpos = currentpos;
                }
                _ = _undecodedChunk.SetReaderIndex(firstpos);
            }
            catch (ErrorDataDecoderException)
            {
                // error while decoding
                _ = _undecodedChunk.SetReaderIndex(firstpos);
                throw;
            }
            catch (IOException e)
            {
                // error while decoding
                _ = _undecodedChunk.SetReaderIndex(firstpos);
                ThrowHelper.ThrowErrorDataDecoderException(e);
            }
            catch (ArgumentException e)
            {
                // error while decoding
                _ = _undecodedChunk.SetReaderIndex(firstpos);
                ThrowHelper.ThrowErrorDataDecoderException(e);
            }
        }

        void SetFinalBuffer(IByteBuffer buffer)
        {
            _currentAttribute.AddContent(buffer, true);
            IByteBuffer decodedBuf = DecodeAttribute(_currentAttribute.GetByteBuffer(), _charset);
            if (decodedBuf is object)
            { // override content only when ByteBuf needed decoding
                _currentAttribute.SetContent(decodedBuf);
            }
            AddHttpData(_currentAttribute);
            _currentAttribute = null;
        }

        static string DecodeAttribute(string s, Encoding charset)
        {
            try
            {
                return QueryStringDecoder.DecodeComponent(s, charset);
            }
            catch (ArgumentException e)
            {
                return ThrowHelper.FromErrorDataDecoderException_BadString(s, e);
            }
        }

        private static IByteBuffer DecodeAttribute(IByteBuffer b, Encoding charset)
        {
            int firstEscaped = b.IndexOfAny(b.ReaderIndex, b.WriterIndex, HttpConstants.Percent, HttpConstants.PlusSign);
            if ((uint)firstEscaped > SharedConstants.TooBigOrNegative) // == -1
            {
                return null; // nothing to decode
            }

            IByteBuffer buf = b.Allocator.Buffer(b.ReadableBytes);
            UrlDecoder urlDecode = new UrlDecoder(buf);
            int idx = b.ForEachByte(urlDecode);
            if (urlDecode.NextEscapedIdx != 0)
            { // incomplete hex byte
                if ((uint)idx > SharedConstants.TooBigOrNegative) // == -1
                {
                    idx = b.ReadableBytes - 1;
                }
                idx -= urlDecode.NextEscapedIdx - 1;
                buf.Release();
                ThrowHelper.ThrowErrorDataDecoderException_Invalid_hex_byte_at_index(idx, b, charset);
            }

            return buf;
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

        sealed class UrlDecoder : IByteProcessor
        {
            private readonly IByteBuffer _output;
            public int NextEscapedIdx;
            private byte _hiByte;

            public UrlDecoder(IByteBuffer output) => _output = output;

            public bool Process(byte value)
            {
                const uint MaxHex2B = 15U;

                if (NextEscapedIdx != 0)
                {
                    if (0u >= (uint)(NextEscapedIdx - 1))
                    {
                        _hiByte = value;
                        ++NextEscapedIdx;
                    }
                    else
                    {
                        int hi = StringUtil.DecodeHexNibble((char)_hiByte);
                        int lo = StringUtil.DecodeHexNibble((char)value);
                        if (MaxHex2B >= (uint)hi && MaxHex2B >= (uint)lo)
                        {
                            _output.WriteByte((hi << 4) + lo);
                            NextEscapedIdx = 0;
                        }
                        else
                        {
                            ++NextEscapedIdx;
                            return false;
                        }
                    }
                }
                else if (0u >= (uint)(value - HttpConstants.Percent)) // '%'
                {
                    NextEscapedIdx = 1;
                }
                else if (0u >= (uint)(value - HttpConstants.PlusSign)) // '+'
                {
                    _output.WriteByte(HttpConstants.Space);
                }
                else
                {
                    _output.WriteByte(value);
                }
                return true;
            }
        }
    }
}
