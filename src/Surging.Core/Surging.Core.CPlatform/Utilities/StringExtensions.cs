using System;
using System.Text.RegularExpressions;

namespace Surging.Core.CPlatform.Utilities
{
    /// <summary>
    /// Defines the <see cref="StringExtensions" />
    /// </summary>
    public static class StringExtensions
    {
        #region 方法

        /// <summary>
        /// The IsIP
        /// </summary>
        /// <param name="input">The input<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public static bool IsIP(this string input) => input.IsMatch(@"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\:\d{2,5}\b");

        /// <summary>
        /// The IsMatch
        /// </summary>
        /// <param name="str">The str<see cref="string"/></param>
        /// <param name="op">The op<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public static bool IsMatch(this string str, string op)
        {
            if (str.Equals(String.Empty) || str == null) return false;
            var re = new Regex(op, RegexOptions.IgnoreCase);
            return re.IsMatch(str);
        }

        #endregion 方法
    }
}