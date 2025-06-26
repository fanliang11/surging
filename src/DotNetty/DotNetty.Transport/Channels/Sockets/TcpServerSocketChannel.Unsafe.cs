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

namespace DotNetty.Transport.Channels.Sockets
{
    using System;
    using System.Diagnostics;
    using System.Net.Sockets;

    partial class TcpServerSocketChannel<TServerChannel, TChannelFactory>
    {
        public sealed class TcpServerSocketChannelUnsafe : AbstractSocketUnsafe
        {
            public TcpServerSocketChannelUnsafe()
                : base()
            {
            }

            public override void FinishRead(SocketChannelAsyncOperation<TServerChannel, TcpServerSocketChannelUnsafe> operation)
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

                var closed = false;
                var aborted = false;
                Exception exception = null;

                try
                {
                    Socket connectedSocket = null;
                    try
                    {
                        connectedSocket = operation.AcceptSocket;
                        operation.AcceptSocket = null;
                        operation.Validate();

                        var message = PrepareChannel(connectedSocket);

                        connectedSocket = null;
                        ch.ReadPending = false;
                        _ = pipeline.FireChannelRead(message);
                        allocHandle.IncMessagesRead(1);

                        if (!config.IsAutoRead && !ch.ReadPending)
                        {
                            // ChannelConfig.setAutoRead(false) was called in the meantime.
                            // Completed Accept has to be processed though.
                            return;
                        }

                        while (allocHandle.ContinueReading())
                        {
                            connectedSocket = ch.Socket.Accept();
                            message = PrepareChannel(connectedSocket);

                            connectedSocket = null;
                            ch.ReadPending = false;
                            _ = pipeline.FireChannelRead(message);
                            allocHandle.IncMessagesRead(1);
                        }
                    }
                    catch (SocketException ex) when (ex.SocketErrorCode.IsSocketAbortError())
                    {
                        ch.Socket.SafeClose(); // Unbind......
                        exception = ex;
                        aborted = true;
                    }
                    catch (SocketException ex) when (ex.SocketErrorCode == SocketError.WouldBlock)
                    {
                    }
                    catch (SocketException ex)
                    {
                        // socket exceptions here are internal to channel's operation and should not go through the pipeline
                        // especially as they have no effect on overall channel's operation
                        if (Logger.InfoEnabled) Logger.ExceptionOnAccept(ex);
                    }
                    catch (ObjectDisposedException)
                    {
                        closed = true;
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }

                    allocHandle.ReadComplete();
                    _ = pipeline.FireChannelReadComplete();

                    if (exception is object)
                    {
                        // ServerChannel should not be closed even on SocketException because it can often continue
                        // accepting incoming connections. (e.g. too many open files)

                        _ = pipeline.FireExceptionCaught(exception);
                    }

                    if (ch.IsOpen)
                    {
                        if (closed) { Close(VoidPromise()); }
                        else if (aborted) { ch.CloseSafe(); }
                    }
                }
                finally
                {
                    // Check if there is a readPending which was not processed yet.
                    if (!closed && (ch.ReadPending || config.IsAutoRead))
                    {
                        ch.DoBeginRead();
                    }
                }
            }

            ISocketChannel PrepareChannel(Socket socket)
            {
                try
                {
                    return _channel._channelFactory.CreateChannel(_channel, socket);
                }
                catch (Exception ex)
                {
                    var warnEnabled = Logger.WarnEnabled;
                    if (warnEnabled) Logger.FailedToCreateANewChannelFromAcceptedSocket(ex);
                    try
                    {
                        socket.Dispose();
                    }
                    catch (Exception ex2)
                    {
                        if (warnEnabled) Logger.FailedToCloseASocketCleanly(ex2);
                    }
                    throw;
                }
            }
        }
    }
}