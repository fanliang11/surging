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
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using DotNetty.Buffers;
    using DotNetty.Common;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;
    using DotNetty.Handlers.Streams;
    using System.Text.Encodings.Web;

    /// <summary>
    /// This encoder will help to encode Request for a FORM as POST.
    ///
    /// <para>According to RFC 7231, POST, PUT and OPTIONS allow to have a body.
    /// This encoder will support widely all methods except TRACE since the RFC notes
    /// for GET, DELETE, HEAD and CONNECT: (replaces XXX by one of these methods)</para>
    /// <para>"A payload within a XXX request message has no defined semantics;
    /// sending a payload body on a XXX request might cause some existing
    /// implementations to reject the request."</para>
    /// <para>On the contrary, for TRACE method, RFC says:</para>
    /// <para>"A client MUST NOT send a message body in a TRACE request."</para>
    /// </summary>
    public class HttpPostRequestEncoder : IChunkedInput<IHttpContent>
    {
        /// <summary>
        /// Different modes to use to encode form data.
        /// </summary>
        public enum EncoderMode
        {
            /// <summary>
            /// Legacy mode which should work for most. It is known to not work with OAUTH. For OAUTH use
            /// <see cref="RFC3986"/>. The W3C form recommendations this for submitting post form data.
            /// </summary>
            RFC1738,

            /// <summary>
            /// Mode which is more new and is used for OAUTH
            /// </summary>
            RFC3986,

            /// <summary>
            /// The HTML5 spec disallows mixed mode in multipart/form-data
            /// requests. More concretely this means that more files submitted
            /// under the same name will not be encoded using mixed mode, but
            /// will be treated as distinct fields.
            /// Reference: http://www.w3.org/TR/html5/forms.html#multipart-form-data
            /// </summary>
            HTML5
        }

        static readonly KeyValuePair<Regex, string>[] PercentEncodings;

        static HttpPostRequestEncoder()
        {
            PercentEncodings = new[]
            {
                new KeyValuePair<Regex, string>(new Regex("\\*", RegexOptions.Compiled), "%2A"),
                new KeyValuePair<Regex, string>(new Regex("\\+", RegexOptions.Compiled), "%20"),
                new KeyValuePair<Regex, string>(new Regex("~", RegexOptions.Compiled), "%7E"),
            };
        }

        // Factory used to create InterfaceHttpData
        readonly IHttpDataFactory factory;

        // Request to encode
        readonly IHttpRequest request;

        // Default charset to use
        readonly Encoding charset = HttpConstants.DefaultEncoding;
        readonly HtmlEncoder htmlEncoder = HtmlEncoder.Default;

        /// <summary>Chunked false by default</summary>
        bool isChunked;

        /// <summary>InterfaceHttpData for Body (without encoding)</summary>
        readonly List<IInterfaceHttpData> bodyListDatas;

        /// <summary>The final Multipart List of InterfaceHttpData including encoding</summary>
        internal readonly List<IInterfaceHttpData> MultipartHttpDatas;

        /// <summary>Does this request is a Multipart request</summary>
        readonly bool isMultipart;

        /// <summary>If multipart, this is the boundary for the global multipart</summary>
        internal string MultipartDataBoundary;

        /// <summary>If multipart, there could be internal multiparts (mixed) to the global multipart. Only one level is allowed.</summary>
        internal string MultipartMixedBoundary;

        /// <summary>To check if the header has been finalized</summary>
        bool headerFinalized;

        readonly EncoderMode encoderMode;

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="request">the request to encode</param>
        /// <param name="multipart">True if the FORM is a ENCTYPE="multipart/form-data"</param>
        public HttpPostRequestEncoder(IHttpRequest request, bool multipart)
            : this(new DefaultHttpDataFactory(DefaultHttpDataFactory.MinSize), request, multipart, EncoderMode.RFC1738)
        {
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="factory">the factory used to create InterfaceHttpData</param>
        /// <param name="request">the request to encode</param>
        /// <param name="multipart">True if the FORM is a ENCTYPE="multipart/form-data"</param>
        public HttpPostRequestEncoder(IHttpDataFactory factory, IHttpRequest request, bool multipart)
            : this(factory, request, multipart, EncoderMode.RFC1738)
        {
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="factory">the factory used to create InterfaceHttpData</param>
        /// <param name="request">the request to encode</param>
        /// <param name="multipart">True if the FORM is a ENCTYPE="multipart/form-data"</param>
        /// <param name="encoderMode">the mode for the encoder to use. See <see cref="EncoderMode"/> for the details.</param>
        public HttpPostRequestEncoder(IHttpDataFactory factory, IHttpRequest request, bool multipart, EncoderMode encoderMode)
        {
            if (request is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.request); }
            if (factory is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.factory); }
            if (charset is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.charset); }

            this.request = request;
            this.factory = factory;
            HttpMethod method = request.Method;
            if (method.Equals(HttpMethod.Trace))
            {
                ThrowHelper.ThrowErrorDataEncoderException_CannotCreate();
            }
            // Fill default values
            this.bodyListDatas = new List<IInterfaceHttpData>();
            // default mode
            this.isLastChunk = false;
            this.isLastChunkSent = false;
            this.isMultipart = multipart;
            this.MultipartHttpDatas = new List<IInterfaceHttpData>();
            this.encoderMode = encoderMode;
            if (this.isMultipart)
            {
                this.InitDataMultipart();
            }
        }
        // Clean all HttpDatas (on Disk) for the current request.
        public void CleanFiles() => this.factory.CleanRequestHttpData(this.request);

        // Does the last non empty chunk already encoded so that next chunk will be empty (last chunk)
        bool isLastChunk;

        // Last chunk already sent
        bool isLastChunkSent;

        // The current FileUpload that is currently in encode process
        IFileUpload currentFileUpload;

        // While adding a FileUpload, is the multipart currently in Mixed Mode
        bool duringMixedMode;

        // Global Body size
        long globalBodySize;

        // Global Transfer progress
        long globalProgress;

        /// <summary>
        /// True if this request is a Multipart request
        /// </summary>
        public bool IsMultipart => this.isMultipart;

        /// <summary>
        /// Init the delimiter for Global Part (Data).
        /// </summary>
        void InitDataMultipart() => this.MultipartDataBoundary = GetNewMultipartDelimiter();

        /// <summary>
        /// Init the delimiter for Mixed Part (Mixed).
        /// </summary>
        void InitMixedMultipart() => this.MultipartMixedBoundary = GetNewMultipartDelimiter();

        /// <summary>construct a generated delimiter</summary>
        /// <returns>a newly generated Delimiter (either for DATA or MIXED)</returns>
        static string GetNewMultipartDelimiter() => Convert.ToString(PlatformDependent.GetThreadLocalRandom().NextLong(), 16).ToLowerInvariant();

        /// <summary>
        /// This getMethod returns a List of all InterfaceHttpData from body part.
        /// </summary>
        /// <returns>the list of InterfaceHttpData from Body part</returns>
        public List<IInterfaceHttpData> GetBodyListAttributes() => this.bodyListDatas;

        /// <summary>
        /// Set the Body HttpDatas list
        /// </summary>
        /// <param name="list"></param>
        public void SetBodyHttpDatas(List<IInterfaceHttpData> list)
        {
            if (list is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.list); }

            this.globalBodySize = 0;
            this.bodyListDatas.Clear();
            this.currentFileUpload = null;
            this.duringMixedMode = false;
            this.MultipartHttpDatas.Clear();
            foreach (IInterfaceHttpData data in list)
            {
                this.AddBodyHttpData(data);
            }
        }

        /// <summary>
        /// Add a simple attribute in the body as Name=Value
        /// </summary>
        /// <param name="name">name of the parameter</param>
        /// <param name="value">the value of the parameter</param>
        /// <exception cref="ArgumentNullException">for name</exception>
        /// <exception cref="ErrorDataEncoderException">if the encoding is in error or if the finalize were already done</exception>
        public void AddBodyAttribute(string name, string value)
        {
            if (name is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.name); }
            IAttribute data = this.factory.CreateAttribute(this.request, name, value ?? StringUtil.EmptyString);
            this.AddBodyHttpData(data);
        }

        public void AddBodyFileUpload(string name, FileStream fileStream, string contentType, bool isText)
        {
            string fileName = Path.GetFileName(fileStream.Name);
            this.AddBodyFileUpload(name, fileName, fileStream, contentType, isText);
        }

        public void AddBodyFileUpload(string name, string fileName, FileStream fileStream, string contentType, bool isText)
        {
            if (name is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.name); }
            if (fileStream is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.fileStream); }

            if (fileName is null)
            {
                fileName = StringUtil.EmptyString;
            }
            string scontentType = contentType;
            string contentTransferEncoding = null;
            if (contentType is null)
            {
                scontentType = isText
                    ? HttpPostBodyUtil.DefaultTextContentType
                    : HttpPostBodyUtil.DefaultBinaryContentType;
            }
            if (!isText)
            {
                contentTransferEncoding = HttpPostBodyUtil.TransferEncodingMechanism.Binary.Value;
            }

            IFileUpload fileUpload = this.factory.CreateFileUpload(this.request, name, fileName, scontentType,
                contentTransferEncoding, null, fileStream.Length);
            try
            {
                fileUpload.SetContent(fileStream);
            }
            catch (IOException e)
            {
                ThrowHelper.ThrowErrorDataEncoderException(e);
            }

            this.AddBodyHttpData(fileUpload);
        }

        public void AddBodyFileUploads(string name, FileStream[] file, string[] contentType, bool[] isText)
        {
            if (file.Length != contentType.Length && file.Length != isText.Length)
            {
                ThrowHelper.ThrowArgumentException_DiffArrayLen();
            }
            for (int i = 0; i < file.Length; i++)
            {
                this.AddBodyFileUpload(name, file[i], contentType[i], isText[i]);
            }
        }

        public void AddBodyHttpData(IInterfaceHttpData data)
        {
            if (data is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.data); }
            if (this.headerFinalized)
            {
                ThrowHelper.ThrowErrorDataEncoderException_CannotAddValue();
            }
            this.bodyListDatas.Add(data);
            if (!this.isMultipart)
            {
                switch (data)
                {
                    case IAttribute dataAttribute:
                        try
                        {
                            // name=value& with encoded name and attribute
                            string key = this.EncodeAttribute(dataAttribute.Name, this.charset);
                            string value = this.EncodeAttribute(dataAttribute.Value, this.charset);
                            IAttribute newattribute = this.factory.CreateAttribute(this.request, key, value);
                            this.MultipartHttpDatas.Add(newattribute);
                            this.globalBodySize += newattribute.Name.Length + 1 + newattribute.Length + 1;
                        }
                        catch (IOException e)
                        {
                            ThrowHelper.ThrowErrorDataEncoderException(e);
                        }
                        break;

                    case IFileUpload fileUpload:
                        // since not Multipart, only name=filename => Attribute
                        // name=filename& with encoded name and filename
                        string key0 = this.EncodeAttribute(fileUpload.Name, this.charset);
                        string value0 = this.EncodeAttribute(fileUpload.FileName, this.charset);
                        IAttribute newattribute0 = this.factory.CreateAttribute(this.request, key0, value0);
                        this.MultipartHttpDatas.Add(newattribute0);
                        this.globalBodySize += newattribute0.Name.Length + 1 + newattribute0.Length + 1;
                        break;
                }
                return;
            }
            //  Logic:
            //  if not Attribute:
            //       add Data to body list
            //       if (duringMixedMode)
            //           add endmixedmultipart delimiter
            //           currentFileUpload = null
            //           duringMixedMode = false;
            //       add multipart delimiter, multipart body header and Data to multipart list
            //       reset currentFileUpload, duringMixedMode
            //  if FileUpload: take care of multiple file for one field => mixed mode
            //       if (duringMixedMode)
            //           if (currentFileUpload.name == data.name)
            //               add mixedmultipart delimiter, mixedmultipart body header and Data to multipart list
            //           else
            //               add endmixedmultipart delimiter, multipart body header and Data to multipart list
            //               currentFileUpload = data
            //               duringMixedMode = false;
            //       else
            //           if (currentFileUpload.name == data.name)
            //               change multipart body header of previous file into multipart list to
            //                       mixedmultipart start, mixedmultipart body header
            //               add mixedmultipart delimiter, mixedmultipart body header and Data to multipart list
            //               duringMixedMode = true
            //           else
            //               add multipart delimiter, multipart body header and Data to multipart list
            //               currentFileUpload = data
            //               duringMixedMode = false;
            //  Do not add last delimiter! Could be:
            //  if duringmixedmode: endmixedmultipart + endmultipart
            //  else only endmultipart
            // 
            if (data is IAttribute attribute)
            {
                InternalAttribute internalAttribute;
                if (this.duringMixedMode)
                {
                    internalAttribute = new InternalAttribute(this.charset);
                    internalAttribute.AddValue($"\r\n--{this.MultipartMixedBoundary}--");
                    this.MultipartHttpDatas.Add(internalAttribute);
                    this.MultipartMixedBoundary = null;
                    this.currentFileUpload = null;
                    this.duringMixedMode = false;
                }
                internalAttribute = new InternalAttribute(this.charset);
                if ((uint)this.MultipartHttpDatas.Count > 0u)
                {
                    // previously a data field so CRLF
                    internalAttribute.AddValue("\r\n");
                }
                internalAttribute.AddValue($"--{this.MultipartDataBoundary}\r\n");
                // content-disposition: form-data; name="field1"
                internalAttribute.AddValue($"{HttpHeaderNames.ContentDisposition}: {HttpHeaderValues.FormData}; {HttpHeaderValues.Name}=\"{attribute.Name}\"\r\n");
                // Add Content-Length: xxx
                internalAttribute.AddValue($"{HttpHeaderNames.ContentLength}: {attribute.Length}\r\n");
                Encoding localcharset = attribute.Charset;
                if (localcharset is object)
                {
                    // Content-Type: text/plain; charset=charset
                    internalAttribute.AddValue($"{HttpHeaderNames.ContentType}: {HttpPostBodyUtil.DefaultTextContentType}; {HttpHeaderValues.Charset}={localcharset.WebName}\r\n");
                }
                // CRLF between body header and data
                internalAttribute.AddValue("\r\n");
                this.MultipartHttpDatas.Add(internalAttribute);
                this.MultipartHttpDatas.Add(data);
                this.globalBodySize += attribute.Length + internalAttribute.Size;
            }
            else if (data is IFileUpload fileUpload)
            {
                var internalAttribute = new InternalAttribute(this.charset);
                if ((uint)this.MultipartHttpDatas.Count > 0u)
                {
                    // previously a data field so CRLF
                    internalAttribute.AddValue("\r\n");
                }
                bool localMixed;
                if (this.duringMixedMode)
                {
                    if (this.currentFileUpload is object && string.Equals(this.currentFileUpload.Name, fileUpload.Name
#if NETCOREAPP_3_0_GREATER || NETSTANDARD_2_0_GREATER
                        ))
#else
                        , StringComparison.Ordinal))
#endif
                    {
                        // continue a mixed mode

                        localMixed = true;
                    }
                    else
                    {
                        // end a mixed mode

                        // add endmixedmultipart delimiter, multipart body header
                        // and
                        // Data to multipart list
                        internalAttribute.AddValue($"--{this.MultipartMixedBoundary}--");
                        this.MultipartHttpDatas.Add(internalAttribute);
                        this.MultipartMixedBoundary = null;
                        // start a new one (could be replaced if mixed start again
                        // from here
                        internalAttribute = new InternalAttribute(this.charset);
                        internalAttribute.AddValue("\r\n");
                        localMixed = false;
                        // new currentFileUpload and no more in Mixed mode
                        this.currentFileUpload = fileUpload;
                        this.duringMixedMode = false;
                    }
                }
                else
                {
                    if (this.encoderMode != EncoderMode.HTML5 && this.currentFileUpload is object
                        && string.Equals(this.currentFileUpload.Name, fileUpload.Name
#if NETCOREAPP_3_0_GREATER || NETSTANDARD_2_0_GREATER
                            ))
#else
                            , StringComparison.Ordinal))
#endif
                    {
                        // create a new mixed mode (from previous file)

                        // change multipart body header of previous file into
                        // multipart list to
                        // mixedmultipart start, mixedmultipart body header

                        // change Internal (size()-2 position in multipartHttpDatas)
                        // from (line starting with *)
                        // --AaB03x
                        // * Content-Disposition: form-data; name="files";
                        // filename="file1.txt"
                        // Content-Type: text/plain
                        // to (lines starting with *)
                        // --AaB03x
                        // * Content-Disposition: form-data; name="files"
                        // * Content-Type: multipart/mixed; boundary=BbC04y
                        // *
                        // * --BbC04y
                        // * Content-Disposition: attachment; filename="file1.txt"
                        // Content-Type: text/plain

                        this.InitMixedMultipart();
                        var pastAttribute = (InternalAttribute)this.MultipartHttpDatas[this.MultipartHttpDatas.Count - 2];
                        // remove past size
                        this.globalBodySize -= pastAttribute.Size;
                        var replacement = StringBuilderManager.Allocate(
                            139 + this.MultipartDataBoundary.Length + this.MultipartMixedBoundary.Length * 2
                                + fileUpload.FileName.Length + fileUpload.Name.Length)
                            .Append("--")
                            .Append(this.MultipartDataBoundary)
                            .Append("\r\n")

                            .Append(HttpHeaderNames.ContentDisposition)
                            .Append(": ")
                            .Append(HttpHeaderValues.FormData)
                            .Append("; ")
                            .Append(HttpHeaderValues.Name)
                            .Append("=\"")
                            .Append(fileUpload.Name)
                            .Append("\"\r\n")

                            .Append(HttpHeaderNames.ContentType)
                            .Append(": ")
                            .Append(HttpHeaderValues.MultipartMixed)
                            .Append("; ")
                            .Append(HttpHeaderValues.Boundary)
                            .Append('=')
                            .Append(this.MultipartMixedBoundary)
                            .Append("\r\n\r\n")

                            .Append("--")
                            .Append(this.MultipartMixedBoundary)
                            .Append("\r\n")

                            .Append(HttpHeaderNames.ContentDisposition)
                            .Append(": ")
                            .Append(HttpHeaderValues.Attachment);

                        if ((uint)fileUpload.FileName.Length > 0u)
                        {
                            replacement.Append("; ")
                                .Append(HttpHeaderValues.FileName)
                                .Append("=\"")
                                .Append(this.currentFileUpload.FileName)
                                .Append('"');
                        }

                        replacement.Append("\r\n");

                        pastAttribute.SetValue(StringBuilderManager.ReturnAndFree(replacement), 1);
                        pastAttribute.SetValue("", 2);

                        // update past size
                        this.globalBodySize += pastAttribute.Size;

                        // now continue
                        // add mixedmultipart delimiter, mixedmultipart body header
                        // and
                        // Data to multipart list
                        localMixed = true;
                        this.duringMixedMode = true;
                    }
                    else
                    {
                        // a simple new multipart
                        // add multipart delimiter, multipart body header and Data
                        // to multipart list
                        localMixed = false;
                        this.currentFileUpload = fileUpload;
                        this.duringMixedMode = false;
                    }
                }

                if (localMixed)
                {
                    // add mixedmultipart delimiter, mixedmultipart body header and
                    // Data to multipart list
                    internalAttribute.AddValue($"--{this.MultipartMixedBoundary}\r\n");

                    if (0u >= (uint)fileUpload.FileName.Length)
                    {
                        // Content-Disposition: attachment
                        internalAttribute.AddValue($"{HttpHeaderNames.ContentDisposition}: {HttpHeaderValues.Attachment}\r\n");
                    }
                    else
                    {
                        // Content-Disposition: attachment; filename="file1.txt"
                        internalAttribute.AddValue($"{HttpHeaderNames.ContentDisposition}: {HttpHeaderValues.Attachment}; {HttpHeaderValues.FileName}=\"{fileUpload.FileName}\"\r\n");
                    }
                }
                else
                {
                    internalAttribute.AddValue($"--{this.MultipartDataBoundary}\r\n");

                    if (0u >= (uint)fileUpload.FileName.Length)
                    {
                        // Content-Disposition: form-data; name="files";
                        internalAttribute.AddValue($"{HttpHeaderNames.ContentDisposition}: {HttpHeaderValues.FormData}; {HttpHeaderValues.Name}=\"{fileUpload.Name}\"\r\n");
                    }
                    else
                    {
                        // Content-Disposition: form-data; name="files";
                        // filename="file1.txt"
                        internalAttribute.AddValue($"{HttpHeaderNames.ContentDisposition}: {HttpHeaderValues.FormData}; {HttpHeaderValues.Name}=\"{fileUpload.Name}\"; {HttpHeaderValues.FileName}=\"{fileUpload.FileName}\"\r\n");
                    }
                }
                // Add Content-Length: xxx
                internalAttribute.AddValue($"{HttpHeaderNames.ContentLength}: {fileUpload.Length}\r\n");
                // Content-Type: image/gif
                // Content-Type: text/plain; charset=ISO-8859-1
                // Content-Transfer-Encoding: binary
                internalAttribute.AddValue($"{HttpHeaderNames.ContentType}: {fileUpload.ContentType}");
                string contentTransferEncoding = fileUpload.ContentTransferEncoding;
                if (contentTransferEncoding is object
                    && string.Equals(contentTransferEncoding, HttpPostBodyUtil.TransferEncodingMechanism.Binary.Value
#if NETCOREAPP_3_0_GREATER || NETSTANDARD_2_0_GREATER
                        ))
#else
                        , StringComparison.Ordinal))
#endif
                {
                    internalAttribute.AddValue($"\r\n{HttpHeaderNames.ContentTransferEncoding}: {HttpPostBodyUtil.TransferEncodingMechanism.Binary.Value}\r\n\r\n");
                }
                else if (fileUpload.Charset is object)
                {
                    internalAttribute.AddValue($"; {HttpHeaderValues.Charset}={fileUpload.Charset.WebName}\r\n\r\n");
                }
                else
                {
                    internalAttribute.AddValue("\r\n\r\n");
                }
                this.MultipartHttpDatas.Add(internalAttribute);
                this.MultipartHttpDatas.Add(data);
                this.globalBodySize += fileUpload.Length + internalAttribute.Size;
            }
        }

        ListIterator iterator;

        public IHttpRequest FinalizeRequest()
        {
            // Finalize the multipartHttpDatas
            if (!this.headerFinalized)
            {
                if (this.isMultipart)
                {
                    var attribute = new InternalAttribute(this.charset);
                    if (this.duringMixedMode)
                    {
                        attribute.AddValue($"\r\n--{this.MultipartMixedBoundary}--");
                    }

                    attribute.AddValue($"\r\n--{this.MultipartDataBoundary}--\r\n");
                    this.MultipartHttpDatas.Add(attribute);
                    this.MultipartMixedBoundary = null;
                    this.currentFileUpload = null;
                    this.duringMixedMode = false;
                    this.globalBodySize += attribute.Size;
                }
                this.headerFinalized = true;
            }
            else
            {
                ThrowHelper.ThrowErrorDataEncoderException_HeaderAlreadyEncoded();
            }

            HttpHeaders headers = this.request.Headers;
            IList<ICharSequence> contentTypes = headers.GetAll(HttpHeaderNames.ContentType);
            IList<ICharSequence> transferEncoding = headers.GetAll(HttpHeaderNames.TransferEncoding);
            if (contentTypes is object)
            {
                _ = headers.Remove(HttpHeaderNames.ContentType);
                foreach (ICharSequence contentType in contentTypes)
                {
                    // "multipart/form-data; boundary=--89421926422648"
                    string lowercased = contentType.ToString().ToLowerInvariant();
                    if (lowercased.StartsWith(HttpHeaderValues.MultipartFormData.ToString(), StringComparison.Ordinal)
                        || lowercased.StartsWith(HttpHeaderValues.ApplicationXWwwFormUrlencoded.ToString(), StringComparison.Ordinal))
                    {
                        // ignore
                    }
                    else
                    {
                        _ = headers.Add(HttpHeaderNames.ContentType, contentType);
                    }
                }
            }
            if (this.isMultipart)
            {
                string value = $"{HttpHeaderValues.MultipartFormData}; {HttpHeaderValues.Boundary}={this.MultipartDataBoundary}";
                _ = headers.Add(HttpHeaderNames.ContentType, value);
            }
            else
            {
                // Not multipart
                _ = headers.Add(HttpHeaderNames.ContentType, HttpHeaderValues.ApplicationXWwwFormUrlencoded);
            }
            // Now consider size for chunk or not
            long realSize = this.globalBodySize;
            if (!this.isMultipart)
            {
                realSize -= 1; // last '&' removed
            }
            this.iterator = new ListIterator(this.MultipartHttpDatas);
            _ = headers.Set(HttpHeaderNames.ContentLength, Convert.ToString(realSize));
            if (realSize > HttpPostBodyUtil.ChunkSize || this.isMultipart)
            {
                this.isChunked = true;
                if (transferEncoding is object)
                {
                    _ = headers.Remove(HttpHeaderNames.TransferEncoding);
                    foreach (ICharSequence v in transferEncoding)
                    {
                        if (HttpHeaderValues.Chunked.ContentEqualsIgnoreCase(v))
                        {
                            // ignore
                        }
                        else
                        {
                            _ = headers.Add(HttpHeaderNames.TransferEncoding, v);
                        }
                    }
                }
                HttpUtil.SetTransferEncodingChunked(this.request, true);

                // wrap to hide the possible content
                return new WrappedHttpRequest(this.request);
            }
            else
            {
                // get the only one body and set it to the request
                IHttpContent chunk = this.NextChunk();
                if (this.request is IFullHttpRequest fullRequest)
                {
                    IByteBuffer chunkContent = chunk.Content;
                    if (!ReferenceEquals(fullRequest.Content, chunkContent))
                    {
                        _ = fullRequest.Content.Clear();
                        _ = fullRequest.Content.WriteBytes(chunkContent);
                        _ = chunkContent.Release();
                    }
                    return fullRequest;
                }
                else
                {
                    return new WrappedFullHttpRequest(this.request, chunk);
                }
            }
        }

        public bool IsChunked => this.isChunked;

        string EncodeAttribute(string value, Encoding stringEncoding)
        {
            if (value is null)
            {
                return string.Empty;
            }

            string encoded = this.htmlEncoder.Encode(value);
            if (this.encoderMode == EncoderMode.RFC3986)
            {
                foreach (KeyValuePair<Regex, string> entry in PercentEncodings)
                {
                    string replacement = entry.Value;
                    encoded = entry.Key.Replace(encoded, replacement);
                }
            }
            return encoded;
        }

        // The ByteBuf currently used by the encoder
        IByteBuffer currentBuffer;

        // The current InterfaceHttpData to encode (used if more chunks are available)
        IInterfaceHttpData currentData;

        // If not multipart, does the currentBuffer stands for the Key or for the Value
        bool isKey = true;

        /// <summary>
        /// Returns the next ByteBuf to send as an HttpChunk and modifying currentBuffer accordingly
        /// </summary>
        IByteBuffer FillByteBuffer()
        {
            int length = this.currentBuffer.ReadableBytes;
            if (length > HttpPostBodyUtil.ChunkSize)
            {
                return this.currentBuffer.ReadRetainedSlice(HttpPostBodyUtil.ChunkSize);
            }
            else
            {
                // to continue
                IByteBuffer slice = this.currentBuffer;
                this.currentBuffer = null;
                return slice;
            }
        }

        // From the current context(currentBuffer and currentData), returns the next 
        // HttpChunk(if possible) trying to get sizeleft bytes more into the currentBuffer.
        // This is the Multipart version.
        IHttpContent EncodeNextChunkMultipart(int sizeleft)
        {
            if (this.currentData is null)
            {
                return null;
            }
            IByteBuffer buffer = null;
            if (this.currentData is InternalAttribute internalAttribute)
            {
                buffer = internalAttribute.ToByteBuffer();
                this.currentData = null;
            }
            else
            {
                try
                {
                    buffer = ((IHttpData)this.currentData).GetChunk(sizeleft);
                }
                catch (IOException e)
                {
                    ThrowHelper.ThrowErrorDataEncoderException(e);
                }
                if (0u >= (uint)buffer.Capacity)
                {
                    // end for current InterfaceHttpData, need more data
                    this.currentData = null;
                    return null;
                }
            }
            this.currentBuffer = this.currentBuffer is null
                ? buffer
                : Unpooled.WrappedBuffer(this.currentBuffer, buffer);

            if (this.currentBuffer.ReadableBytes < HttpPostBodyUtil.ChunkSize)
            {
                this.currentData = null;
                return null;
            }

            buffer = this.FillByteBuffer();
            return new DefaultHttpContent(buffer);
        }

        // From the current context(currentBuffer and currentData), returns the next HttpChunk(if possible)
        // trying to get* sizeleft bytes more into the currentBuffer.This is the UrlEncoded version.
        IHttpContent EncodeNextChunkUrlEncoded(int sizeleft)
        {
            if (this.currentData is null)
            {
                return null;
            }
            int size = sizeleft;
            IByteBuffer buffer = null;

            // Set name=
            if (this.isKey)
            {
                string key = this.currentData.Name;
                buffer = Unpooled.WrappedBuffer(TextEncodings.UTF8NoBOM.GetBytes(key));
                this.isKey = false;
                if (this.currentBuffer is null)
                {
                    this.currentBuffer = Unpooled.WrappedBuffer(buffer,
                        Unpooled.WrappedBuffer(TextEncodings.UTF8NoBOM.GetBytes("=")));
                }
                else
                {
                    this.currentBuffer = Unpooled.WrappedBuffer(this.currentBuffer, buffer,
                        Unpooled.WrappedBuffer(TextEncodings.UTF8NoBOM.GetBytes("=")));
                }
                // continue
                size -= buffer.ReadableBytes + 1;
                if (this.currentBuffer.ReadableBytes >= HttpPostBodyUtil.ChunkSize)
                {
                    buffer = this.FillByteBuffer();
                    return new DefaultHttpContent(buffer);
                }
            }

            // Put value into buffer
            try
            {
                buffer = ((IHttpData)this.currentData).GetChunk(size);
            }
            catch (IOException e)
            {
                ThrowHelper.ThrowErrorDataEncoderException(e);
            }

            // Figure out delimiter
            IByteBuffer delimiter = null;
            if (buffer.ReadableBytes < size)
            {
                this.isKey = true;
                delimiter = this.iterator.HasNext()
                    ? Unpooled.WrappedBuffer(TextEncodings.UTF8NoBOM.GetBytes("&"))
                    : null;
            }

            // End for current InterfaceHttpData, need potentially more data
            if (0u >= (uint)buffer.Capacity)
            {
                this.currentData = null;
                if (this.currentBuffer is null)
                {
                    if (delimiter is null)
                    {
                        return null;
                    }
                    else
                    {
                        this.currentBuffer = delimiter;
                    }
                }
                else
                {
                    if (delimiter is object)
                    {
                        this.currentBuffer = Unpooled.WrappedBuffer(this.currentBuffer, delimiter);
                    }
                }
                Debug.Assert(this.currentBuffer is object);
                if (this.currentBuffer.ReadableBytes >= HttpPostBodyUtil.ChunkSize)
                {
                    buffer = this.FillByteBuffer();
                    return new DefaultHttpContent(buffer);
                }
                return null;
            }

            // Put it all together: name=value&
            if (this.currentBuffer is null)
            {
                this.currentBuffer = delimiter is object
                    ? Unpooled.WrappedBuffer(buffer, delimiter)
                    : buffer;
            }
            else
            {
                this.currentBuffer = delimiter is object
                    ? Unpooled.WrappedBuffer(this.currentBuffer, buffer, delimiter)
                    : Unpooled.WrappedBuffer(this.currentBuffer, buffer);
            }

            // end for current InterfaceHttpData, need more data
            if (this.currentBuffer.ReadableBytes < HttpPostBodyUtil.ChunkSize)
            {
                this.currentData = null;
                this.isKey = true;
                return null;
            }

            buffer = this.FillByteBuffer();
            return new DefaultHttpContent(buffer);
        }

        public void Close()
        {
            // NO since the user can want to reuse (broadcast for instance)
            // cleanFiles();
        }
        public IHttpContent ReadChunk(IByteBufferAllocator allocator)
        {
            if (this.isLastChunkSent)
            {
                return null;
            }
            else
            {
                IHttpContent nextChunk = this.NextChunk();
                this.globalProgress += nextChunk.Content.ReadableBytes;
                return nextChunk;
            }
        }

        IHttpContent NextChunk()
        {
            if (this.isLastChunk)
            {
                this.isLastChunkSent = true;
                return EmptyLastHttpContent.Default;
            }
            // first test if previous buffer is not empty
            int size = this.CalculateRemainingSize();
            if (size <= 0)
            {
                // NextChunk from buffer
                IByteBuffer buffer = this.FillByteBuffer();
                return new DefaultHttpContent(buffer);
            }
            // size > 0
            if (this.currentData is object)
            {
                // continue to read data
                IHttpContent chunk = this.isMultipart
                    ? this.EncodeNextChunkMultipart(size)
                    : this.EncodeNextChunkUrlEncoded(size);
                if (chunk is object)
                {
                    // NextChunk from data
                    return chunk;
                }
                size = this.CalculateRemainingSize();
            }
            if (!this.iterator.HasNext())
            {
                return this.LastChunk();
            }
            while (size > 0 && this.iterator.HasNext())
            {
                this.currentData = this.iterator.Next();
                IHttpContent chunk;
                if (this.isMultipart)
                {
                    chunk = this.EncodeNextChunkMultipart(size);
                }
                else
                {
                    chunk = this.EncodeNextChunkUrlEncoded(size);
                }
                if (chunk is null)
                {
                    // not enough
                    size = this.CalculateRemainingSize();
                    continue;
                }
                // NextChunk from data
                return chunk;
            }
            // end since no more data
            return this.LastChunk();
        }

        int CalculateRemainingSize()
        {
            int size = HttpPostBodyUtil.ChunkSize;
            if (this.currentBuffer is object)
            {
                size -= this.currentBuffer.ReadableBytes;
            }
            return size;
        }

        IHttpContent LastChunk()
        {
            this.isLastChunk = true;
            if (this.currentBuffer is null)
            {
                this.isLastChunkSent = true;
                // LastChunk with no more data
                return EmptyLastHttpContent.Default;
            }
            // NextChunk as last non empty from buffer
            IByteBuffer buffer = this.currentBuffer;
            this.currentBuffer = null;
            return new DefaultHttpContent(buffer);
        }

        public bool IsEndOfInput => this.isLastChunkSent;

        public long Length => this.isMultipart ? this.globalBodySize : this.globalBodySize - 1;

        // Global Transfer progress
        public long Progress => this.globalProgress;

        class WrappedHttpRequest : IHttpRequest
        {
            readonly IHttpRequest request;

            internal WrappedHttpRequest(IHttpRequest request)
            {
                this.request = request;
            }


            public IHttpMessage SetProtocolVersion(HttpVersion version)
            {
                _ = this.request.SetProtocolVersion(version);
                return this;
            }

            public IHttpRequest SetMethod(HttpMethod method)
            {
                _ = this.request.SetMethod(method);
                return this;
            }

            public IHttpRequest SetUri(string uri)
            {
                _ = this.request.SetUri(uri);
                return this;
            }

            public HttpVersion ProtocolVersion => this.request.ProtocolVersion;

            public HttpMethod Method => this.request.Method;

            public string Uri => this.request.Uri;

            public HttpHeaders Headers => this.request.Headers;

            public DecoderResult Result
            {
                get => this.request.Result;
                set => this.request.Result = value;
            }
        }

        sealed class WrappedFullHttpRequest : WrappedHttpRequest, IFullHttpRequest
        {
            readonly IHttpContent content;

            public WrappedFullHttpRequest(IHttpRequest request, IHttpContent content)
                : base(request)
            {
                this.content = content;
            }

            public IByteBufferHolder Copy() => this.Replace(this.Content.Copy());

            public IByteBufferHolder Duplicate() => this.Replace(this.Content.Duplicate());

            public IByteBufferHolder RetainedDuplicate() => this.Replace(this.Content.RetainedDuplicate());

            public IByteBufferHolder Replace(IByteBuffer newContent)
            {
                var duplicate = new DefaultFullHttpRequest(this.ProtocolVersion, this.Method, this.Uri, newContent);
                _ = duplicate.Headers.Set(this.Headers);
                _ = duplicate.TrailingHeaders.Set(this.TrailingHeaders);
                return duplicate;
            }

            public IReferenceCounted Retain(int increment)
            {
                _ = this.content.Retain(increment);
                return this;
            }

            public IReferenceCounted Retain()
            {
                _ = this.content.Retain();
                return this;
            }

            public IReferenceCounted Touch()
            {
                _ = this.content.Touch();
                return this;
            }

            public IReferenceCounted Touch(object hint)
            {
                _ = this.content.Touch(hint);
                return this;
            }

            public IByteBuffer Content => this.content.Content;

            public HttpHeaders TrailingHeaders
            {
                get
                {
                    if (this.content is ILastHttpContent httpContent)
                    {
                        return httpContent.TrailingHeaders;
                    }

                    return EmptyHttpHeaders.Default;
                }
            }

            public int ReferenceCount => this.content.ReferenceCount;

            public bool Release() => this.content.Release();

            public bool Release(int decrement) => this.content.Release(decrement);
        }

        sealed class ListIterator
        {
            readonly List<IInterfaceHttpData> list;
            int index;

            public ListIterator(List<IInterfaceHttpData> list)
            {
                this.list = list;
                this.index = 0;
            }

            public bool HasNext() => this.index < this.list.Count;

            public IInterfaceHttpData Next()
            {
                if (!this.HasNext())
                {
                    ThrowHelper.ThrowInvalidOperationException_NoMoreElement();
                }

                IInterfaceHttpData data = this.list[this.index++];
                return data;
            }
        }
    }
}
