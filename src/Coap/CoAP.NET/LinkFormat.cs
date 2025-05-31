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
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using CoAP.EndPoint.Resources; 
using CoAP.Server.Resources;
using CoAP.Util;
using Microsoft.Extensions.Logging;
using Resource = CoAP.EndPoint.Resources.Resource;

namespace CoAP
{
    /// <summary>
    /// This class provides link format definitions as specified in
    /// draft-ietf-core-link-format-06
    /// </summary>
    public static class LinkFormat
    {
        /// <summary>
        /// Name of the attribute Resource Type
        /// </summary>
        public static readonly String ResourceType = "rt";
        /// <summary>
        /// Name of the attribute Interface Description
        /// </summary>
        public static readonly String InterfaceDescription = "if";
        /// <summary>
        /// Name of the attribute Content Type
        /// </summary>
        public static readonly String ContentType = "ct";
        /// <summary>
        /// Name of the attribute Max Size Estimate
        /// </summary>
        public static readonly String MaxSizeEstimate = "sz";
        /// <summary>
        /// Name of the attribute Title
        /// </summary>
        public static readonly String Title = "title";
        /// <summary>
        /// Name of the attribute Observable
        /// </summary>
        public static readonly String Observable = "obs";
        /// <summary>
        /// Name of the attribute link
        /// </summary>
        public static readonly String Link = "href";

        /// <summary>
        /// The string as the delimiter between resources
        /// </summary>
        public static readonly String Delimiter = ",";
        /// <summary>
        /// The string to separate attributes
        /// </summary>
        public static readonly String Separator = ";";

        public static readonly Regex DelimiterRegex = new Regex("\\s*" + Delimiter + "+\\s*");
        public static readonly Regex SeparatorRegex = new Regex("\\s*" + Separator + "+\\s*");

        public static readonly Regex ResourceNameRegex = new Regex("<[^>]*>");
        public static readonly Regex WordRegex = new Regex("\\w+");
        public static readonly Regex QuotedString = new Regex("\\G\".*?\"");
        public static readonly Regex Cardinal = new Regex("\\G\\d+");
        static readonly Regex EqualRegex = new Regex("=");
        static readonly Regex BlankRegex = new Regex("\\s");
          
        public static String Serialize(IResource root)
        {
            return Serialize(root, null);
        }

        public static String Serialize(IResource root, IEnumerable<String> queries)
        {
            StringBuilder linkFormat = new StringBuilder();

            foreach (IResource child in root.Children)
            {
                SerializeTree(child, queries, linkFormat);
            }

            if (linkFormat.Length > 1)
                linkFormat.Remove(linkFormat.Length - 1, 1);

            return linkFormat.ToString();
        }

        public static IEnumerable<WebLink> Parse(String linkFormat)
        {
            if (!String.IsNullOrEmpty(linkFormat))
            {
                Scanner scanner = new Scanner(linkFormat);
                String path = null;
                while ((path = scanner.Find(ResourceNameRegex)) != null)
                {
                    path = path.Substring(1, path.Length - 2);
                    WebLink link = new WebLink(path);

                    String attr = null;
                    while (scanner.Find(DelimiterRegex, 1) == null &&
                        (attr = scanner.Find(WordRegex)) != null)
                    {
                        if (scanner.Find(EqualRegex, 1) == null)
                        {
                            // flag attribute without value
                            link.Attributes.Add(attr);
                        }
                        else
                        {
                            String value = null;
                            if ((value = scanner.Find(QuotedString)) != null)
                            {
                                // trim " "
                                value = value.Substring(1, value.Length - 2);
                                if (Title.Equals(attr))
                                    link.Attributes.Add(attr, value);
                                else
                                    foreach (String part in BlankRegex.Split(value))
                                        link.Attributes.Add(attr, part);
                            }
                            else if ((value = scanner.Find(WordRegex)) != null)
                            {
                                link.Attributes.Set(attr, value);
                            }
                            else if ((value = scanner.Find(Cardinal)) != null)
                            {
                                link.Attributes.Set(attr, value);
                            }
                        }
                    }

                    yield return link;
                }
            }

            yield break;
        }

        private static void SerializeTree(IResource resource, IEnumerable<String> queries, StringBuilder sb)
        {
            if (resource.Visible && Matches(resource, queries))
            {
                SerializeResource(resource, sb);
                sb.Append(",");
            }

            // sort by resource name
            List<IResource> childrens = new List<IResource>(resource.Children);
            childrens.Sort(delegate(IResource r1, IResource r2) { return String.Compare(r1.Name, r2.Name); });

            foreach (IResource child in childrens)
            {
                SerializeTree(child, queries, sb);
            }
        }

        private static void SerializeResource(IResource resource, StringBuilder sb)
        {
            sb.Append("<")
                .Append(resource.Path)
                .Append(resource.Name)
                .Append(">");
            SerializeAttributes(resource.Attributes, sb);
        }

        private static void SerializeAttributes(ResourceAttributes attributes, StringBuilder sb)
        {
            List<String> keys = new List<String>(attributes.Keys);
            keys.Sort();
            foreach (String name in keys)
            {
                List<String> values = new List<String>(attributes.GetValues(name));
                if (values.Count == 0)
                    continue;
                sb.Append(Separator);
                SerializeAttribute(name, values, sb);
            }
        }

        private static void SerializeAttribute(String name, IEnumerable<String> values, StringBuilder sb)
        {
            String delimiter = "=";
            Boolean quotes = false;

            sb.Append(name);

            using (IEnumerator<String> it = values.GetEnumerator())
            {
                if (!it.MoveNext() || String.IsNullOrEmpty(it.Current))
                    return;

                sb.Append(delimiter);

                String first = it.Current;
                Boolean more = it.MoveNext();
                if (more || !IsNumber(first))
                {
                    sb.Append('"');
                    quotes = true;
                }

                sb.Append(first);
                while (more)
                {
                    sb.Append(' ');
                    sb.Append(it.Current);
                    more = it.MoveNext();
                }

                if (quotes)
                    sb.Append('"');
            }
        }

        private static Boolean IsNumber(String value)
        {
            if (String.IsNullOrEmpty(value))
                return false;
            foreach (Char c in value)
            {
                if (!Char.IsNumber(c))
                    return false;
            }
            return true;
        }

        public static String Serialize(Resource resource, IEnumerable<Option> query, Boolean recursive)
        {
            StringBuilder linkFormat = new StringBuilder();

            // skip hidden and empty root in recursive mode, always skip non-matching resources
            if ((!resource.Hidden && (resource.Name.Length > 0) || !recursive) 
                && Matches(resource, query))
            {
                linkFormat.Append("<")
                    .Append(resource.Path)
                    .Append(">");

                foreach (LinkAttribute attr in resource.Attributes)
                {
                    linkFormat.Append(Separator);
                    attr.Serialize(linkFormat);
                }
            }

            if (recursive)
            {
                foreach (Resource sub in resource.GetSubResources())
                {
                    String next = Serialize(sub, query, true);

                    if (next.Length > 0)
                    {
                        if (linkFormat.Length > 3)
                            linkFormat.Append(Delimiter);
                        linkFormat.Append(next);
                    }
                }
            }

            return linkFormat.ToString();
        }

        public static RemoteResource Deserialize(String linkFormat)
        {
            RemoteResource root = new RemoteResource(String.Empty);
            Scanner scanner = new Scanner(linkFormat);

            String path = null;
            while ((path = scanner.Find(ResourceNameRegex)) != null)
            {
                path = path.Substring(1, path.Length - 2);

                // Retrieve specified resource, create if necessary
                RemoteResource resource = new RemoteResource(path);

                LinkAttribute attr = null;
                while (scanner.Find(DelimiterRegex, 1) == null && (attr = ParseAttribute(scanner)) != null)
                {
                    AddAttribute(resource.Attributes, attr);
                }

                root.AddSubResource(resource);
            }

            return root;
        }

        private static LinkAttribute ParseAttribute(Scanner scanner)
        {
            String name = scanner.Find(WordRegex);
            if (name == null)
                return null;
            else
            {
                Object value = null;
                // check for name-value-pair
                if (scanner.Find(new Regex("="), 1) == null)
                    // flag attribute
                    value = true;
                else
                {
                    String s = null;
                    if ((s = scanner.Find(QuotedString)) != null)
                        // trim " "
                        value = s.Substring(1, s.Length - 2);
                    else if ((s = scanner.Find(Cardinal)) != null)
                        value = Int32.Parse(s);
                    // TODO what if both pattern failed?
                }
                return new LinkAttribute(name, value);
            }
        }

        private static Boolean Matches(Resource resource, IEnumerable<Option> query)
        {
            if (resource == null)
                return false;

            if (query == null)
                return true;

            foreach (Option q in query)
            {
                String s = q.StringValue;
                Int32 delim = s.IndexOf('=');
                if (delim == -1)
                {
                    // flag attribute
                    if (resource.GetAttributes(s).Count > 0)
                        return true;
                }
                else
                {
                    String attrName = s.Substring(0, delim);
                    String expected = s.Substring(delim + 1);

                    if (attrName.Equals(LinkFormat.Link))
                    {
                        if (expected.EndsWith("*"))
                            return resource.Path.StartsWith(expected.Substring(0, expected.Length - 1));
                        else
                            return resource.Path.Equals(expected);
                    }
                    
                    foreach (LinkAttribute attr in resource.GetAttributes(attrName))
                    {
                        String actual = attr.Value.ToString();

                        // get prefix length according to "*"
                        Int32 prefixLength = expected.IndexOf('*');
                        if (prefixLength >= 0 && prefixLength < actual.Length)
                        {
                            // reduce to prefixes
                            expected = expected.Substring(0, prefixLength);
                            actual = actual.Substring(0, prefixLength);
                        }

                        // handle case like rt=[Type1 Type2]
                        if (actual.IndexOf(' ') > -1)
                        {
                            foreach (String part in actual.Split(' '))
                            {
                                if (part.Equals(expected))
                                    return true;
                            }
                        }

                        if (expected.Equals(actual))
                            return true;
                    }
                }
            }

            return false;
        }

        private static Boolean Matches(IResource resource, IEnumerable<String> query)
        {
            if (resource == null)
                return false;
            if (query == null)
                return true;

            using (IEnumerator<String> ie = query.GetEnumerator())
            {
                if (!ie.MoveNext())
                    return true;

                ResourceAttributes attributes = resource.Attributes;
                String path = resource.Path + resource.Name;

                do
                {
                    String s = ie.Current;

                    Int32 delim = s.IndexOf('=');
                    if (delim == -1)
                    {
                        // flag attribute
                        if (attributes.Contains(s))
                            return true;
                    }
                    else
                    {
                        String attrName = s.Substring(0, delim);
                        String expected = s.Substring(delim + 1);

                        if (attrName.Equals(LinkFormat.Link))
                        {
                            if (expected.EndsWith("*"))
                                return path.StartsWith(expected.Substring(0, expected.Length - 1));
                            else
                                return path.Equals(expected);
                        }
                        else if (attributes.Contains(attrName))
                        {
                            // lookup attribute value
                            foreach (String value in attributes.GetValues(attrName))
                            {
                                String actual = value;
                                // get prefix length according to "*"
                                Int32 prefixLength = expected.IndexOf('*');
                                if (prefixLength >= 0 && prefixLength < actual.Length)
                                {
                                    // reduce to prefixes
                                    expected = expected.Substring(0, prefixLength);
                                    actual = actual.Substring(0, prefixLength);
                                }

                                // handle case like rt=[Type1 Type2]
                                if (actual.IndexOf(' ') > -1)
                                {
                                    foreach (String part in actual.Split(' '))
                                    {
                                        if (part.Equals(expected))
                                            return true;
                                    }
                                }

                                if (expected.Equals(actual))
                                    return true;
                            }
                        }
                    }
                } while (ie.MoveNext());
            }

            return false;
        }

        internal static Boolean AddAttribute(ICollection<LinkAttribute> attributes, LinkAttribute attrToAdd)
        {
            if (IsSingle(attrToAdd.Name))
            {
                foreach (LinkAttribute attr in attributes)
                {
                    if (attr.Name.Equals(attrToAdd.Name))
                    {
                        return false;
                    }
                }
            }

            // special rules
            if (attrToAdd.Name.Equals(ContentType) && attrToAdd.IntValue < 0)
                return false;
            if (attrToAdd.Name.Equals(MaxSizeEstimate) && attrToAdd.IntValue < 0)
                return false;

            attributes.Add(attrToAdd);
            return true;
        }

        private static Boolean IsSingle(String name)
        {
            return name.Equals(Title) || name.Equals(MaxSizeEstimate) || name.Equals(Observable);
        }
    }
}
