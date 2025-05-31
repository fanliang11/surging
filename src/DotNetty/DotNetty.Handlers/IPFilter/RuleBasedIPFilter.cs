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

namespace DotNetty.Handlers.IPFilter
{
    using System;
    using System.Net;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// This class allows one to filter new <see cref="IChannel"/>s based on the
    /// <see cref="IIPFilterRule"/>s passed to its constructor. If no rules are provided, all connections
    /// will be accepted.
    ///
    /// If you would like to explicitly take action on rejected <see cref="IChannel"/>s, you should override
    /// <see cref="AbstractRemoteAddressFilter{IPEndPoint}.ChannelRejected"/>.
    /// </summary>
    public class RuleBasedIPFilter : AbstractRemoteAddressFilter<IPEndPoint>
    {
        readonly IIPFilterRule[] rules;

        public RuleBasedIPFilter(params IIPFilterRule[] rules)
        {
            if (rules is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.rules); }
            this.rules = rules;
        }

        protected override bool Accept(IChannelHandlerContext ctx, IPEndPoint remoteAddress)
        {
            foreach (IIPFilterRule rule in this.rules)
            {
                if (rule is null) { break; }
                if (rule.Matches(remoteAddress))
                {
                    return rule.RuleType == IPFilterRuleType.Accept;
                }
            }
            return true;
        }
    }
}