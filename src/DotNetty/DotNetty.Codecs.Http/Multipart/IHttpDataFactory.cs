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
    using System.Text;

    /// <summary>
    /// Interface to enable creation of IPostHttpData objects
    /// </summary>
    public interface IHttpDataFactory
    {
        void SetMaxLimit(long max);

        IAttribute CreateAttribute(IHttpRequest request, string name);

        IAttribute CreateAttribute(IHttpRequest request, string name, long definedSize);

        IAttribute CreateAttribute(IHttpRequest request, string name, string value);

        IFileUpload CreateFileUpload(IHttpRequest request, string name, string filename, 
            string contentType, string contentTransferEncoding, Encoding charset, long size);

        void RemoveHttpDataFromClean(IHttpRequest request, IInterfaceHttpData data);

        void CleanRequestHttpData(IHttpRequest request);

        void CleanAllHttpData();
    }
}
