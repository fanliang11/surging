// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotNetty.Codecs.Http.Utilities
{
    internal static class HttpEncoderUtility
    {
        internal static readonly HashSet<char> UrlSafeChars;
        private static readonly HashSet<char> s_digitalHexChars;
        private static readonly HashSet<char> s_uppercaseHexChars;
        private static readonly HashSet<char> s_lowercaseHexChars;

        static HttpEncoderUtility()
        {
            UrlSafeChars = new HashSet<char>() { '-', '_', '.', '!', '*', '(', ')' };
            s_digitalHexChars = new HashSet<char>();
            s_uppercaseHexChars = new HashSet<char>();
            s_lowercaseHexChars = new HashSet<char>();

            for (var ch = '0'; ch <= '9'; ch++)
            {
                _ = UrlSafeChars.Add(ch);
                _ = s_digitalHexChars.Add(ch);
            }
            for (var ch = 'a'; ch <= 'z'; ch++)
            {
                _ = UrlSafeChars.Add(ch);
                if (ch <= 'f') { _ = s_lowercaseHexChars.Add(ch); }
            }
            for (var ch = 'A'; ch <= 'Z'; ch++)
            {
                _ = UrlSafeChars.Add(ch);
                if (ch <= 'F') { _ = s_uppercaseHexChars.Add(ch); }
            }
        }

        public static int HexToInt(char h)
        {
            const int char_0 = '0';
            const int char_a = 'a' - 10;
            const int char_A = 'A' - 10;

            if (s_digitalHexChars.Contains(h)) { return h - char_0; }
            if (s_lowercaseHexChars.Contains(h)) { return h - char_a; }
            if (s_uppercaseHexChars.Contains(h)) { return h - char_A; }

            return -1;
            //return h >= '0' && h <= '9'
            //    ? h - '0'
            //    : h >= 'a' && h <= 'f'
            //        ? h - 'a' + 10
            //        : h >= 'A' && h <= 'F'
            //            ? h - 'A' + 10
            //            : -1;
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static char IntToHex(int n)
        {
            Debug.Assert(n < 0x10);

            return n < 10 ? (char)(n + '0') : (char)(n - 10 + 'a');
        }

        // Set of safe chars, from RFC 1738.4 minus '+'
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static bool IsUrlSafeChar(char ch)
        {
            return UrlSafeChars.Contains(ch);
            //if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9'))
            //{
            //    return true;
            //}

            //switch (ch)
            //{
            //    case '-':
            //    case '_':
            //    case '.':
            //    case '!':
            //    case '*':
            //    case '(':
            //    case ')':
            //        return true;
            //}

            //return false;
        }

        //  Helper to encode spaces only
#if NETCOREAPP_3_0_GREATER || NETSTANDARD_2_0_GREATER
        internal static string UrlEncodeSpaces(string str) => str is object && str.Contains(' ') ? str.Replace(" ", "%20") : str;
#else
        internal static string UrlEncodeSpaces(string str) => str is object && str.IndexOf(' ') >= 0 ? str.Replace(" ", "%20") : str;
#endif
    }
}
