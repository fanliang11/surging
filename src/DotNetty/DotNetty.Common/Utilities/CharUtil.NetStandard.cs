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

namespace DotNetty.Common.Utilities
{
    using System;
    using System.Collections.Generic;

    partial class CharUtil
    {
        public static ICharSequence[] Split(ICharSequence sequence, int startIndex, params char[] delimiters)
        {
            if (sequence is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.sequence); }
            if (delimiters is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.delimiters); }
            int length = sequence.Count;
            uint uLength = (uint)length;
            if (0u >= uLength) { return new[] { sequence }; }
            if ((uint)startIndex >= uLength) { ThrowHelper.ThrowIndexOutOfRangeException(); }

            var delimitersSpan = delimiters.AsSpan();
            List<ICharSequence> result = InternalThreadLocalMap.Get().CharSequenceList();

            int i = startIndex;

            while ((uint)i < uLength)
            {
                while ((uint)i < uLength && delimitersSpan.IndexOf(sequence[i]) >= 0)
                {
                    i++;
                }

                int position = i;
                if ((uint)i < uLength)
                {
                    if (delimitersSpan.IndexOf(sequence[position]) >= 0)
                    {
                        result.Add(sequence.SubSequence(position++, i + 1));
                    }
                    else
                    {
                        ICharSequence seq = null;
                        for (position++; position < length; position++)
                        {
                            if (delimitersSpan.IndexOf(sequence[position]) >= 0)
                            {
                                seq = sequence.SubSequence(i, position);
                                break;
                            }
                        }
                        result.Add(seq ?? sequence.SubSequence(i));
                    }
                    i = position;
                }
            }

            return 0u >= (uint)result.Count ? new[] { sequence } : result.ToArray();
        }
    }
}
