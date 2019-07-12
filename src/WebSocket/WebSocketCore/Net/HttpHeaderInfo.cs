/*
 * HttpHeaderInfo.cs
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
    /// <summary>
    /// Defines the <see cref="HttpHeaderInfo" />
    /// </summary>
    internal class HttpHeaderInfo
    {
        #region 字段

        /// <summary>
        /// Defines the _name
        /// </summary>
        private string _name;

        /// <summary>
        /// Defines the _type
        /// </summary>
        private HttpHeaderType _type;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHeaderInfo"/> class.
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="type">The type<see cref="HttpHeaderType"/></param>
        internal HttpHeaderInfo(string name, HttpHeaderType type)
        {
            _name = name;
            _type = type;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets a value indicating whether IsRequest
        /// </summary>
        public bool IsRequest
        {
            get
            {
                return (_type & HttpHeaderType.Request) == HttpHeaderType.Request;
            }
        }

        /// <summary>
        /// Gets a value indicating whether IsResponse
        /// </summary>
        public bool IsResponse
        {
            get
            {
                return (_type & HttpHeaderType.Response) == HttpHeaderType.Response;
            }
        }

        /// <summary>
        /// Gets the Name
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the Type
        /// </summary>
        public HttpHeaderType Type
        {
            get
            {
                return _type;
            }
        }

        /// <summary>
        /// Gets a value indicating whether IsMultiValueInRequest
        /// </summary>
        internal bool IsMultiValueInRequest
        {
            get
            {
                return (_type & HttpHeaderType.MultiValueInRequest) == HttpHeaderType.MultiValueInRequest;
            }
        }

        /// <summary>
        /// Gets a value indicating whether IsMultiValueInResponse
        /// </summary>
        internal bool IsMultiValueInResponse
        {
            get
            {
                return (_type & HttpHeaderType.MultiValueInResponse) == HttpHeaderType.MultiValueInResponse;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The IsMultiValue
        /// </summary>
        /// <param name="response">The response<see cref="bool"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool IsMultiValue(bool response)
        {
            return (_type & HttpHeaderType.MultiValue) == HttpHeaderType.MultiValue
                   ? (response ? IsResponse : IsRequest)
                   : (response ? IsMultiValueInResponse : IsMultiValueInRequest);
        }

        /// <summary>
        /// The IsRestricted
        /// </summary>
        /// <param name="response">The response<see cref="bool"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool IsRestricted(bool response)
        {
            return (_type & HttpHeaderType.Restricted) == HttpHeaderType.Restricted
                   ? (response ? IsResponse : IsRequest)
                   : false;
        }

        #endregion 方法
    }
}