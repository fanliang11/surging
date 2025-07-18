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
 * Copyright (c) Microsoft. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
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
    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    /// <inheritdoc cref="ReplayingDecoder{T}" />
    public abstract class ReplayingDecoder : ReplayingDecoder<ReplayingDecoder.Void>
    {
        public enum Void
        {
            // Empty state
        }

        /// <summary>Creates a new instance.</summary>
        protected ReplayingDecoder() { }
    }

    /// <summary>
    /// A specialized variation of <see cref="ByteToMessageDecoder"/> which enables implementation
    /// of a non-blocking decoder in the blocking I/O paradigm.
    /// <para>
    /// The biggest difference between <see cref="ReplayingDecoder{TState}"/> and
    /// <see cref="ByteToMessageDecoder"/> is that <see cref="ReplayingDecoder{TState}"/> allows you to
    /// implement the {@code decode()} and {@code decodeLast()} methods just like
    /// all required bytes were received already, rather than checking the
    /// availability of the required bytes.  For example, the following
    /// <see cref="ByteToMessageDecoder"/> implementation:
    /// </para>
    /// <code>
    /// public class IntegerHeaderFrameDecoder extends <see cref="ByteToMessageDecoder"/> {
    ///
    ///   {@code @Override}
    ///   protected void decode(<see cref="IChannelHandlerContext"/> ctx,
    ///                           <see cref="IByteBuffer"/> buf, List&lt;Object&gt; out) throws Exception {
    ///
    ///     if (buf.readableBytes() &lt; 4) {
    ///        return;
    ///     }
    ///
    ///     buf.markReaderIndex();
    ///     int length = buf.readInt();
    ///
    ///     if (buf.readableBytes() &lt; length) {
    ///        buf.resetReaderIndex();
    ///        return;
    ///     }
    ///
    ///     out.add(buf.readBytes(length));
    ///   }
    /// }
    /// </code>
    /// is simplified like the following with <see cref="ReplayingDecoder{TState}"/>:
    /// <code>
    /// public class IntegerHeaderFrameDecoder
    ///      extends <see cref="ReplayingDecoder{TState}"/>&lt;{@link Void}&gt; {
    ///
    ///   protected void decode(<see cref="IChannelHandlerContext"/> ctx,
    ///                           <see cref="IByteBuffer"/> buf, List&lt;Object&gt; output) throws Exception {
    ///
    ///     out.add(buf.readBytes(buf.readInt()));
    ///   }
    /// }
    /// </code>
    ///
    /// <h3>How does this work?</h3>
    /// <para>
    /// <see cref="ReplayingDecoder{TState}"/> passes a specialized <see cref="IByteBuffer"/>
    /// implementation which throws an {@link Error} of certain type when there's not
    /// enough data in the buffer.  In the {@code IntegerHeaderFrameDecoder} above,
    /// you just assumed that there will be 4 or more bytes in the buffer when
    /// you call {@code buf.readInt()}.  If there's really 4 bytes in the buffer,
    /// it will return the integer header as you expected.  Otherwise, the
    /// {@link Error} will be raised and the control will be returned to
    /// <see cref="ReplayingDecoder{TState}"/>.  If <see cref="ReplayingDecoder{TState}"/> catches the
    /// {@link Error}, then it will rewind the {@code readerIndex} of the buffer
    /// back to the 'initial' position (i.e. the beginning of the buffer) and call
    /// the {@code decode(..)} method again when more data is received into the
    /// buffer.
    /// </para>
    /// <para>
    /// Please note that <see cref="ReplayingDecoder{TState}"/> always throws the same cached
    /// {@link Error} instance to avoid the overhead of creating a new {@link Error}
    /// and filling its stack trace for every throw.
    /// </para>
    ///
    /// <h3>Limitations</h3>
    /// <para>
    /// At the cost of the simplicity, <see cref="ReplayingDecoder{TState}"/> enforces you a few
    /// limitations:
    /// </para>
    /// <ul>
    /// <li>Some buffer operations are prohibited.</li>
    /// <li>Performance can be worse if the network is slow and the message
    ///     format is complicated unlike the example above.  In this case, your
    ///     decoder might have to decode the same part of the message over and over
    ///     again.</li>
    /// <li>You must keep in mind that {@code decode(..)} method can be called many
    ///     times to decode a single message.  For example, the following code will
    ///     not work:
    /// <code> public class MyDecoder extends <see cref="ReplayingDecoder{TState}"/>&lt;{@link Void}&gt; {
    ///
    ///   private final Queue&lt;Integer&gt; values = new LinkedList&lt;Integer&gt;();
    ///
    ///   {@code @Override}
    ///   public void decode(.., <see cref="IByteBuffer"/> buf, List&lt;Object&gt; out) throws Exception {
    ///
    ///     // A message contains 2 integers.
    ///     values.offer(buf.readInt());
    ///     values.offer(buf.readInt());
    ///
    ///     // This assertion will fail intermittently since values.offer()
    ///     // can be called more than two times!
    ///     assert values.size() == 2;
    ///     out.add(values.poll() + values.poll());
    ///   }
    /// }</code>
    ///      The correct implementation looks like the following, and you can also
    ///      utilize the 'checkpoint' feature which is explained in detail in the
    ///      next section.
    /// <code> public class MyDecoder extends <see cref="ReplayingDecoder{TState}"/>&lt;{@link Void}&gt; {
    ///
    ///   private final Queue&lt;Integer&gt; values = new LinkedList&lt;Integer&gt;();
    ///
    ///   {@code @Override}
    ///   public void decode(.., <see cref="IByteBuffer"/> buf, List&lt;Object&gt; out) throws Exception {
    ///
    ///     // Revert the state of the variable that might have been changed
    ///     // since the last partial decode.
    ///     values.clear();
    ///
    ///     // A message contains 2 integers.
    ///     values.offer(buf.readInt());
    ///     values.offer(buf.readInt());
    ///
    ///     // Now we know this assertion will never fail.
    ///     assert values.size() == 2;
    ///     out.add(values.poll() + values.poll());
    ///   }
    /// }</code>
    ///     </li>
    /// </ul>
    ///
    /// <h3>Improving the performance</h3>
    /// <para>
    /// Fortunately, the performance of a complex decoder implementation can be
    /// improved significantly with the {@code checkpoint()} method.  The
    /// {@code checkpoint()} method updates the 'initial' position of the buffer so
    /// that <see cref="ReplayingDecoder{TState}"/> rewinds the {@code readerIndex} of the buffer
    /// to the last position where you called the {@code checkpoint()} method.
    /// </para>
    /// <h4>Calling {@code checkpoint(T)} with an {@link Enum}</h4>
    /// <para>
    /// Although you can just use {@code checkpoint()} method and manage the state
    /// of the decoder by yourself, the easiest way to manage the state of the
    /// decoder is to create an {@link Enum} type which represents the current state
    /// of the decoder and to call {@code checkpoint(T)} method whenever the state
    /// changes.  You can have as many states as you want depending on the
    /// complexity of the message you want to decode:
    /// </para>
    /// <code>
    /// public enum MyDecoderState {
    ///   READ_LENGTH,
    ///   READ_CONTENT;
    /// }
    ///
    /// public class IntegerHeaderFrameDecoder
    ///      extends <see cref="ReplayingDecoder{TState}"/>&lt;<strong>MyDecoderState</strong>&gt; {
    ///
    ///   private int length;
    ///
    ///   public IntegerHeaderFrameDecoder() {
    ///     // Set the initial state.
    ///     <strong>super(MyDecoderState.READ_LENGTH);</strong>
    ///   }
    ///
    ///   {@code @Override}
    ///   protected void decode(<see cref="IChannelHandlerContext"/> ctx,
    ///                           <see cref="IByteBuffer"/> buf, List&lt;Object&gt; out) throws Exception {
    ///     switch (state()) {
    ///     case READ_LENGTH:
    ///       length = buf.readInt();
    ///       <strong>checkpoint(MyDecoderState.READ_CONTENT);</strong>
    ///     case READ_CONTENT:
    ///       ByteBuf frame = buf.readBytes(length);
    ///       <strong>checkpoint(MyDecoderState.READ_LENGTH);</strong>
    ///       out.add(frame);
    ///       break;
    ///     default:
    ///       throw new Error("Shouldn't reach here.");
    ///     }
    ///   }
    /// }
    /// </code>
    ///
    /// <h4>Calling {@code checkpoint()} with no parameter</h4>
    /// <para>
    /// An alternative way to manage the decoder state is to manage it by yourself.
    /// </para>
    /// <code>
    /// public class IntegerHeaderFrameDecoder
    ///      extends <see cref="ReplayingDecoder{TState}"/>&lt;<strong>{@link Void}</strong>&gt; {
    ///
    ///   <strong>private boolean readLength;</strong>
    ///   private int length;
    ///
    ///   {@code @Override}
    ///   protected void decode(<see cref="IChannelHandlerContext"/> ctx,
    ///                           <see cref="IByteBuffer"/> buf, List&lt;Object&gt; out) throws Exception {
    ///     if (!readLength) {
    ///       length = buf.readInt();
    ///       <strong>readLength = true;</strong>
    ///       <strong>checkpoint();</strong>
    ///     }
    ///
    ///     if (readLength) {
    ///       ByteBuf frame = buf.readBytes(length);
    ///       <strong>readLength = false;</strong>
    ///       <strong>checkpoint();</strong>
    ///       out.add(frame);
    ///     }
    ///   }
    /// }
    /// </code>
    ///
    /// <h3>Replacing a decoder with another decoder in a pipeline</h3>
    /// <para>
    /// If you are going to write a protocol multiplexer, you will probably want to
    /// replace a <see cref="ReplayingDecoder{TState}"/> (protocol detector) with another
    /// <see cref="ReplayingDecoder{TState}"/>, <see cref="ByteToMessageDecoder"/> or {@link MessageToMessageDecoder}
    /// (actual protocol decoder).
    /// It is not possible to achieve this simply by calling
    /// <see cref="IChannelPipeline.Replace(IChannelHandler, string, IChannelHandler)"/>, but
    /// some additional steps are required:
    /// </para>
    /// <code>
    /// public class FirstDecoder extends <see cref="ReplayingDecoder{TState}"/>&lt;{@link Void}&gt; {
    ///
    ///     {@code @Override}
    ///     protected void decode(<see cref="IChannelHandlerContext"/> ctx,
    ///                             <see cref="IByteBuffer"/> buf, List&lt;Object&gt; out) {
    ///         ...
    ///         // Decode the first message
    ///         Object firstMessage = ...;
    ///
    ///         // Add the second decoder
    ///         ctx.pipeline().addLast("second", new SecondDecoder());
    ///
    ///         if (buf.isReadable()) {
    ///             // Hand off the remaining data to the second decoder
    ///             out.add(firstMessage);
    ///             out.add(buf.readBytes(<b>super.actualReadableBytes()</b>));
    ///         } else {
    ///             // Nothing to hand off
    ///             out.add(firstMessage);
    ///         }
    ///         // Remove the first decoder (me)
    ///         ctx.pipeline().remove(this);
    ///     }
    /// </code>
    /// </summary>
    /// <typeparam name="TState">the state type which is usually an <see cref="Enum"/>; use <see cref="T:Void"/> if state management is
    /// unused</typeparam>
    public abstract class ReplayingDecoder<TState> : ByteToMessageDecoder
        where TState : struct
    {
        internal static readonly Signal REPLAY = ReplayingDecoderByteBuffer.REPLAY;

        private readonly ReplayingDecoderByteBuffer _replayable;
        private TState _state;
        private int _checkpoint;
        private bool _replayRequested;

        /// <summary>
        /// Creates a new instance with no initial state (i.e: <c>null</c>).
        /// </summary>
        protected ReplayingDecoder()
            : this(default)
        {
        }

        /// <summary>
        /// Creates a new instance with the specified initial state.
        /// </summary>
        /// <param name="initialState"></param>
        protected ReplayingDecoder(TState initialState)
        {
            _replayable = new ReplayingDecoderByteBuffer();
            _state = initialState;
        }

        /// <summary>
        /// Stores the internal cumulative buffer's reader position.
        /// </summary>
        protected void Checkpoint()
        {
            _checkpoint = InternalBuffer.ReaderIndex;
        }

        /// <summary>
        /// Stores the internal cumulative buffer's reader position and updates
        /// the current decoder state.
        /// </summary>
        /// <param name="newState"></param>
        protected void Checkpoint(TState newState)
        {
            Checkpoint();
            _state = newState;
        }

        /// <summary>
        /// Returns the current state of this decoder.
        /// </summary>
        protected TState State => _state;

        /// <summary>
        /// Sets the current state of this decoder.
        /// </summary>
        /// <param name="newState"></param>
        /// <returns>the old state of this decoder</returns>
        protected TState ExchangeState(TState newState)
        {
            TState oldState = _state;
            _state = newState;
            return oldState;
        }

        protected bool ReplayRequested => _replayRequested;

        protected void RequestReplay()
        {
            _replayRequested = true;
        }

        /// <inheritdoc />
        protected override void ChannelInputClosed(IChannelHandlerContext ctx, List<object> output)
        {
            try
            {
                _replayable.Terminate();
                if (_cumulation is object)
                {
                    CallDecode(ctx, InternalBuffer, output);
                }
                else
                {
                    _replayable.SetCumulation(Unpooled.Empty);
                }
                DecodeLast(ctx, _replayable, output);
            }
            catch (Signal replay)
            {
                // Ignore
                replay.Expect(REPLAY);
            }
        }

        /// <inheritdoc />
        protected override void CallDecode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            _replayable.SetCumulation(input);
            try
            {
                while (input.IsReadable())
                {
                    _replayRequested = false;
                    int oldReaderIndex = _checkpoint = input.ReaderIndex;
                    int outSize = output.Count;

                    if ((uint)outSize > 0u)
                    {
                        FireChannelRead(context, output, outSize);
                        output.Clear();

                        // Check if this handler was removed before continuing with decoding.
                        // If it was removed, it is not safe to continue to operate on the buffer.
                        //
                        // See:
                        // - https://github.com/netty/netty/issues/4635
                        if (context.IsRemoved) { break; }
                        outSize = 0;
                    }

                    TState oldState = _state;
                    int oldInputLength = input.ReadableBytes;
                    try
                    {
                        DecodeRemovalReentryProtection(context, _replayable, output);

                        // Check if this handler was removed before continuing the loop.
                        // If it was removed, it is not safe to continue to operate on the buffer.
                        //
                        // See https://github.com/netty/netty/issues/1664
                        if (context.IsRemoved) { break; }

                        if (_replayRequested)
                        {
                            // Return to the checkpoint (or oldPosition) and retry.
                            int restorationPoint = _checkpoint;
                            if (SharedConstants.TooBigOrNegative >= (uint)restorationPoint) // restorationPoint >= 0
                            {
                                _ = input.SetReaderIndex(restorationPoint);
                            }
                            else
                            {
                                // Called by cleanup() - no need to maintain the readerIndex
                                // anymore because the buffer has been released already.
                            }
                            break;
                        }

                        if (0u >= (uint)(outSize - output.Count))
                        {
                            if (0u >= (uint)(oldInputLength - input.ReadableBytes) && s_comparer.Equals(oldState, _state))
                            {
                                CThrowHelper.ThrowDecoderException_Anything(GetType());
                            }
                            else
                            {
                                // Previous data has been discarded or caused state transition.
                                // Probably it is reading on.
                                continue;
                            }
                        }
                    }
                    catch (Signal replay)
                    {
                        replay.Expect(REPLAY);

                        // Check if this handler was removed before continuing the loop.
                        // If it was removed, it is not safe to continue to operate on the buffer.
                        //
                        // See https://github.com/netty/netty/issues/1664
                        if (context.IsRemoved) { break; }

                        // Return to the checkpoint (or oldPosition) and retry.
                        int restorationPoint = _checkpoint;
                        if (SharedConstants.TooBigOrNegative >= (uint)restorationPoint) // restorationPoint >= 0
                        {
                            _ = input.SetReaderIndex(restorationPoint);
                        }
                        else
                        {
                            // Called by cleanup() - no need to maintain the readerIndex
                            // anymore because the buffer has been released already.
                        }
                        break;
                    }

                    if (0u >= (uint)(oldReaderIndex - input.ReaderIndex) && s_comparer.Equals(oldState, _state))
                    {
                        CThrowHelper.ThrowDecoderException_Something(GetType());
                    }

                    if (SingleDecode) { break; }
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

        private static readonly IEqualityComparer<TState> s_comparer = EqualityComparer<TState>.Default;
    }
}