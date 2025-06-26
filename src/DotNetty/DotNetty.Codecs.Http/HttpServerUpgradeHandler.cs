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

namespace DotNetty.Codecs.Http
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using DotNetty.Buffers;
    using DotNetty.Common;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    public class HttpServerUpgradeHandler : HttpObjectAggregator
    {
        /// <summary>
        /// The source codec that is used in the pipeline initially.
        /// </summary>
        public interface ISourceCodec
        {
            /// <summary>
            /// Removes this codec (i.e. all associated handlers) from the pipeline.
            /// </summary>
            void UpgradeFrom(IChannelHandlerContext ctx);
        }

        /// <summary>
        /// A codec that the source can be upgraded to.
        /// </summary>
        public interface IUpgradeCodec
        {
            /// <summary>
            /// Gets all protocol-specific headers required by this protocol for a successful upgrade.
            /// Any supplied header will be required to appear in the <see cref="HttpHeaderNames.Connection"/> header as well.
            /// </summary>
            IReadOnlyList<AsciiString> RequiredUpgradeHeaders { get; }

            /// <summary>
            /// Prepares the <paramref name="upgradeHeaders"/> for a protocol update based upon the contents of <paramref name="upgradeRequest"/>.
            /// This method returns a boolean value to proceed or abort the upgrade in progress. If <c>false</c> is
            /// returned, the upgrade is aborted and the <paramref name="upgradeRequest"/> will be passed through the inbound pipeline
            /// as if no upgrade was performed. If <c>true</c> is returned, the upgrade will proceed to the next
            /// step which invokes <see cref="UpgradeTo(IChannelHandlerContext, IFullHttpRequest)"/>. When returning <c>true</c>, you can add headers to
            /// the <paramref name="upgradeHeaders"/> so that they are added to the 101 Switching protocols response.
            /// </summary>
            bool PrepareUpgradeResponse(IChannelHandlerContext ctx, IFullHttpRequest upgradeRequest, HttpHeaders upgradeHeaders);

            /// <summary>
            /// Performs an HTTP protocol upgrade from the source codec. This method is responsible for
            /// adding all handlers required for the new protocol.
            ///
            /// ctx the context for the current handler.
            /// upgradeRequest the request that triggered the upgrade to this protocol.
            /// </summary>
            void UpgradeTo(IChannelHandlerContext ctx, IFullHttpRequest upgradeRequest);
        }

        /// <summary>
        ///  Creates a new <see cref="IUpgradeCodec"/> for the requested protocol name.
        /// </summary>
        public interface IUpgradeCodecFactory
        {
            /// <summary>
            ///  Invoked by <see cref="HttpServerUpgradeHandler"/> for all the requested protocol names in the order of
            ///  the client preference.The first non-<c>null</c> <see cref="IUpgradeCodec"/> returned by this method
            ///  will be selected.
            /// </summary>
            IUpgradeCodec NewUpgradeCodec(ICharSequence protocol);
        }

        /// <summary>
        /// User event that is fired to notify about the completion of an HTTP upgrade
        /// to another protocol. Contains the original upgrade request so that the response
        /// (if required) can be sent using the new protocol.
        /// </summary>
        public sealed class UpgradeEvent : IReferenceCounted
        {
            readonly ICharSequence protocol;
            readonly IFullHttpRequest upgradeRequest;

            internal UpgradeEvent(ICharSequence protocol, IFullHttpRequest upgradeRequest)
            {
                this.protocol = protocol;
                this.upgradeRequest = upgradeRequest;
            }

            /// <summary>
            /// The protocol that the channel has been upgraded to.
            /// </summary>
            public ICharSequence Protocol => this.protocol;

            /// <summary>
            /// Gets the request that triggered the protocol upgrade.
            /// </summary>
            public IFullHttpRequest UpgradeRequest => this.upgradeRequest;

            public int ReferenceCount => this.upgradeRequest.ReferenceCount;

            public IReferenceCounted Retain()
            {
                _ = this.upgradeRequest.Retain();
                return this;
            }

            public IReferenceCounted Retain(int increment)
            {
                _ = this.upgradeRequest.Retain(increment);
                return this;
            }

            public IReferenceCounted Touch()
            {
                _ = this.upgradeRequest.Touch();
                return this;
            }

            public IReferenceCounted Touch(object hint)
            {
                _ = this.upgradeRequest.Touch(hint);
                return this;
            }

            public bool Release() => this.upgradeRequest.Release();

            public bool Release(int decrement) => this.upgradeRequest.Release(decrement);

            public override string ToString() => $"UpgradeEvent [protocol={this.protocol}, upgradeRequest={this.upgradeRequest}]";
        }

        readonly ISourceCodec sourceCodec;
        readonly IUpgradeCodecFactory upgradeCodecFactory;
        bool handlingUpgrade;

        /// <summary>
        /// Constructs the upgrader with the supported codecs.
        /// <para>
        /// The handler instantiated by this constructor will reject an upgrade request with non-empty content.
        /// It should not be a concern because an upgrade request is most likely a GET request.
        /// If you have a client that sends a non-GET upgrade request, please consider using
        /// <see cref="HttpServerUpgradeHandler(ISourceCodec, IUpgradeCodecFactory, int)"/> to specify the maximum
        /// length of the content of an upgrade request.
        /// </para>
        /// </summary>
        /// <param name="sourceCodec">the codec that is being used initially</param>
        /// <param name="upgradeCodecFactory">the factory that creates a new upgrade codec
        /// for one of the requested upgrade protocols</param>
        public HttpServerUpgradeHandler(ISourceCodec sourceCodec, IUpgradeCodecFactory upgradeCodecFactory)
            : this(sourceCodec, upgradeCodecFactory, 0)
        {
        }

        /// <summary>
        /// Constructs the upgrader with the supported codecs.
        /// </summary>
        /// <param name="sourceCodec">the codec that is being used initially</param>
        /// <param name="upgradeCodecFactory">the factory that creates a new upgrade codec
        /// for one of the requested upgrade protocols</param>
        /// <param name="maxContentLength">the maximum length of the content of an upgrade request</param>
        public HttpServerUpgradeHandler(ISourceCodec sourceCodec, IUpgradeCodecFactory upgradeCodecFactory, int maxContentLength)
            : base(maxContentLength)
        {
            if (sourceCodec is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.sourceCodec); }
            if (upgradeCodecFactory is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.upgradeCodecFactory); }

            this.sourceCodec = sourceCodec;
            this.upgradeCodecFactory = upgradeCodecFactory;
        }

        protected override void Decode(IChannelHandlerContext context, IHttpObject message, List<object> output)
        {
            // Determine if we're already handling an upgrade request or just starting a new one.
            this.handlingUpgrade |= IsUpgradeRequest(message);
            if (!this.handlingUpgrade)
            {
                // Not handling an upgrade request, just pass it to the next handler.
                _ = ReferenceCountUtil.Retain(message);
                output.Add(message);
                return;
            }

            if (message is IFullHttpRequest fullRequest)
            {
                _ = ReferenceCountUtil.Retain(fullRequest);
                output.Add(fullRequest);
            }
            else
            {
                // Call the base class to handle the aggregation of the full request.
                base.Decode(context, message, output);
                if (0u >= (uint)output.Count)
                {
                    // The full request hasn't been created yet, still awaiting more data.
                    return;
                }

                // Finished aggregating the full request, get it from the output list.
                Debug.Assert(output.Count == 1);
                this.handlingUpgrade = false;
                fullRequest = (IFullHttpRequest)output[0];
            }

            if (this.Upgrade(context, fullRequest))
            {
                // The upgrade was successful, remove the message from the output list
                // so that it's not propagated to the next handler. This request will
                // be propagated as a user event instead.
                output.Clear();
            }

            // The upgrade did not succeed, just allow the full request to propagate to the
            // next handler.
        }

        /// <summary>
        /// Determines whether or not the message is an HTTP upgrade request.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        static bool IsUpgradeRequest(IHttpObject msg)
        {
            return msg is IHttpRequest request && request.Headers.Contains(HttpHeaderNames.Upgrade);
        }

        /// <summary>
        /// Attempts to upgrade to the protocol(s) identified by the <see cref="HttpHeaderNames.Upgrade"/> header (if provided
        /// in the request).
        /// </summary>
        /// <param name="ctx">the context for this handler.</param>
        /// <param name="request">the HTTP request.</param>
        /// <returns><c>true</c> if the upgrade occurred, otherwise <c>false</c>.</returns>
        bool Upgrade(IChannelHandlerContext ctx, IFullHttpRequest request)
        {
            // Select the best protocol based on those requested in the UPGRADE header.
            var requestedProtocols = SplitHeader(request.Headers.Get(HttpHeaderNames.Upgrade, null));
            int numRequestedProtocols = requestedProtocols.Count;
            IUpgradeCodec upgradeCodec = null;
            ICharSequence upgradeProtocol = null;
            for (int i = 0; i < numRequestedProtocols; i++)
            {
                ICharSequence p = requestedProtocols[i];
                IUpgradeCodec c = this.upgradeCodecFactory.NewUpgradeCodec(p);
                if (c is object)
                {
                    upgradeProtocol = p;
                    upgradeCodec = c;
                    break;
                }
            }

            if (upgradeCodec is null)
            {
                // None of the requested protocols are supported, don't upgrade.
                return false;
            }

            // Make sure the CONNECTION header is present.
            var connectionHeaderValues = request.Headers.GetAll(HttpHeaderNames.Connection);
            if (connectionHeaderValues is null) { return false; }

            var concatenatedConnectionValue = StringBuilderManager.Allocate(connectionHeaderValues.Count * 10);
            for (var idx = 0; idx < connectionHeaderValues.Count; idx++)
            {
                var connectionHeaderValue = connectionHeaderValues[idx];
                _ = concatenatedConnectionValue
                    .Append(connectionHeaderValue.ToString())
                    .Append(StringUtil.Comma);
            }
            concatenatedConnectionValue.Length -= 1;

            // Make sure the CONNECTION header contains UPGRADE as well as all protocol-specific headers.
            var requiredHeaders = upgradeCodec.RequiredUpgradeHeaders;
            var values = SplitHeader(StringBuilderManager.ReturnAndFree(concatenatedConnectionValue).AsSpan());
            if (!AsciiString.ContainsContentEqualsIgnoreCase(values, HttpHeaderNames.Upgrade) ||
                !AsciiString.ContainsAllContentEqualsIgnoreCase(values, requiredHeaders))
            {
                return false;
            }

            // Ensure that all required protocol-specific headers are found in the request.
            for (int idx = 0; idx < requiredHeaders.Count; idx++)
            {
                if (!request.Headers.Contains(requiredHeaders[idx]))
                {
                    return false;
                }
            }

            // Prepare and send the upgrade response. Wait for this write to complete before upgrading,
            // since we need the old codec in-place to properly encode the response.
            IFullHttpResponse upgradeResponse = CreateUpgradeResponse(upgradeProtocol);
            if (!upgradeCodec.PrepareUpgradeResponse(ctx, request, upgradeResponse.Headers))
            {
                return false;
            }

            // Create the user event to be fired once the upgrade completes.
            var upgradeEvent = new UpgradeEvent(upgradeProtocol, request);

            // After writing the upgrade response we immediately prepare the
            // pipeline for the next protocol to avoid a race between completion
            // of the write future and receiving data before the pipeline is
            // restructured.
            try
            {
                var writeComplete = ctx.WriteAndFlushAsync(upgradeResponse);

                // Perform the upgrade to the new protocol.
                this.sourceCodec.UpgradeFrom(ctx);
                upgradeCodec.UpgradeTo(ctx, request);

                // Remove this handler from the pipeline.
                _ = ctx.Pipeline.Remove(this);

                // Notify that the upgrade has occurred. Retain the event to offset
                // the release() in the finally block.
                _ = ctx.FireUserEventTriggered(upgradeEvent.Retain());

                // Add the listener last to avoid firing upgrade logic after
                // the channel is already closed since the listener may fire
                // immediately if the write failed eagerly.
                _ = writeComplete.ContinueWith(CloseOnFailureAction, ctx, TaskContinuationOptions.ExecuteSynchronously);
            }
            finally
            {
                // Release the event if the upgrade event wasn't fired.
                _ = upgradeEvent.Release();
            }
            return true;
        }

        static readonly Action<Task, object> CloseOnFailureAction = (t, s) => CloseOnFailure(t, s);
        static void CloseOnFailure(Task t, object s)
        {
            if (t.IsFailure())
            {
                _ = ((IChannelHandlerContext)s).Channel.CloseAsync();
            }
        }

        /// <summary>
        /// Creates the 101 Switching Protocols response message.
        /// </summary>
        /// <param name="upgradeProtocol"></param>
        /// <returns></returns>
        static IFullHttpResponse CreateUpgradeResponse(ICharSequence upgradeProtocol)
        {
            var res = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.SwitchingProtocols,
                Unpooled.Empty, false);
            _ = res.Headers.Add(HttpHeaderNames.Connection, HttpHeaderValues.Upgrade);
            _ = res.Headers.Add(HttpHeaderNames.Upgrade, upgradeProtocol);
            return res;
        }

        /// <summary>
        /// Splits a comma-separated header value. The returned set is case-insensitive and contains each
        /// part with whitespace removed.
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        static IReadOnlyList<ICharSequence> SplitHeader(ICharSequence header)
        {
            if (header is IHasUtf16Span hasUtf16) { return SplitHeader(hasUtf16.Utf16Span); }

            var builder = StringBuilderManager.Allocate(header.Count);
            var protocols = new List<ICharSequence>(4);
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < header.Count; ++i)
            {
                char c = header[i];
                if (char.IsWhiteSpace(c))
                {
                    // Don't include any whitespace.
                    continue;
                }
                if (c == HttpConstants.CommaChar)
                {
                    // Add the string and reset the builder for the next protocol.
                    protocols.Add(new AsciiString(builder.ToString()));
                    builder.Length = 0;
                }
                else
                {
                    _ = builder.Append(c);
                }
            }

            // Add the last protocol
            if ((uint)builder.Length > 0u)
            {
                protocols.Add(new AsciiString(builder.ToString()));
            }
            StringBuilderManager.Free(builder);

            return protocols;
        }

        /// <summary>
        /// Splits a comma-separated header value. The returned set is case-insensitive and contains each
        /// part with whitespace removed.
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        static IReadOnlyList<ICharSequence> SplitHeader(in ReadOnlySpan<char> header)
        {
            var builder = StringBuilderManager.Allocate(header.Length);
            var protocols = new List<ICharSequence>(4);
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < header.Length; ++i)
            {
                char c = header[i];
                if (char.IsWhiteSpace(c))
                {
                    // Don't include any whitespace.
                    continue;
                }
                if (c == HttpConstants.CommaChar)
                {
                    // Add the string and reset the builder for the next protocol.
                    protocols.Add(new AsciiString(builder.ToString()));
                    builder.Length = 0;
                }
                else
                {
                    _ = builder.Append(c);
                }
            }

            // Add the last protocol
            if ((uint)builder.Length > 0u)
            {
                protocols.Add(new AsciiString(builder.ToString()));
            }
            StringBuilderManager.Free(builder);

            return protocols;
        }
    }
}
