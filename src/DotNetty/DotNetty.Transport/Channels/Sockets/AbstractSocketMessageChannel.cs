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
    using System.Net.Sockets;

    /// <summary>
    /// <see cref="AbstractSocketChannel{TChannel, TUnsafe}"/> base class for <see cref="IChannel"/>s that operate on messages.
    /// </summary>
    public abstract partial class AbstractSocketMessageChannel<TChannel, TUnsafe> : AbstractSocketChannel<TChannel, TUnsafe>
        where TChannel : AbstractSocketMessageChannel<TChannel, TUnsafe>
        where TUnsafe : AbstractSocketMessageChannel<TChannel, TUnsafe>.SocketMessageUnsafe, new()
    {
        private bool _inputShutdown;

        /// <summary>
        /// Creates a new <see cref="AbstractSocketMessageChannel{TChannel, TUnsafe}"/> instance.
        /// </summary>
        /// <param name="parent">The parent <see cref="IChannel"/>. Pass <c>null</c> if there's no parent.</param>
        /// <param name="socket">The <see cref="Socket"/> used by the <see cref="IChannel"/> for communication.</param>
        protected AbstractSocketMessageChannel(IChannel parent, Socket socket)
            : base(parent, socket)
        {
        }

        protected override void DoBeginRead()
        {
            if (_inputShutdown) { return; }
            base.DoBeginRead();
        }

        protected override void DoWrite(ChannelOutboundBuffer input)
        {
            while (true)
            {
                var msg = input.Current;
                if (msg is null)
                {
                    break;
                }
                try
                {
                    var done = false;
                    for (int i = Configuration.WriteSpinCount - 1; i >= 0; i--)
                    {
                        if (DoWriteMessage(msg, input))
                        {
                            done = true;
                            break;
                        }
                    }

                    if (done)
                    {
                        _ = input.Remove();
                    }
                    else
                    {
                        // Did not write all messages.
                        ScheduleMessageWrite(msg);
                        break;
                    }
                }
                catch (SocketException e)
                {
                    if (ContinueOnWriteError)
                    {
                        _ = input.Remove(e);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        protected abstract void ScheduleMessageWrite(object message);

        /// <summary>
        /// Returns <c>true</c> if we should continue the write loop on a write error.
        /// </summary>
        protected virtual bool ContinueOnWriteError => false;

        protected virtual bool CloseOnReadError(Exception cause)
        {
            if (!IsActive)
            {
                // If the channel is not active anymore for whatever reason we should not try to continue reading.
                return true;
            }
            if (cause is SocketException asSocketException && asSocketException.SocketErrorCode != SocketError.TryAgain) // todo: other conditions for not closing message-based socket?
            {
                // ServerChannel should not be closed even on IOException because it can often continue
                // accepting incoming connections. (e.g. too many open files)
                return !(this is IServerChannel);
            }
            return true;
        }

        /// <summary>
        /// Reads messages into the given list and returns the amount which was read.
        /// </summary>
        /// <param name="buf">The list into which message objects should be inserted.</param>
        /// <returns>The number of messages which were read.</returns>
        protected abstract int DoReadMessages(List<object> buf);

        /// <summary>
        /// Writes a message to the underlying <see cref="IChannel"/>.
        /// </summary>
        /// <param name="msg">The message to be written.</param>
        /// <param name="input">The destination channel buffer for the message.</param>
        /// <returns><c>true</c> if the message was successfully written, otherwise <c>false</c>.</returns>
        protected abstract bool DoWriteMessage(object msg, ChannelOutboundBuffer input);
    }
}