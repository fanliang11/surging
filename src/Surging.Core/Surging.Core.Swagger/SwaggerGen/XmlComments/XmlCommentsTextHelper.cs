using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Surging.Core.SwaggerGen
{
    /// <summary>
    /// Defines the <see cref="XmlCommentsTextHelper" />
    /// </summary>
    public static class XmlCommentsTextHelper
    {
        #region 字段

        /// <summary>
        /// Defines the CodeTagPattern
        /// </summary>
        private static Regex CodeTagPattern = new Regex(@"<c>(?<display>.+?)</c>");

        /// <summary>
        /// Defines the RefTagPattern
        /// </summary>
        private static Regex RefTagPattern = new Regex(@"<(see|paramref) (name|cref)=""([TPF]{1}:)?(?<display>.+?)"" ?/>");

        #endregion 字段

        #region 方法

        /// <summary>
        /// The Humanize
        /// </summary>
        /// <param name="text">The text<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        public static string Humanize(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            return text
                .NormalizeIndentation()
                .HumanizeRefTags()
                .HumanizeCodeTags();
        }

        /// <summary>
        /// The GetCommonLeadingWhitespace
        /// </summary>
        /// <param name="lines">The lines<see cref="string[]"/></param>
        /// <returns>The <see cref="string"/></returns>
        private static string GetCommonLeadingWhitespace(string[] lines)
        {
            if (null == lines)
                throw new ArgumentException("lines");

            if (lines.Length == 0)
                return null;

            string[] nonEmptyLines = lines
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            if (nonEmptyLines.Length < 1)
                return null;

            int padLen = 0;

            // use the first line as a seed, and see what is shared over all nonEmptyLines
            string seed = nonEmptyLines[0];
            for (int i = 0, l = seed.Length; i < l; ++i)
            {
                if (!char.IsWhiteSpace(seed, i))
                    break;

                if (nonEmptyLines.Any(line => line[i] != seed[i]))
                    break;

                ++padLen;
            }

            if (padLen > 0)
                return seed.Substring(0, padLen);

            return null;
        }

        /// <summary>
        /// The HumanizeCodeTags
        /// </summary>
        /// <param name="text">The text<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        private static string HumanizeCodeTags(this string text)
        {
            return CodeTagPattern.Replace(text, (match) => "{" + match.Groups["display"].Value + "}");
        }

        /// <summary>
        /// The HumanizeRefTags
        /// </summary>
        /// <param name="text">The text<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        private static string HumanizeRefTags(this string text)
        {
            return RefTagPattern.Replace(text, (match) => match.Groups["display"].Value);
        }

        /// <summary>
        /// The NormalizeIndentation
        /// </summary>
        /// <param name="text">The text<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        private static string NormalizeIndentation(this string text)
        {
            string[] lines = text.Split('\n');
            string padding = GetCommonLeadingWhitespace(lines);

            int padLen = padding == null ? 0 : padding.Length;

            // remove leading padding from each line
            for (int i = 0, l = lines.Length; i < l; ++i)
            {
                string line = lines[i].TrimEnd('\r'); // remove trailing '\r'

                if (padLen != 0 && line.Length >= padLen && line.Substring(0, padLen) == padding)
                    line = line.Substring(padLen);

                lines[i] = line;
            }

            // remove leading empty lines, but not all leading padding
            // remove all trailing whitespace, regardless
            return string.Join("\r\n", lines.SkipWhile(x => string.IsNullOrWhiteSpace(x))).TrimEnd();
        }

        #endregion 方法
    }
}