using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Tools.Cli.Utilities
{
    public static class HttpContentExtensions
    {
        public static async Task<byte[]> ReadAsByteArrayAsync(this HttpContent httpContent, Encoding dstEncoding)
        {
            var encoding = httpContent.GetEncoding();
            var byteArray = await httpContent.ReadAsByteArrayAsync().ConfigureAwait(false);

            return encoding.Equals(dstEncoding)
                ? byteArray
                : Encoding.Convert(encoding, dstEncoding, byteArray);
        }

        public static Encoding GetEncoding(this HttpContent httpContent)
        {
            var charSet = httpContent.Headers.ContentType?.CharSet;
            if (string.IsNullOrEmpty(charSet) || charSet == Encoding.UTF8.WebName)
            {
                return Encoding.UTF8;
            }
            return Encoding.GetEncoding(charSet);
        }
    }
}
