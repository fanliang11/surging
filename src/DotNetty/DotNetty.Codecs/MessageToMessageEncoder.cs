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
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs
{
    using System;
    using System.Collections.Generic;
    using DotNetty.Common;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// <see cref="ChannelHandlerAdapter"/> which encodes from one message to an other message
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MessageToMessageEncoder<T> : ChannelHandlerAdapter
    {
        /// <summary>
        /// Returns <c>true</c> if the given message should be handled. If <c>false</c> it will be passed to the next
        /// <see cref="IChannelHandler"/> in the <see cref="IChannelPipeline"/>.
        /// </summary>
        public virtual bool AcceptOutboundMessage(object msg) => msg is T;

        /// <inheritdoc />
        public override void Write(IChannelHandlerContext ctx, object msg, IPromise promise)
        {
            ThreadLocalObjectList output = null;
            try
            {
                if (AcceptOutboundMessage(msg))
                {
                    output = ThreadLocalObjectList.NewInstance();
                    var cast = (T)msg;
                    try
                    {
                        Encode(ctx, cast, output);
                    }
                    finally
                    {
                        _ = ReferenceCountUtil.Release(cast);
                    }

                    if (0u >= (uint)output.Count)
                    {
                        CThrowHelper.ThrowEncoderException_MustProduceAtLeastOneMsg(GetType());
                    }
                }
                else
                {
                    _ = ctx.WriteAsync(msg, promise);
                }
            }
            catch (EncoderException)
            {
                throw;
            }
            catch (Exception ex)
            {
                CThrowHelper.ThrowEncoderException(ex); // todo: we don't have a stack on EncoderException but it's present on inner exception.
            }
            finally
            {
                if (output is object)
                {
                    try
                    {
                        int lastItemIndex = output.Count - 1;
                        if (0u >= (uint)lastItemIndex)
                        {
                            _ = ctx.WriteAsync(output[0], promise);
                        }
                        else if (lastItemIndex > 0)
                        {
                            // Check if we can use a voidPromise for our extra writes to reduce GC-Pressure
                            // See https://github.com/netty/netty/issues/2525
                            if (promise == ctx.VoidPromise())
                            {
                                WriteVoidPromise(ctx, output);
                            }
                            else
                            {
                                WritePromiseCombiner(ctx, output, promise);
                            }
                        }
                    }
                    finally
                    {
                        output.Return(); 
                    }
                }
            }
        }

        static void WriteVoidPromise(IChannelHandlerContext ctx, List<object> output)
        {
            IPromise voidPromise = ctx.VoidPromise();
            for (int i = 0; i < output.Count; i++)
            {
                _ = ctx.WriteAsync(output[i], voidPromise);
            }
        }

        static void WritePromiseCombiner(IChannelHandlerContext ctx, List<object> output, IPromise promise)
        {
            PromiseCombiner combiner = new PromiseCombiner(ctx.Executor);
            for (int i = 0; i < output.Count; i++)
            {
                combiner.Add(ctx.WriteAndFlushAsync(output[i]));
            }
            combiner.Finish(promise);
        }

        /// <summary>
        /// Encode from one message to an other. This method will be called for each written message that can be handled
        /// by this encoder.
        /// </summary>
        /// <param name="context">the <see cref="IChannelHandlerContext"/> which this <see cref="MessageToMessageEncoder{T}"/> belongs to</param>
        /// <param name="message">the message to encode to an other one</param>
        /// <param name="output">the <see cref="List{Object}"/> into which the encoded msg should be added
        /// needs to do some kind of aggregation</param>
        protected internal abstract void Encode(IChannelHandlerContext context, T message, List<object> output);
    }
}