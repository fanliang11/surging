using System.IO;
using System.Text.Encodings.Web;

namespace DotNetty.Codecs.Http.Utilities
{
    partial class HttpUtility
    {
        private static readonly JavaScriptEncoder s_scriptEncoder = JavaScriptEncoder.Default;

        public static string JavaScriptStringEncode(string value)
        {
            if (string.IsNullOrEmpty(value)) { return value; }

            return s_scriptEncoder.Encode(value);
        }

        public static void JavaScriptStringEncode(TextWriter output, string value)
        {
            s_scriptEncoder.Encode(output, value, 0, value.Length);
        }

        public static void JavaScriptStringEncode(TextWriter output, string value, int startIndex, int characterCount)
        {
            if (string.IsNullOrEmpty(value)) { return; }

            s_scriptEncoder.Encode(output, value, startIndex, characterCount);
        }

        public static void JavaScriptStringEncode(TextWriter output, char[] value, int startIndex, int characterCount)
        {
            if (value is null || 0u >= (uint)value.Length) { return; }

            s_scriptEncoder.Encode(output, value, startIndex, characterCount);
        }
    }
}
