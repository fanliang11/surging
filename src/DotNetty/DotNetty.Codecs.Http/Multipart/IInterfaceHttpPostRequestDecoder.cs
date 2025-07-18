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
    using System.Collections.Generic;
    using DotNetty.Common.Utilities;

    public interface IInterfaceHttpPostRequestDecoder
    {
        bool IsMultipart { get; }

        int DiscardThreshold { get; set; }

        List<IInterfaceHttpData> GetBodyHttpDatas();

        List<IInterfaceHttpData> GetBodyHttpDatas(string name);

        IInterfaceHttpData GetBodyHttpData(string name);

        IInterfaceHttpPostRequestDecoder Offer(IHttpContent content);

        bool HasNext { get; }

        IInterfaceHttpData Next();

        IInterfaceHttpData CurrentPartialHttpData { get; }

        void Destroy();

        void CleanFiles();

        void RemoveHttpDataFromClean(IInterfaceHttpData data);
    }
}
