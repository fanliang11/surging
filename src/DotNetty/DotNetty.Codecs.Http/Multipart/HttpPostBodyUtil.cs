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
    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;

    static class HttpPostBodyUtil
    {
        public static readonly int ChunkSize = 8096;

        public const string DefaultBinaryContentType = "application/octet-stream";

        public const string DefaultTextContentType = "text/plain";

        public sealed class TransferEncodingMechanism
        {
            // Default encoding
            public static readonly TransferEncodingMechanism Bit7 = new TransferEncodingMechanism("7bit");

            // Short lines but not in ASCII - no encoding
            public static readonly TransferEncodingMechanism Bit8 = new TransferEncodingMechanism("8bit");

            // Could be long text not in ASCII - no encoding
            public static readonly TransferEncodingMechanism Binary = new TransferEncodingMechanism("binary");

            readonly string value;

            TransferEncodingMechanism(string value)
            {
                this.value = value;
            }

            public string Value => this.value;

            public override string ToString() => this.value;
        }

        internal class SeekAheadOptimize
        {
            internal byte[] Bytes;
            internal int ReaderIndex;
            internal int Pos;
            internal int OrigPos;
            internal int Limit;
            internal IByteBuffer Buffer;

            internal SeekAheadOptimize(IByteBuffer buffer)
            {
                if (!buffer.HasArray)
                {
                    ThrowHelper.ThrowArgumentException_BufferNoBacking();
                }
                this.Buffer = buffer;
                this.Bytes = buffer.Array;
                this.ReaderIndex = buffer.ReaderIndex;
                this.OrigPos = this.Pos = buffer.ArrayOffset + this.ReaderIndex;
                this.Limit = buffer.ArrayOffset + buffer.WriterIndex;
            }

            internal void SetReadPosition(int minus)
            {
                this.Pos -= minus;
                this.ReaderIndex = this.GetReadPosition(this.Pos);
                _ = this.Buffer.SetReaderIndex(this.ReaderIndex);
            }

            internal int GetReadPosition(int index) => index - this.OrigPos + this.ReaderIndex;
        }

        internal static int FindNonWhitespace(ICharSequence sb, int offset)
        {
            int result;
            for (result = offset; result < sb.Count; result++)
            {
                if (!char.IsWhiteSpace(sb[result]))
                {
                    break;
                }
            }

            return result;
        }

        internal static int FindWhitespace(ICharSequence sb, int offset)
        {
            int result;
            for (result = offset; result < sb.Count; result++)
            {
                if (char.IsWhiteSpace(sb[result]))
                {
                    break;
                }
            }

            return result;
        }

        internal static int FindEndOfString(ICharSequence sb)
        {
            int result;
            for (result = sb.Count; result > 0; result--)
            {
                if (!char.IsWhiteSpace(sb[result - 1]))
                {
                    break;
                }
            }

            return result;
        }
    }
}
