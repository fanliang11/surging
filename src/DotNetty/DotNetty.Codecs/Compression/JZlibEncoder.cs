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
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Buffers;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    public class JZlibEncoder : ZlibEncoder
    {
        readonly int wrapperOverhead;
        readonly Deflater z = new Deflater();

        int finished;
        IChannelHandlerContext ctx;

        public JZlibEncoder() : this(6)
        {
        }

        public JZlibEncoder(int compressionLevel) : this(ZlibWrapper.Zlib, compressionLevel)
        {
        }

        public JZlibEncoder(ZlibWrapper wrapper) : this(wrapper, 6)
        {
        }

        public JZlibEncoder(ZlibWrapper wrapper, int compressionLevel) : this(wrapper, compressionLevel, 15, 8)
        {
        }

        /**
         * Creates a new zlib encoder with the specified {@code compressionLevel},
         * the specified {@code windowBits}, the specified {@code memLevel}, and
         * the specified wrapper.
         *
         * @param compressionLevel
         *        {@code 1} yields the fastest compression and {@code 9} yields the
         *        best compression.  {@code 0} means no compression.  The default
         *        compression level is {@code 6}.
         * @param windowBits
         *        The base two logarithm of the size of the history buffer.  The
         *        value should be in the range {@code 9} to {@code 15} inclusive.
         *        Larger values result in better compression at the expense of
         *        memory usage.  The default value is {@code 15}.
         * @param memLevel
         *        How much memory should be allocated for the internal compression
         *        state.  {@code 1} uses minimum memory and {@code 9} uses maximum
         *        memory.  Larger values result in better and faster compression
         *        at the expense of memory usage.  The default value is {@code 8}
         *
         * @throws CompressionException if failed to initialize zlib
         */
        public JZlibEncoder(ZlibWrapper wrapper, int compressionLevel, int windowBits, int memLevel)
        {
            if (compressionLevel < 0 || compressionLevel > 9) { CThrowHelper.ThrowArgumentException_CompressionLevel(compressionLevel); }
            if (windowBits < 9 || windowBits > 15) { CThrowHelper.ThrowArgumentException_WindowBits(windowBits); }
            if (memLevel < 1 || memLevel > 9) { CThrowHelper.ThrowArgumentException_MemLevel(memLevel); }

            int resultCode = this.z.Init(
                compressionLevel, windowBits, memLevel,
                ZlibUtil.ConvertWrapperType(wrapper));
            if (resultCode != JZlib.Z_OK)
            {
                ZlibUtil.Fail(this.z, "initialization failure", resultCode);
            }

            this.wrapperOverhead = ZlibUtil.WrapperOverhead(wrapper);
        }

        public JZlibEncoder(byte[] dictionary) : this(6, dictionary)
        {
        }

        public JZlibEncoder(int compressionLevel, byte[] dictionary) : this(compressionLevel, 15, 8, dictionary)
        {
        }

        public JZlibEncoder(int compressionLevel, int windowBits, int memLevel, byte[] dictionary)
        {
            if (compressionLevel < 0 || compressionLevel > 9) { CThrowHelper.ThrowArgumentException_CompressionLevel(compressionLevel); }
            if (windowBits < 9 || windowBits > 15) { CThrowHelper.ThrowArgumentException_WindowBits(windowBits); }
            if (memLevel < 1 || memLevel > 9) { CThrowHelper.ThrowArgumentException_MemLevel(memLevel); }

            int resultCode = this.z.DeflateInit(
                    compressionLevel, windowBits, memLevel,
                    JZlib.W_ZLIB); // Default: ZLIB format

            if (resultCode != JZlib.Z_OK)
            {
                ZlibUtil.Fail(this.z, "initialization failure", resultCode);
            }
            else
            {
                resultCode = this.z.DeflateSetDictionary(dictionary, dictionary.Length);
                if (resultCode != JZlib.Z_OK)
                {
                    ZlibUtil.Fail(this.z, "failed to set the dictionary", resultCode);
                }
            }

            this.wrapperOverhead = ZlibUtil.WrapperOverhead(ZlibWrapper.Zlib);
        }

        public override Task CloseAsync() => this.CloseAsync(this.CurrentContext().NewPromise());

        public override Task CloseAsync(IPromise promise)
        {
            var ctx = this.CurrentContext();
            var executor = ctx.Executor;
            if (executor.InEventLoop)
            {
                return this.FinishEncode(ctx, promise);
            }
            else
            {
                var p = ctx.NewPromise();
                executor.Execute((e, p) => InvokeFinishEncode(e, p), this, (p, promise));
                return p.Task;
            }
        }

        static void InvokeFinishEncode(object e, object p)
        {
            var self = (JZlibEncoder)e;
            var promise = ((IPromise, IPromise))p;
            var f = self.FinishEncode(self.CurrentContext(), promise.Item1);
            f.LinkOutcome(promise.Item2);
        }

        IChannelHandlerContext CurrentContext()
        {
            IChannelHandlerContext context = Volatile.Read(ref this.ctx);
            if (context is null)
            {
                throw new InvalidOperationException("not added to a pipeline");
            }

            return context;
        }

        public override bool IsClosed => SharedConstants.False < (uint)Volatile.Read(ref this.finished);

        protected override void Encode(IChannelHandlerContext context, IByteBuffer message, IByteBuffer output)
        {
            if (SharedConstants.False < (uint)Volatile.Read(ref this.finished))
            {
                _ = output.WriteBytes(message);
                return;
            }

            int inputLength = message.ReadableBytes;
            if (inputLength == 0)
            {
                return;
            }

            try
            {
                // Configure input.
                bool inHasArray = message.HasArray;
                this.z.avail_in = inputLength;
                if (inHasArray)
                {
                    this.z.next_in = message.Array;
                    this.z.next_in_index = message.ArrayOffset + message.ReaderIndex;
                }
                else
                {
                    var array = new byte[inputLength];
                    _ = message.GetBytes(message.ReaderIndex, array);
                    this.z.next_in = array;
                    this.z.next_in_index = 0;
                }
                int oldNextInIndex = this.z.next_in_index;

                // Configure output.
                int maxOutputLength = (int)Math.Ceiling(inputLength * 1.001) + 12 + this.wrapperOverhead;
                _ = output.EnsureWritable(maxOutputLength);
                this.z.avail_out = maxOutputLength;
                this.z.next_out = output.Array;
                this.z.next_out_index = output.ArrayOffset + output.WriterIndex;
                int oldNextOutIndex = this.z.next_out_index;

                int resultCode;
                try
                {
                    resultCode = this.z.Deflate(JZlib.Z_SYNC_FLUSH);
                }
                finally
                {
                    _ = message.SkipBytes(this.z.next_in_index - oldNextInIndex);
                }

                if (resultCode != JZlib.Z_OK)
                {
                    ZlibUtil.Fail(this.z, "compression failure", resultCode);
                }

                int outputLength = this.z.next_out_index - oldNextOutIndex;
                if (outputLength > 0)
                {
                    _ = output.SetWriterIndex(output.WriterIndex + outputLength);
                }
            }
            finally
            {
                this.z.next_in = null;
                this.z.next_out = null;
            }
        }

        public override void Close(IChannelHandlerContext context, IPromise promise)
        {
            var completion = this.FinishEncode(context, context.NewPromise());
            _ = completion.ContinueWith(CloseOnCompleteAction, (context, promise), TaskContinuationOptions.ExecuteSynchronously);
            if (!completion.IsCompleted)
            {
                _ = ctx.Executor.Schedule(CloseHandlerContextAction, context, promise, TimeSpan.FromSeconds(10));
            }
        }

        Task FinishEncode(IChannelHandlerContext context, IPromise promise)
        {
            if (SharedConstants.False < (uint)Interlocked.Exchange(ref this.finished, SharedConstants.True))
            {
                _ = promise.TryComplete();
                return promise.Task;
            }

            IByteBuffer footer;
            try
            {
                // Configure input.
                this.z.next_in = ArrayExtensions.ZeroBytes;
                this.z.next_in_index = 0;
                this.z.avail_in = 0;

                // Configure output.
                var output = new byte[32]; // room for ADLER32 + ZLIB / CRC32 + GZIP header
                this.z.next_out = output;
                this.z.next_out_index = 0;
                this.z.avail_out = output.Length;

                // Write the ADLER32 checksum(stream footer).
                int resultCode = this.z.Deflate(JZlib.Z_FINISH);
                if (resultCode != JZlib.Z_OK && resultCode != JZlib.Z_STREAM_END)
                {
                    var err = new CompressionException($"Compression failure ({resultCode}) {this.z.msg}");
                    _ = promise.TrySetException(err);
                    return promise.Task;
                }
                else if (this.z.next_out_index != 0)
                {
                    footer = Unpooled.WrappedBuffer(output, 0, this.z.next_out_index);
                }
                else
                {
                    footer = Unpooled.Empty;
                }
            }
            finally
            {
                _ = this.z.DeflateEnd();

                this.z.next_in = null;
                this.z.next_out = null;
            }

            return context.WriteAndFlushAsync(footer, promise);
        }

        static readonly Action<Task, object> CloseOnCompleteAction = (t, s) => CloseOnComplete(t, s);
        static void CloseOnComplete(Task t, object s)
        {
            var wrapped = ((IChannelHandlerContext, IPromise))s;
            if (t.IsFaulted)
            {
                _ = wrapped.Item1.FireExceptionCaught(t.Exception.InnerException);
            }
            _ = wrapped.Item1.CloseAsync(wrapped.Item2);
        }

        static readonly Action<object, object> CloseHandlerContextAction = (c, p) => CloseHandlerContext(c, p);
        static void CloseHandlerContext(object ctx, object p) => ((IChannelHandlerContext)ctx).CloseAsync((IPromise)p);

        public override void HandlerAdded(IChannelHandlerContext context) => Interlocked.Exchange(ref this.ctx, context);
    }
}
