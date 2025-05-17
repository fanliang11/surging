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
    using System;

    partial class AbstractChannelHandlerContext
    {
        private static readonly Action<object> InvokeChannelRegisteredAction = ctx => OnInvokeChannelRegistered(ctx);
        private static readonly Action<object> InvokeChannelUnregisteredAction = ctx => OnInvokeChannelUnregistered(ctx);
        private static readonly Action<object> InvokeChannelActiveAction = ctx => OnInvokeChannelActive(ctx);
        private static readonly Action<object> InvokeChannelInactiveAction = ctx => OnInvokeChannelInactive(ctx);

        private static readonly Action<object, object> InvokeUserEventTriggeredAction = (c, e) => OnInvokeUserEventTriggered(c, e);
        private static readonly Action<object, object> InvokeChannelReadAction = (c, e) => OnInvokeChannelRead(c, e);
        private static readonly Action<object, object> InvokeExceptionCaughtAction = (c, e) => OnInvokeExceptionCaught(c, e);

        private static void OnInvokeUserEventTriggered(object ctx, object evt)
        {
            ((AbstractChannelHandlerContext)ctx).InvokeUserEventTriggered(evt);
        }

        private static void OnInvokeChannelRead(object ctx, object msg)
        {
            ((AbstractChannelHandlerContext)ctx).InvokeChannelRead(msg);
        }

        private static void OnInvokeChannelRegistered(object ctx)
        {
            ((AbstractChannelHandlerContext)ctx).InvokeChannelRegistered();
        }

        private static void OnInvokeChannelUnregistered(object ctx)
        {
            ((AbstractChannelHandlerContext)ctx).InvokeChannelUnregistered();
        }

        private static void OnInvokeChannelActive(object ctx)
        {
            ((AbstractChannelHandlerContext)ctx).InvokeChannelActive();
        }

        private static void OnInvokeChannelInactive(object ctx)
        {
            ((AbstractChannelHandlerContext)ctx).InvokeChannelInactive();
        }

        private static void OnInvokeExceptionCaught(object c, object e)
        {
            ((AbstractChannelHandlerContext)c).InvokeExceptionCaught((Exception)e);
        }
    }
}



