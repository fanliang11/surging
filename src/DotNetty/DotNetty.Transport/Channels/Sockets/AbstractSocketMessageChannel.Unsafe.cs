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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Transport.Channels.Sockets
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    partial class AbstractSocketMessageChannel<TChannel, TUnsafe>
    {
        public class SocketMessageUnsafe : AbstractSocketUnsafe
        {
            readonly List<object> _readBuf = new List<object>();

            public SocketMessageUnsafe()
                : base()
            {
            }

            public override void FinishRead(SocketChannelAsyncOperation<TChannel, TUnsafe> operation)
            {
                Debug.Assert(_channel.EventLoop.InEventLoop);

                var ch = _channel;
                if (0u >= (uint)(ch.ResetState(StateFlags.ReadScheduled) & StateFlags.Active))
                {
                    return; // read was signaled as a result of channel closure
                }
                IChannelConfiguration config = ch.Configuration;

                IChannelPipeline pipeline = ch.Pipeline;
                IRecvByteBufAllocatorHandle allocHandle = ch.Unsafe.RecvBufAllocHandle;
                allocHandle.Reset(config);

                bool closed = false;
                Exception exception = null;
                try
                {
                    try
                    {
                        do
                        {
                            int localRead = ch.DoReadMessages(_readBuf);
                            uint uLocalRead = (uint)localRead;
                            if (0u >= uLocalRead)
                            {
                                break;
                            }
                            if (uLocalRead > SharedConstants.TooBigOrNegative) // localRead < 0
                            {
                                closed = true;
                                break;
                            }

                            allocHandle.IncMessagesRead(localRead);
                        }
                        while (allocHandle.ContinueReading());
                    }
                    catch (Exception t)
                    {
                        exception = t;
                    }
                    int size = _readBuf.Count;
                    for (int i = 0; i < size; i++)
                    {
                        ch.ReadPending = false;
                        _ = pipeline.FireChannelRead(_readBuf[i]);
                    }

                    _readBuf.Clear();
                    allocHandle.ReadComplete();
                    _ = pipeline.FireChannelReadComplete();

                    if (exception is object)
                    {
                        closed = ch.CloseOnReadError(exception);

                        _ = pipeline.FireExceptionCaught(exception);
                    }

                    if (closed)
                    {
                        ch._inputShutdown = true;
                        if (ch.IsOpen)
                        {
                            Close(VoidPromise());
                        }
                    }
                }
                finally
                {
                    // Check if there is a readPending which was not processed yet.
                    // This could be for two reasons:
                    // /// The user called Channel.read() or ChannelHandlerContext.read() in channelRead(...) method
                    // /// The user called Channel.read() or ChannelHandlerContext.read() in channelReadComplete(...) method
                    //
                    // See https://github.com/netty/netty/issues/2254
                    if (!closed && (ch.ReadPending || config.IsAutoRead))
                    {
                        ch.DoBeginRead();
                    }
                }
            }
        }
    }
}