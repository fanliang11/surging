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

namespace DotNetty.Codecs.Compression
{
    public static class ZlibCodecFactory
    {
        public static bool IsSupportingWindowSizeAndMemLevel => true;

        public static ZlibEncoder NewZlibEncoder(int compressionLevel) => new JZlibEncoder(compressionLevel);

        public static ZlibEncoder NewZlibEncoder(ZlibWrapper wrapper) => new JZlibEncoder(wrapper);

        public static ZlibEncoder NewZlibEncoder(ZlibWrapper wrapper, int compressionLevel) => new JZlibEncoder(wrapper, compressionLevel);

        public static ZlibEncoder NewZlibEncoder(ZlibWrapper wrapper, int compressionLevel, int windowBits, int memLevel) => 
            new JZlibEncoder(wrapper, compressionLevel, windowBits, memLevel);

        public static ZlibEncoder NewZlibEncoder(byte[] dictionary) => new JZlibEncoder(dictionary);

        public static ZlibEncoder NewZlibEncoder(int compressionLevel, byte[] dictionary) => new JZlibEncoder(compressionLevel, dictionary);

        public static ZlibEncoder NewZlibEncoder(int compressionLevel, int windowBits, int memLevel, byte[] dictionary) => 
            new JZlibEncoder(compressionLevel, windowBits, memLevel, dictionary);

        public static ZlibDecoder NewZlibDecoder() => new JZlibDecoder();

        public static ZlibDecoder NewZlibDecoder(ZlibWrapper wrapper) => new JZlibDecoder(wrapper);

        public static ZlibDecoder NewZlibDecoder(byte[] dictionary) => new JZlibDecoder(dictionary);
    }
}
