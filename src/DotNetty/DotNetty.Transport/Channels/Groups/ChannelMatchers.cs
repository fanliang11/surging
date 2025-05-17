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

namespace DotNetty.Transport.Channels.Groups
{
    using System;

    public static class ChannelMatchers
    {
        static readonly IChannelMatcher AllMatcher;
        static readonly IChannelMatcher ServerChannelMatcher;
        static readonly IChannelMatcher NonServerChannelMatcher;

        static ChannelMatchers()
        {
            AllMatcher = new AllChannelMatcher();
            ServerChannelMatcher = IsInstanceOf(typeof(IServerChannel));
            NonServerChannelMatcher = IsNotInstanceOf(typeof(IServerChannel));
        }

        public static IChannelMatcher IsServerChannel() => ServerChannelMatcher;

        public static IChannelMatcher IsNonServerChannel() => NonServerChannelMatcher;

        public static IChannelMatcher All() => AllMatcher;

        public static IChannelMatcher IsNot(IChannel channel) => Invert(Is(channel));

        public static IChannelMatcher Is(IChannel channel) => new InstanceMatcher(channel);

        public static IChannelMatcher IsInstanceOf(Type type) => new TypeMatcher(type);

        public static IChannelMatcher IsNotInstanceOf(Type type) => Invert(IsInstanceOf(type));

        public static IChannelMatcher Invert(IChannelMatcher matcher) => new InvertMatcher(matcher);

        public static IChannelMatcher Compose(params IChannelMatcher[] matchers)
        {
            if (0u >= (uint)matchers.Length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.matchers);
            }
            if (1 == matchers.Length)
            {
                return matchers[0];
            }
            return new CompositeMatcher(matchers);
        }

        sealed class AllChannelMatcher : IChannelMatcher
        {
            public bool Matches(IChannel channel) => true;
        }

        sealed class CompositeMatcher : IChannelMatcher
        {
            readonly IChannelMatcher[] _matchers;

            public CompositeMatcher(params IChannelMatcher[] matchers)
            {
                _matchers = matchers;
            }

            public bool Matches(IChannel channel)
            {
                for (int i = 0; i < _matchers.Length; i++)
                {
                    if (!_matchers[i].Matches(channel))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        sealed class InvertMatcher : IChannelMatcher
        {
            readonly IChannelMatcher _matcher;

            public InvertMatcher(IChannelMatcher matcher)
            {
                _matcher = matcher;
            }

            public bool Matches(IChannel channel) => !_matcher.Matches(channel);
        }

        sealed class InstanceMatcher : IChannelMatcher
        {
            readonly IChannel _channel;

            public InstanceMatcher(IChannel channel)
            {
                _channel = channel;
            }

            public bool Matches(IChannel ch) => _channel == ch;
        }

        sealed class TypeMatcher : IChannelMatcher
        {
            readonly Type _type;

            public TypeMatcher(Type clazz)
            {
                _type = clazz;
            }

            public bool Matches(IChannel channel) => _type.IsInstanceOfType(channel);
        }
    }
}