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
    using System.Net;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using DotNetty.Common.Concurrency;

    partial class AbstractChannelHandlerContext
    {
        protected internal static class SkipFlags
        {
            // Using to mask which methods must be called for a ChannelHandler.
            public const int ExceptionCaught = 1;
            public const int ChannelRegistered = 1 << 1;
            public const int ChannelUnregistered = 1 << 2;
            public const int ChannelActive = 1 << 3;
            public const int ChannelInactive = 1 << 4;
            public const int ChannelRead = 1 << 5;
            public const int ChannelReadComplete = 1 << 6;
            public const int UserEventTriggered = 1 << 7;
            public const int ChannelWritabilityChanged = 1 << 8;
            public const int Bind = 1 << 9;
            public const int Connect = 1 << 10;
            public const int Disconnect = 1 << 11;
            public const int Close = 1 << 12;
            public const int Deregister = 1 << 13;
            public const int Read = 1 << 14;
            public const int Write = 1 << 15;
            public const int Flush = 1 << 16;

            public const int WriteAndFlush = Write | Flush;

            public const int OnlyInbound = ChannelRegistered |
                ChannelUnregistered | ChannelActive | ChannelInactive | ChannelRead |
                ChannelReadComplete | UserEventTriggered | ChannelWritabilityChanged;
            public const int AllInbound = ExceptionCaught | OnlyInbound;

            public const int OnlyOutbound = Bind | Connect | Disconnect |
                Close | Deregister | Read | Write | Flush;
            public const int AllOutbound = ExceptionCaught | OnlyOutbound;
        }

        private static readonly ConditionalWeakTable<Type, Tuple<int>> SkipTable = new ConditionalWeakTable<Type, Tuple<int>>();

        protected static int GetSkipPropagationFlags(IChannelHandler handler)
        {
            Tuple<int> skipDirection = SkipTable.GetValue(
                handler.GetType(),
                handlerType => Tuple.Create(CalculateSkipPropagationFlags(handlerType)));

            return skipDirection?.Item1 ?? 0;
        }

        protected static int CalculateSkipPropagationFlags(Type handlerType)
        {
            int flags = SkipFlags.ExceptionCaught;

            // this method should never throw

            flags |= SkipFlags.AllInbound;

            if (IsSkippable(handlerType, nameof(IChannelHandler.ChannelRegistered)))
            {
                flags &= ~SkipFlags.ChannelRegistered;
            }
            if (IsSkippable(handlerType, nameof(IChannelHandler.ChannelUnregistered)))
            {
                flags &= ~SkipFlags.ChannelUnregistered;
            }
            if (IsSkippable(handlerType, nameof(IChannelHandler.ChannelActive)))
            {
                flags &= ~SkipFlags.ChannelActive;
            }
            if (IsSkippable(handlerType, nameof(IChannelHandler.ChannelInactive)))
            {
                flags &= ~SkipFlags.ChannelInactive;
            }
            if (IsSkippable(handlerType, nameof(IChannelHandler.ChannelRead), typeof(object)))
            {
                flags &= ~SkipFlags.ChannelRead;
            }
            if (IsSkippable(handlerType, nameof(IChannelHandler.ChannelReadComplete)))
            {
                flags &= ~SkipFlags.ChannelReadComplete;
            }
            if (IsSkippable(handlerType, nameof(IChannelHandler.ChannelWritabilityChanged)))
            {
                flags &= ~SkipFlags.ChannelWritabilityChanged;
            }
            if (IsSkippable(handlerType, nameof(IChannelHandler.UserEventTriggered), typeof(object)))
            {
                flags &= ~SkipFlags.UserEventTriggered;
            }

            flags |= SkipFlags.AllOutbound;

            if (IsSkippable(handlerType, nameof(IChannelHandler.BindAsync), typeof(EndPoint)))
            {
                flags &= ~SkipFlags.Bind;
            }
            if (IsSkippable(handlerType, nameof(IChannelHandler.ConnectAsync), typeof(EndPoint), typeof(EndPoint)))
            {
                flags &= ~SkipFlags.Connect;
            }
            if (IsSkippable(handlerType, nameof(IChannelHandler.Disconnect), typeof(IPromise)))
            {
                flags &= ~SkipFlags.Disconnect;
            }
            if (IsSkippable(handlerType, nameof(IChannelHandler.Close), typeof(IPromise)))
            {
                flags &= ~SkipFlags.Close;
            }
            if (IsSkippable(handlerType, nameof(IChannelHandler.Deregister), typeof(IPromise)))
            {
                flags &= ~SkipFlags.Deregister;
            }
            if (IsSkippable(handlerType, nameof(IChannelHandler.Read)))
            {
                flags &= ~SkipFlags.Read;
            }
            if (IsSkippable(handlerType, nameof(IChannelHandler.Write), typeof(object), typeof(IPromise)))
            {
                flags &= ~SkipFlags.Write;
            }
            if (IsSkippable(handlerType, nameof(IChannelHandler.Flush)))
            {
                flags &= ~SkipFlags.Flush;
            }

            if (IsSkippable(handlerType, nameof(IChannelHandler.ExceptionCaught), typeof(Exception)))
            {
                flags &= ~SkipFlags.ExceptionCaught;
            }

            return flags;
        }

        protected static bool IsSkippable(Type handlerType, string methodName) => IsSkippable(handlerType, methodName, Type.EmptyTypes);

        protected static bool IsSkippable(Type handlerType, string methodName, params Type[] paramTypes)
        {
            if (paramTypes is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.paramTypes); }

            var paramTypeLength = paramTypes.Length;
            var newParamTypes = new Type[paramTypeLength + 1];
            newParamTypes[0] = typeof(IChannelHandlerContext);
            if ((uint)paramTypeLength > 0U)
            {
                Array.Copy(paramTypes, 0, newParamTypes, 1, paramTypeLength);
            }
            return handlerType.GetMethod(methodName, newParamTypes).GetCustomAttribute<SkipAttribute>(false) is object;
        }
    }
}
