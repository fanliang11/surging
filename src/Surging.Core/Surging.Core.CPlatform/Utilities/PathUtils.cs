using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Utilities
{
    public class PathUtils
    {
        public static bool Match(string pattern, string path)
        {

            if (pattern.Equals(path))
            {
                return true;
            }

            if (!pattern.Contains("*")
                && !pattern.Contains("#") && !pattern.Contains("+")
                && !pattern.Contains("{"))
            {
                return false;
            }
            pattern = Regex.Escape(pattern);
            pattern = pattern.Replace("#", ".*").Replace("+", ".*").Replace("\\*", ".*").Replace("\\?", ".");
            return Regex.IsMatch(path, pattern);
        }

    }
}
