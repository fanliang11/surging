using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Surging.Core.CPlatform.Utilities
{
    public static class StringExtensions
    {
        public static bool IsIP(this string input) => input.IsMatch(@"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\:\d{2,5}\b");

        public static bool IsMatch(this string str, string op)
        {
            if (str.Equals(String.Empty) || str == null) return false;
            var re = new Regex(op, RegexOptions.IgnoreCase);
            return re.IsMatch(str);
        }

        public static string GetMd5Hash(this string input)
        {
            using (MD5 md5Hash = MD5.Create())
            { 
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                 
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                 
                return sBuilder.ToString();
            }
        }
    }
}