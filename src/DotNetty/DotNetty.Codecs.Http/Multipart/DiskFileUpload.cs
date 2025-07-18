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
    using System.IO;
    using System.Text;
    using DotNetty.Buffers;

    public class DiskFileUpload : AbstractDiskHttpData, IFileUpload
    {
        public static string FileBaseDirectory;
        public static bool DeleteOnExitTemporaryFile = true;
        public const string FilePrefix = "FUp_";
        public const string FilePostfix = ".tmp";

        private readonly string _baseDir;
        private readonly bool _deleteOnExit;

        private string _filename;
        private string _contentType;
        private string _contentTransferEncoding;

        public DiskFileUpload(string name, string filename, string contentType,
            string contentTransferEncoding, Encoding charset, long size)
            : this(name, filename, contentType, contentTransferEncoding,
                charset, size, FileBaseDirectory, DeleteOnExitTemporaryFile)
        {
        }

        public DiskFileUpload(string name, string filename, string contentType,
            string contentTransferEncoding, Encoding charset, long size, string baseDir, bool deleteOnExit)
            : base(name, charset, size)
        {
            if (filename is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.filename); }
            if (contentType is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.contentType); }

            _filename = filename;
            _contentType = contentType;
            _contentTransferEncoding = contentTransferEncoding;
            _baseDir = baseDir ?? FileBaseDirectory;
            _deleteOnExit = deleteOnExit;
        }

        public override HttpDataType DataType => HttpDataType.FileUpload;

        public string FileName
        {
            get => _filename;
            set
            {
                if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }
                _filename = value;
            }
        }

        public override int GetHashCode() => FileUploadUtil.HashCode(this);

        public override bool Equals(object obj) => obj is IFileUpload fileUpload && FileUploadUtil.Equals(this, fileUpload);

        public override int CompareTo(IInterfaceHttpData other)
        {
            if (other is IFileUpload fu)
            {
                return CompareTo(fu);
            }

            return ThrowHelper.FromArgumentException_CompareToHttpData(DataType, other.DataType);
        }

        public int CompareTo(IFileUpload other) => FileUploadUtil.CompareTo(this, other);

        public string ContentType
        {
            get => _contentType;
            set
            {
                if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }
                _contentType = value;
            }
        }

        public string ContentTransferEncoding
        {
            get => _contentTransferEncoding;
            set => _contentTransferEncoding = value;
        }

        public override string ToString()
        {
            FileStream fileStream = null;
            try
            {
                fileStream = GetFile();
            }
            catch (IOException)
            {
                // Should not occur.
            }

            return HttpHeaderNames.ContentDisposition + ": " +
               HttpHeaderValues.FormData + "; " + HttpHeaderValues.Name + "=\"" + Name +
                "\"; " + HttpHeaderValues.FileName + "=\"" + _filename + "\"\r\n" +
                HttpHeaderNames.ContentType + ": " + _contentType +
                (Charset is object ? "; " + HttpHeaderValues.Charset + '=' + Charset.WebName + "\r\n" : "\r\n") +
                HttpHeaderNames.ContentLength + ": " + Length + "\r\n" +
                "Completed: " + IsCompleted +
                "\r\nIsInMemory: " + IsInMemory + "\r\nRealFile: " +
                (fileStream is object ? fileStream.Name : "null") + " DefaultDeleteAfter: " +
                DeleteOnExitTemporaryFile;
        }

        protected internal override bool DeleteOnExit => _deleteOnExit;

        protected internal override string BaseDirectory => _baseDir;

        protected override string DiskFilename => "upload";

        protected override string Postfix => FilePostfix;

        protected override string Prefix => FilePrefix;

        public override IByteBufferHolder Copy() => Replace(Content?.Copy());

        public override IByteBufferHolder Duplicate() => Replace(Content?.Duplicate());

        public override IByteBufferHolder RetainedDuplicate()
        {
            IByteBuffer content = Content;
            if (content is object)
            {
                content = content.RetainedDuplicate();
                bool success = false;
                try
                {
                    var duplicate = (IFileUpload)Replace(content);
                    success = true;
                    return duplicate;
                }
                finally
                {
                    if (!success)
                    {
                        _ = content.Release();
                    }
                }
            }
            else
            {
                return Replace(null);
            }
        }

        public override IByteBufferHolder Replace(IByteBuffer content)
        {
            var upload = new DiskFileUpload(
                Name, FileName, ContentType, ContentTransferEncoding, Charset, Size, _baseDir, _deleteOnExit);
            if (content is object)
            {
                try
                {
                    upload.SetContent(content);
                }
                catch (IOException e)
                {
                    ThrowHelper.ThrowChannelException_IO(e);
                }
            }

            return upload;
        }
    }
}
