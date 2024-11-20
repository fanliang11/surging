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
using CoAP.Server.Resources;
using CoAP.Util;

namespace CoAP
{
    /// <summary>
    /// This class can be used to programmatically browse a remote CoAP endoint.
    /// </summary>
    public class WebLink : IComparable<WebLink>
    {
        readonly String _uri;
        readonly ResourceAttributes _attributes = new ResourceAttributes();

        /// <summary>
        /// Instantiates.
        /// </summary>
        /// <param name="uri">the uri of this resource.</param>
        public WebLink(String uri)
        {
            _uri = uri;
        }

        /// <summary>
        /// Gets the uri of this resource.
        /// </summary>
        public String Uri { get { return _uri; } }

        /// <summary>
        /// Gets the attributes of this resource.
        /// </summary>
        public ResourceAttributes Attributes { get { return _attributes; } }

        /// <inheritdoc/>
        public Int32 CompareTo(WebLink other)
        {
            if (other == null)
                throw ThrowHelper.ArgumentNull("other");
            return _uri.CompareTo(other._uri);
        }

        /// <inheritdoc/>
        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append('<').Append(_uri).Append('>')
                .Append(' ').Append(_attributes.Title);
            if (_attributes.Contains(LinkFormat.ResourceType))
                sb.Append("\n\t").Append(LinkFormat.ResourceType)
                    .Append(":\t").Append(_attributes.GetResourceTypes());
            if (_attributes.Contains(LinkFormat.InterfaceDescription))
                sb.Append("\n\t").Append(LinkFormat.InterfaceDescription)
                    .Append(":\t").Append(_attributes.GetInterfaceDescriptions());
            if (_attributes.Contains(LinkFormat.ContentType))
                sb.Append("\n\t").Append(LinkFormat.ContentType)
                    .Append(":\t").Append(_attributes.GetContentTypes());
            if (_attributes.Contains(LinkFormat.MaxSizeEstimate))
                sb.Append("\n\t").Append(LinkFormat.MaxSizeEstimate)
                    .Append(":\t").Append(_attributes.MaximumSizeEstimate);
            if (_attributes.Observable)
                sb.Append("\n\t").Append(LinkFormat.Observable);
            return sb.ToString();
        }
    }
}
