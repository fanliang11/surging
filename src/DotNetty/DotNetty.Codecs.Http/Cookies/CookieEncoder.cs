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
    using System;
    using DotNetty.Common.Utilities;

    using static CookieUtil;

    public abstract class CookieEncoder
    {
        protected readonly bool Strict;

        protected CookieEncoder(bool strict)
        {
            this.Strict = strict;
        }

        protected void ValidateCookie(string name, string value)
        {
            if (!this.Strict)
            {
                return;
            }

            int pos;
            if ((pos = FirstInvalidCookieNameOctet(name)) >= 0)
            {
                ThrowHelper.ThrowArgumentException_CookieName(name, pos);
            }

            var sequnce = new StringCharSequence(value);
            ICharSequence unwrappedValue = UnwrapValue(sequnce);
            if (unwrappedValue is null)
            {
                ThrowHelper.ThrowArgumentException_CookieValue(value);
            }

            if ((pos = FirstInvalidCookieValueOctet(unwrappedValue)) >= 0)
            {
                ThrowHelper.ThrowArgumentException_CookieValue(unwrappedValue, pos);
            }
        }
    }
}
