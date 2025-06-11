//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// System.ServiceModel\System\ServiceModel\EmptyArray.cs
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DotNetty.Common.Internal
{
    public sealed class EmptyArray<T>
    {
        public static readonly T[] Instance;

        static EmptyArray()
        {
#if NET_4_5_GREATER || NETSTANDARD || NETCOREAPP
            Instance = Array.Empty<T>();
#else
            Instance = new T[0];
#endif
        }

        public static T[] Allocate(int n)
        {
            if (0u >= (uint)n) { return Instance; }
            else { return new T[n]; }
        }

        public static T[] ToArray(IList<T> collection)
        {
            if (0u >= (uint)collection.Count)
            {
                return Instance;
            }
            T[] array = new T[collection.Count];
            collection.CopyTo(array, 0);
            return array;
        }
    }

    public sealed class EmptyArray
    {
        public static readonly object[] Instance;

        static EmptyArray()
        {
#if NET_4_5_GREATER || NETSTANDARD || NETCOREAPP
            Instance = Array.Empty<object>();
#else
            Instance = new object[0];
#endif
        }

        public static object[] Allocate(int n)
        {
            return 0u >= (uint)n ? Instance : (new object[n]);
        }
    }
}
