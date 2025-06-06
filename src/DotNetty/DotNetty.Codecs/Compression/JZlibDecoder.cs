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
    using System.Collections.Generic;
    using System.Threading;
    using DotNetty.Buffers;
    using DotNetty.Transport.Channels;

    public class JZlibDecoder : ZlibDecoder
    {
        readonly Inflater z = new Inflater();
        readonly byte[] dictionary;
        int finished;

        /// <summary>
        /// Creates a new instance with the default wrapper (<see cref="ZlibWrapper.ZlibOrNone"/>).
        /// </summary>
        public JZlibDecoder()
            : this(ZlibWrapper.ZlibOrNone, 0)
        {
        }

        /// <summary>
        /// Creates a new instance with the default wrapper (<see cref="ZlibWrapper.ZlibOrNone"/>)
        /// and specified maximum buffer allocation.
        /// </summary>
        /// <param name="maxAllocation">Maximum size of the decompression buffer. Must be &gt;= 0.
        /// If zero, maximum size is decided by the <see cref="IByteBufferAllocator"/>.</param>
        public JZlibDecoder(int maxAllocation)
            : this(ZlibWrapper.ZlibOrNone, maxAllocation)
        {
        }

        /// <summary>
        /// Creates a new instance with the specified wrapper.
        /// </summary>
        /// <param name="wrapper"></param>
        public JZlibDecoder(ZlibWrapper wrapper)
            : this(wrapper, 0)
        {
        }

        /// <summary>
        /// Creates a new instance with the specified wrapper and maximum buffer allocation.
        /// </summary>
        /// <param name="wrapper"></param>
        /// <param name="maxAllocation">Maximum size of the decompression buffer. Must be &gt;= 0.
        /// If zero, maximum size is decided by the <see cref="IByteBufferAllocator"/>.</param>
        public JZlibDecoder(ZlibWrapper wrapper, int maxAllocation)
            : base(maxAllocation)
        {
            int resultCode = this.z.Init(ZlibUtil.ConvertWrapperType(wrapper));
            if (resultCode != JZlib.Z_OK)
            {
                ZlibUtil.Fail(this.z, "initialization failure", resultCode);
            }
        }

        /// <summary>
        /// Creates a new instance with the specified preset dictionary. The wrapper
        /// is always <see cref="ZlibWrapper.Zlib"/> because it is the only format that
        /// supports the preset dictionary.
        /// </summary>
        /// <param name="dictionary"></param>
        public JZlibDecoder(byte[] dictionary)
            : this(dictionary, 0)
        {
        }

        /// <summary>
        /// Creates a new instance with the specified preset dictionary and maximum buffer allocation.
        /// The wrapper is always <see cref="ZlibWrapper.Zlib"/> because it is the only format that
        /// supports the preset dictionary.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="maxAllocation">Maximum size of the decompression buffer. Must be &gt;= 0.
        /// If zero, maximum size is decided by the <see cref="IByteBufferAllocator"/>.</param>
        public JZlibDecoder(byte[] dictionary, int maxAllocation)
            : base(maxAllocation)
        {
            if (dictionary is null) { CThrowHelper.ThrowArgumentNullException(CExceptionArgument.dictionary); }
            this.dictionary = dictionary;

            int resultCode;
            resultCode = this.z.InflateInit(JZlib.W_ZLIB);
            if (resultCode != JZlib.Z_OK)
            {
                ZlibUtil.Fail(this.z, "initialization failure", resultCode);
            }
        }

        public override bool IsClosed => SharedConstants.False < (uint)Volatile.Read(ref this.finished);

        protected internal override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            if (SharedConstants.False < (uint)Volatile.Read(ref this.finished))
            {
                // Skip data received after finished.
                _ = input.SkipBytes(input.ReadableBytes);
                return;
            }

            int inputLength = input.ReadableBytes;
            if (0u >= (uint)inputLength)
            {
                return;
            }

            try
            {
                // Configure input.
                this.z.avail_in = inputLength;
                if (input.HasArray)
                {
                    this.z.next_in = input.Array;
                    this.z.next_in_index = input.ArrayOffset + input.ReaderIndex;
                }
                else
                {
                    var array = new byte[inputLength];
                    _ = input.GetBytes(input.ReaderIndex, array);
                    this.z.next_in = array;
                    this.z.next_in_index = 0;
                }
                int oldNextInIndex = this.z.next_in_index;

                // Configure output.
                IByteBuffer decompressed = PrepareDecompressBuffer(context, null, inputLength << 1);

                try
                {
                    while (true)
                    {
                        decompressed = PrepareDecompressBuffer(context, decompressed, z.avail_in << 1);
                        this.z.avail_out = decompressed.WritableBytes;
                        this.z.next_out = decompressed.Array;
                        this.z.next_out_index = decompressed.ArrayOffset + decompressed.WriterIndex;
                        int oldNextOutIndex = this.z.next_out_index;

                        // Decompress 'in' into 'out'
                        int resultCode = this.z.Inflate(JZlib.Z_SYNC_FLUSH);
                        int outputLength = this.z.next_out_index - oldNextOutIndex;
                        if (outputLength > 0)
                        {
                            _ = decompressed.SetWriterIndex(decompressed.WriterIndex + outputLength);
                        }

                        if (resultCode == JZlib.Z_NEED_DICT)
                        {
                            if (this.dictionary is null)
                            {
                                ZlibUtil.Fail(this.z, "decompression failure", resultCode);
                            }
                            else
                            {
                                resultCode = this.z.InflateSetDictionary(this.dictionary, this.dictionary.Length);
                                if (resultCode != JZlib.Z_OK)
                                {
                                    ZlibUtil.Fail(this.z, "failed to set the dictionary", resultCode);
                                }
                            }
                            continue;
                        }
                        if (resultCode == JZlib.Z_STREAM_END)
                        {
                            _ = Interlocked.Exchange(ref this.finished, SharedConstants.True); // Do not decode anymore.
                            _ = this.z.InflateEnd();
                            break;
                        }
                        if (resultCode == JZlib.Z_OK)
                        {
                            continue;
                        }
                        if (resultCode == JZlib.Z_BUF_ERROR)
                        {
                            if (this.z.avail_in <= 0)
                            {
                                break;
                            }

                            continue;
                        }
                        //default
                        ZlibUtil.Fail(this.z, "decompression failure", resultCode);
                    }
                }
                finally
                {
                    _ = input.SkipBytes(this.z.next_in_index - oldNextInIndex);
                    if (decompressed.IsReadable())
                    {
                        output.Add(decompressed);
                    }
                    else
                    {
                        _ = decompressed.Release();
                    }
                }
            }
            finally
            {
                this.z.next_in = null;
                this.z.next_out = null;
            }

        }

        protected override void DecompressionBufferExhausted(IByteBuffer buffer)
        {
            _ = Interlocked.Exchange(ref this.finished, SharedConstants.True);
        }
    }
}
