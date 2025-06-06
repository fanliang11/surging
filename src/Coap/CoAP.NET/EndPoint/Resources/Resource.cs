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
using System.Collections.Generic;
using System.Text; 
using CoAP.Util;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Utilities;

namespace CoAP.EndPoint.Resources
{
    /// <summary>
    /// This class describes the functionality of a CoAP resource.
    /// </summary>
    public abstract class Resource : IComparable<Resource>
    {
        private readonly ILogger log;

        private Int32 _totalSubResourceCount;
        private String _resourceIdentifier;
        private HashSet<LinkAttribute> _attributes;
        private Resource _parent;
        private SortedDictionary<String, Resource> _subResources;
        private Boolean _hidden;

        /// <summary>
        /// Initialize a resource.
        /// </summary>
        /// <param name="resourceIdentifier">The identifier of this resource</param>
        public Resource(String resourceIdentifier) : this(resourceIdentifier, false) { }

        /// <summary>
        /// Initialize a resource.
        /// </summary>
        /// <param name="resourceIdentifier">The identifier of this resource</param>
        /// <param name="hidden">True if this resource is hidden</param>
        public Resource(String resourceIdentifier, Boolean hidden)
        {
            log=ServiceLocator.GetService<ILogger<Resource>>();
            this._resourceIdentifier = resourceIdentifier;
            this._hidden = hidden;
            this._attributes = new HashSet<LinkAttribute>();
        }

        /// <summary>
        /// Gets the URI of this resource.
        /// </summary>
        public String Path
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Name);
                if (_parent == null)
                    sb.Append("/");
                else
                {
                    Resource res = _parent;
                    while (res != null)
                    {
                        sb.Insert(0, "/");
                        sb.Insert(0, res.Name);
                        res = res._parent;
                    }
                }
                return sb.ToString();
            }
        }

        public String Name
        {
            get { return _resourceIdentifier; }
            set { _resourceIdentifier = value; }
        }

        public ICollection<LinkAttribute> Attributes
        {
            get { return _attributes; }
        }

        public IList<LinkAttribute> GetAttributes(String name)
        {
            List<LinkAttribute> list = new List<LinkAttribute>();
            foreach (LinkAttribute attr in Attributes)
            {
                if (attr.Name.Equals(name))
                    list.Add(attr);
            }
            return list.AsReadOnly();
        }

        public Boolean SetAttribute(LinkAttribute attr)
        {
            // Adds depending on the Link Format rules
            return LinkFormat.AddAttribute(Attributes, attr);
        }

        public Boolean ClearAttribute(String name)
        {
            Boolean cleared = false;
            foreach (LinkAttribute attr in GetAttributes(name))
            {
                cleared |= _attributes.Remove(attr);
            }
            return cleared;
        }

        public Boolean Hidden
        {
            get { return _hidden; }
            set { _hidden = value; }
        }

        public IList<String> ResourceTypes
        {
            get
            {
                return GetStringValues(GetAttributes(LinkFormat.ResourceType));
            }
        }

        /// <summary>
        /// Gets or sets the type attribute of this resource.
        /// </summary>
        public String ResourceType
        {
            get
            {
                IList<LinkAttribute> attrs = GetAttributes(LinkFormat.ResourceType);
                return attrs.Count == 0 ? null : attrs[0].StringValue;
            }
            set
            {
                SetAttribute(new LinkAttribute(LinkFormat.ResourceType, value));
            }
        }

        /// <summary>
        /// Gets or sets the title attribute of this resource.
        /// </summary>
        public String Title
        {
            get
            {
                IList<LinkAttribute> attrs = GetAttributes(LinkFormat.Title);
                return attrs.Count == 0 ? null : attrs[0].StringValue;
            }
            set
            {
                ClearAttribute(LinkFormat.Title);
                SetAttribute(new LinkAttribute(LinkFormat.Title, value));
            }
        }

        public IList<String> InterfaceDescriptions
        {
            get
            {
                return GetStringValues(GetAttributes(LinkFormat.InterfaceDescription));
            }
        }

        /// <summary>
        /// Gets or sets the interface description attribute of this resource.
        /// </summary>
        public String InterfaceDescription
        {
            get
            {
                IList<LinkAttribute> attrs = GetAttributes(LinkFormat.InterfaceDescription);
                return attrs.Count == 0 ? null : attrs[0].StringValue;
            }
            set
            {
                SetAttribute(new LinkAttribute(LinkFormat.InterfaceDescription, value));
            }
        }

        public IList<Int32> GetContentTypeCodes
        {
            get
            {
                return GetIntValues(GetAttributes(LinkFormat.ContentType));
            }
        }

        /// <summary>
        /// Gets or sets the content type code attribute of this resource.
        /// </summary>
        public Int32 ContentTypeCode
        {
            get
            {
                IList<LinkAttribute> attrs = GetAttributes(LinkFormat.ContentType);
                return attrs.Count == 0 ? 0 : attrs[0].IntValue;
            }
            set
            {
                SetAttribute(new LinkAttribute(LinkFormat.ContentType, value));
            }
        }

        /// <summary>
        /// Gets or sets the maximum size estimate attribute of this resource.
        /// </summary>
        public Int32 MaximumSizeEstimate
        {
            get
            {
                IList<LinkAttribute> attrs = GetAttributes(LinkFormat.MaxSizeEstimate);
                return attrs.Count == 0 ? -1 : attrs[0].IntValue;
            }
            set
            {
                SetAttribute(new LinkAttribute(LinkFormat.MaxSizeEstimate, value));
            }
        }

        /// <summary>
        /// Gets or sets the observable attribute of this resource.
        /// </summary>
        public Boolean Observable
        {
            get
            {
                return GetAttributes(LinkFormat.Observable).Count > 0;
            }
            set
            {
                if (value)
                    SetAttribute(new LinkAttribute(LinkFormat.Observable, value));
                else
                    ClearAttribute(LinkFormat.Observable);
            }
        }

        /// <summary>
        /// Gets the total count of sub-resources, including children and children's children...
        /// </summary>
        public Int32 TotalSubResourceCount
        {
            get { return _totalSubResourceCount; }
        }

        /// <summary>
        /// Gets the count of sub-resources of this resource.
        /// </summary>
        public Int32 SubResourceCount
        {
            get { return null == _subResources ? 0 : _subResources.Count; }
        }

        /// <summary>
        /// Removes this resource from its parent.
        /// </summary>
        public void Remove()
        {
            if (_parent != null)
                _parent.RemoveSubResource(this);
        }

        /// <summary>
        /// Gets sub-resources of this resource.
        /// </summary>
        /// <returns></returns>
        public Resource[] GetSubResources()
        {
            if (null == _subResources)
                return new Resource[0];

            Resource[] resources = new Resource[_subResources.Count];
            this._subResources.Values.CopyTo(resources, 0);
            return resources;
        }

        public Resource GetResource(String path)
        {
            return GetResource(path, false);
        }

        public Resource GetResource(String path, Boolean last)
        {
            if (String.IsNullOrEmpty(path))
                return this;

            // find root for absolute path
            if (path.StartsWith("/"))
            {
                Resource root = this;
                while (root._parent != null)
                    root = root._parent;
                path = path.Equals("/") ? null : path.Substring(1);
                return root.GetResource(path);
            }

            Int32 pos = path.IndexOf('/');
            String head = null, tail = null;

            // note: "some/resource/" addresses a resource "" under "resource"
            if (pos == -1)
            {
                head = path;
            }
            else
            {
                head = path.Substring(0, pos);
                tail = path.Substring(pos + 1);
            }

            if (SubResources.ContainsKey(head))
                return SubResources[head].GetResource(tail, last);
            else if (last)
                return this;
            else
                return null;
        }

        private SortedDictionary<String, Resource> SubResources
        {
            get
            {
                if (_subResources == null)
                    _subResources = new SortedDictionary<String, Resource>();
                return _subResources;
            }
        }

        /// <summary>
        /// Adds a resource as a sub-resource of this resource.
        /// </summary>
        /// <param name="resource">The sub-resource to be added</param>
        public void AddSubResource(Resource resource)
        {
            if (null == resource)
                throw new ArgumentNullException("resource");

            // no absolute paths allowed, use root directly
            while (resource.Name.StartsWith("/"))
            {
                if (_parent != null)
                {
                    if (log.IsEnabled(LogLevel.Warning))
                        log.LogWarning("Adding absolute path only allowed for root: made relative " + resource.Name);
                }
                resource.Name = resource.Name.Substring(1);
            }

            // get last existing resource along path
            Resource baseRes = GetResource(resource.Name, true);

            String path = this.Path;
            if (!path.EndsWith("/"))
                path += "/";
            path += resource.Name;

            path = path.Substring(baseRes.Path.Length);
            if (path.StartsWith("/"))
                path = path.Substring(1);

            if (path.Length == 0)
            {
                // resource replaces base
                if (log.IsEnabled(LogLevel.Information))
                    log.LogInformation("Replacing resource " + baseRes.Path);
                foreach (Resource sub in baseRes.GetSubResources())
                {
                    sub._parent = resource;
                    resource.SubResources[sub.Name] = sub;
                }
                resource._parent = baseRes._parent;
                baseRes._parent.SubResources[baseRes.Name] = resource;
            }
            else
            {
                // resource is added to base

                String[] segments = path.Split('/');
                if (segments.Length > 1)
                {
                    if (log.IsEnabled(LogLevel.Debug))
                        log.LogDebug("Splitting up compound resource " + resource.Name);
                    resource.Name = segments[segments.Length - 1];

                    // insert middle segments
                    Resource sub = null;
                    for (Int32 i = 0; i < segments.Length - 1; i++)
                    {
                        sub = baseRes.CreateInstance(segments[i]);
                        sub.Hidden = true;
                        baseRes.AddSubResource(sub);
                        baseRes = sub;
                    }
                }
                else
                    resource.Name = path;

                resource._parent = baseRes;
                baseRes.SubResources[resource.Name] = resource;

                if (log.IsEnabled(LogLevel.Debug))
                    log.LogDebug("Add resource " + resource.Name);
            }

            // update number of sub-resources in the tree
            Resource p = resource._parent;
            while (p != null)
            {
                p._totalSubResourceCount++;
                p = p._parent;
            }
        }

        /// <summary>
        /// Removes a sub-resource from this resource by its identifier.
        /// </summary>
        /// <param name="resourcePath">the path of the sub-resource to remove</param>
        public void RemoveSubResource(String resourcePath)
        {
            RemoveSubResource(GetResource(resourcePath));
        }

        /// <summary>
        /// Removes a sub-resource from this resource.
        /// </summary>
        /// <param name="resource">the sub-resource to remove</param>
        public void RemoveSubResource(Resource resource)
        {
            if (null == resource)
                return;

            if (SubResources.Remove(resource._resourceIdentifier))
            {
                Resource p = resource._parent;
                while (p != null)
                {
                    p._totalSubResourceCount--;
                    p = p._parent;
                }

                resource._parent = null;
            }
        }

        public void CreateSubResource(Request request, String newIdentifier)
        {
            DoCreateSubResource(request, newIdentifier);
        }

        public Int32 CompareTo(Resource other)
        {
            return Path.CompareTo(other.Path);
        }

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            Print(sb, 0);
            return sb.ToString();
        }

        private void Print(StringBuilder sb, Int32 indent)
        {
            for (Int32 i = 0; i < indent; i++)
                sb.Append(" ");
            sb.AppendFormat("+[{0}]",_resourceIdentifier);
            
            String title = Title;
            if (title != null)
                sb.AppendFormat(" {0}", title);
            sb.AppendLine();

            foreach (LinkAttribute attr in Attributes)
            {
                if (attr.Name.Equals(LinkFormat.Title))
                    continue;
                for (Int32 i = 0; i < indent + 3; i++)
                    sb.Append(" ");
                sb.AppendFormat("- ");
                attr.Serialize(sb);
                sb.AppendLine();
            }

            if (_subResources != null)
                foreach (Resource sub in _subResources.Values)
                {
                    sub.Print(sb, indent + 2);
                }
        }

        /// <summary>
        /// Creates a resouce instance with proper subtype.
        /// </summary>
        /// <returns></returns>
        protected abstract Resource CreateInstance(String name);
        protected abstract void DoCreateSubResource(Request request, String newIdentifier);

        private static IList<String> GetStringValues(IEnumerable<LinkAttribute> attributes)
        {
            List<String> list = new List<String>();
            foreach (LinkAttribute attr in attributes)
            {
                list.Add(attr.StringValue);
            }
            return list;
        }

        private static IList<Int32> GetIntValues(IEnumerable<LinkAttribute> attributes)
        {
            List<Int32> list = new List<Int32>();
            foreach (LinkAttribute attr in attributes)
            {
                list.Add(attr.IntValue);
            }
            return list;
        }
    }
}
