/*
 * Copyright (c) 2011-2014, Longxiang He <helongxiang@smeshlink.com>,
 * SmeshLink Technology Co.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY.
 * 
 * This file is part of the CoAP.NET, a CoAP framework in C#.
 * Please see README for more information.
 */

using System;

namespace CoAP.Util
{
    static class ThrowHelper
    {
        public static Exception ArgumentNull(String paramName)
        {
            return new ArgumentNullException(paramName);
        }

        public static Exception Argument(String paramName, String message)
        {
            return new ArgumentException(message, paramName);
        }
    }
}
