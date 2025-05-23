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
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using DotNetty.Codecs;
    using DotNetty.Common.Utilities;

    public class DefaultHttpHeaders : HttpHeaders
    {
        const int HighestInvalidValueCharMask = ~15;
        internal static readonly INameValidator<ICharSequence> HttpNameValidator = new HeaderNameValidator();
        public static readonly INameValidator<ICharSequence> NotNullValidator = NullNameValidator<ICharSequence>.Instance;

        sealed class NameProcessor : IByteProcessor
        {
            public bool Process(byte value)
            {
                ValidateHeaderNameElement(value);
                return true;
            }
        }

        sealed class HeaderNameValidator : INameValidator<ICharSequence>
        {
            static readonly NameProcessor ByteProcessor = new NameProcessor();

            [MethodImpl(InlineMethod.AggressiveInlining)]
            public void ValidateName(ICharSequence name)
            {
                if (name is null || 0u >= (uint)name.Count)
                {
                    ThrowHelper.ThrowArgumentException_HeaderName();
                }
                if (name is AsciiString asciiString)
                {
                    _ = asciiString.ForEachByte(ByteProcessor);
                }
                else
                {
                    // Go through each character in the name
                    Debug.Assert(name is object);
                    // ReSharper disable once ForCanBeConvertedToForeach
                    // Avoid new enumerator instance
                    for (int index = 0; index < name.Count; ++index)
                    {
                        ValidateHeaderNameElement(name[index]);
                    }
                }
            }
        }

        readonly DefaultHeaders<AsciiString, ICharSequence> headers;

        public DefaultHttpHeaders() : this(true, NameValidator(true))
        {
        }

        /// <summary>
        /// <c>Warning!</c> Setting <paramref name="validate"/> to <c>false</c> will mean that Netty won't
        /// validate &amp; protect against user-supplied header values that are malicious.
        /// This can leave your server implementation vulnerable to
        /// <a href="https://cwe.mitre.org/data/definitions/113.html">
        ///     CWE-113: Improper Neutralization of CRLF Sequences in HTTP Headers ('HTTP Response Splitting')
        /// </a>.
        /// When disabling this validation, it is the responsibility of the caller to ensure that the values supplied
        /// do not contain a non-url-escaped carriage return (CR) and/or line feed (LF) characters.
        /// </summary>
        /// <param name="validate">Should Netty validate Header values to ensure they aren't malicious.</param>
        public DefaultHttpHeaders(bool validate) : this(validate, NameValidator(validate))
        {
        }

        protected DefaultHttpHeaders(bool validate, INameValidator<ICharSequence> nameValidator) 
            : this(new DefaultHeaders<AsciiString, ICharSequence>(AsciiString.CaseInsensitiveHasher, 
                ValueConverter(validate), nameValidator))
        {
        }

        protected DefaultHttpHeaders(DefaultHeaders<AsciiString, ICharSequence> headers)
        {
            this.headers = headers;
        }

        public override HttpHeaders Add(HttpHeaders httpHeaders)
        {
            if (httpHeaders is DefaultHttpHeaders defaultHttpHeaders)
            {
                _ = this.headers.Add(defaultHttpHeaders.headers);
                return this;
            }
            return base.Add(httpHeaders);
        }

        public override HttpHeaders Set(HttpHeaders httpHeaders)
        {
            if (httpHeaders is DefaultHttpHeaders defaultHttpHeaders)
            {
                _ = this.headers.Set(defaultHttpHeaders.headers);
                return this;
            }
            return base.Set(httpHeaders);
        }

        public override HttpHeaders Add(AsciiString name, object value)
        {
            _ = this.headers.AddObject(name, value);
            return this;
        }

        public override HttpHeaders AddInt(AsciiString name, int value)
        {
            _ = this.headers.AddInt(name, value);
            return this;
        }

        public override HttpHeaders AddShort(AsciiString name, short value)
        {
            _ = this.headers.AddShort(name, value);
            return this;
        }

        public override HttpHeaders Remove(AsciiString name)
        {
            _ = this.headers.Remove(name);
            return this;
        }

        public override HttpHeaders Set(AsciiString name, object value)
        {
            _ = this.headers.SetObject(name, value);
            return this;
        }

        public override HttpHeaders Set(AsciiString name, IEnumerable<object> values)
        {
            _ = this.headers.SetObject(name, values);
            return this;
        }

        public override HttpHeaders SetInt(AsciiString name, int value)
        {
            _ = this.headers.SetInt(name, value);
            return this;
        }

        public override HttpHeaders SetShort(AsciiString name, short value)
        {
            _ = this.headers.SetShort(name, value);
            return this;
        }

        public override HttpHeaders Clear()
        {
            _ = this.headers.Clear();
            return this;
        }

        public override bool TryGet(AsciiString name, out ICharSequence value) => this.headers.TryGet(name, out value);

        public override bool TryGetInt(AsciiString name, out int value) => this.headers.TryGetInt(name, out value);

        public override int GetInt(AsciiString name, int defaultValue) => this.headers.GetInt(name, defaultValue);

        public override bool TryGetShort(AsciiString name, out short value) => this.headers.TryGetShort(name, out value);

        public override short GetShort(AsciiString name, short defaultValue) => this.headers.GetShort(name, defaultValue);

        public override bool TryGetTimeMillis(AsciiString name, out long value) => this.headers.TryGetTimeMillis(name, out value);

        public override long GetTimeMillis(AsciiString name, long defaultValue) => this.headers.GetTimeMillis(name, defaultValue);

        public override IList<ICharSequence> GetAll(AsciiString name) => this.headers.GetAll(name);

        public override IEnumerable<ICharSequence> ValueCharSequenceIterator(AsciiString name) => this.headers.ValueIterator(name);

        public override IList<HeaderEntry<AsciiString, ICharSequence>> Entries()
        {
            if (this.IsEmpty)
            {
                return ImmutableList<HeaderEntry<AsciiString, ICharSequence>>.Empty;
            }
            var entriesConverted = new List<HeaderEntry<AsciiString, ICharSequence>>(this.headers.Size);
            foreach(HeaderEntry<AsciiString, ICharSequence> entry in this)
            {
                entriesConverted.Add(entry);
            }
            return entriesConverted;
        }

        public override IEnumerator<HeaderEntry<AsciiString, ICharSequence>> GetEnumerator() => this.headers.GetEnumerator();

        public override bool Contains(AsciiString name) => this.headers.Contains(name);

        public override bool IsEmpty => this.headers.IsEmpty;

        public override int Size => this.headers.Size;

        public override bool Contains(AsciiString name, ICharSequence value, bool ignoreCase) =>  
            this.headers.Contains(name, value, 
                ignoreCase ? AsciiString.CaseInsensitiveHasher : AsciiString.CaseSensitiveHasher);

        public override ISet<AsciiString> Names() => this.headers.Names();

        public override bool Equals(object obj) => obj is DefaultHttpHeaders other 
            && this.headers.Equals(other.headers, AsciiString.CaseSensitiveHasher);

        public override int GetHashCode() => this.headers.HashCode(AsciiString.CaseSensitiveHasher);

        public override HttpHeaders Copy() => new DefaultHttpHeaders(this.headers.Copy());

        [MethodImpl(InlineMethod.AggressiveInlining)]
        static void ValidateHeaderNameElement(uint value)
        {
            switch (value)
            {
                case 0x00u:
                case 0x09u: //'\t':
                case 0x0au: //'\n':
                case 0x0bu:
                case 0x0cu: //'\f':
                case 0x0du: //'\r':
                case 0x20u: //' ':
                case 0x2cu: //',':
                case 0x3au: //':':
                case 0x3bu: //';':
                case 0x3du: //'=':
                    ThrowHelper.ThrowArgumentException_HeaderValue(value);
                    break;
                default:
                    // Check to see if the character is not an ASCII character, or invalid
                    if (value > 127u)
                    {
                        ThrowHelper.ThrowArgumentException_HeaderValueNonAscii(value);
                    }
                    break;
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        static void ValidateHeaderNameElement(char value)
        {
            switch (value)
            {
                case '\x00':
                case '\t':
                case '\n':
                case '\x0b':
                case '\f':
                case '\r':
                case ' ':
                case ',':
                case ':':
                case ';':
                case '=':
                    ThrowHelper.ThrowArgumentException_HeaderValue(value);
                    break;
                default:
                    // Check to see if the character is not an ASCII character, or invalid
                    if (value > 127)
                    {
                        ThrowHelper.ThrowArgumentException_HeaderValueNonAscii(value);
                    }
                    break;
            }
        }

        protected static IValueConverter<ICharSequence> ValueConverter(bool validate) => 
            validate ? DefaultHeaderValueConverterAndValidator : DefaultHeaderValueConverter;

        protected static INameValidator<ICharSequence> NameValidator(bool validate) => 
            validate ? HttpNameValidator : NotNullValidator;

        static readonly HeaderValueConverter DefaultHeaderValueConverter = new HeaderValueConverter();

        class HeaderValueConverter : CharSequenceValueConverter
        {
            public override ICharSequence ConvertObject(object value)
            {
                switch (value)
                {
                    case ICharSequence seq:
                        return seq;

                    case DateTime time:
                        return new StringCharSequence(DateFormatter.Format(time));

                    default:
                        return new StringCharSequence(value.ToString());
                }
            }
        }

        static readonly HeaderValueConverterAndValidator DefaultHeaderValueConverterAndValidator = new HeaderValueConverterAndValidator();

        sealed class HeaderValueConverterAndValidator : HeaderValueConverter
        {
            public override ICharSequence ConvertObject(object value)
            {
                ICharSequence seq = base.ConvertObject(value);
                int state = 0;
                // Start looping through each of the character
                // ReSharper disable once ForCanBeConvertedToForeach
                // Avoid enumerator allocation
                for (int index = 0; index < seq.Count; index++)
                {
                    state = ValidateValueChar(state, seq[index]);
                }

                if (state != 0)
                {
                    ThrowHelper.ThrowArgumentException_HeaderValueEnd(seq);
                }
                return seq;
            }

            [MethodImpl(InlineMethod.AggressiveInlining)]
            static int ValidateValueChar(int state, char character)
            {
                // State:
                // 0: Previous character was neither CR nor LF
                // 1: The previous character was CR
                // 2: The previous character was LF
                if (0u >= (uint)(character & HighestInvalidValueCharMask))
                {
                    // Check the absolutely prohibited characters.
                    switch (character)
                    {
                        case '\x00': // NULL
                            ThrowHelper.ThrowArgumentException_HeaderValueNullChar();
                            break;
                        case '\x0b': // Vertical tab
                            ThrowHelper.ThrowArgumentException_HeaderValueVerticalTabChar();
                            break;
                        case '\f':
                            ThrowHelper.ThrowArgumentException_HeaderValueFormFeed();
                            break;
                    }
                }

                // Check the CRLF (HT | SP) pattern
                switch (state)
                {
                    case 0:
                        switch (character)
                        {
                            case '\r':
                                return 1;
                            case '\n':
                                return 2;
                        }
                        break;
                    case 1:
                        switch (character)
                        {
                            case '\n':
                                return 2;
                            default:
                                ThrowHelper.ThrowArgumentException_NewLineAfterLineFeed();
                                break;
                        }
                        break;
                    case 2:
                        switch (character)
                        {
                            case '\t':
                            case ' ':
                                return 0;
                            default:
                                ThrowHelper.ThrowArgumentException_TabAndSpaceAfterLineFeed();
                                break;
                        }
                        break;
                }

                return state;
            }
        }
    }
}
