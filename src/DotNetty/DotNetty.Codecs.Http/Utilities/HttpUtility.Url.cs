using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using DotNetty.Common.Internal;

namespace DotNetty.Codecs.Http.Utilities
{
    partial class HttpUtility
    {
        #region -- UrlEncode --

        private static readonly UrlEncoder s_urlEncoder = UrlEncoder.Default;

        public static string UrlEncode(string value)
        {
            if (string.IsNullOrEmpty(value)) { return value; }

            return s_urlEncoder.Encode(value);
        }

        public static void UrlEncode(TextWriter output, string value)
        {
            s_urlEncoder.Encode(output, value, 0, value.Length);
        }

        public static void UrlEncode(TextWriter output, string value, int startIndex, int characterCount)
        {
            if (string.IsNullOrEmpty(value)) { return; }

            s_urlEncoder.Encode(output, value, startIndex, characterCount);
        }

        public static void UrlEncode(TextWriter output, char[] value, int startIndex, int characterCount)
        {
            if (value is null || 0u >= (uint)value.Length) { return; }

            s_urlEncoder.Encode(output, value, startIndex, characterCount);
        }

        public static string UrlEncode(string str, Encoding e)
        {
            if (str is null) { return null; }
#if !DEBUG
            if (e is null || TextEncodings.UTF8CodePage == e.CodePage)
            {
                return s_urlEncoder.Encode(str);
            }
#endif

            var bytes = e.GetBytes(str);
            var encoded = UrlEncodeToBytesImpl(bytes, 0, bytes.Length);//, alwaysCreateNewReturnValue: false);
            return Encoding.ASCII.GetString(encoded);
        }

        public static string UrlEncode(byte[] bytes)
            => bytes is null ? null : Encoding.ASCII.GetString(UrlEncodeToBytesImpl(bytes, 0, bytes.Length, alwaysCreateNewReturnValue: true));

        public static string UrlEncode(byte[] bytes, int offset, int count)
        {
            if (!ValidateUrlEncodingParameters(bytes, offset, count)) { return null; }

            return Encoding.ASCII.GetString(UrlEncodeToBytesImpl(bytes, offset, count, alwaysCreateNewReturnValue: true));
        }

        #endregion

        #region -- UrlEncodeToBytes --

        public static byte[] UrlEncodeToBytes(string str) => UrlEncodeToBytes(str, Encoding.UTF8);

        public static byte[] UrlEncodeToBytes(string str, Encoding e)
        {
            if (str is null) { return null; }
            if (e is null) { e = Encoding.UTF8; }

            byte[] bytes = e.GetBytes(str);
            return UrlEncodeToBytesImpl(bytes, 0, bytes.Length);//, alwaysCreateNewReturnValue: false);
        }

        public static byte[] UrlEncodeToBytes(byte[] bytes)
            => bytes is null ? null : UrlEncodeToBytesImpl(bytes, 0, bytes.Length, alwaysCreateNewReturnValue: true);

        public static byte[] UrlEncodeToBytes(byte[] bytes, int offset, int count)
        {
            if (!ValidateUrlEncodingParameters(bytes, offset, count)) { return null; }

            return UrlEncodeToBytesImpl(bytes, offset, count, alwaysCreateNewReturnValue: true);
        }

        #endregion

        #region ** UrlEncodeToBytesImpl **

        [MethodImpl(InlineMethod.AggressiveInlining)]
        private static byte[] UrlEncodeToBytesImpl(byte[] bytes, int offset, int count, bool alwaysCreateNewReturnValue)
        {
            byte[] encoded = UrlEncodeToBytesImpl(bytes, offset, count);

            return (alwaysCreateNewReturnValue && (encoded is object) && (encoded == bytes))
                ? (byte[])encoded.Clone()
                : encoded;
        }

        private static byte[] UrlEncodeToBytesImpl(byte[] bytes, int offset, int count)
        {
            int cSpaces = 0;
            int cUnsafe = 0;

            // count them first
            for (int i = 0; i < count; i++)
            {
                char ch = (char)bytes[offset + i];

                if (ch == ' ')
                {
                    cSpaces++;
                }
                else if (!HttpEncoderUtility.IsUrlSafeChar(ch))
                {
                    cUnsafe++;
                }
            }

            // nothing to expand?
            if (0u >= (uint)cSpaces && 0u >= (uint)cUnsafe)
            {
                // DevDiv 912606: respect "offset" and "count"
                if (0 == offset && bytes.Length == count)
                {
                    return bytes;
                }
                else
                {
                    byte[] subarray = new byte[count];
                    Buffer.BlockCopy(bytes, offset, subarray, 0, count);
                    return subarray;
                }
            }

            // expand not 'safe' characters into %XX, spaces to +s
            byte[] expandedBytes = new byte[count + cUnsafe * 2];
            int pos = 0;

            for (int i = 0; i < count; i++)
            {
                byte b = bytes[offset + i];
                char ch = (char)b;

                if (HttpEncoderUtility.IsUrlSafeChar(ch))
                {
                    expandedBytes[pos++] = b;
                }
                else if (ch == ' ')
                {
                    expandedBytes[pos++] = (byte)'+';
                }
                else
                {
                    expandedBytes[pos++] = (byte)'%';
                    expandedBytes[pos++] = (byte)HttpEncoderUtility.IntToHex((b >> 4) & 0xf);
                    expandedBytes[pos++] = (byte)HttpEncoderUtility.IntToHex(b & 0x0f);
                }
            }

            return expandedBytes;
        }

        #endregion

        #region -- UrlPathEncode --

        public static string UrlPathEncode(string value)
        {
            if (string.IsNullOrEmpty(value)) { return value; }

            bool isValidUrl = UriUtil.TrySplitUriForPathEncode(value, out var schemeAndAuthority, out var path, out var queryAndFragment);

            if (!isValidUrl)
            {
                // If the value is not a valid url, we treat it as a relative url.
                // We don't need to extract query string from the url since UrlPathEncode() 
                // does not encode query string.
                schemeAndAuthority = null;
                path = value;
                queryAndFragment = null;
            }

            return schemeAndAuthority + UrlPathEncodeImpl(path) + queryAndFragment;
        }

        #endregion

        #region ** UrlPathEncodeImpl **

        // This is the original UrlPathEncode(string)
        private static string UrlPathEncodeImpl(string value)
        {
            if (string.IsNullOrEmpty(value)) { return value; }

            // recurse in case there is a query string
            int i = value.IndexOf('?');
            if (i >= 0)
            {
//#if NETCOREAPP_3_0_GREATER || NETSTANDARD_2_0_GREATER
//                return string.Concat(UrlPathEncodeImpl(value.Substring(0, i)), value.AsSpan(i));
//#else
                return UrlPathEncodeImpl(value.Substring(0, i)) + value.Substring(i);
//#endif
            }

            // encode DBCS characters and spaces only
            return HttpEncoderUtility.UrlEncodeSpaces(UrlEncodeNonAscii(value, Encoding.UTF8));
        }

        #endregion

        #region ** UrlEncodeNonAscii **

        //  Helper to encode the non-ASCII url characters only
        private static string UrlEncodeNonAscii(string str, Encoding e)
        {
            Debug.Assert(!string.IsNullOrEmpty(str));
            Debug.Assert(e is object);
            byte[] bytes = e.GetBytes(str);
            byte[] encodedBytes = UrlEncodeNonAscii(bytes, 0, bytes.Length);
            return Encoding.ASCII.GetString(encodedBytes);
        }

        private static byte[] UrlEncodeNonAscii(byte[] bytes, int offset, int count)
        {
            int cNonAscii = 0;

            // count them first
            for (int i = 0; i < count; i++)
            {
                if (IsNonAsciiByte(bytes[offset + i]))
                {
                    cNonAscii++;
                }
            }

            // nothing to expand?
            if (0u >= (uint)cNonAscii)
            {
                return bytes;
            }

            // expand not 'safe' characters into %XX, spaces to +s
            byte[] expandedBytes = new byte[count + cNonAscii * 2];
            int pos = 0;

            for (int i = 0; i < count; i++)
            {
                byte b = bytes[offset + i];

                if (IsNonAsciiByte(b))
                {
                    expandedBytes[pos++] = (byte)'%';
                    expandedBytes[pos++] = (byte)HttpEncoderUtility.IntToHex((b >> 4) & 0xf);
                    expandedBytes[pos++] = (byte)HttpEncoderUtility.IntToHex(b & 0x0f);
                }
                else
                {
                    expandedBytes[pos++] = b;
                }
            }

            return expandedBytes;
        }

        #endregion

        #region ** IsNonAsciiByte **

        [MethodImpl(InlineMethod.AggressiveInlining)]
        private static bool IsNonAsciiByte(byte b) => b >= 0x7F || b < 0x20;

        #endregion

        #region -- UrlDecode --

        public static string UrlDecode(string str) => UrlDecodeImpl(str, Encoding.UTF8);

        public static string UrlDecode(string str, Encoding e)
        {
            if (e is null) { e = Encoding.UTF8; }
            return UrlDecodeImpl(str, e);
        }

        public static string UrlDecode(byte[] bytes, Encoding e) => bytes is null ? null : UrlDecodeImpl(bytes, 0, bytes.Length, e);

        public static string UrlDecode(byte[] bytes, int offset, int count, Encoding e)
        {
            if (!ValidateUrlEncodingParameters(bytes, offset, count)) { return null; }

            return UrlDecodeImpl(bytes, offset, count, e);
        }

        #endregion

        #region ** UrlDecodeImpl **

        private static string UrlDecodeImpl(string value, Encoding encoding)
        {
            if (value is null) { return null; }

            int count = value.Length;
            UrlDecoder helper = new UrlDecoder(count, encoding);

            // go through the string's chars collapsing %XX and %uXXXX and
            // appending each char as char, with exception of %XX constructs
            // that are appended as bytes

            for (int pos = 0; pos < count; pos++)
            {
                char ch = value[pos];

                if (ch == '+')
                {
                    ch = ' ';
                }
                else if (ch == '%' && pos < count - 2)
                {
                    if (value[pos + 1] == 'u' && pos < count - 5)
                    {
                        int h1 = HttpEncoderUtility.HexToInt(value[pos + 2]);
                        int h2 = HttpEncoderUtility.HexToInt(value[pos + 3]);
                        int h3 = HttpEncoderUtility.HexToInt(value[pos + 4]);
                        int h4 = HttpEncoderUtility.HexToInt(value[pos + 5]);

                        if (h1 >= 0 && h2 >= 0 && h3 >= 0 && h4 >= 0)
                        {   // valid 4 hex chars
                            ch = (char)((h1 << 12) | (h2 << 8) | (h3 << 4) | h4);
                            pos += 5;

                            // only add as char
                            helper.AddChar(ch);
                            continue;
                        }
                    }
                    else
                    {
                        int h1 = HttpEncoderUtility.HexToInt(value[pos + 1]);
                        int h2 = HttpEncoderUtility.HexToInt(value[pos + 2]);

                        if (h1 >= 0 && h2 >= 0)
                        {     // valid 2 hex chars
                            byte b = (byte)((h1 << 4) | h2);
                            pos += 2;

                            // don't add as char
                            helper.AddByte(b);
                            continue;
                        }
                    }
                }

                if (0u >= (uint)(ch & 0xFF80))
                {
                    helper.AddByte((byte)ch); // 7 bit have to go as bytes because of Unicode
                }
                else
                {
                    helper.AddChar(ch);
                }
            }

            return Utf16StringValidator.ValidateString(helper.GetString());
        }

        private static string UrlDecodeImpl(byte[] bytes, int offset, int count, Encoding encoding)
        {
            if (encoding is null) { encoding = Encoding.UTF8; }

            UrlDecoder helper = new UrlDecoder(count, encoding);

            // go through the bytes collapsing %XX and %uXXXX and appending
            // each byte as byte, with exception of %uXXXX constructs that
            // are appended as chars

            for (int i = 0; i < count; i++)
            {
                int pos = offset + i;
                byte b = bytes[pos];

                // The code assumes that + and % cannot be in multibyte sequence

                if (b == '+')
                {
                    b = (byte)' ';
                }
                else if (b == '%' && i < count - 2)
                {
                    if (bytes[pos + 1] == 'u' && i < count - 5)
                    {
                        int h1 = HttpEncoderUtility.HexToInt((char)bytes[pos + 2]);
                        int h2 = HttpEncoderUtility.HexToInt((char)bytes[pos + 3]);
                        int h3 = HttpEncoderUtility.HexToInt((char)bytes[pos + 4]);
                        int h4 = HttpEncoderUtility.HexToInt((char)bytes[pos + 5]);

                        if (h1 >= 0 && h2 >= 0 && h3 >= 0 && h4 >= 0)
                        {   // valid 4 hex chars
                            char ch = (char)((h1 << 12) | (h2 << 8) | (h3 << 4) | h4);
                            i += 5;

                            // don't add as byte
                            helper.AddChar(ch);
                            continue;
                        }
                    }
                    else
                    {
                        int h1 = HttpEncoderUtility.HexToInt((char)bytes[pos + 1]);
                        int h2 = HttpEncoderUtility.HexToInt((char)bytes[pos + 2]);

                        if (h1 >= 0 && h2 >= 0)
                        {     // valid 2 hex chars
                            b = (byte)((h1 << 4) | h2);
                            i += 2;
                        }
                    }
                }

                helper.AddByte(b);
            }

            return Utf16StringValidator.ValidateString(helper.GetString());
        }

        #endregion

        #region -- UrlDecodeToBytes --

        public static byte[] UrlDecodeToBytes(string str) => UrlDecodeToBytes(str, Encoding.UTF8);

        public static byte[] UrlDecodeToBytes(string str, Encoding e)
        {
            if (str is null) { return null; }
            if (e is null) { e = Encoding.UTF8; }

            var bytes = e.GetBytes(str);
            return UrlDecodeToBytesImpl(bytes, 0, bytes.Length);
        }

        public static byte[] UrlDecodeToBytes(byte[] bytes) => bytes is null ? null : UrlDecodeToBytesImpl(bytes, 0, bytes.Length);

        public static byte[] UrlDecodeToBytes(byte[] bytes, int offset, int count)
        {
            if (!ValidateUrlEncodingParameters(bytes, offset, count)) { return null; }

            return UrlDecodeToBytesImpl(bytes, offset, count);
        }

        #endregion

        #region ** UrlDecodeToBytesImpl **

        private static byte[] UrlDecodeToBytesImpl(byte[] bytes, int offset, int count)
        {
            int decodedBytesCount = 0;
            byte[] decodedBytes = new byte[count];

            for (int i = 0; i < count; i++)
            {
                int pos = offset + i;
                byte b = bytes[pos];

                if (b == '+')
                {
                    b = (byte)' ';
                }
                else if (b == '%' && i < count - 2)
                {
                    int h1 = HttpEncoderUtility.HexToInt((char)bytes[pos + 1]);
                    int h2 = HttpEncoderUtility.HexToInt((char)bytes[pos + 2]);

                    if (h1 >= 0 && h2 >= 0)
                    {
                        // valid 2 hex chars
                        b = (byte)((h1 << 4) | h2);
                        i += 2;
                    }
                }

                decodedBytes[decodedBytesCount++] = b;
            }

            if (decodedBytesCount < decodedBytes.Length)
            {
                byte[] newDecodedBytes = new byte[decodedBytesCount];
                Array.Copy(decodedBytes, 0, newDecodedBytes, 0, decodedBytesCount);
                decodedBytes = newDecodedBytes;
            }

            return decodedBytes;
        }

        #endregion

        #region ** ValidateUrlEncodingParameters **

        [MethodImpl(InlineMethod.AggressiveInlining)]
        private static bool ValidateUrlEncodingParameters(byte[] bytes, int offset, int count)
        {
            if (bytes is null && 0u >= (uint)count)
            {
                return false;
            }

            if (bytes is null)
            {
                return false;
                //throw new ArgumentNullException(nameof(bytes));
            }
            if (offset < 0 || (uint)offset > (uint)bytes.Length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.offset);
            }
            if (count < 0 || (uint)(offset + count) > (uint)bytes.Length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count);
            }

            return true;
        }

        #endregion

        #region ** struct UrlDecoder **

        // Internal class to facilitate URL decoding -- keeps char buffer and byte buffer, allows appending of either chars or bytes
        private struct UrlDecoder
        {
            private readonly int _bufferSize;

            // Accumulate characters in a special array
            private int _numChars;
            private readonly char[] _charBuffer;

            // Accumulate bytes for decoding into characters in a special array
            private int _numBytes;
            private byte[] _byteBuffer;

            // Encoding to convert chars to bytes
            private readonly Encoding _encoding;

            private void FlushBytes()
            {
                if (_numBytes > 0)
                {
                    _numChars += _encoding.GetChars(_byteBuffer, 0, _numBytes, _charBuffer, _numChars);
                    _numBytes = 0;
                }
            }

            internal UrlDecoder(int bufferSize, Encoding encoding)
            {
                _bufferSize = bufferSize;
                _encoding = encoding;

                _charBuffer = new char[bufferSize]; // char buffer created on demand

                _numChars = 0;
                _numBytes = 0;
                _byteBuffer = null; // byte buffer created on demand
            }

            internal void AddChar(char ch)
            {
                if (_numBytes > 0)
                {
                    FlushBytes();
                }

                _charBuffer[_numChars++] = ch;
            }

            internal void AddByte(byte b)
            {
                // if there are no pending bytes treat 7 bit bytes as characters
                // this optimization is temp disable as it doesn't work for some encodings
                /*
                                if (_numBytes == 0 && ((b & 0x80) == 0)) {
                                    AddChar((char)b);
                                }
                                else
                */
                {
                    if (_byteBuffer is null)
                    {
                        _byteBuffer = new byte[_bufferSize];
                    }

                    _byteBuffer[_numBytes++] = b;
                }
            }

            internal string GetString()
            {
                if (_numBytes > 0)
                {
                    FlushBytes();
                }

                return _numChars > 0 ? new string(_charBuffer, 0, _numChars) : "";
            }
        }

        #endregion

        #region -- ParseQueryString --

        public static NameValueCollection ParseQueryString(string query) => ParseQueryString(query, Encoding.UTF8);

        public static NameValueCollection ParseQueryString(string query, Encoding encoding)
        {
            if (query is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.query); }
            if (encoding is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.encoding); }

            HttpQSCollection result = new HttpQSCollection();
            int queryLength = query.Length;
            int namePos = queryLength > 0 && query[0] == '?' ? 1 : 0;
            if (queryLength == namePos)
            {
                return result;
            }

            while (namePos <= queryLength)
            {
                int valuePos = -1, valueEnd = -1;
                for (int q = namePos; q < queryLength; q++)
                {
                    if (valuePos == -1 && query[q] == '=')
                    {
                        valuePos = q + 1;
                    }
                    else if (query[q] == '&')
                    {
                        valueEnd = q;
                        break;
                    }
                }

                string name;
                if (valuePos == -1)
                {
                    name = null;
                    valuePos = namePos;
                }
                else
                {
                    name = UrlDecode(query.Substring(namePos, valuePos - namePos - 1), encoding);
                }

                if (valueEnd < 0)
                {
                    valueEnd = query.Length;
                }

                namePos = valueEnd + 1;
                string value = UrlDecode(query.Substring(valuePos, valueEnd - valuePos), encoding);
                result.Add(name, value);
            }

            return result;
        }

        #endregion

        #region ** class HttpQSCollection **

        private sealed class HttpQSCollection : NameValueCollection
        {
            internal HttpQSCollection()
                : base(StringComparer.OrdinalIgnoreCase)
            {
            }

            public override string ToString()
            {
                int count = Count;
                if (0u >= (uint)count)
                {
                    return "";
                }

                StringBuilder sb = new StringBuilder();
                string[] keys = AllKeys;
                for (int i = 0; i < count; i++)
                {
#if DEBUG
                    sb.AppendFormat("{0}={1}&", keys[i], UrlEncode(this[keys[i]], Encoding.UTF8));
#else
                    sb.AppendFormat("{0}={1}&", keys[i], UrlEncode(this[keys[i]]));
#endif
                }

                return sb.ToString(0, sb.Length - 1);
            }
        }

        #endregion
    }
}
