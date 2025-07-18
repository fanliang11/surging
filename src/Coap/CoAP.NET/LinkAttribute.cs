/*
 * Copyright (c) 2011-2013, Longxiang He <helongxiang@smeshlink.com>,
 * SmeshLink Technology Co.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY.
 * 
 * This file is part of the CoAP.NET, a CoAP framework in C#.
 * Please see README for more information.
 */

using System;
using System.Text; 
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Utilities;

namespace CoAP
{
    /// <summary>
    /// Class for linkformat attributes.
    /// </summary>
    public class LinkAttribute : IComparable<LinkAttribute>
    {
        private   readonly ILogger log ;

        private String _name;
        private Object _value;

        /// <summary>
        /// Initializes an attribute.
        /// </summary>
        public LinkAttribute(String name, Object value)
        {
            log = ServiceLocator.GetService<ILogger<LinkAttribute>>();
            _name = name;
            _value = value;
        }

        /// <summary>
        /// Gets the name of this attribute.
        /// </summary>
        public String Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets the value of this attribute.
        /// </summary>
        public Object Value
        {
            get { return _value; }
        }

        /// <summary>
        /// Gets the int value of this attribute.
        /// </summary>
        public Int32 IntValue
        {
            get { return (_value is Int32) ? (Int32)_value : -1; }
        }

        /// <summary>
        /// Gets the string value of this attribute.
        /// </summary>
        public String StringValue
        {
            get { return (_value is String) ? (String)_value : null; }
        }

        /// <summary>
        /// Serializes this attribute into its string representation.
        /// </summary>
        /// <param name="builder"></param>
        public void Serialize(StringBuilder builder)
        {
            // check if there's something to write
            if (_name != null && _value != null)
            {
                if (_value is Boolean)
                {
                    // flag attribute
                    if ((Boolean)_value)
                        builder.Append(_name);
                }
                else
                {
                    // name-value-pair
                    builder.Append(_name);
                    builder.Append('=');
                    if (_value is String)
                    {
                        builder.Append('"');
                        builder.Append((String)_value);
                        builder.Append('"');
                    }
                    else if (_value is Int32)
                    {
                        builder.Append(((Int32)_value));
                    }
                    else
                    {
                        if (log.IsEnabled(LogLevel.Error))
                            log.LogError(String.Format("Serializing attribute of unexpected type: {0} ({1})", _name, _value.GetType().Name));
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override String ToString()
        {
            return String.Format("name: {0} value: {1}", _name, _value);
        }

        /// <inheritdoc/>
        public Int32 CompareTo(LinkAttribute other)
        {
            Int32 ret = _name.CompareTo(other.Name);
            if (ret == 0)
            {
                if (_value is String)
                    return StringValue.CompareTo(other.StringValue);
                else if (_value is Int32)
                    return IntValue.CompareTo(other.IntValue);
            }
            return ret;
        }
    }
}
