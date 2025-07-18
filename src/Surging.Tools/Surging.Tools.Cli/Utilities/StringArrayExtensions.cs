using System;
using System.Collections.Generic;
using System.Linq;

namespace Surging.Tools.Cli.Utilities
{
    public  static  class StringArrayExtensions
    {
        public static Dictionary<string,string> ToDictionary(this string[] strArray)
        {
            var result = new Dictionary<string, string>();
            foreach (var str in strArray)
            {
                result = str.Split(';')
                .Select(x => x.Split('='))
                .Where(x => x.Length > 1 && !String.IsNullOrEmpty(x[0].Trim())
                && !String.IsNullOrEmpty(x[1].Trim()))
                .ToDictionary(x => x[0].Trim(), x => x[1].Trim()).Union(result).ToDictionary(x=>x.Key.ToLower(),x=>x.Value);
            }
            return result;
        }
    }
}
