using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Surging.Core.CPlatform.Routing.Template
{
    /// <summary>
    /// Defines the <see cref="RoutePatternParser" />
    /// </summary>
    public class RoutePatternParser
    {
        #region 方法

        /// <summary>
        /// The Parse
        /// </summary>
        /// <param name="routeTemplet">The routeTemplet<see cref="string"/></param>
        /// <param name="service">The service<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        public static string Parse(string routeTemplet, string service)
        {
            StringBuilder result = new StringBuilder();
            var parameters = routeTemplet.Split(@"/");
            foreach (var parameter in parameters)
            {
                var param = GetParameters(parameter).FirstOrDefault();
                if (param == null)
                {
                    result.Append(parameter);
                }
                else if (service.EndsWith(param))
                {
                    result.Append(service.Substring(1, service.Length - param.Length - 1));
                }
                result.Append("/");
            }

            return result.ToString().TrimEnd('/').ToLower();
        }

        /// <summary>
        /// The Parse
        /// </summary>
        /// <param name="routeTemplet">The routeTemplet<see cref="string"/></param>
        /// <param name="service">The service<see cref="string"/></param>
        /// <param name="method">The method<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        public static string Parse(string routeTemplet, string service, string method)
        {
            StringBuilder result = new StringBuilder();
            var parameters = routeTemplet.Split(@"/");
            bool isAppendMethod = false;
            foreach (var parameter in parameters)
            {
                var param = GetParameters(parameter).FirstOrDefault();
                if (param == null)
                {
                    result.Append(parameter);
                }
                else if (service.EndsWith(param))
                {
                    result.Append(service.Substring(1, service.Length - param.Length - 1));
                }
                else if (param == "Method")
                {
                    result.Append(method);
                    isAppendMethod = true;
                }
                else
                {
                    if (!isAppendMethod) result.AppendFormat("{0}/", method);
                    result.Append(parameter);
                    isAppendMethod = true;
                }
                result.Append("/");
            }
            result.Length = result.Length - 1;
            if (!isAppendMethod) result.AppendFormat("/{0}", method);
            return result.ToString().ToLower();
        }

        /// <summary>
        /// The GetParameters
        /// </summary>
        /// <param name="text">The text<see cref="string"/></param>
        /// <returns>The <see cref="List{string}"/></returns>
        private static List<string> GetParameters(string text)
        {
            var matchVale = new List<string>();
            string Reg = @"(?<={)[^{}]*(?=})";
            string key = string.Empty;
            foreach (Match m in Regex.Matches(text, Reg))
            {
                matchVale.Add(m.Value);
            }
            return matchVale;
        }

        #endregion 方法
    }
}