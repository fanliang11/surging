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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using DotNetty.Common.Utilities;

    using static Common.Utilities.StringUtil;

    public sealed class CombinedHttpHeaders : DefaultHttpHeaders
    {
        public CombinedHttpHeaders(bool validate)
            : base(new CombinedHttpHeadersImpl(AsciiString.CaseInsensitiveHasher, ValueConverter(validate), NameValidator(validate)))
        {
        }

        public override bool ContainsValue(AsciiString name, ICharSequence value, bool ignoreCase) =>
            base.ContainsValue(name, TrimOws(value), ignoreCase);

        sealed class CombinedHttpHeadersImpl : DefaultHeaders<AsciiString, ICharSequence>
        {
            // An estimate of the size of a header value.
            const int ValueLengthEstimate = 10;

            public CombinedHttpHeadersImpl(IHashingStrategy<AsciiString> nameHashingStrategy,
                IValueConverter<ICharSequence> valueConverter, INameValidator<ICharSequence> nameValidator)
                : base(nameHashingStrategy, valueConverter, nameValidator)
            {
            }

            public override IEnumerable<ICharSequence> ValueIterator(AsciiString name)
            {
                var itr = base.ValueIterator(name);
                if (!itr.Any() || CannotBeCombined(name))
                {
                    return itr;
                }
                ICharSequence value = null;
                foreach (ICharSequence v in itr)
                {
                    if (value is object)
                    {
                        ThrowHelper.ThrowInvalidOperationException_OnlyHaveOneValue();
                    }
                    value = v;
                }
                return UnescapeCsvFields(value);
            }

            public override IList<ICharSequence> GetAll(AsciiString name)
            {
                IList<ICharSequence> values = base.GetAll(name);
                uint uCount = (uint)values.Count;
                if (0u >= uCount || CannotBeCombined(name))
                {
                    return values;
                }
                if (uCount > 1u) // != 1
                {
                    ThrowHelper.ThrowInvalidOperationException_OnlyHaveOneValue();
                }

                return UnescapeCsvFields(values[0]);
            }

            public override IHeaders<AsciiString, ICharSequence> Add(IHeaders<AsciiString, ICharSequence> headers)
            {
                // Override the fast-copy mechanism used by DefaultHeaders
                if (ReferenceEquals(headers, this))
                {
                    ThrowHelper.ThrowArgumentException_HeadCantAddSelf();
                }

                if (headers is CombinedHttpHeadersImpl)
                {
                    if (this.IsEmpty)
                    {
                        // Can use the fast underlying copy
                        this.AddImpl(headers);
                    }
                    else
                    {
                        // Values are already escaped so don't escape again
                        foreach (HeaderEntry<AsciiString, ICharSequence> header in headers)
                        {
                            _ = this.AddEscapedValue(header.Key, header.Value);
                        }
                    }
                }
                else
                {
                    foreach (HeaderEntry<AsciiString, ICharSequence> header in headers)
                    {
                        _ = this.Add(header.Key, header.Value);
                    }
                }

                return this;
            }

            public override IHeaders<AsciiString, ICharSequence> Set(IHeaders<AsciiString, ICharSequence> headers)
            {
                if (ReferenceEquals(headers, this))
                {
                    return this;
                }
                _ = this.Clear();
                return this.Add(headers);
            }

            public override IHeaders<AsciiString, ICharSequence> SetAll(IHeaders<AsciiString, ICharSequence> headers)
            {
                if (ReferenceEquals(headers, this))
                {
                    return this;
                }
                foreach (AsciiString key in headers.Names())
                {
                    _ = this.Remove(key);
                }
                return this.Add(headers);
            }

            public override IHeaders<AsciiString, ICharSequence> Add(AsciiString name, ICharSequence value) =>
                this.AddEscapedValue(name, EscapeCsv(value));

            public override IHeaders<AsciiString, ICharSequence> Add(AsciiString name, IEnumerable<ICharSequence> values) =>
                this.AddEscapedValue(name, CommaSeparate(values));

            public override IHeaders<AsciiString, ICharSequence> AddObject(AsciiString name, object value) =>
                this.AddEscapedValue(name, EscapeCsv(this.ValueConverter.ConvertObject(value)));

            public override IHeaders<AsciiString, ICharSequence> AddObject(AsciiString name, IEnumerable<object> values) =>
                this.AddEscapedValue(name, this.CommaSeparate(values));

            public override IHeaders<AsciiString, ICharSequence> AddObject(AsciiString name, params object[] values) =>
                this.AddEscapedValue(name, this.CommaSeparate(values));

            public override IHeaders<AsciiString, ICharSequence> Set(AsciiString name, IEnumerable<ICharSequence> values)
            {
                _ = base.Set(name, CommaSeparate(values));
                return this;
            }

            public override IHeaders<AsciiString, ICharSequence> SetObject(AsciiString name, object value)
            {
                ICharSequence charSequence = EscapeCsv(this.ValueConverter.ConvertObject(value));
                _ = base.Set(name, charSequence);
                return this;
            }

            public override IHeaders<AsciiString, ICharSequence> SetObject(AsciiString name, IEnumerable<object> values)
            {
                _ = base.Set(name, this.CommaSeparate(values));
                return this;
            }

            [MethodImpl(InlineMethod.AggressiveInlining)]
            static bool CannotBeCombined(ICharSequence name)
            {
                return HttpHeaderNames.SetCookie.ContentEqualsIgnoreCase(name);
            }

            CombinedHttpHeadersImpl AddEscapedValue(AsciiString name, ICharSequence escapedValue)
            {
                if (!this.TryGet(name, out ICharSequence currentValue) || CannotBeCombined(name))
                {
                    _ = base.Add(name, escapedValue);
                }
                else
                {
                    _ = base.Set(name, CommaSeparateEscapedValues(currentValue, escapedValue));
                }

                return this;
            }

            ICharSequence CommaSeparate(IEnumerable<object> values)
            {
                StringBuilderCharSequence sb = values is ICollection collection
                    ? new StringBuilderCharSequence(collection.Count * ValueLengthEstimate)
                    : new StringBuilderCharSequence();

                foreach (object value in values)
                {
                    if ((uint)sb.Count > 0u)
                    {
                        sb.Append(Comma);
                    }

                    sb.Append(EscapeCsv(this.ValueConverter.ConvertObject(value)));
                }

                return sb;
            }

            static ICharSequence CommaSeparate(IEnumerable<ICharSequence> values)
            {
                StringBuilderCharSequence sb = values is ICollection collection
                    ? new StringBuilderCharSequence(collection.Count * ValueLengthEstimate)
                    : new StringBuilderCharSequence();

                foreach (ICharSequence value in values)
                {
                    if ((uint)sb.Count > 0u)
                    {
                        sb.Append(Comma);
                    }

                    sb.Append(EscapeCsv(value));
                }

                return sb;
            }

            static ICharSequence CommaSeparateEscapedValues(ICharSequence currentValue, ICharSequence value)
            {
                var builder = new StringBuilderCharSequence(currentValue.Count + 1 + value.Count);
                builder.Append(currentValue);
                builder.Append(Comma);
                builder.Append(value);

                return builder;
            }

            static ICharSequence EscapeCsv(ICharSequence value) => StringUtil.EscapeCsv(value, true);
        }
    }
}
