/*
 * Copyright (c) 2011-2012, Longxiang He <helongxiang@smeshlink.com>,
 * SmeshLink Technology Co.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY.
 * 
 * This file is part of the CoAP.NET, a CoAP framework in C#.
 * Please see README for more information.
 */

using System;

namespace CoAP.EndPoint.Resources
{
    public class RemoteResource : Resource
    {
        public RemoteResource(String resourceIdentifier)
            : base(resourceIdentifier)
        { }

        public static RemoteResource NewRoot(String linkFormat)
        {
            return LinkFormat.Deserialize(linkFormat);
        }

        /// <summary>
        /// Creates a resouce instance with proper subtype.
        /// </summary>
        /// <returns></returns>
        protected override Resource CreateInstance(String name)
        {
            return new RemoteResource(name);
        }

        protected override void DoCreateSubResource(Request request, String newIdentifier)
        { 
        }
    }
}
