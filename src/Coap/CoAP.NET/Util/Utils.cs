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
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;

namespace CoAP.Util
{
    /// <summary>
    /// Utility methods.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Insertion sort, to make the options list stably ordered.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">the list to sort</param>
        /// <param name="comparison">the delegate for comparing</param>
        public static void InsertionSort<T>(IList<T> list, Comparison<T> comparison)
        {
            for (Int32 i = 1; i < list.Count; i++)
            {
                Int32 j;
                T temp = list[i];
                for (j = i; j > 0; j--)
                {
                    if (comparison(list[j - 1], temp) > 0)
                    {
                        list[j] = list[j - 1];
                    }
                    else
                    {
                        break;
                    }
                }
                if (i != j)
                    list[j] = temp;
            }
        }

        /// <summary>
        /// Checks if all items in both of the two enumerables are equal.
        /// </summary>
        public static Boolean AreSequenceEqualTo<T>(IEnumerable<T> first, IEnumerable<T> second)
        {
            return AreSequenceEqualTo<T>(first, second, null);
        }

        /// <summary>
        /// Checks if all items in both of the two enumerables are equal.
        /// </summary>
        public static Boolean AreSequenceEqualTo<T>(IEnumerable<T> first, IEnumerable<T> second, IEqualityComparer<T> comparer)
        {
            if (first == null && second == null)
                return true;
            else if (first != null && second != null)
            {
                if (comparer == null)
                    comparer = EqualityComparer<T>.Default;

                using (IEnumerator<T> it1 = first.GetEnumerator())
                using (IEnumerator<T> it2 = second.GetEnumerator())
                {
                    while (it1.MoveNext() && it2.MoveNext())
                    {
                        if (!comparer.Equals(it1.Current, it2.Current))
                            return false;
                    }
                    if (it1.MoveNext() || it2.MoveNext())
                        return false;
                    return true;
                }
            }
            else
                return false;
        }

        /// <summary>
        /// Finds the first matched item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">the source to search</param>
        /// <param name="condition">the condition delegate</param>
        /// <returns>the item found, or null if none is matched</returns>
        public static T FirstOrDefault<T>(IEnumerable<T> source, Predicate<T> condition)
        {
            if (source != null)
            {
                foreach (var item in source)
                {
                    if (condition(item))
                        return item;
                }
            }
            return default(T);
        }

        /// <summary>
        /// Checks if matched item exists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">the source to search</param>
        /// <param name="condition">the condition delegate</param>
        /// <returns>true if exists any matched item, otherwise false</returns>
        public static Boolean Contains<T>(IEnumerable<T> source, Predicate<T> condition)
        {
            if (source != null)
            {
                foreach (var item in source)
                {
                    if (condition(item))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Stringify a message.
        /// </summary>
        public static String ToString(Message msg)
        {
            StringBuilder sb = new StringBuilder();
            String kind = "Message", code = "Code";
            if (msg.IsRequest)
            {
                kind = "Request";
                code = "Method";
            }
            else if (msg.IsResponse)
            {
                kind = "Response";
                code = "Status";
            }

            sb.AppendFormat("==[ COAP {0} ]============================================\n", kind)
                .AppendFormat("ID     : {0}\n", msg.ID)
                .AppendFormat("Type   : {0}\n", msg.Type)
                .AppendFormat("Token  : {0}\n", msg.TokenString)
                .AppendFormat("{1}: {0}\n", CoAP.Code.ToString(msg.Code), code.PadRight(7));
            
            if (msg.Source != null)
                sb.AppendFormat("Source : {0}\n", msg.Source);
            if (msg.Destination != null)
                sb.AppendFormat("Dest   : {0}\n", msg.Destination);
            
            sb.AppendFormat("Options: {0}\n", OptionsToString(msg))
                .AppendFormat("Payload: {0} Bytes\n", msg.PayloadSize);

            if (msg.PayloadSize > 0 && MediaType.IsPrintable(msg.ContentType))
            {
                sb.AppendLine("---------------------------------------------------------------");
                sb.AppendLine(msg.PayloadString);
            }
            sb.AppendLine("===============================================================");

            return sb.ToString();
        }

        /// <summary>
        /// Stringify options in a message.
        /// </summary>
        public static String OptionsToString(Message msg)
        {
            Boolean first = true;
            Action<StringBuilder, String, String> appendIfNotNullOrEmpty =
                delegate(StringBuilder builder, String header, String value)
                {
                    if (String.IsNullOrEmpty(value))
                        return;

                    if (first)
                        first = false;
                    else
                        builder.Append(", ");
                    builder.Append(header).Append("=").Append(value);
                };

            StringBuilder sb = new StringBuilder();
            appendIfNotNullOrEmpty(sb, "If-Match", ToString(msg.IfMatches, bs => ByteArrayUtils.ToHexString(bs)));
            if (msg.HasOption(OptionType.UriHost))
                appendIfNotNullOrEmpty(sb, "URI-Host", msg.UriHost);
            appendIfNotNullOrEmpty(sb, "ETag", ToString(msg.ETags, bs => ByteArrayUtils.ToHexString(bs)));
            if (msg.IfNoneMatch)
                appendIfNotNullOrEmpty(sb, "If-None-Match", msg.IfNoneMatch.ToString());
            if (msg.UriPort > 0)
            appendIfNotNullOrEmpty(sb, "URI-Port", msg.UriPort.ToString());
            appendIfNotNullOrEmpty(sb, "Location-Path", ToString(msg.LocationPaths));
            appendIfNotNullOrEmpty(sb, "URI-Path", ToString(msg.UriPaths));
            if (msg.ContentType != MediaType.Undefined)
                appendIfNotNullOrEmpty(sb, "Content-Type", MediaType.ToString(msg.ContentType));
            if (msg.HasOption(OptionType.MaxAge))
                appendIfNotNullOrEmpty(sb, "Max-Age", msg.MaxAge.ToString());
            appendIfNotNullOrEmpty(sb, "URI-Query", ToString(msg.UriQueries));
            if (msg.Accept != MediaType.Undefined)
                appendIfNotNullOrEmpty(sb, "Accept", MediaType.ToString(msg.Accept));
            appendIfNotNullOrEmpty(sb, "Location-Query", ToString(msg.LocationQueries));
            if (msg.HasOption(OptionType.ProxyUri))
                appendIfNotNullOrEmpty(sb, "Proxy-URI", msg.ProxyUri.ToString());
            appendIfNotNullOrEmpty(sb, "Proxy-Scheme", msg.ProxyScheme);
            if (msg.Block1 != null)
                appendIfNotNullOrEmpty(sb, "Block1", msg.Block1.ToString());
            if (msg.Block2 != null)
                appendIfNotNullOrEmpty(sb, "Block2", msg.Block2.ToString());
            if (msg.Observe.HasValue)
                appendIfNotNullOrEmpty(sb, "Observe", msg.Observe.ToString());
            return sb.ToString();
        }

        /// <summary>
        /// Stringify an enumerable.
        /// </summary>
        public static String ToString<T>(IEnumerable<T> source)
        {
            return ToString(source, o => o.ToString());
        }

        /// <summary>
        /// Stringify an enumerable.
        /// </summary>
        public static String ToString<T>(IEnumerable<T> source, Func<T, String> toString)
        {
            if (source == null)
                return String.Empty;
            StringBuilder sb = new StringBuilder();
            using (IEnumerator<T> it = source.GetEnumerator())
            {
                if (!it.MoveNext())
                    return String.Empty;
                sb.Append(toString(it.Current));
                while (it.MoveNext())
                {
                    sb.Append(", ");
                    sb.Append(toString(it.Current));
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Puts a value associated with a key into a ConcurrentDictionary,
        /// and returns the old value, or null if not exists.
        /// </summary>
        internal static TValue Put<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dic, TKey key, TValue value)
        {
            TValue old = default(TValue);
            dic.AddOrUpdate(key, value, (k, v) =>
            {
                old = v;
                return value;
            });
            return old;
        }
    }
}
