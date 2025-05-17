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
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs.Http.Cookies
{
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Common.Utilities;

    using static CookieUtil;

    public abstract class CookieDecoder
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<CookieDecoder>();
        protected readonly bool Strict;

        protected CookieDecoder(bool strict)
        {
            this.Strict = strict;
        }

        protected DefaultCookie InitCookie(string header, int nameBegin, int nameEnd, int valueBegin, int valueEnd)
        {
            if (nameBegin == -1 || nameBegin == nameEnd)
            {
#if DEBUG
                if (Logger.DebugEnabled) Logger.SkippingCookieWithNullName();
#endif
                return null;
            }

            if (valueBegin == -1)
            {
#if DEBUG
                if (Logger.DebugEnabled) Logger.SkippingCookieWithNullValue();
#endif
                return null;
            }

            var sequence = new StringCharSequence(header, valueBegin, valueEnd - valueBegin);
            ICharSequence unwrappedValue = UnwrapValue(sequence);
            if (unwrappedValue is null)
            {
#if DEBUG
                if (Logger.DebugEnabled) Logger.SkippingCookieBecauseStartingQuotesAreNotProperlyBalancedIn(sequence);
#endif
                return null;
            }

            string name = header.Substring(nameBegin, nameEnd - nameBegin);

            int invalidOctetPos;
            if (this.Strict && (invalidOctetPos = FirstInvalidCookieNameOctet(name)) >= 0)
            {
#if DEBUG
                if (Logger.DebugEnabled)
                {
                    Logger.SkippingCookieBecauseNameContainsInvalidChar(name, invalidOctetPos);
                }
#endif
                return null;
            }

            bool wrap = unwrappedValue.Count != valueEnd - valueBegin;

            if (this.Strict && (invalidOctetPos = FirstInvalidCookieValueOctet(unwrappedValue)) >= 0)
            {
#if DEBUG
                if (Logger.DebugEnabled)
                {
                    Logger.SkippingCookieBecauseValueContainsInvalidChar(unwrappedValue, invalidOctetPos);
                }
#endif

                return null;
            }

            var cookie = new DefaultCookie(name, unwrappedValue.ToString());
            cookie.Wrap = wrap;
            return cookie;
        }
    }
}
