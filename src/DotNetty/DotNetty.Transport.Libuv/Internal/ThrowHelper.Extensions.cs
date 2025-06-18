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

using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DotNetty.Common.Concurrency;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Libuv.Native;

namespace DotNetty.Transport.Libuv
{
    #region -- ExceptionArgument --

    /// <summary>The convention for this enum is using the argument name as the enum name</summary>
    internal enum ExceptionArgument
    {
        s,

        pi,
        fi,
        ts,

        asm,
        key,
        obj,
        str,

        list,
        pool,
        name,
        path,
        item,
        type,
        func,
        task,

        match,
        array,
        other,
        inner,
        types,
        value,
        index,
        count,

        policy,
        offset,
        method,
        buffer,
        source,
        values,
        parent,
        length,
        target,
        member,

        feature,
        manager,
        newSize,
        invoker,
        options,

        assembly,
        capacity,
        fullName,
        typeInfo,
        typeName,
        nThreads,

        defaultFn,
        fieldInfo,
        predicate,

        memberInfo,
        returnType,
        collection,
        expression,
        startIndex,

        directories,
        dirEnumArgs,
        destination,

        valueFactory,
        propertyInfo,
        instanceType,

        attributeType,

        chooserFactory,
        eventLoopGroup,
        parameterTypes,

        assemblyPredicate,
        qualifiedTypeName,

        includedAssemblies,
    }

    #endregion

    #region -- ExceptionResource --

    /// <summary>The convention for this enum is using the resource name as the enum name</summary>
    internal enum ExceptionResource
    {
    }

    #endregion

    partial class ThrowHelper
    {
        #region -- ArgumentException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_Positive(int value, ExceptionArgument argument)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"{GetArgumentName(argument)}: {value} (expected: > 0)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_Positive(long value, ExceptionArgument argument)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"{GetArgumentName(argument)}: {value} (expected: > 0)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_PositiveOrZero(int value, ExceptionArgument argument)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"{GetArgumentName(argument)}: {value} (expected: >= 0)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_PositiveOrZero(long value, ExceptionArgument argument)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"{GetArgumentName(argument)}: {value} (expected: >= 0)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_RegChannel()
        {
            throw GetArgumentException();

            static ArgumentException GetArgumentException()
            {
                return new ArgumentException($"channel must be of {typeof(INativeChannel)}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_PipeName()
        {
            throw GetArgumentException();

            static ArgumentException GetArgumentException()
            {
                return new ArgumentException("Pipe name is required for worker event loop", "parent");
            }
        }

        #endregion

        #region -- InvalidOperationException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Task ThrowInvalidOperationException(IntPtr loopHandle)
        {
            throw GetInvalidOperationException();
            InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException($"Loop {loopHandle} does not exist");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_ExecutionState(int executionState)
        {
            throw GetInvalidOperationException();
            InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException($"Invalid state {executionState}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_ExecutionState0(int executionState)
        {
            throw GetInvalidOperationException();
            InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException($"Invalid {nameof(LoopExecutor)} state {executionState}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_ExpectingTcpHandle(uv_handle_type type)
        {
            throw GetInvalidOperationException();
            InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException($"Expecting tcp handle, {type} not supported");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_Dispatch()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("No pipe connections to dispatch handles.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_ConnAttempt()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("connection attempt already made");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_TcpHandle()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("Tcp handle not intialized");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static uint ThrowInvalidOperationException_Dispatch(AddressFamily addressFamily)
        {
            throw GetInvalidOperationException();
            InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException($"Address family : {addressFamily} platform : {RuntimeInformation.OSDescription} not supported");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_FailedToCreateChildEventLoop(Exception ex)
        {
            throw GetInvalidOperationException();
            InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("failed to create a child event loop.", ex);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_CreateChild(Exception ex)
        {
            throw GetInvalidOperationException();
            InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException($"Failed to create a child {nameof(WorkerEventLoop)}.", ex.Unwrap());
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_HandleNotInit()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("tcpListener handle not intialized");
            }
        }

        #endregion

        #region -- SocketException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowSocketException(int errorCode)
        {
            throw GetSocketException();
            SocketException GetSocketException()
            {
                return new SocketException(errorCode);
            }
        }

        #endregion

        #region -- ChannelException --

        internal static ChannelException GetChannelException(OperationException ex)
        {
            return new ChannelException(ex);
        }

        internal static ChannelException GetChannelException_FailedToWrite(OperationException error)
        {
            return new ChannelException("Failed to write", error);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowChannelException(Exception exc)
        {
            throw GetChannelException();
            ChannelException GetChannelException()
            {
                return new ChannelException(exc);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowChannelException(ChannelOption option)
        {
            throw GetChannelException();
            ChannelException GetChannelException()
            {
                return new ChannelException($"Invalid channel option {option}");
            }
        }

        #endregion

        #region -- TimeoutException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowTimeoutException(string pipeName)
        {
            throw GetException();
            TimeoutException GetException()
            {
                return new TimeoutException($"Connect to dispatcher pipe {pipeName} timed out.");
            }
        }

        #endregion

        #region -- NotSupportedException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static NotSupportedException GetNotSupportedException(IPEndPoint endPoint)
        {
            return new NotSupportedException($"End point {endPoint} is not supported, expecting InterNetwork/InterNetworkV6.");
        }

        #endregion

        #region -- ConnectTimeoutException --

        internal static ConnectTimeoutException GetConnectTimeoutException(OperationException error)
        {
            return new ConnectTimeoutException(error.ToString());
        }

        #endregion

        #region -- ClosedChannelException --

        internal static ClosedChannelException GetClosedChannelException()
        {
            return new ClosedChannelException();
        }

        internal static ClosedChannelException GetClosedChannelException_FailedToWrite(Exception ex)
        {
            return new ClosedChannelException("Failed to write", ex);
        }

        #endregion
    }
}
