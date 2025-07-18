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
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs
{
    using System;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;

    public class DecoderResult
    {
        protected static readonly Signal SignalUnfinished = Signal.ValueOf(typeof(DecoderResult), "UNFINISHED");
        protected static readonly Signal SignalSuccess = Signal.ValueOf(typeof(DecoderResult), "SUCCESS");

        public static readonly DecoderResult Unfinished = new DecoderResult(SignalUnfinished);
        public static readonly DecoderResult Success = new DecoderResult(SignalSuccess);

        public static DecoderResult Failure(Exception cause)
        {
            if (cause is null) { CThrowHelper.ThrowArgumentNullException(CExceptionArgument.cause); }
            return new DecoderResult(cause);
        }

        readonly Exception cause;

        protected DecoderResult(Exception cause)
        {
            if (cause is null) { CThrowHelper.ThrowArgumentNullException(CExceptionArgument.cause); }
            this.cause = cause;
        }

        public bool IsFinished => !ReferenceEquals(this.cause, SignalUnfinished);

        public bool IsSuccess => ReferenceEquals(this.cause, SignalSuccess);

        public bool IsFailure => !ReferenceEquals(this.cause, SignalSuccess)
            && !ReferenceEquals(this.cause, SignalUnfinished);

        public Exception Cause => this.IsFailure ? this.cause : null;

        public override string ToString()
        {
            if (!this.IsFinished)
            {
                return "unfinished";
            }

            if (this.IsSuccess)
            {
                return "success";
            }

            string error = this.cause.ToString();
            var sb = StringBuilderManager.Allocate(error.Length + 17)
                .Append("failure(")
                .Append(error)
                .Append(')');
            return StringBuilderManager.ReturnAndFree(sb);
        }
    }
}
