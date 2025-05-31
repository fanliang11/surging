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

namespace DotNetty.Handlers
{
    using System;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Channels.Sockets;
    using DotNetty.Transport.Libuv.Native;

    public sealed class ServerChannelRebindHandler : ChannelHandlerAdapter
    {
        private readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<ServerChannelRebindHandler>();

        private readonly Action _doBindAction;
        private readonly int _delaySeconds;

        public ServerChannelRebindHandler(Action doBindAction) : this(doBindAction, 2) { }

        public ServerChannelRebindHandler(Action doBindAction, int delaySeconds)
        {
            if (doBindAction is null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.doBindAction);
            if (delaySeconds <= 0) { delaySeconds = 2; }

            _doBindAction = doBindAction;
            _delaySeconds = delaySeconds;
        }

        /// <inheritdoc />
        public override bool IsSharable => true;

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            if (Logger.WarnEnabled)
            {
                Logger.Warn($"Channel {context.Channel} caught exception", exception);
            }
            switch (exception)
            {
                case SocketException se when se.SocketErrorCode.IsSocketAbortError():
                case OperationException oe when oe.ErrorCode.IsConnectionAbortError():
                case ChannelException ce when (ce.InnerException is OperationException exc && exc.ErrorCode.IsConnectionAbortError()):
                    DoBind();
                    break;

                default:
                    context.FireExceptionCaught(exception);
                    break;
            }
        }

        private async void DoBind()
        {
            await Task.Delay(TimeSpan.FromSeconds(_delaySeconds));
            _doBindAction();
        }
    }
}
