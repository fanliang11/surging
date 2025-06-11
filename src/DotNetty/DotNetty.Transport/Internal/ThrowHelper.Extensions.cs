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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DotNetty.Common.Concurrency;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Local;
using DotNetty.Transport.Channels.Pool;
using DotNetty.Transport.Channels.Sockets;

namespace DotNetty.Transport
{
    #region -- ExceptionArgument --

    /// <summary>The convention for this enum is using the argument name as the enum name</summary>
    internal enum ExceptionArgument
    {
        s,
        pi,
        e1,
        e2,
        fi,
        id,
        ts,

        asm,
        buf,
        evt,
        msg,
        ctx,
        pos,
        key,
        obj,
        str,

        name,
        item,
        list,
        path,
        pool,
        func,
        type,
        file,
        task,

        bytes,
        types,
        inner,
        other,
        value,
        array,
        count,
        cause,
        group,
        match,
        index,

        holder,
        socket,
        config,
        buffer,
        method,
        source,
        values,
        policy,
        offset,
        target,
        member,
        length,
        option,

        options,
        message,
        matcher,
        futures,
        channel,
        minimum,
        promise,
        content,
        handler,
        feature,
        manager,
        invoker,
        newSize,
        inbound,

        capacity,
        childKey,
        handlers,
        matchers,
        outbound,
        typeInfo,
        assembly,
        resolver,
        pipeline,
        typeName,
        fullName,

        bootstrap,
        eventLoop,
        allocator,
        fieldInfo,
        predicate,
        recipient,
        processor,
        defaultFn,

        exceptions,
        collection,
        startIndex,
        expression,
        childGroup,
        memberInfo,
        newHandler,
        bufferSize,
        returnType,
        paramTypes,

        childOption,
        directories,
        dirEnumArgs,
        destination,
        unknownSize,
        propertyInfo,
        valueFactory,
        childHandler,
        localAddress,
        instanceType,

        attributeType,
        healthChecker,
        remoteAddress,
        tailTaskQueue,

        eventLoopCount,
        initialization,
        inboundHandler,
        parameterTypes,
        channelFactory,

        outboundHandler,
        estimatorHandle,

        aggregatePromise,
        eventLoopFactory,
        networkInterface,

        qualifiedTypeName,
        assemblyPredicate,

        includedAssemblies,

        initializationAction,

        defaultMaxMessagesPerRead,
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
        internal static int ThrowArgumentException_PositiveOrOne(int value, ExceptionArgument argument)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"{GetArgumentName(argument)}: {value} (expected: >= 1)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_FileRegionPosition(long position)
        {
            throw GetArgumentException();
            ArgumentException GetArgumentException()
            {
                return new ArgumentException("position must be >= 0 but was " + position);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_FileRegionCount(long count)
        {
            throw GetArgumentException();
            ArgumentException GetArgumentException()
            {
                return new ArgumentException("count must be >= 0 but was " + count);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_Excs()
        {
            throw GetArgumentException();

            static ArgumentException GetArgumentException()
            {
                return new ArgumentException("excetpions must be not empty.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_TheLastOpCompleted()
        {
            throw GetArgumentException();

            static ArgumentException GetArgumentException()
            {
                return new ArgumentException("The last operation completed on the socket was not expected");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_Action()
        {
            throw GetArgumentException();

            static ArgumentException GetArgumentException()
            {
                return new ArgumentException("action");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static bool FromArgumentException_ChannelWasNotAcquiredFromPool(IChannel channel)
        {
            throw GetArgumentException();
            ArgumentException GetArgumentException()
            {
                return new ArgumentException($"Channel {channel} was not acquired from this ChannelPool");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_MustBeGreaterThanZero(TimeSpan value, ExceptionArgument argument)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"{GetArgumentName(argument)} must be greater than 0: {value}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_AcquireTimeoutMillis(TimeSpan acquireTimeout)
        {
            throw GetArgumentException();
            ArgumentException GetArgumentException()
            {
                return new ArgumentException($"acquireTimeoutMillis: {acquireTimeout} (expected: >= 0)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_MaxPendingAcquires(int maxPendingAcquires)
        {
            throw GetArgumentException();
            ArgumentException GetArgumentException()
            {
                return new ArgumentException($"maxPendingAcquires: {maxPendingAcquires} (expected: >= 1)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_MaxConnections(int maxConnections)
        {
            throw GetArgumentException();
            ArgumentException GetArgumentException()
            {
                return new ArgumentException($"maxConnections: {maxConnections} (expected: >= 1)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_DuplicateHandler(string name)
        {
            throw GetArgumentException();
            ArgumentException GetArgumentException()
            {
                return new ArgumentException("Duplicate handler name: " + name, nameof(name));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_Context<T>()
        {
            throw GetArgumentException();

            static ArgumentException GetArgumentException()
            {
                return new ArgumentException($"Handler of type `{typeof(T).Name}` could not be found in the pipeline.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_Context(string name)
        {
            throw GetArgumentException();
            ArgumentException GetArgumentException()
            {
                return new ArgumentException($"Handler with a name `{name}` could not be found in the pipeline.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_Context(IChannelHandler handler)
        {
            throw GetArgumentException();
            ArgumentException GetArgumentException()
            {
                return new ArgumentException($"Handler of type `{handler.GetType().Name}` could not be found in the pipeline.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_Position(long pos, long count)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"position out of range: {pos} (expected: 0 - {count - 1})");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_VoidPromiseIsNotAllowed()
        {
            throw GetException();

            static ArgumentException GetException()
            {
                return new ArgumentException("Void promise is not allowed for this operation");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_PromiseAlreadyCompleted(IPromise promise)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"Promise {promise} already completed");
            }
        }

        #endregion

        #region -- ArgumentOutOfRangeException --

        #endregion

        #region -- InvalidOperationException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_ChannelPoolFull()
        {
            throw SimpleChannelPool.FullException;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static ValueTask<IChannel> FromInvalidOperationException_TooManyOutstandingAcquireOperations()
        {
            return new ValueTask<IChannel>(TaskUtil.FromException<IChannel>(FixedChannelPool.FullException));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_PoolClosedOnReleaseException()
        {
            throw FixedChannelPool.PoolClosedOnReleaseException;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static ValueTask<IChannel> FromInvalidOperationException_PoolClosedOnAcquireException()
        {
            return new ValueTask<IChannel>(TaskUtil.FromException<IChannel>(FixedChannelPool.PoolClosedOnAcquireException));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_ChildGroupSetAlready()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("childGroup set already");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_HandlerNotSet()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("handler not set");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_RemoteAddrNotSet()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("remoteAddress not set");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_LocalAddrMustBeSetBeforehand()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("localAddress must be set beforehand.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_ChannelOrFactoryNotSet()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("channel or channelFactory not set");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_GroupNotSet()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("group not set");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_GroupHasAlreadyBeenSet()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("group has already been set.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_ChannelFactoryHasAlreadyBeenSet()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("channelFactory has already been set.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_ChildHandlerNotYet()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("childHandler not set");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_HandlerNotAddedToPipeYet()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("handler not added to pipeline yet");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_ConnAttemptAlreadyMade()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("connection attempt already made");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_Close0()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("close() must be invoked after the channel is closed.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_Close1()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("close() must be invoked after all flushed writes are handled.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_ChannelNotReg()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("channel not registered to an event loop");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_Pipeline()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("Pipeline is empty.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static int FromInvalidOperationException_WritabilityMask(int index)
        {
            throw GetInvalidOperationException();
            InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("index: " + index + " (expected: 1~31)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_EnsureNotSharable(object handlerAdapter)
        {
            throw GetInvalidOperationException();
            InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException($"ChannelHandler {StringUtil.SimpleClassName(handlerAdapter)} is not allowed to be shared");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_InitCannotBeInvokedIf(object msg)
        {
            throw GetInvalidOperationException();
            InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException($"init() can not be invoked if {StringUtil.SimpleClassName(msg)} was constructed with non-default constructor.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_InitMustBeInvokedBefore(object msg)
        {
            throw GetInvalidOperationException();
            InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException($"Init() must be invoked before being added to a {nameof(IChannelPipeline)} if {StringUtil.SimpleClassName(msg)} was constructed with the default constructor.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Task FromInvalidOperationException_RegisteredToEventLoopAlready()
        {
            return TaskUtil.FromException(GetInvalidOperationException());

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("registered to an event loop already");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Task FromInvalidOperationException_IncompatibleEventLoopType(IEventLoop eventLoop)
        {
            return TaskUtil.FromException(GetInvalidOperationException());
            InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("incompatible event loop type: " + eventLoop.GetType().Name);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_CoalescingBufferQueuePending(Exception pending)
        {
            throw GetInvalidOperationException();
            InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException(pending.Message, pending);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_BufferQueueLengthOverflow(int readableBytes, int increment)
        {
            throw GetInvalidOperationException();
            InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("buffer queue length overflow: " + readableBytes + " + " + increment);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_UnexpectedType_expecting_DatagramPacket_IAddressedEnvelope(object message)
        {
            throw GetInvalidOperationException();
            InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException(
                    $"Unexpected type: {message.GetType().FullName}, expecting DatagramPacket or IAddressedEnvelope.");
            }
        }

        #endregion

        #region -- ChannelException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowChannelException()
        {
            throw GetException();

            static ChannelException GetException()
            {
                return new ChannelException();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowChannelException_AlreadyBound()
        {
            throw GetException();

            static ChannelException GetException()
            {
                return new ChannelException("already bound");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowChannelException_UnsupportedAddrType(EndPoint localAddress)
        {
            throw GetException();
            ChannelException GetException()
            {
                return new ChannelException($"unsupported address type: {localAddress?.GetType()}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowChannelException_AddrAlreadyInUseBy(IChannel result)
        {
            throw GetException();
            ChannelException GetException()
            {
                return new ChannelException($"address already in use by: {result}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowChannelException_FailedToEnterNonBlockingMode(SocketException ex)
        {
            throw GetException();
            ChannelException GetException()
            {
                return new ChannelException("Failed to enter non-blocking mode.", ex);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static bool ThrowChannelException_Get_Bool(ObjectDisposedException ex)
        {
            throw GetException();
            ChannelException GetException()
            {
                return new ChannelException(ex);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static bool ThrowChannelException_Get_Bool(SocketException ex)
        {
            throw GetException();
            ChannelException GetException()
            {
                return new ChannelException(ex);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static int FromChannelException_Get_Int(ObjectDisposedException ex)
        {
            throw GetException();
            ChannelException GetException()
            {
                return new ChannelException(ex);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static int FromChannelException_Get_Int(SocketException ex)
        {
            throw GetException();
            ChannelException GetException()
            {
                return new ChannelException(ex);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowChannelException_Set(ObjectDisposedException ex)
        {
            throw GetException();
            ChannelException GetException()
            {
                return new ChannelException(ex);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowChannelException_Set(SocketException ex)
        {
            throw GetException();
            ChannelException GetException()
            {
                return new ChannelException(ex);
            }
        }

        #endregion

        #region -- ClosedChannelException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Task FromClosedChannelException()
        {
            return TaskUtil.FromException(GetClosedChannelException());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ClosedChannelException GetClosedChannelException()
        {
            return new ClosedChannelException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ClosedChannelException GetClosedChannelException_FailedToWrite(Exception ex)
        {
            return new ClosedChannelException("Failed to write", ex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowDoWriteClosedChannelException()
        {
            throw LocalChannel.DoWriteClosedChannelException;
        }

        #endregion

        #region -- ChannelPipelineException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ChannelPipelineException GetChannelPipelineException_HandlerAddedThrowRemovedExc(AbstractChannelHandlerContext ctx, Exception ex)
        {
            return new ChannelPipelineException($"{ctx.Handler.GetType().Name}.HandlerAdded() has thrown an exception; removed.", ex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ChannelPipelineException GetChannelPipelineException_HandlerAddedThrowAlsoFailedToRemovedExc(AbstractChannelHandlerContext ctx, Exception ex)
        {
            return new ChannelPipelineException($"{ctx.Handler.GetType().Name}.HandlerAdded() has thrown an exception; also failed to remove.", ex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ChannelPipelineException GetChannelPipelineException_HandlerRemovedThrowExc(AbstractChannelHandlerContext ctx, Exception ex)
        {
            return new ChannelPipelineException($"{ctx.Handler.GetType().Name}.HandlerRemoved() has thrown an exception.", ex);
        }

        #endregion

        #region -- NotSupportedException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Task FromNotSupportedException()
        {
            return TaskUtil.FromException(new NotSupportedException());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static NotSupportedException GetNotSupportedException()
        {
            return new NotSupportedException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static NotSupportedException GetNotSupportedException_Socket_address_family(AddressFamily addressFamily)
        {
            return new NotSupportedException($"Socket address family {addressFamily} not supported, expecting InterNetwork or InterNetworkV6");
        }

        #endregion

        #region -- Others --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static ChannelOutputShutdownException GetChannelOutputShutdownException(Exception cause = null)
        {
            const string errMsg = "Channel output shutdown";
            return cause is object ? new ChannelOutputShutdownException(errMsg, cause) : new ChannelOutputShutdownException(errMsg);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowNotImplementedException_OnlyIByteBufferImpl()
        {
            throw GetNotImplementedException();

            static NotImplementedException GetNotImplementedException()
            {
                return new NotImplementedException("Only IByteBuffer implementations backed by array are supported.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowChannelPipelineException(ChannelHandlerAdapter h)
        {
            throw GetChannelPipelineException();
            ChannelPipelineException GetChannelPipelineException()
            {
                return new ChannelPipelineException(
                        h.GetType().Name + " is not a @Sharable handler, so can't be added or removed multiple times.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIllegalReferenceCountException(int count)
        {
            throw GetInvalidOperationException();

            static IllegalReferenceCountException GetInvalidOperationException()
            {
                return new IllegalReferenceCountException(0);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowNotYetConnectedException()
        {
            throw GetException();

            static NotYetConnectedException GetException()
            {
                return new NotYetConnectedException();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Task FromConnectionPendingException()
        {
            return TaskUtil.FromException(GetException());

            static ConnectionPendingException GetException()
            {
                return new ConnectionPendingException();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowSocketException(SocketError err)
        {
            throw GetException();
            SocketException GetException()
            {
                return new SocketException((int)err);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowNullReferenceException_Command()
        {
            throw GetException();

            static NullReferenceException GetException()
            {
                return new NullReferenceException("command");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIOException_Underlying_file_size_smaller_then_requested_count(long size, long regionCount)
        {
            throw GetException();
            IOException GetException()
            {
                return new IOException("Underlying file size " + size + " smaller then requested count " + regionCount);
            }
        }

        #endregion
    }
}
