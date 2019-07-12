using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Surging.Core.CPlatform.Routing.Template
{
    /// <summary>
    /// Defines the <see cref="RouteTemplateSegmenter" />
    /// </summary>
    public class RouteTemplateSegmenter
    {
        #region 方法

        /// <summary>
        /// The Segment
        /// </summary>
        /// <param name="routePath">The routePath<see cref="string"/></param>
        /// <param name="path">The path<see cref="string"/></param>
        /// <returns>The <see cref="Dictionary{string,object}"/></returns>
        public static Dictionary<string, object> Segment(string routePath, string path)
        {
            var pattern = "/{.*?}";
            var result = new Dictionary<string, object>();
            if (Regex.IsMatch(routePath, pattern))
            {
                var routeTemplate = Regex.Replace(routePath, pattern, "");
                var routeSegments = routeTemplate.Split('/');
                var pathSegments = path.Split('/');
                var segments = routePath.Split("/");
                for (var i = routeSegments.Length; i < pathSegments.Length; i++)
                {
                    result.Add(segments[i].Replace("{", "").Replace("}", ""), pathSegments[i]);
                }
            }
            return result;
        }

        #endregion 方法
    }
}