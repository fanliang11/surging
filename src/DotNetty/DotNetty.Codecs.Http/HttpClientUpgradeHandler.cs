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
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// Client-side handler for handling an HTTP upgrade handshake to another protocol. When the first
    /// HTTP request is sent, this handler will add all appropriate headers to perform an upgrade to the
    /// new protocol. If the upgrade fails (i.e. response is not 101 Switching Protocols), this handler
    /// simply removes itself from the pipeline. If the upgrade is successful, upgrades the pipeline to
    /// the new protocol.
    /// </summary>
    public class HttpClientUpgradeHandler : HttpObjectAggregator
    {
        /// <summary>
        /// User events that are fired to notify about upgrade status.
        /// </summary>
        public enum UpgradeEvent
        {
            /// <summary>
            /// The Upgrade request was sent to the server.
            /// </summary>
            UpgradeIssued,

            /// <summary>
            /// The Upgrade to the new protocol was successful.
            /// </summary>
            UpgradeSuccessful,

            /// <summary>
            /// The Upgrade was unsuccessful due to the server not issuing
            /// with a 101 Switching Protocols response.
            /// </summary>
            UpgradeRejected
        }

        /// <summary>
        /// The source codec that is used in the pipeline initially.
        /// </summary>
        public interface ISourceCodec
        {
            /// <summary>
            /// Removes or disables the encoder of this codec so that the <see cref="IUpgradeCodec"/> can send an initial greeting
            /// (if any).
            /// </summary>
            /// <param name="ctx"></param>
            void PrepareUpgradeFrom(IChannelHandlerContext ctx);

            /// <summary>
            /// Removes this codec (i.e. all associated handlers) from the pipeline.
            /// </summary>
            /// <param name="ctx"></param>
            void UpgradeFrom(IChannelHandlerContext ctx);
        }

        /// <summary>
        /// A codec that the source can be upgraded to.
        /// </summary>
        public interface IUpgradeCodec
        {
            /// <summary>
            /// Returns the name of the protocol supported by this codec, as indicated by the <c>'UPGRADE'</c> header.
            /// </summary>
            ICharSequence Protocol { get; }

            /// <summary>
            /// Sets any protocol-specific headers required to the upgrade request. Returns the names of
            /// all headers that were added. These headers will be used to populate the CONNECTION header.
            /// </summary>
            /// <param name="ctx">the context for the current handler.</param>
            /// <param name="upgradeRequest"></param>
            /// <returns></returns>
            ICollection<ICharSequence> SetUpgradeHeaders(IChannelHandlerContext ctx, IHttpRequest upgradeRequest);

            /// <summary>
            /// Performs an HTTP protocol upgrade from the source codec. This method is responsible for
            /// adding all handlers required for the new protocol.
            /// </summary>
            /// <param name="ctx">the context for the current handler.</param>
            /// <param name="upgradeResponse">the 101 Switching Protocols response that indicates that the server
            /// has switched to this protocol.</param>
            void UpgradeTo(IChannelHandlerContext ctx, IFullHttpResponse upgradeResponse);
        }

        internal static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<HttpClientUpgradeHandler>();

        readonly ISourceCodec sourceCodec;
        readonly IUpgradeCodec upgradeCodec;
        bool upgradeRequested;

        /// <summary>Constructs the client upgrade handler.</summary>
        /// <param name="sourceCodec">the codec that is being used initially.</param>
        /// <param name="upgradeCodec">the codec that the client would like to upgrade to.</param>
        /// <param name="maxContentLength">the maximum length of the aggregated content.</param>
        public HttpClientUpgradeHandler(ISourceCodec sourceCodec, IUpgradeCodec upgradeCodec, int maxContentLength)
            : base(maxContentLength)
        {
            if (sourceCodec is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.sourceCodec); }
            if (upgradeCodec is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.upgradeCodec); }

            this.sourceCodec = sourceCodec;
            this.upgradeCodec = upgradeCodec;
        }

        /// <inheritdoc />
        public override void Write(IChannelHandlerContext context, object message, IPromise promise)
        {
            if (!(message is IHttpRequest request))
            {
                _ = context.WriteAsync(message, promise);
                return;
            }

            if (this.upgradeRequested)
            {
                promise.TrySetException(ThrowHelper.GetInvalidOperationException_Attempting());
                return;
            }

            this.upgradeRequested = true;
            this.SetUpgradeRequestHeaders(context, request);

            // Continue writing the request.
            _ = context.WriteAsync(message, promise);

            // Notify that the upgrade request was issued.
            _ = context.FireUserEventTriggered(UpgradeEvent.UpgradeIssued);
            // Now we wait for the next HTTP response to see if we switch protocols.
        }

        /// <inheritdoc />
        protected override void Decode(IChannelHandlerContext context, IHttpObject message, List<object> output)
        {
            IFullHttpResponse response = null;
            try
            {
                if (!this.upgradeRequested)
                {
                    ThrowHelper.ThrowInvalidOperationException_ReadHttpResponse();
                }

                if (message is IHttpResponse rep)
                {
                    if (!HttpResponseStatus.SwitchingProtocols.Equals(rep.Status))
                    {
                        // The server does not support the requested protocol, just remove this handler
                        // and continue processing HTTP.
                        // NOTE: not releasing the response since we're letting it propagate to the
                        // next handler.
                        _ = context.FireUserEventTriggered(UpgradeEvent.UpgradeRejected);
                        RemoveThisHandler(context);
                        _ = context.FireChannelRead(rep);
                        return;
                    }
                }

                if (message is IFullHttpResponse fullRep)
                {
                    response = fullRep;
                    // Need to retain since the base class will release after returning from this method.
                    _ = response.Retain();
                    output.Add(response);
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

                    Debug.Assert(output.Count == 1);
                    response = (IFullHttpResponse)output[0];
                }

                if (response.Headers.TryGet(HttpHeaderNames.Upgrade, out ICharSequence upgradeHeader) && !AsciiString.ContentEqualsIgnoreCase(this.upgradeCodec.Protocol, upgradeHeader))
                {
                    ThrowHelper.ThrowInvalidOperationException_UnexpectedUpgradeProtocol(upgradeHeader);
                }

                // Upgrade to the new protocol.
                this.sourceCodec.PrepareUpgradeFrom(context);
                this.upgradeCodec.UpgradeTo(context, response);

                // Notify that the upgrade to the new protocol completed successfully.
                _ = context.FireUserEventTriggered(UpgradeEvent.UpgradeSuccessful);

                // We guarantee UPGRADE_SUCCESSFUL event will be arrived at the next handler
                // before http2 setting frame and http response.
                this.sourceCodec.UpgradeFrom(context);

                // We switched protocols, so we're done with the upgrade response.
                // Release it and clear it from the output.
                _ = response.Release();
                output.Clear();
                RemoveThisHandler(context);
            }
            catch (Exception exception)
            {
                _ = ReferenceCountUtil.Release(response);
                _ = context.FireExceptionCaught(exception);
                RemoveThisHandler(context);
            }
        }

        static void RemoveThisHandler(IChannelHandlerContext ctx) => ctx.Pipeline.Remove(ctx.Name);

        /// <summary>
        /// Adds all upgrade request headers necessary for an upgrade to the supported protocols.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        void SetUpgradeRequestHeaders(IChannelHandlerContext ctx, IHttpRequest request)
        {
            // Set the UPGRADE header on the request.
            _ = request.Headers.Set(HttpHeaderNames.Upgrade, this.upgradeCodec.Protocol);

            // Add all protocol-specific headers to the request.
            var connectionParts = new List<ICharSequence>(2);
            connectionParts.AddRange(this.upgradeCodec.SetUpgradeHeaders(ctx, request));

            // Set the CONNECTION header from the set of all protocol-specific headers that were added.
            var builder = StringBuilderManager.Allocate();
            foreach (ICharSequence part in connectionParts)
            {
                _ = builder.Append(part);
                _ = builder.Append(',');
            }
            _ = builder.Append(HttpHeaderValues.Upgrade);
            _ = request.Headers.Add(HttpHeaderNames.Connection, new StringCharSequence(StringBuilderManager.ReturnAndFree(builder)));
        }
    }
}
