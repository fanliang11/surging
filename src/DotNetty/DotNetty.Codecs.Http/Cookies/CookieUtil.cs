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
    using System.Collections;
    using System.Text;
    using DotNetty.Common;
    using DotNetty.Common.Utilities;

    static class CookieUtil
    {
        static readonly BitArray ValidCookieNameOctets = GetValidCookieNameOctets();
        static readonly BitArray ValidCookieValueOctects = GetValidCookieValueOctets();
        static readonly BitArray ValidCookieAttributeOctets = GetValidCookieAttributeValueOctets();

        // token = 1*<any CHAR except CTLs or separators>
        // separators = 
        //"(" | ")" | "<" | ">" | "@"
        // | "," | ";" | ":" | "\" | <">
        // | "/" | "[" | "]" | "?" | "="
        // | "{" | "}" | SP | HT
        static BitArray GetValidCookieNameOctets()
        {
            var bitArray = new BitArray(128, false);
            for (int i = 32; i < 127; i++)
            {
                bitArray[i] = true;
            }

            var separators = new int[]
                { '(', ')', '<', '>', '@', ',', ';', ':', '\\', '"', '/', '[', ']', '?', '=', '{', '}', ' ', '\t' };
            foreach (int separator in separators)
            {
                bitArray[separator] = false;
            }

            return bitArray;
        }

        // cookie-octet = %x21 / %x23-2B / %x2D-3A / %x3C-5B / %x5D-7E
        // US-ASCII characters excluding CTLs, whitespace, DQUOTE, comma, semicolon, and backslash
        static BitArray GetValidCookieValueOctets()
        {
            var bitArray = new BitArray(128, false);
            bitArray[0x21] = true;
            for (int i = 0x23; i <= 0x2B; i++)
            {
                bitArray[i] = true;
            }
            for (int i = 0x2D; i <= 0x3A; i++)
            {
                bitArray[i] = true;
            }
            for (int i = 0x3C; i <= 0x5B; i++)
            {
                bitArray[i] = true;
            }
            for (int i = 0x5D; i <= 0x7E; i++)
            {
                bitArray[i] = true;
            }

            return bitArray;
        }

        // path-value        = <any CHAR except CTLs or ";">
        static BitArray GetValidCookieAttributeValueOctets()
        {
            var bitArray = new BitArray(128, false);
            for (int i = 32; i < 127; i++)
            {
                bitArray[i] = true;
            }
            bitArray[';'] = false;

            return bitArray;
        }

        internal static StringBuilder StringBuilder() => InternalThreadLocalMap.Get().StringBuilder;

        internal static string StripTrailingSeparatorOrNull(StringBuilder buf) =>
            0u >= (uint)buf.Length ? null : StripTrailingSeparator(buf);

        internal static string StripTrailingSeparator(StringBuilder buf)
        {
            if ((uint)buf.Length > 0u)
            {
                buf.Length -= 2;
            }

            return buf.ToString();
        }

        internal static void Add(StringBuilder sb, string name, long val)
        {
            _ = sb
                .Append(name)
                .Append(HttpConstants.EqualsSignChar)
                .Append(val)
                .Append(HttpConstants.SemicolonChar)
                .Append(HttpConstants.HorizontalSpaceChar);
        }

        internal static void Add(StringBuilder sb, string name, string val)
        {
            _ = sb
                .Append(name)
                .Append(HttpConstants.EqualsSignChar)
                .Append(val)
                .Append(HttpConstants.SemicolonChar)
                .Append(HttpConstants.HorizontalSpaceChar);
        }

        internal static void Add(StringBuilder sb, string name)
        {
            _ = sb
                .Append(name)
                .Append(HttpConstants.SemicolonChar)
                .Append(HttpConstants.HorizontalSpaceChar);
        }

        internal static void AddQuoted(StringBuilder sb, string name, string val)
        {
            if (val is null) { val = string.Empty; }

            _ = sb
                .Append(name)
                .Append(HttpConstants.EqualsSignChar)
                .Append(HttpConstants.DoubleQuoteChar)
                .Append(val)
                .Append(HttpConstants.DoubleQuoteChar)
                .Append(HttpConstants.SemicolonChar)
                .Append(HttpConstants.HorizontalSpaceChar);
        }

        internal static int FirstInvalidCookieNameOctet(string cs) => FirstInvalidOctet(cs, ValidCookieNameOctets);

        internal static int FirstInvalidCookieValueOctet(ICharSequence cs) => FirstInvalidOctet(cs, ValidCookieValueOctects);

        static int FirstInvalidOctet(string cs, BitArray bits)
        {
            for (int i = 0; i < cs.Length; i++)
            {
                char c = cs[i];
                if (!bits[c])
                {
                    return i;
                }
            }
            return -1;
        }

        static int FirstInvalidOctet(ICharSequence cs, BitArray bits)
        {
            for (int i = 0; i < cs.Count; i++)
            {
                char c = cs[i];
                if (!bits[c])
                {
                    return i;
                }
            }
            return -1;
        }

        internal static ICharSequence UnwrapValue(ICharSequence cs)
        {
            int len = cs.Count;
            if (len > 0 && cs[0] == HttpConstants.DoubleQuoteChar)
            {
                if (len >= 2 && cs[len - 1] == HttpConstants.DoubleQuoteChar)
                {
                    // properly balanced
                    return len == 2 ? StringCharSequence.Empty : cs.SubSequence(1, len - 1);
                }
                else
                {
                    return null;
                }
            }

            return cs;
        }

        internal static string ValidateAttributeValue(string name, string value)
        {
            value = value?.Trim();
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            int i = FirstInvalidOctet(value, ValidCookieAttributeOctets);
            if (i != -1)
            {
                ThrowHelper.ThrowArgumentException_ValidateAttrValue(name, value, i);
            }

            return value;
        }
    }
}
