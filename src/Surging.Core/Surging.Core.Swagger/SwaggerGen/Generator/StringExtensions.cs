namespace Surging.Core.SwaggerGen
{
    /// <summary>
    /// Defines the <see cref="StringExtensions" />
    /// </summary>
    internal static class StringExtensions
    {
        #region 方法

        /// <summary>
        /// The ToCamelCase
        /// </summary>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        internal static string ToCamelCase(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return char.ToLowerInvariant(value[0]) + value.Substring(1);
        }

        /// <summary>
        /// The ToTitleCase
        /// </summary>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        internal static string ToTitleCase(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

        #endregion 方法
    }
}