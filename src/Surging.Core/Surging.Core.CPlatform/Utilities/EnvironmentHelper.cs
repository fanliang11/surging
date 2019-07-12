using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Surging.Core.CPlatform.Utilities
{
    /// <summary>
    /// Defines the <see cref="EnvironmentHelper" />
    /// </summary>
    public class EnvironmentHelper
    {
        #region 方法

        /// <summary>
        /// The GetEnvironmentVariable
        /// </summary>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        public static string GetEnvironmentVariable(string value)
        {
            var result = value;
            var param = GetParameters(result).FirstOrDefault();
            if (!string.IsNullOrEmpty(param))
            {
                var env = Environment.GetEnvironmentVariable(param);
                result = env;
                if (string.IsNullOrEmpty(env))
                {
                    var arrayData = value.ToString().Split("|");
                    result = arrayData.Length == 2 ? arrayData[1] : env;
                }
            }
            return result;
        }

        /// <summary>
        /// The GetEnvironmentVariableAsBool
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="defaultValue">The defaultValue<see cref="bool"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public static bool GetEnvironmentVariableAsBool(string name, bool defaultValue = false)
        {
            var str = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(str))
            {
                return defaultValue;
            }

            switch (str.ToLowerInvariant())
            {
                case "true":
                case "1":
                case "yes":
                    return true;

                case "false":
                case "0":
                case "no":
                    return false;

                default:
                    return defaultValue;
            }
        }

        /// <summary>
        /// The GetParameters
        /// </summary>
        /// <param name="text">The text<see cref="string"/></param>
        /// <returns>The <see cref="List{string}"/></returns>
        private static List<string> GetParameters(string text)
        {
            var matchVale = new List<string>();
            string Reg = @"(?<=\${)[^\${}]*(?=})";
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