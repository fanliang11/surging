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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com)
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using DotNetty.Buffers;
    using DotNetty.Common;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Channels.Sockets;

    /// <summary>
    /// <see cref="ChannelHandlerAdapter"/> which decodes bytes in a stream-like fashion from one <see cref="IByteBuffer"/> to an
    /// other Message type.
    ///
    /// For example here is an implementation which reads all readable bytes from
    /// the input <see cref="IByteBuffer"/> and create a new {<see cref="IByteBuffer"/>.
    ///
    /// <![CDATA[
    ///     public class SquareDecoder : ByteToMessageDecoder
    ///     {
    ///         public override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
    ///         {
    ///             output.add(input.ReadBytes(input.ReadableBytes));
    ///         }
    ///     }
    /// ]]>
    ///
    /// <c>Frame detection</c>
    /// <para>
    /// Generally frame detection should be handled earlier in the pipeline by adding a
    /// <see cref="DelimiterBasedFrameDecoder"/>, <see cref="FixedLengthFrameDecoder"/>, <see cref="LengthFieldBasedFrameDecoder"/>,
    /// or <see cref="LineBasedFrameDecoder"/>.
    /// </para>
    /// <para>
    /// If a custom frame decoder is required, then one needs to be careful when implementing
    /// one with <see cref="ByteToMessageDecoder"/>. Ensure there are enough bytes in the buffer for a
    /// complete frame by checking <see cref="IByteBuffer.ReadableBytes"/>. If there are not enough bytes
    /// for a complete frame, return without modifying the reader index to allow more bytes to arrive.
    /// </para>
    /// <para>
    /// To check for complete frames without modifying the reader index, use methods like <see cref="IByteBuffer.GetInt(int)"/>.
    /// One <c>MUST</c> use the reader index when using methods like <see cref="IByteBuffer.GetInt(int)"/>.
    /// For example calling <tt>input.GetInt(0)</tt> is assuming the frame starts at the beginning of the buffer, which
    /// is not always the case. Use <tt>input.GetInt(input.ReaderIndex)</tt> instead.
    /// <c>Pitfalls</c>
    /// </para>
    /// <para>
    /// Be aware that sub-classes of <see cref="ByteToMessageDecoder"/> <c>MUST NOT</c>
    /// annotated with <see cref="ChannelHandlerAdapter.IsSharable"/>.
    /// </para>
    /// Some methods such as <see cref="IByteBuffer.ReadBytes(int)"/> will cause a memory leak if the returned buffer
    /// is not released or added to the <tt>output</tt> <see cref="List{Object}"/>. Use derived buffers like <see cref="IByteBuffer.ReadSlice(int)"/>
    /// to avoid leaking memory.
    /// </summary>
    public abstract partial class ByteToMessageDecoder : ChannelHandlerAdapter
    {
        /// <summary>
        /// Cumulates instances of <see cref="IByteBuffer" /> by merging them into one <see cref="IByteBuffer" />, using memory copies.
        /// </summary>
        public static readonly ICumulator MergeCumulator = new MergeCumulatorImpl();

        sealed class MergeCumulatorImpl : ICumulator
        {
            public IByteBuffer Cumulate(IByteBufferAllocator alloc, IByteBuffer cumulation, IByteBuffer input)
            {
                if (!cumulation.IsReadable() && input.IsContiguous)
                {
                    // If cumulation is empty and input buffer is contiguous, use it directly
                    _ = cumulation.Release();
                    return input;
                }
                try
                {
                    int required = input.ReadableBytes;
                    if (required > cumulation.MaxWritableBytes ||
                            (required > cumulation.MaxFastWritableBytes && cumulation.ReferenceCount > 1) ||
                            cumulation.IsReadOnly)
                    {
                        // Expand cumulation (by replacing it) under the following conditions:
                        // - cumulation cannot be resized to accommodate the additional data
                        // - cumulation can be expanded with a reallocation operation to accommodate but the buffer is
                        //   assumed to be shared (e.g. refCnt() > 1) and the reallocation may not be safe.
                        return ExpandCumulation(alloc, cumulation, input);
                    }
                    _ = cumulation.WriteBytes(input, input.ReaderIndex, required);
                    _ = input.SetReaderIndex(input.WriterIndex);
                    return cumulation;
                }
                finally
                {
                    // We must release in in all cases as otherwise it may produce a leak if writeBytes(...) throw
                    // for whatever release (for example because of OutOfMemoryError)
                    _ = input.Release(); 
                }
            }
        }

        /// <summary>
        /// Cumulate instances of <see cref="IByteBuffer" /> by add them to a <see cref="CompositeByteBuffer" /> and therefore
        /// avoiding memory copy when possible.
        /// </summary>
        /// <remarks>
        /// Be aware that <see cref="CompositeByteBuffer" /> use a more complex indexing implementation so depending on your use-case
        /// and the decoder implementation this may be slower then just use the <see cref="MergeCumulator" />.
        /// </remarks>
        public static readonly ICumulator CompositionCumulation = new CompositionCumulationImpl();

        sealed class CompositionCumulationImpl : ICumulator
        {
            public IByteBuffer Cumulate(IByteBufferAllocator alloc, IByteBuffer cumulation, IByteBuffer input)
            {
                if (!cumulation.IsReadable())
                {
                    _ = cumulation.Release();
                    return input;
                }
                CompositeByteBuffer composite = null;
                try
                {
                    composite = cumulation as CompositeByteBuffer;
                    if (composite is object && 0u >= (uint)(cumulation.ReferenceCount - 1))
                    {
                        // Writer index must equal capacity if we are going to "write"
                        // new components to the end
                        if (composite.WriterIndex != composite.Capacity)
                        {
                            _ = composite.AdjustCapacity(composite.WriterIndex);
                        }
                    }
                    else
                    {
                        composite = alloc.CompositeBuffer(int.MaxValue).AddFlattenedComponents(true, cumulation);
                    }
                    _ = composite.AddFlattenedComponents(true, input);
                    input = null;
                    return composite;
                }
                finally
                {
                    if (input is object)
                    {
                        // We must release if the ownership was not transferred as otherwise it may produce a leak
                        _ = input.Release();
                        // Also release any new buffer allocated if we're not returning it
                        if (composite is object && composite != cumulation)
                        {
                            _ = composite.Release();
                        }
                    }
                }
            }
        }

        private const byte STATE_INIT = 0;
        private const byte STATE_CALLING_CHILD_DECODE = 1;
        private const byte STATE_HANDLER_REMOVED_PENDING = 2;

        internal IByteBuffer _cumulation;
        private ICumulator _cumulator = MergeCumulator;
        private bool _first;

        /// <summary>
        /// This flag is used to determine if we need to call <see cref="IChannelHandlerContext.Read"/> to consume more data
        /// when <see cref="IChannelConfiguration.IsAutoRead"/> is <c>false</c>.
        /// </summary>
        private bool _firedChannelRead;

        /// <summary>
        /// A bitmask where the bits are defined as
        /// <see cref="STATE_INIT"/>
        /// <see cref="STATE_CALLING_CHILD_DECODE"/>
        /// <see cref="STATE_HANDLER_REMOVED_PENDING"/>
        /// </summary>
        private byte _decodeState = STATE_INIT;
        private int _discardAfterReads = 16;
        private int _numReads;

        protected ByteToMessageDecoder()
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor -- used for safety check only
            if (IsSharable)
            {
                CThrowHelper.ThrowInvalidOperationException_ByteToMessageDecoder();
            }
        }

        /// <summary>
        /// Determines whether only one message should be decoded per <see cref="ChannelRead" /> call.
        /// This may be useful if you need to do some protocol upgrade and want to make sure nothing is mixed up.
        /// 
        /// Default is <c>false</c> as this has performance impacts.
        /// </summary>
        public bool SingleDecode { get; set; }

        /// <summary>
        /// Set the <see cref="ICumulator"/> to use for cumulate the received <see cref="IByteBuffer"/>s.
        /// </summary>
        /// <param name="cumulator"></param>
        public void SetCumulator(ICumulator cumulator)
        {
            if (cumulator is null) { CThrowHelper.ThrowArgumentNullException(CExceptionArgument.cumulator); }

            _cumulator = cumulator;
        }

        /// <summary>
        /// Set the number of reads after which <see cref="IByteBuffer.DiscardSomeReadBytes"/> are called and so free up memory.
        /// The default is <code>16</code>.
        /// </summary>
        /// <param name="discardAfterReads"></param>
        public void SetDiscardAfterReads(int discardAfterReads)
        {
            if ((uint)(discardAfterReads - 1) > SharedConstants.TooBigOrNegative) // <= 0
            {
                CThrowHelper.ThrowArgumentException_DiscardAfterReads();
            }
            _discardAfterReads = discardAfterReads;
        }

        /// <summary>
        /// Returns the actual number of readable bytes in the internal cumulative
        /// buffer of this decoder. You usually do not need to rely on this value
        /// to write a decoder. Use it only when you must use it at your own risk.
        /// This method is a shortcut to <see cref="IByteBuffer.ReadableBytes" /> of <see cref="InternalBuffer" />.
        /// </summary>
        protected int ActualReadableBytes => InternalBuffer.ReadableBytes;

        /// <summary>
        /// Returns the internal cumulative buffer of this decoder. You usually
        /// do not need to access the internal buffer directly to write a decoder.
        /// Use it only when you must use it at your own risk.
        /// </summary>
        protected IByteBuffer InternalBuffer
        {
            get
            {
                if (_cumulation is object)
                {
                    return _cumulation;
                }
                else
                {
                    return Unpooled.Empty;
                }
            }
        }

        /// <inheritdoc />
        public override void HandlerRemoved(IChannelHandlerContext context)
        {
            if (_decodeState == STATE_CALLING_CHILD_DECODE)
            {
                _decodeState = STATE_HANDLER_REMOVED_PENDING;
                return;
            }
            IByteBuffer buf = _cumulation;
            if (buf is object)
            {
                // Directly set this to null so we are sure we not access it in any other method here anymore.
                _cumulation = null;
                _numReads = 0;
                int readable = buf.ReadableBytes;
                if (readable > 0)
                {
                    _ = context.FireChannelRead(buf);
                    _ = context.FireChannelReadComplete();
                }
                else
                {
                    _ = buf.Release();
                }
            }
            HandlerRemovedInternal(context);
        }

        /// <summary>
        /// Gets called after the <see cref="ByteToMessageDecoder"/> was removed from the actual context and it doesn't handle
        /// events anymore.
        /// </summary>
        /// <param name="context"></param>
        protected virtual void HandlerRemovedInternal(IChannelHandlerContext context)
        {
        }

        /// <inheritdoc />
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is IByteBuffer data)
            {
                ThreadLocalObjectList output = ThreadLocalObjectList.NewInstance();
                try
                {
                    _first = _cumulation is null;
                    _cumulation = _cumulator.Cumulate(context.Allocator, _first ? Unpooled.Empty : _cumulation, data);
                    CallDecode(context, _cumulation, output);
                }
                catch (DecoderException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    CThrowHelper.ThrowDecoderException(ex);
                }
                finally
                {
                    try
                    {
                        int size = output.Count;
                        if (_cumulation is object && !_cumulation.IsReadable())
                        {
                            _numReads = 0; 
                            _ = _cumulation.Release();
                            _cumulation = null;
                        }
                        else if (++_numReads >= _discardAfterReads)
                        {
                            // We did enough reads already try to discard some bytes so we not risk to see a OOME.
                            // See https://github.com/netty/netty/issues/4275
                            _numReads = 0;
                            DiscardSomeReadBytes();
                        }

         
                        _firedChannelRead |= (uint)size > 0u;
                        FireChannelRead(context, output, size);
                    }
                    finally
                    {
                        output.Return();
                    }
                }
            }
            else
            {
                _ = context.FireChannelRead(message);
            }
        }

        /// <summary>
        /// Get <paramref name="numElements"/> out of the <paramref name="output"/> and forward these through the pipeline.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="output"></param>
        /// <param name="numElements"></param>
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        protected static void FireChannelRead(IChannelHandlerContext ctx, List<object> output, int numElements)
        {
            for (int i = 0; i < numElements; i++)
            {
                _ = ctx.FireChannelRead(output[i]);
            }
        }

        /// <inheritdoc />
        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            _numReads = 0;
            DiscardSomeReadBytes();
            if (!_firedChannelRead && !context.Channel.Configuration.IsAutoRead)
            {
                _ = context.Read();
            }
            _firedChannelRead = false;
            _ = context.FireChannelReadComplete();
        }

        protected void DiscardSomeReadBytes()
        {
            if (_cumulation is object && !_first && 0u >= (uint)(_cumulation.ReferenceCount - 1))
            {
                // discard some bytes if possible to make more room input the
                // buffer but only if the refCnt == 1  as otherwise the user may have
                // used slice().retain() or duplicate().retain().
                //
                // See:
                // - https://github.com/netty/netty/issues/2327
                // - https://github.com/netty/netty/issues/1764
                _ = _cumulation.DiscardSomeReadBytes();
            }
        }

        /// <inheritdoc />
        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            ChannelInputClosed(ctx, true);
        }

        /// <inheritdoc />
        public override void UserEventTriggered(IChannelHandlerContext ctx, object evt)
        {
            if (evt is ChannelInputShutdownEvent)
            {
                // The decodeLast method is invoked when a channelInactive event is encountered.
                // This method is responsible for ending requests in some situations and must be called
                // when the input has been shutdown.
                ChannelInputClosed(ctx, false);
            }
            _ = ctx.FireUserEventTriggered(evt);
        }

        private void ChannelInputClosed(IChannelHandlerContext ctx, bool callChannelInactive)
        {
            ThreadLocalObjectList output = ThreadLocalObjectList.NewInstance();
            try
            {
                ChannelInputClosed(ctx, output);
            }
            catch (DecoderException)
            {
                throw;
            }
            catch (Exception e)
            {
                CThrowHelper.ThrowDecoderException(e);
            }
            finally
            {
                try
                {
                    if (_cumulation is object)
                    {
                        _ = _cumulation.Release();
                        _cumulation = null;
                    }
                    int size = output.Count;
                    if ((uint)size > 0u)
                    {
                        FireChannelRead(ctx, output, size);
                        // Something was read, call fireChannelReadComplete()
                        _ = ctx.FireChannelReadComplete();
                    }
                    if (callChannelInactive)
                    {
                        _ = ctx.FireChannelInactive();
                    }
                }
                finally
                {
                    // Recycle in all cases
                    output.Return();
                }
            }
        }

        /// <summary>
        /// Called when the input of the channel was closed which may be because it changed to inactive or because of
        /// <see cref="ChannelInputShutdownEvent"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="output"></param>
        protected virtual void ChannelInputClosed(IChannelHandlerContext ctx, List<object> output)
        {
            if (_cumulation is object)
            {
                CallDecode(ctx, _cumulation, output);
                DecodeLast(ctx, _cumulation, output);
            }
            else
            {
                DecodeLast(ctx, Unpooled.Empty, output);
            }
        }

        /// <summary>
        /// Called once data should be decoded from the given <see cref="IByteBuffer"/>. This method will call
        /// <see cref="Decode(IChannelHandlerContext, IByteBuffer, List{object})"/> as long as decoding should take place.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <param name="output"></param>
        protected virtual void CallDecode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            if (context is null) { CThrowHelper.ThrowArgumentNullException(CExceptionArgument.context); }
            if (input is null) { CThrowHelper.ThrowArgumentNullException(CExceptionArgument.input); }
            if (output is null) { CThrowHelper.ThrowArgumentNullException(CExceptionArgument.output); }

            try
            {
                while (input.IsReadable())
                {
                    int initialOutputCount = output.Count;
                    if ((uint)initialOutputCount > 0u)
                    {
                        FireChannelRead(context, output, initialOutputCount);
                        output.Clear();

                        // Check if this handler was removed before continuing with decoding.
                        // If it was removed, it is not safe to continue to operate on the buffer.
                        //
                        // See:
                        // - https://github.com/netty/netty/issues/4635
                        if (context.IsRemoved)
                        {
                            break;
                        }
                        initialOutputCount = 0;
                    }

                    int oldInputLength = input.ReadableBytes;
                    DecodeRemovalReentryProtection(context, input, output);

                    // Check if this handler was removed before continuing the loop.
                    // If it was removed, it is not safe to continue to operate on the buffer.
                    //
                    // See https://github.com/netty/netty/issues/1664
                    if (context.IsRemoved)
                    {
                        break;
                    }

                    bool noOutgoingMessages = 0u >= (uint)(oldInputLength - input.ReadableBytes);
                    if (0u >= (uint)(initialOutputCount - output.Count))
                    {
                        // no outgoing messages have been produced

                        if (noOutgoingMessages)
                        {
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (noOutgoingMessages)
                    {
                        CThrowHelper.ThrowDecoderException_ByteToMessageDecoder(GetType());
                    }

                    if (SingleDecode)
                    {
                        break;
                    }
                }
            }
            catch (DecoderException)
            {
                throw;
            }
            catch (Exception cause)
            {
                CThrowHelper.ThrowDecoderException(cause);
            }
        }

        /// <summary>
        /// Decode the from one <see cref="IByteBuffer"/> to an other. This method will be called till either the input
        /// <see cref="IByteBuffer"/> has nothing to read when return from this method or till nothing was read from the input
        /// <see cref="IByteBuffer"/>.
        /// </summary>
        /// <param name="context">the <see cref="IChannelHandlerContext"/> which this <see cref="ByteToMessageDecoder"/> belongs to</param>
        /// <param name="input">the <see cref="IByteBuffer"/> from which to read data</param>
        /// <param name="output">the <see cref="List{Object}"/> to which decoded messages should be added</param>
        protected internal abstract void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output);

        /// <summary>
        /// Decode the from one <see cref="IByteBuffer"/> to an other. This method will be called till either the input
        /// <see cref="IByteBuffer"/> has nothing to read when return from this method or till nothing was read from the input
        /// <see cref="IByteBuffer"/>.
        /// </summary>
        /// <param name="ctx">the <see cref="IChannelHandlerContext"/> which this <see cref="ByteToMessageDecoder"/> belongs to</param>
        /// <param name="input">the <see cref="IByteBuffer"/> from which to read data</param>
        /// <param name="output">the <see cref="List{Object}"/> to which decoded messages should be added</param>
        protected void DecodeRemovalReentryProtection(IChannelHandlerContext ctx, IByteBuffer input, List<object> output)
        {
            _decodeState = STATE_CALLING_CHILD_DECODE;
            try
            {
                Decode(ctx, input, output);
            }
            finally
            {
                var removePending = _decodeState == STATE_HANDLER_REMOVED_PENDING;
                _decodeState = STATE_INIT;
                if (removePending)
                {
                    FireChannelRead(ctx, output, output.Count);
                    output.Clear();
                    HandlerRemoved(ctx);
                }
            }
        }

        /// <summary>
        /// Is called one last time when the <see cref="IChannelHandlerContext"/> goes in-active. Which means the
        /// <see cref="ChannelInactive(IChannelHandlerContext)"/> was triggered.
        /// 
        /// By default this will just call <see cref="Decode(IChannelHandlerContext, IByteBuffer, List{object})"/> but sub-classes may
        /// override this for some special cleanup operation.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <param name="output"></param>
        protected virtual void DecodeLast(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            if (input.IsReadable())
            {
                // Only call decode() if there is something left in the buffer to decode.
                // See https://github.com/netty/netty/issues/4386
                Decode(context, input, output);
            }
        }

        private static IByteBuffer ExpandCumulation(IByteBufferAllocator alloc, IByteBuffer oldCumulation, IByteBuffer input)
        {
            int oldBytes = oldCumulation.ReadableBytes;
            int newBytes = input.ReadableBytes;
            int totalBytes = oldBytes + newBytes;
            IByteBuffer newCumulation = alloc.Buffer(alloc.CalculateNewCapacity(totalBytes, int.MaxValue));
            try
            {
                // This avoids redundant checks and stack depth compared to calling writeBytes(...)
                _ = newCumulation.SetBytes(0, oldCumulation, oldCumulation.ReaderIndex, oldBytes)
                    .SetBytes(oldBytes, input, input.ReaderIndex, newBytes)
                    .SetWriterIndex(totalBytes);
                _ = input.SetReaderIndex(input.WriterIndex);
                return newCumulation;
            }
            finally
            {
                _ = oldCumulation.Release();//fanly update 
            }
        }

        /// <summary>
        /// Cumulate <see cref="IByteBuffer"/>s.
        /// </summary>
        public interface ICumulator
        {
            /// <summary>
            /// Cumulate the given <see cref="IByteBuffer"/>s and return the <see cref="IByteBuffer"/> that holds the cumulated bytes.
            /// The implementation is responsible to correctly handle the life-cycle of the given <see cref="IByteBuffer"/>s and so
            /// call <see cref="IReferenceCounted.Release()"/> if a <see cref="IByteBuffer"/> is fully consumed.
            /// </summary>
            /// <param name="alloc"></param>
            /// <param name="cumulation"></param>
            /// <param name="input"></param>
            /// <returns></returns>
            IByteBuffer Cumulate(IByteBufferAllocator alloc, IByteBuffer cumulation, IByteBuffer input);
        }
    }
}