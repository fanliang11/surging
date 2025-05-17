﻿/*
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

namespace DotNetty.Codecs.Compression
{
    /**
     * The container file formats that wrap the stream compressed by the DEFLATE
     * algorithm.
     */
    public enum ZlibWrapper
    {
        /**
         * The ZLIB wrapper as specified in <a href="http://tools.ietf.org/html/rfc1950">RFC 1950</a>.
         */
        Zlib,
        /**
         * The GZIP wrapper as specified in <a href="http://tools.ietf.org/html/rfc1952">RFC 1952</a>.
         */
        Gzip,
        /**
         * Raw DEFLATE stream only (no header and no footer).
         */
        None,
        /**
         * Try {@link #ZLIB} first and then {@link #NONE} if the first attempt fails.
         * Please note that you can specify this wrapper type only when decompressing.
         */
        ZlibOrNone
    }
}
