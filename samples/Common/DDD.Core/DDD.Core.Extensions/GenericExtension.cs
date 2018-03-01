using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace DDD.Core.Extensions
{
    /// <summary>
    /// 泛型扩展
    /// </summary>
    public static class GenericExtension
    {
        public static bool Equal<T>(this T x, T y)
        {
            return ((IComparable)(x)).CompareTo(y) == 0;
        }

        #region ToDictionary

        public static Dictionary<string, string> ToDictionary<T>(this T t, Dictionary<string, string> dic = null) where T : class
        {
            if (dic == null)
                dic = new Dictionary<string, string>();
            var properties = t.GetType().GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(t, null);
                dic.Add(property.Name, value?.ToString() ?? "");
            }
            return dic;
        }

        public static Dictionary<string, string> ToDictionary<TInterface, T>(this TInterface t, Dictionary<string, string> dic = null) where T : class, TInterface
        {
            if (dic == null)
                dic = new Dictionary<string, string>();
            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(t, null);
                if (value == null) continue;
                dic.Add(property.Name, value?.ToString() ?? "");
            }
            return dic;
        }

        #endregion

        /// <summary>
        ///     将字符串转换为指定的类型，如果转换不成功，返回默认值。
        /// </summary>
        /// <typeparam name="T">结构体类型或枚举类型</typeparam>
        /// <param name="str">需要转换的字符串</param>
        /// <returns>返回指定的类型。</returns>
        public static T ParseTo<T>(this string str) where T : struct
        {
            return str.ParseTo(default(T));
        }

        /// <summary>
        ///     将字符串转换为指定的类型，如果转换不成功，返回默认值。
        /// </summary>
        /// <typeparam name="T">结构体类型或枚举类型</typeparam>
        /// <param name="str">需要转换的字符串</param>
        /// <param name="defaultValue">如果转换失败，需要使用的默认值</param>
        /// <returns>返回指定的类型。</returns>
        public static T ParseTo<T>(this string str, T defaultValue) where T : struct
        {
            var t = str.ParseToNullable<T>();
            if (t.HasValue)
            {
                return t.Value;
            }
            return defaultValue;
        }

        /// <summary>
        ///     将字符串转换为指定的类型，如果转换不成功，返回null
        /// </summary>
        /// <typeparam name="T">结构体类型或枚举类型</typeparam>
        /// <param name="str">需要转换的字符串</param>
        /// <returns>返回指定的类型</returns>
        public static T? ParseToNullable<T>(this string str) where T : struct
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            var typeFromHandle = typeof(T);
            if (typeFromHandle.IsEnum)
            {
                return str.ToEnum<T>();
            }
            return (T?)str.ParseTo(typeFromHandle.FullName);
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> source)
        {
            DataTable dtReturn = new DataTable();

            // column names 
            PropertyInfo[] oProps = null;

            if (source == null) return dtReturn;

            foreach (var rec in source)
            {
                // Use reflection to get property names, to create table, Only first time, others will follow 
                if (oProps == null)
                {
                    oProps = rec.GetType().GetProperties();
                    foreach (var pi in oProps)
                    {
                        var colType = pi.PropertyType;

                        if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                        {
                            colType = colType.GetGenericArguments()[0];
                        }

                        dtReturn.Columns.Add(new DataColumn(pi.Name, colType));
                    }
                }

                var dr = dtReturn.NewRow();

                foreach (var pi in oProps)
                {
                    dr[pi.Name] = pi.GetValue(rec, null) == null
                        ? DBNull.Value
                        : pi.GetValue
                            (rec, null);
                }

                dtReturn.Rows.Add(dr);
            }
            return dtReturn;
        }
    }
}
