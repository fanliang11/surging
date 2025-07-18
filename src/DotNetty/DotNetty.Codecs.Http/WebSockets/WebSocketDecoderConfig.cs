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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com)
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs.Http.WebSockets
{
    /// <summary>
    /// Frames decoder configuration.
    /// </summary>
    public sealed class WebSocketDecoderConfig
    {
        internal static readonly WebSocketDecoderConfig Default = new WebSocketDecoderConfig(65536, true, false, false, true, true);

        private WebSocketDecoderConfig(int maxFramePayloadLength, bool expectMaskedFrames, bool allowMaskMismatch,
                                       bool allowExtensions, bool closeOnProtocolViolation, bool withUTF8Validator)
        {
            MaxFramePayloadLength = maxFramePayloadLength;
            ExpectMaskedFrames = expectMaskedFrames;
            AllowMaskMismatch = allowMaskMismatch;
            AllowExtensions = allowExtensions;
            CloseOnProtocolViolation = closeOnProtocolViolation;
            WithUTF8Validator = withUTF8Validator;
        }

        /// <summary>
        /// Maximum length of a frame's payload. Setting this to an appropriate value for you application
        /// helps check for denial of services attacks.
        /// </summary>
        public readonly int MaxFramePayloadLength;

        /// <summary>
        /// Web socket servers must set this to true processed incoming masked payload. Client implementations
        /// must set this to false.
        /// </summary>
        public readonly bool ExpectMaskedFrames;

        /// <summary>
        /// Allows to loosen the masking requirement on received frames. When this is set to false then also
        /// frames which are not masked properly according to the standard will still be accepted.
        /// </summary>
        public readonly bool AllowMaskMismatch;

        /// <summary>
        /// Flag to allow reserved extension bits to be used or not
        /// </summary>
        public readonly bool AllowExtensions;

        /// <summary>
        /// Flag to send close frame immediately on any protocol violation.ion.
        /// </summary>
        public readonly bool CloseOnProtocolViolation;

        /// <summary>
        /// Allows you to avoid adding of Utf8FrameValidator to the pipeline on the
        /// <see cref="WebSocketServerProtocolHandler"/> creation. This is useful (less overhead)
        /// when you use only BinaryWebSocketFrame within your web socket connection.
        /// </summary>
        public readonly bool WithUTF8Validator;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"WebSocketDecoderConfig [maxFramePayloadLength={MaxFramePayloadLength}, expectMaskedFrames={ExpectMaskedFrames}, allowMaskMismatch={AllowMaskMismatch}, allowExtensions={AllowExtensions}, closeOnProtocolViolation={CloseOnProtocolViolation}, withUTF8Validator={WithUTF8Validator}]";
        }

        public Builder ToBuilder()
        {
            return new Builder(this);
        }

        public static Builder NewBuilder()
        {
            return new Builder(Default);
        }

        public sealed class Builder
        {
            private int _maxFramePayloadLength;
            private bool _expectMaskedFrames;
            private bool _allowMaskMismatch;
            private bool _allowExtensions;
            private bool _closeOnProtocolViolation;
            private bool _withUTF8Validator;

            internal Builder(WebSocketDecoderConfig decoderConfig)
            {
                if (decoderConfig is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.decoderConfig); }

                _maxFramePayloadLength = decoderConfig.MaxFramePayloadLength;
                _expectMaskedFrames = decoderConfig.ExpectMaskedFrames;
                _allowMaskMismatch = decoderConfig.AllowMaskMismatch;
                _allowExtensions = decoderConfig.AllowExtensions;
                _closeOnProtocolViolation = decoderConfig.CloseOnProtocolViolation;
                _withUTF8Validator = decoderConfig.WithUTF8Validator;
            }

            /// <summary>
            /// Maximum length of a frame's payload. Setting this to an appropriate value for you application
            /// helps check for denial of services attacks.
            /// </summary>
            public Builder MaxFramePayloadLength(int maxFramePayloadLength)
            {
                _maxFramePayloadLength = maxFramePayloadLength;
                return this;
            }

            /// <summary>
            /// Web socket servers must set this to true processed incoming masked payload. Client implementations
            /// must set this to false.
            /// </summary>
            public Builder ExpectMaskedFrames(bool expectMaskedFrames)
            {
                _expectMaskedFrames = expectMaskedFrames;
                return this;
            }

            /// <summary>
            /// Allows to loosen the masking requirement on received frames. When this is set to false then also
            /// frames which are not masked properly according to the standard will still be accepted.
            /// </summary>
            public Builder AllowMaskMismatch(bool allowMaskMismatch)
            {
                _allowMaskMismatch = allowMaskMismatch;
                return this;
            }

            /// <summary>
            /// Flag to allow reserved extension bits to be used or not
            /// </summary>
            public Builder AllowExtensions(bool allowExtensions)
            {
                _allowExtensions = allowExtensions;
                return this;
            }

            /// <summary>
            /// Flag to send close frame immediately on any protocol violation.ion.
            /// </summary>
            public Builder CloseOnProtocolViolation(bool closeOnProtocolViolation)
            {
                _closeOnProtocolViolation = closeOnProtocolViolation;
                return this;
            }

            /// <summary>
            /// Allows you to avoid adding of Utf8FrameValidator to the pipeline on the
            /// <see cref="WebSocketServerProtocolHandler"/> creation. This is useful (less overhead)
            /// when you use only BinaryWebSocketFrame within your web socket connection.
            /// </summary>
            public Builder WithUTF8Validator(bool withUTF8Validator)
            {
                _withUTF8Validator = withUTF8Validator;
                return this;
            }

            /// <summary>
            /// Build unmodifiable decoder configuration.
            /// </summary>
            public WebSocketDecoderConfig Build()
            {
                return new WebSocketDecoderConfig(
                    _maxFramePayloadLength, _expectMaskedFrames, _allowMaskMismatch,
                    _allowExtensions, _closeOnProtocolViolation, _withUTF8Validator);
            }
        }
    }
}
