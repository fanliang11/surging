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

namespace DotNetty.Transport.Channels
{
    using DotNetty.Common.Utilities;

    public abstract class SimpleUserEventChannelHandler<I> : ChannelHandlerAdapter
    {
        readonly bool _autoRelease;

        protected SimpleUserEventChannelHandler()
            : this(true)
        {
        }

        protected SimpleUserEventChannelHandler(bool autoRelease)
        {
            _autoRelease = autoRelease;
        }

        public bool AcceptEvent(object msg) => msg is I;

        public override void UserEventTriggered(IChannelHandlerContext ctx, object evt)
        {
            bool release = true;
            try
            {
                if (AcceptEvent(evt))
                {
                    I ievt = (I)evt;
                    EventReceived(ctx, ievt);
                }
                else
                {
                    release = false;
                    _ = ctx.FireUserEventTriggered(evt);
                }
            }
            finally
            {
                if (_autoRelease && release)
                {
                    _ = ReferenceCountUtil.Release(evt);
                }
            }
        }

        protected abstract void EventReceived(IChannelHandlerContext ctx, I evt);
    }
}
