/*
 * Copyright (c) 2011-2015, Longxiang He <helongxiang@smeshlink.com>,
 * SmeshLink Technology Co.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY.
 * 
 * This file is part of the CoAP.NET, a CoAP framework in C#.
 * Please see README for more information.
 */

using System;

namespace CoAP
{
    /// <summary>
    /// This class describes the CoAP Code Registry as defined in 
    /// draft-ietf-core-coap-08, section 11.1
    /// </summary>
    public class Code
    {
        public const Int32 Empty = 0;

        public const Int32 SuccessCode = 2;
        public const Int32 ClientErrorCode = 4;
        public const Int32 ServerErrorCode = 5;

        #region Method Codes

        /// <summary>
        /// The GET method
        /// </summary>
        public const Int32 GET = 1;
        /// <summary>
        /// The POST method
        /// </summary>
        public const Int32 POST = 2;
        /// <summary>
        /// The PUT method
        /// </summary>
        public const Int32 PUT = 3;
        /// <summary>
        /// The DELETE method
        /// </summary>
        public const Int32 DELETE = 4;

        #endregion

        #region Response Codes

        /// <summary>
        /// 2.01 Created
        /// </summary>
        public const Int32 Created = 65;
        /// <summary>
        /// 2.02 Deleted
        /// </summary>
        public const Int32 Deleted = 66;
        /// <summary>
        /// 2.03 Valid 
        /// </summary>
        public const Int32 Valid = 67;
        /// <summary>
        /// 2.04 Changed
        /// </summary>
        public const Int32 Changed = 68;
        /// <summary>
        /// 2.05 Content
        /// </summary>
        public const Int32 Content = 69;
        /// <summary>
        /// 2.?? Continue
        /// </summary>
        public const Int32 Continue = 95;
        /// <summary>
        /// 4.00 Bad Request
        /// </summary>
        public const Int32 BadRequest = 128;
        /// <summary>
        /// 4.01 Unauthorized
        /// </summary>
        public const Int32 Unauthorized = 129;
        /// <summary>
        /// 4.02 Bad Option
        /// </summary>
        public const Int32 BadOption = 130;
        /// <summary>
        /// 4.03 Forbidden
        /// </summary>
        public const Int32 Forbidden = 131;
        /// <summary>
        /// 4.04 Not Found
        /// </summary>
        public const Int32 NotFound = 132;
        /// <summary>
        /// 4.05 Method Not Allowed
        /// </summary>
        public const Int32 MethodNotAllowed = 133;
        /// <summary>
        /// 4.06 Not Acceptable
        /// </summary>
        public const Int32 NotAcceptable = 134;
        /// <summary>
        /// 4.08 Request Entity Incomplete (draft-ietf-core-block)
        /// </summary>
        public const Int32 RequestEntityIncomplete = 136;
        /// <summary>
        /// 
        /// </summary>
        public const Int32 PreconditionFailed = 140;
        /// <summary>
        /// 4.13 Request Entity Too Large
        /// </summary>
        public const Int32 RequestEntityTooLarge = 141;
        /// <summary>
        /// 4.15 Unsupported Media Type
        /// </summary>
        public const Int32 UnsupportedMediaType = 143;
        /// <summary>
        /// 5.00 Internal Server Error
        /// </summary>
        public const Int32 InternalServerError = 160;
        /// <summary>
        /// 5.01 Not Implemented
        /// </summary>
        public const Int32 NotImplemented = 161;
        /// <summary>
        /// 5.02 Bad Gateway
        /// </summary>
        public const Int32 BadGateway = 162;
        /// <summary>
        /// 5.03 Service Unavailable 
        /// </summary>
        public const Int32 ServiceUnavailable = 163;
        /// <summary>
        /// 5.04 Gateway Timeout
        /// </summary>
        public const Int32 GatewayTimeout = 164;
        /// <summary>
        /// 5.05 Proxying Not Supported
        /// </summary>
        public const Int32 ProxyingNotSupported = 165;

        #endregion

        public static Int32 GetResponseClass(Int32 code)
        {
            return (code >> 5) & 0x7;
        }

        /// <summary>
        /// Checks whether a code indicates a request
        /// </summary>
        /// <param name="code">The code to be checked</param>
        /// <returns>True iff the code indicates a request</returns>
        public static Boolean IsRequest(Int32 code)
        {
            return (code >= 1) && (code <= 31);
        }

        /// <summary>
        /// Checks whether a code indicates a response
        /// </summary>
        /// <param name="code">The code to be checked</param>
        /// <returns>True iff the code indicates a response</returns>
        public static Boolean IsResponse(Int32 code)
        {
            return (code >= 64) && (code <= 191);
        }

        /// <summary>
        /// Checks whether a code represents a success code.
        /// </summary>
        public static Boolean IsSuccess(Int32 code)
        {
            return code >= 64 && code < 96;
        }

        /// <summary>
        /// Checks whether a code is valid
        /// </summary>
        /// <param name="code">The code to be checked</param>
        /// <returns>True iff the code is valid</returns>
        public static Boolean IsValid(Int32 code)
        {
            // allow unknown custom codes
            return (code >= 0) && (code <= 255);
        }

        /// <summary>
        /// Returns a string representation of the code
        /// </summary>
        /// <param name="code">The code to be described</param>
        /// <returns>A string describing the code</returns>
        public static String ToString(Int32 code)
        {
            switch (code)
            {
                case Empty:
                    return "Empty Message";
                case GET:
                    return "GET";
                case POST:
                    return "POST";
                case PUT:
                    return "PUT";
                case DELETE:
                    return "DELETE";
                case Created:
                    return "2.01 Created";
                case Deleted:
                    return "2.02 Deleted";
                case Valid:
                    return "2.03 Valid";
                case Changed:
                    return "2.04 Changed";
                case Content:
                    return "2.05 Content";
                case BadRequest:
                    return "4.00 Bad Request";
                case Unauthorized:
                    return "4.01 Unauthorized";
                case BadOption:
                    return "4.02 Bad Option";
                case Forbidden:
                    return "4.03 Forbidden";
                case NotFound:
                    return "4.04 Not Found";
                case MethodNotAllowed:
                    return "4.05 Method Not Allowed";
                case NotAcceptable:
                    return "4.06 Not Acceptable";
                case RequestEntityIncomplete:
                    return "4.08 Request Entity Incomplete";
                case PreconditionFailed:
                    return "4.12 Precondition Failed";
                case RequestEntityTooLarge:
                    return "4.13 Request Entity Too Large";
                case UnsupportedMediaType:
                    return "4.15 Unsupported Media Type";
                case InternalServerError:
                    return "5.00 Internal Server Error";
                case NotImplemented:
                    return "5.01 Not Implemented";
                case BadGateway:
                    return "5.02 Bad Gateway";
                case ServiceUnavailable:
                    return "5.03 Service Unavailable";
                case GatewayTimeout:
                    return "5.04 Gateway Timeout";
                case ProxyingNotSupported:
                    return "5.05 Proxying Not Supported";
                default:
                    break;
            }

            if (IsValid(code))
            {
                if (IsRequest(code))
                {
                    return String.Format("Unknown Request [code {0}]", code);
                }
                else if (IsResponse(code))
                {
                    return String.Format("Unknown Response [code {0}]", code);
                }
                else
                {
                    return String.Format("Reserved [code {0}]", code);
                }
            }
            else
            {
                return String.Format("Invalid Message [code {0}]", code);
            }
        }
    }

    /// <summary>
    /// Methods of request
    /// </summary>
    public enum Method
    {
        /// <summary>
        /// GET method
        /// </summary>
        GET = 1,
        /// <summary>
        /// POST method
        /// </summary>
        POST = 2,
        /// <summary>
        /// PUT method
        /// </summary>
        PUT = 3,
        /// <summary>
        /// DELETE method
        /// </summary>
        DELETE = 4
    }

    /// <summary>
    /// Response status codes.
    /// </summary>
    public enum StatusCode
    {
        /// <summary>
        /// 2.01 Created
        /// </summary>
        Created = 65,
        /// <summary>
        /// 2.02 Deleted
        /// </summary>
        Deleted = 66,
        /// <summary>
        /// 2.03 Valid 
        /// </summary>
        Valid = 67,
        /// <summary>
        /// 2.04 Changed
        /// </summary>
        Changed = 68,
        /// <summary>
        /// 2.05 Content
        /// </summary>
        Content = 69,
        /// <summary>
        /// 2.?? Continue
        /// </summary>
        Continue = 95,
        /// <summary>
        /// 4.00 Bad Request
        /// </summary>
        BadRequest = 128,
        /// <summary>
        /// 4.01 Unauthorized
        /// </summary>
        Unauthorized = 129,
        /// <summary>
        /// 4.02 Bad Option
        /// </summary>
        BadOption = 130,
        /// <summary>
        /// 4.03 Forbidden
        /// </summary>
        Forbidden = 131,
        /// <summary>
        /// 4.04 Not Found
        /// </summary>
        NotFound = 132,
        /// <summary>
        /// 4.05 Method Not Allowed
        /// </summary>
        MethodNotAllowed = 133,
        /// <summary>
        /// 4.06 Not Acceptable
        /// </summary>
        NotAcceptable = 134,
        /// <summary>
        /// 4.08 Request Entity Incomplete (draft-ietf-core-block)
        /// </summary>
        RequestEntityIncomplete = 136,
        /// <summary>
        /// 
        /// </summary>
        PreconditionFailed = 140,
        /// <summary>
        /// 4.13 Request Entity Too Large
        /// </summary>
        RequestEntityTooLarge = 141,
        /// <summary>
        /// 4.15 Unsupported Media Type
        /// </summary>
        UnsupportedMediaType = 143,
        /// <summary>
        /// 5.00 Internal Server Error
        /// </summary>
        InternalServerError = 160,
        /// <summary>
        /// 5.01 Not Implemented
        /// </summary>
        NotImplemented = 161,
        /// <summary>
        /// 5.02 Bad Gateway
        /// </summary>
        BadGateway = 162,
        /// <summary>
        /// 5.03 Service Unavailable 
        /// </summary>
        ServiceUnavailable = 163,
        /// <summary>
        /// 5.04 Gateway Timeout
        /// </summary>
        GatewayTimeout = 164,
        /// <summary>
        /// 5.05 Proxying Not Supported
        /// </summary>
        ProxyingNotSupported = 165
    }
}
