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

namespace DotNetty.Transport.Libuv
{
    using System;
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Libuv.Native;

    internal interface IServerNativeUnsafe
    {
        void Accept(RemoteConnection connection);

        void Accept(NativeHandle handle);
    }

    partial class TcpServerChannel<TServerChannel, TChannelFactory>
    {
        public sealed class TcpServerChannelUnsafe : NativeChannelUnsafe, IServerNativeUnsafe
        {
            static readonly Action<object, object> AcceptAction = (u, e) => OnAccept(u, e);

            public TcpServerChannelUnsafe() : base()
            {
            }

            public override IntPtr UnsafeHandle => _channel._tcpListener.Handle;

            // Connection callback from Libuv thread
            void IServerNativeUnsafe.Accept(RemoteConnection connection)
            {
                var ch = _channel;
                NativeHandle client = connection.Client;

                var connError = connection.Error;
                // If the AutoRead is false, reject the connection
                if (!ch._config.IsAutoRead || connError is object)
                {
                    if (connError is object)
                    {
                        if (Logger.InfoEnabled) Logger.AcceptClientConnectionFailed(connError);
                        _ = _channel.Pipeline.FireExceptionCaught(connError);
                    }
                    try
                    {
                        client?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        if (Logger.WarnEnabled) Logger.FailedToDisposeAClientConnection(ex);
                    }
                    finally
                    {
                        client = null;
                    }
                }
                if (client is null)
                {
                    return;
                }

                if (ch.EventLoop is DispatcherEventLoop dispatcher)
                {
                    // Dispatch handle to other Libuv loop/thread
                    dispatcher.Dispatch(client);
                }
                else
                {
                    Accept((Tcp)client);
                }
            }

            // Called from other Libuv loop/thread received tcp handle from pipe
            void IServerNativeUnsafe.Accept(NativeHandle handle)
            {
                var ch = _channel;
                if (ch.EventLoop.InEventLoop)
                {
                    Accept((Tcp)handle);
                }
                else
                {
                    _channel.EventLoop.Execute(AcceptAction, this, handle);
                }
            }

            void Accept(Tcp tcp)
            {
                var ch = _channel;
                IChannelPipeline pipeline = ch.Pipeline;
                IRecvByteBufAllocatorHandle allocHandle = RecvBufAllocHandle;

                bool closed = false;
                Exception exception = null;
                try
                {
                    var tcpChannel = ch._channelFactory.CreateChannel(ch, tcp);
                    _ = ch.Pipeline.FireChannelRead(tcpChannel);
                    allocHandle.IncMessagesRead(1);
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
                    _ = pipeline.FireExceptionCaught(exception);
                }

                if (closed && ch.IsOpen)
                {
                    CloseSafe();
                }
            }

            private static void OnAccept(object u, object e)
            {
                ((TcpServerChannelUnsafe)u).Accept((Tcp)e);
            }
        }
    }
}
