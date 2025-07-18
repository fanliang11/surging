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
 * Copyright (c) The DotNetty Project (Microsoft). All rights reserved.
 *
 *   https://github.com/azure/dotnetty
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs.Http
{
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;

    using static HttpConstants;

    static class HttpHeadersEncoder
    {
        const int ColonAndSpaceShort = (Colon << 8) | HorizontalSpace;

        public static void EncoderHeader(AsciiString name, ICharSequence value, IByteBuffer buf)
        {
            int nameLen = name.Count;
            int valueLen = value.Count;
            int entryLen = nameLen + valueLen + 4;
            _ = buf.EnsureWritable(entryLen);
            int offset = buf.WriterIndex;
            WriteAscii(buf, offset, name);
            offset += nameLen;
            _ = buf.SetShort(offset, ColonAndSpaceShort);
            offset += 2;
            WriteAscii(buf, offset, value);
            offset += valueLen;
            _ = buf.SetShort(offset, CrlfShort);
            offset += 2;
            _ = buf.SetWriterIndex(offset);
        }

        static void WriteAscii(IByteBuffer buf, int offset, ICharSequence value)
        {
            if (value is AsciiString asciiString)
            {
                ByteBufferUtil.Copy(asciiString, 0, buf, offset, value.Count);
            }
            else
            {
                _ = buf.SetCharSequence(offset, value, Encoding.ASCII);
            }
        }
    }
}
