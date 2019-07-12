/*
 * HttpHeaderType.cs
 *
 * The MIT License
 *
 * Copyright (c) 2013-2014 sta.blockhead
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;

namespace WebSocketCore.Net
{
    #region Ã¶¾Ù

    /// <summary>
    /// Defines the HttpHeaderType
    /// </summary>
    [Flags]
    internal enum HttpHeaderType
    {
        /// <summary>
        /// Defines the Unspecified
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// Defines the Request
        /// </summary>
        Request = 1,

        /// <summary>
        /// Defines the Response
        /// </summary>
        Response = 1 << 1,

        /// <summary>
        /// Defines the Restricted
        /// </summary>
        Restricted = 1 << 2,

        /// <summary>
        /// Defines the MultiValue
        /// </summary>
        MultiValue = 1 << 3,

        /// <summary>
        /// Defines the MultiValueInRequest
        /// </summary>
        MultiValueInRequest = 1 << 4,

        /// <summary>
        /// Defines the MultiValueInResponse
        /// </summary>
        MultiValueInResponse = 1 << 5
    }

    #endregion Ã¶¾Ù
}