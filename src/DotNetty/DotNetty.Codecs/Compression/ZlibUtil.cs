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
    using System;
    using System.Runtime.CompilerServices;

    static class ZlibUtil
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Fail(Inflater z, string message, int resultCode)
        {
            throw new DecompressionException($"{message} ({resultCode})" + (z.msg is object ? " : " + z.msg : ""));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Fail(Deflater z, string message, int resultCode)
        {
            throw new CompressionException($"{message} ({resultCode})" + (z.msg is object ? " : " + z.msg : ""));
        }

        public static JZlib.WrapperType ConvertWrapperType(ZlibWrapper wrapper)
        {
            JZlib.WrapperType convertedWrapperType;
            switch (wrapper)
            {
                case ZlibWrapper.None:
                    convertedWrapperType = JZlib.W_NONE;
                    break;
                case ZlibWrapper.Zlib:
                    convertedWrapperType = JZlib.W_ZLIB;
                    break;
                case ZlibWrapper.Gzip:
                    convertedWrapperType = JZlib.W_GZIP;
                    break;
                case ZlibWrapper.ZlibOrNone:
                    convertedWrapperType = JZlib.W_ANY;
                    break;
                default:
                    throw new ArgumentException($"Unknown type {wrapper}");
            }

            return convertedWrapperType;
        }

        public static int WrapperOverhead(ZlibWrapper wrapper)
        {
            int overhead;
            switch (wrapper)
            {
                case ZlibWrapper.None:
                    overhead = 0;
                    break;
                case ZlibWrapper.Zlib:
                case ZlibWrapper.ZlibOrNone:
                    overhead = 2;
                    break;
                case ZlibWrapper.Gzip:
                    overhead = 10;
                    break;
                default:
                    throw new NotSupportedException($"Unknown value {wrapper}");
            }

            return overhead;
        }
    }
}
