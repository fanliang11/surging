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
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

using System;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using DotNetty.Codecs.Internal;

namespace DotNetty.Codecs
{
    internal static partial class CThrowHelper
    {
        #region -- Throw ArgumentException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException()
        {
            throw GetArgumentException();

            static ArgumentException GetArgumentException()
            {
                return new ArgumentException();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException(CExceptionResource resource)
        {
            throw GetArgumentException(resource);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException(CExceptionResource resource, CExceptionArgument argument)
        {
            throw GetArgumentException(resource, argument);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException(string message, CExceptionArgument argument)
        {
            throw GetArgumentException(message, argument);

            static ArgumentException GetArgumentException(string message, CExceptionArgument argument)
            {
                return new ArgumentException(message, GetArgumentName(argument));

            }
        }

        #endregion

        #region -- Get ArgumentException --

        internal static ArgumentException GetArgumentException(CExceptionResource resource)
        {
            return new ArgumentException(GetResourceString(resource));
        }

        internal static ArgumentException GetArgumentException(CExceptionResource resource, CExceptionArgument argument)
        {
            return new ArgumentException(GetResourceString(resource), GetArgumentName(argument));
        }

        #endregion


        #region -- Throw ArgumentOutOfRangeException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException()
        {
            throw GetArgumentOutOfRangeException();

            static ArgumentOutOfRangeException GetArgumentOutOfRangeException()
            {
                return new ArgumentOutOfRangeException();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException(CExceptionArgument argument)
        {
            throw GetArgumentOutOfRangeException(argument);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException(CExceptionArgument argument, CExceptionResource resource)
        {
            throw GetArgumentOutOfRangeException(argument, resource);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException(CExceptionArgument argument, int paramNumber, CExceptionResource resource)
        {
            throw GetArgumentOutOfRangeException(argument, paramNumber, resource);
        }

        #endregion

        #region -- Get ArgumentOutOfRangeException --

        internal static ArgumentOutOfRangeException GetArgumentOutOfRangeException(CExceptionArgument argument)
        {
            return new ArgumentOutOfRangeException(GetArgumentName(argument));
        }

        internal static ArgumentOutOfRangeException GetArgumentOutOfRangeException(CExceptionArgument argument, CExceptionResource resource)
        {
            return new ArgumentOutOfRangeException(GetArgumentName(argument), GetResourceString(resource));
        }

        internal static ArgumentOutOfRangeException GetArgumentOutOfRangeException(CExceptionArgument argument, int paramNumber, CExceptionResource resource)
        {
            return new ArgumentOutOfRangeException(GetArgumentName(argument) + "[" + paramNumber.ToString() + "]", GetResourceString(resource));
        }

        #endregion


        #region -- Throw ArgumentNullException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentNullException(CExceptionArgument argument)
        {
            throw GetArgumentNullException(argument);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentNullException(CExceptionResource resource)
        {
            throw GetArgumentNullException(resource);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentNullException(CExceptionArgument argument, CExceptionResource resource)
        {
            throw GetArgumentNullException(argument, resource);
        }

        #endregion

        #region -- Get ArgumentNullException --

        internal static ArgumentNullException GetArgumentNullException(CExceptionArgument argument)
        {
            return new ArgumentNullException(GetArgumentName(argument));
        }

        internal static ArgumentNullException GetArgumentNullException(CExceptionResource resource)
        {
            return new ArgumentNullException(GetResourceString(resource), innerException: null);
        }

        internal static ArgumentNullException GetArgumentNullException(CExceptionArgument argument, CExceptionResource resource)
        {
            return new ArgumentNullException(GetArgumentName(argument), GetResourceString(resource));
        }

        #endregion


        #region -- IndexOutOfRangeException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIndexOutOfRangeException()
        {
            throw GetIndexOutOfRangeException();

            static IndexOutOfRangeException GetIndexOutOfRangeException()
            {
                return new IndexOutOfRangeException();
            }
        }

        #endregion

        #region -- Throw InvalidOperationException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException(CExceptionResource resource)
        {
            throw GetInvalidOperationException(resource);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException(CExceptionResource resource, Exception e)
        {
            throw GetInvalidOperationException(resource, e);

            static InvalidOperationException GetInvalidOperationException(CExceptionResource resource, Exception e)
            {
                return new InvalidOperationException(GetResourceString(resource), e);
            }
        }

        internal static InvalidOperationException GetInvalidOperationException(CExceptionResource resource)
        {
            return new InvalidOperationException(GetResourceString(resource));
        }

        #endregion

        #region ** GetArgumentName **

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetArgumentName(CExceptionArgument argument)
        {
            Debug.Assert(Enum.IsDefined(typeof(CExceptionArgument), argument),
                "The enum value is not defined, please check the CExceptionArgument Enum.");

            return argument.ToString();
        }

        #endregion

        #region ** GetResourceString **

        // This function will convert an CExceptionResource enum value to the resource string.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetResourceString(CExceptionResource resource)
        {
            Debug.Assert(Enum.IsDefined(typeof(CExceptionResource), resource),
                "The enum value is not defined, please check the CExceptionResource Enum.");

            return SR.GetResourceString(resource.ToString());
        }

        #endregion
    }
}
