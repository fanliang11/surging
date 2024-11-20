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

namespace CoAP.Stack
{
    /// <summary>
    /// Represents a name-filter pair that an <see cref="IChain&lt;TFilter, TNextFilter&gt;"/> contains.
    /// </summary>
    public interface IEntry<TFilter, TNextFilter>
    {
        /// <summary>
        /// Gets the name of the filter.
        /// </summary>
        String Name { get; }
        /// <summary>
        /// Gets the filter.
        /// </summary>
        TFilter Filter { get; }
        /// <summary>
        /// Gets the <typeparamref name="TNextFilter"/> of the filter.
        /// </summary>
        TNextFilter NextFilter { get; }
        /// <summary>
        /// Adds the specified filter with the specified name just before this entry.
        /// </summary>
        void AddBefore(String name, TFilter filter);
        /// <summary>
        /// Adds the specified filter with the specified name just after this entry.
        /// </summary>
        void AddAfter(String name, TFilter filter);
        /// <summary>
        /// Replace the filter of this entry with the specified new filter.
        /// </summary>
        void Replace(TFilter newFilter);
        /// <summary>
        /// Removes this entry from the chain it belongs to.
        /// </summary>
        void Remove();
    }
}
