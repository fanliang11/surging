using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using DotNetty.Common.Internal;

namespace DotNetty.Codecs.Http.Utilities
{
    partial class HttpUtility
    {
        #region -- HtmlEncode --

        private static readonly HtmlEncoder s_htmlEncoder = HtmlEncoder.Default;

        public static string HtmlEncode(string value)
        {
            if (string.IsNullOrEmpty(value)) { return value; }

            return s_htmlEncoder.Encode(value);
        }

        public static void HtmlEncode(TextWriter output, string value)
        {
            s_htmlEncoder.Encode(output, value, 0, value.Length);
        }

        public static void HtmlEncode(TextWriter output, string value, int startIndex, int characterCount)
        {
            if (string.IsNullOrEmpty(value)) { return; }

            s_htmlEncoder.Encode(output, value, startIndex, characterCount);
        }

        public static void HtmlEncode(TextWriter output, char[] value, int startIndex, int characterCount)
        {
            if (value is null || 0u >= (uint)value.Length) { return; }

            s_htmlEncoder.Encode(output, value, startIndex, characterCount);
        }

        #endregion

        #region -- HtmlAttributeEncode --

        public static string HtmlAttributeEncode(string value)
        {
            if (string.IsNullOrEmpty(value)) { return value; }

            // Don't create string writer if we don't have nothing to encode
            int pos = IndexOfHtmlAttributeEncodingChars(value, 0);
            if (pos == -1) { return value; }

            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            HtmlAttributeEncode(value, writer);
            return writer.ToString();
        }

        public static void HtmlAttributeEncode(string value, TextWriter output)
        {
            if (value is null) { return; }
            if (output is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.output); }

            HtmlAttributeEncodeInternal(value, output);
        }

        private static unsafe void HtmlAttributeEncodeInternal(string s, TextWriter output)
        {
            int index = IndexOfHtmlAttributeEncodingChars(s, 0);
            if (index == -1)
            {
                output.Write(s);
            }
            else
            {
                int cch = s.Length - index;
                fixed (char* str = s)
                {
                    char* pch = str;
                    while (index-- > 0)
                    {
                        output.Write(*pch++);
                    }

                    while (cch-- > 0)
                    {
                        char ch = *pch++;
                        if (ch <= '<')
                        {
                            switch (ch)
                            {
                                case '<':
                                    output.Write("&lt;");
                                    break;
                                case '"':
                                    output.Write("&quot;");
                                    break;
                                case '\'':
                                    output.Write("&#39;");
                                    break;
                                case '&':
                                    output.Write("&amp;");
                                    break;
                                default:
                                    output.Write(ch);
                                    break;
                            }
                        }
                        else
                        {
                            output.Write(ch);
                        }
                    }
                }
            }
        }

        #endregion

        #region ** IndexOfHtmlAttributeEncodingChars **

        private static unsafe int IndexOfHtmlAttributeEncodingChars(string s, int startPos)
        {
            Debug.Assert(0 <= startPos && startPos <= s.Length, "0 <= startPos && startPos <= s.Length");
            int cch = s.Length - startPos;
            fixed (char* str = s)
            {
                for (char* pch = &str[startPos]; cch > 0; pch++, cch--)
                {
                    char ch = *pch;
                    if (ch <= '<')
                    {
                        switch (ch)
                        {
                            case '<':
                            case '"':
                            case '\'':
                            case '&':
                                return s.Length - cch;
                        }
                    }
                }
            }

            return -1;
        }

        #endregion

        #region -- HtmlDecode --

        private const char HIGH_SURROGATE_START = '\uD800';
        private const char LOW_SURROGATE_START = '\uDC00';
        private const char LOW_SURROGATE_END = '\uDFFF';
        private const int UNICODE_PLANE00_END = 0x00FFFF;
        private const int UNICODE_PLANE01_START = 0x10000;
        private const int UNICODE_PLANE16_END = 0x10FFFF;

        private const int UnicodeReplacementChar = '\uFFFD';

        private static readonly char[] s_htmlEntityEndingChars = new char[] { ';', '&' };

        public static string HtmlDecode(string value)
        {
            if (string.IsNullOrEmpty(value)) { return value; }

            // Don't create StringBuilder if we don't have anything to encode
            if (!StringRequiresHtmlDecoding(value)) { return value; }

            var sb = StringBuilderManager.Allocate(value.Length);
            HtmlDecodeImpl(value, sb);
            return StringBuilderManager.ReturnAndFree(sb);
        }

        public static void HtmlDecode(string value, TextWriter output)
        {
            if (value is null) { return; }
            if (output is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.output); }

            output.Write(HtmlDecode(value));
        }

        #endregion

        #region ** HtmlDecodeImpl **

        private static void HtmlDecodeImpl(string value, StringBuilder output)
        {
            Debug.Assert(output is object);

            int l = value.Length;
            for (int i = 0; i < l; i++)
            {
                char ch = value[i];

                if (ch == '&')
                {
                    // We found a '&'. Now look for the next ';' or '&'. The idea is that
                    // if we find another '&' before finding a ';', then this is not an entity,
                    // and the next '&' might start a real entity (VSWhidbey 275184)
                    int index = value.IndexOfAny(s_htmlEntityEndingChars, i + 1);
                    if (index > 0 && value[index] == ';')
                    {
                        int entityOffset = i + 1;
                        int entityLength = index - entityOffset;

                        if (entityLength > 1 && value[entityOffset] == '#')
                        {
                            // The # syntax can be in decimal or hex, e.g.
                            //      &#229;  --> decimal
                            //      &#xE5;  --> same char in hex
                            // See http://www.w3.org/TR/REC-html40/charset.html#entities

                            bool parsedSuccessfully;
                            uint parsedValue;
                            if (value[entityOffset + 1] == 'x' || value[entityOffset + 1] == 'X')
                            {
                                parsedSuccessfully = uint.TryParse(value.Substring(entityOffset + 2, entityLength - 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out parsedValue);
                            }
                            else
                            {
                                parsedSuccessfully = uint.TryParse(value.Substring(entityOffset + 1, entityLength - 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedValue);
                            }

                            if (parsedSuccessfully)
                            {
                                // decoded character must be U+0000 .. U+10FFFF, excluding surrogates
                                parsedSuccessfully = ((parsedValue < HIGH_SURROGATE_START) || (LOW_SURROGATE_END < parsedValue && parsedValue <= UNICODE_PLANE16_END));
                            }

                            if (parsedSuccessfully)
                            {
                                if (parsedValue <= UNICODE_PLANE00_END)
                                {
                                    // single character
                                    _ = output.Append((char)parsedValue);
                                }
                                else
                                {
                                    // multi-character
                                    char leadingSurrogate, trailingSurrogate;
                                    ConvertSmpToUtf16(parsedValue, out leadingSurrogate, out trailingSurrogate);
                                    _ = output.Append(leadingSurrogate);
                                    _ = output.Append(trailingSurrogate);
                                }

                                i = index; // already looked at everything until semicolon
                                continue;
                            }
                        }
                        else
                        {
                            string entity = value.Substring(entityOffset, entityLength);
                            i = index; // already looked at everything until semicolon

                            char entityChar = HtmlEntities.Lookup(entity);
                            if (entityChar != (char)0)
                            {
                                ch = entityChar;
                            }
                            else
                            {
                                _ = output.Append('&');
                                _ = output.Append(entity);
                                _ = output.Append(';');
                                continue;
                            }
                        }
                    }
                }

                _ = output.Append(ch);
            }
        }

        #endregion

        #region ** ConvertSmpToUtf16 **

        // similar to Char.ConvertFromUtf32, but doesn't check arguments or generate strings
        // input is assumed to be an SMP character
        private static void ConvertSmpToUtf16(uint smpChar, out char leadingSurrogate, out char trailingSurrogate)
        {
            Debug.Assert(UNICODE_PLANE01_START <= smpChar && smpChar <= UNICODE_PLANE16_END);

            int utf32 = (int)(smpChar - UNICODE_PLANE01_START);
            leadingSurrogate = (char)((utf32 / 0x400) + HIGH_SURROGATE_START);
            trailingSurrogate = (char)((utf32 % 0x400) + LOW_SURROGATE_START);
        }

        #endregion

        #region ** StringRequiresHtmlDecoding **

        private static bool StringRequiresHtmlDecoding(string s)
        {
            // this string requires html decoding if it contains '&' or a surrogate character
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '&' || Char.IsSurrogate(c))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region ** class HtmlEntities **

        // helper class for lookup of HTML encoding entities
        private static class HtmlEntities
        {
#if DEBUG
            static HtmlEntities()
            {
                // Make sure the initial capacity for s_lookupTable is correct
                Debug.Assert(s_lookupTable.Count == Count, $"There should be {Count} HTML entities, but {nameof(s_lookupTable)} has {s_lookupTable.Count} of them.");
            }
#endif

            // The list is from http://www.w3.org/TR/REC-html40/sgml/entities.html, except for &apos;, which
            // is defined in http://www.w3.org/TR/2008/REC-xml-20081126/#sec-predefined-ent.

            private const int Count = 253;

            // maps entity strings => unicode chars
            private static readonly Dictionary<string, char> s_lookupTable =
                new Dictionary<string, char>(Count, StringComparer.Ordinal)
                {
                    ["quot"] = '\x0022',
                    ["amp"] = '\x0026',
                    ["apos"] = '\x0027',
                    ["lt"] = '\x003c',
                    ["gt"] = '\x003e',
                    ["nbsp"] = '\x00a0',
                    ["iexcl"] = '\x00a1',
                    ["cent"] = '\x00a2',
                    ["pound"] = '\x00a3',
                    ["curren"] = '\x00a4',
                    ["yen"] = '\x00a5',
                    ["brvbar"] = '\x00a6',
                    ["sect"] = '\x00a7',
                    ["uml"] = '\x00a8',
                    ["copy"] = '\x00a9',
                    ["ordf"] = '\x00aa',
                    ["laquo"] = '\x00ab',
                    ["not"] = '\x00ac',
                    ["shy"] = '\x00ad',
                    ["reg"] = '\x00ae',
                    ["macr"] = '\x00af',
                    ["deg"] = '\x00b0',
                    ["plusmn"] = '\x00b1',
                    ["sup2"] = '\x00b2',
                    ["sup3"] = '\x00b3',
                    ["acute"] = '\x00b4',
                    ["micro"] = '\x00b5',
                    ["para"] = '\x00b6',
                    ["middot"] = '\x00b7',
                    ["cedil"] = '\x00b8',
                    ["sup1"] = '\x00b9',
                    ["ordm"] = '\x00ba',
                    ["raquo"] = '\x00bb',
                    ["frac14"] = '\x00bc',
                    ["frac12"] = '\x00bd',
                    ["frac34"] = '\x00be',
                    ["iquest"] = '\x00bf',
                    ["Agrave"] = '\x00c0',
                    ["Aacute"] = '\x00c1',
                    ["Acirc"] = '\x00c2',
                    ["Atilde"] = '\x00c3',
                    ["Auml"] = '\x00c4',
                    ["Aring"] = '\x00c5',
                    ["AElig"] = '\x00c6',
                    ["Ccedil"] = '\x00c7',
                    ["Egrave"] = '\x00c8',
                    ["Eacute"] = '\x00c9',
                    ["Ecirc"] = '\x00ca',
                    ["Euml"] = '\x00cb',
                    ["Igrave"] = '\x00cc',
                    ["Iacute"] = '\x00cd',
                    ["Icirc"] = '\x00ce',
                    ["Iuml"] = '\x00cf',
                    ["ETH"] = '\x00d0',
                    ["Ntilde"] = '\x00d1',
                    ["Ograve"] = '\x00d2',
                    ["Oacute"] = '\x00d3',
                    ["Ocirc"] = '\x00d4',
                    ["Otilde"] = '\x00d5',
                    ["Ouml"] = '\x00d6',
                    ["times"] = '\x00d7',
                    ["Oslash"] = '\x00d8',
                    ["Ugrave"] = '\x00d9',
                    ["Uacute"] = '\x00da',
                    ["Ucirc"] = '\x00db',
                    ["Uuml"] = '\x00dc',
                    ["Yacute"] = '\x00dd',
                    ["THORN"] = '\x00de',
                    ["szlig"] = '\x00df',
                    ["agrave"] = '\x00e0',
                    ["aacute"] = '\x00e1',
                    ["acirc"] = '\x00e2',
                    ["atilde"] = '\x00e3',
                    ["auml"] = '\x00e4',
                    ["aring"] = '\x00e5',
                    ["aelig"] = '\x00e6',
                    ["ccedil"] = '\x00e7',
                    ["egrave"] = '\x00e8',
                    ["eacute"] = '\x00e9',
                    ["ecirc"] = '\x00ea',
                    ["euml"] = '\x00eb',
                    ["igrave"] = '\x00ec',
                    ["iacute"] = '\x00ed',
                    ["icirc"] = '\x00ee',
                    ["iuml"] = '\x00ef',
                    ["eth"] = '\x00f0',
                    ["ntilde"] = '\x00f1',
                    ["ograve"] = '\x00f2',
                    ["oacute"] = '\x00f3',
                    ["ocirc"] = '\x00f4',
                    ["otilde"] = '\x00f5',
                    ["ouml"] = '\x00f6',
                    ["divide"] = '\x00f7',
                    ["oslash"] = '\x00f8',
                    ["ugrave"] = '\x00f9',
                    ["uacute"] = '\x00fa',
                    ["ucirc"] = '\x00fb',
                    ["uuml"] = '\x00fc',
                    ["yacute"] = '\x00fd',
                    ["thorn"] = '\x00fe',
                    ["yuml"] = '\x00ff',
                    ["OElig"] = '\x0152',
                    ["oelig"] = '\x0153',
                    ["Scaron"] = '\x0160',
                    ["scaron"] = '\x0161',
                    ["Yuml"] = '\x0178',
                    ["fnof"] = '\x0192',
                    ["circ"] = '\x02c6',
                    ["tilde"] = '\x02dc',
                    ["Alpha"] = '\x0391',
                    ["Beta"] = '\x0392',
                    ["Gamma"] = '\x0393',
                    ["Delta"] = '\x0394',
                    ["Epsilon"] = '\x0395',
                    ["Zeta"] = '\x0396',
                    ["Eta"] = '\x0397',
                    ["Theta"] = '\x0398',
                    ["Iota"] = '\x0399',
                    ["Kappa"] = '\x039a',
                    ["Lambda"] = '\x039b',
                    ["Mu"] = '\x039c',
                    ["Nu"] = '\x039d',
                    ["Xi"] = '\x039e',
                    ["Omicron"] = '\x039f',
                    ["Pi"] = '\x03a0',
                    ["Rho"] = '\x03a1',
                    ["Sigma"] = '\x03a3',
                    ["Tau"] = '\x03a4',
                    ["Upsilon"] = '\x03a5',
                    ["Phi"] = '\x03a6',
                    ["Chi"] = '\x03a7',
                    ["Psi"] = '\x03a8',
                    ["Omega"] = '\x03a9',
                    ["alpha"] = '\x03b1',
                    ["beta"] = '\x03b2',
                    ["gamma"] = '\x03b3',
                    ["delta"] = '\x03b4',
                    ["epsilon"] = '\x03b5',
                    ["zeta"] = '\x03b6',
                    ["eta"] = '\x03b7',
                    ["theta"] = '\x03b8',
                    ["iota"] = '\x03b9',
                    ["kappa"] = '\x03ba',
                    ["lambda"] = '\x03bb',
                    ["mu"] = '\x03bc',
                    ["nu"] = '\x03bd',
                    ["xi"] = '\x03be',
                    ["omicron"] = '\x03bf',
                    ["pi"] = '\x03c0',
                    ["rho"] = '\x03c1',
                    ["sigmaf"] = '\x03c2',
                    ["sigma"] = '\x03c3',
                    ["tau"] = '\x03c4',
                    ["upsilon"] = '\x03c5',
                    ["phi"] = '\x03c6',
                    ["chi"] = '\x03c7',
                    ["psi"] = '\x03c8',
                    ["omega"] = '\x03c9',
                    ["thetasym"] = '\x03d1',
                    ["upsih"] = '\x03d2',
                    ["piv"] = '\x03d6',
                    ["ensp"] = '\x2002',
                    ["emsp"] = '\x2003',
                    ["thinsp"] = '\x2009',
                    ["zwnj"] = '\x200c',
                    ["zwj"] = '\x200d',
                    ["lrm"] = '\x200e',
                    ["rlm"] = '\x200f',
                    ["ndash"] = '\x2013',
                    ["mdash"] = '\x2014',
                    ["lsquo"] = '\x2018',
                    ["rsquo"] = '\x2019',
                    ["sbquo"] = '\x201a',
                    ["ldquo"] = '\x201c',
                    ["rdquo"] = '\x201d',
                    ["bdquo"] = '\x201e',
                    ["dagger"] = '\x2020',
                    ["Dagger"] = '\x2021',
                    ["bull"] = '\x2022',
                    ["hellip"] = '\x2026',
                    ["permil"] = '\x2030',
                    ["prime"] = '\x2032',
                    ["Prime"] = '\x2033',
                    ["lsaquo"] = '\x2039',
                    ["rsaquo"] = '\x203a',
                    ["oline"] = '\x203e',
                    ["frasl"] = '\x2044',
                    ["euro"] = '\x20ac',
                    ["image"] = '\x2111',
                    ["weierp"] = '\x2118',
                    ["real"] = '\x211c',
                    ["trade"] = '\x2122',
                    ["alefsym"] = '\x2135',
                    ["larr"] = '\x2190',
                    ["uarr"] = '\x2191',
                    ["rarr"] = '\x2192',
                    ["darr"] = '\x2193',
                    ["harr"] = '\x2194',
                    ["crarr"] = '\x21b5',
                    ["lArr"] = '\x21d0',
                    ["uArr"] = '\x21d1',
                    ["rArr"] = '\x21d2',
                    ["dArr"] = '\x21d3',
                    ["hArr"] = '\x21d4',
                    ["forall"] = '\x2200',
                    ["part"] = '\x2202',
                    ["exist"] = '\x2203',
                    ["empty"] = '\x2205',
                    ["nabla"] = '\x2207',
                    ["isin"] = '\x2208',
                    ["notin"] = '\x2209',
                    ["ni"] = '\x220b',
                    ["prod"] = '\x220f',
                    ["sum"] = '\x2211',
                    ["minus"] = '\x2212',
                    ["lowast"] = '\x2217',
                    ["radic"] = '\x221a',
                    ["prop"] = '\x221d',
                    ["infin"] = '\x221e',
                    ["ang"] = '\x2220',
                    ["and"] = '\x2227',
                    ["or"] = '\x2228',
                    ["cap"] = '\x2229',
                    ["cup"] = '\x222a',
                    ["int"] = '\x222b',
                    ["there4"] = '\x2234',
                    ["sim"] = '\x223c',
                    ["cong"] = '\x2245',
                    ["asymp"] = '\x2248',
                    ["ne"] = '\x2260',
                    ["equiv"] = '\x2261',
                    ["le"] = '\x2264',
                    ["ge"] = '\x2265',
                    ["sub"] = '\x2282',
                    ["sup"] = '\x2283',
                    ["nsub"] = '\x2284',
                    ["sube"] = '\x2286',
                    ["supe"] = '\x2287',
                    ["oplus"] = '\x2295',
                    ["otimes"] = '\x2297',
                    ["perp"] = '\x22a5',
                    ["sdot"] = '\x22c5',
                    ["lceil"] = '\x2308',
                    ["rceil"] = '\x2309',
                    ["lfloor"] = '\x230a',
                    ["rfloor"] = '\x230b',
                    ["lang"] = '\x2329',
                    ["rang"] = '\x232a',
                    ["loz"] = '\x25ca',
                    ["spades"] = '\x2660',
                    ["clubs"] = '\x2663',
                    ["hearts"] = '\x2665',
                    ["diams"] = '\x2666',
                };

            public static char Lookup(string entity)
            {
                char theChar;
                _ = s_lookupTable.TryGetValue(entity, out theChar);
                return theChar;
            }
        }

        #endregion
    }
}
