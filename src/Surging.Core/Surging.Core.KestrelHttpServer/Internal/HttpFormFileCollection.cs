using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.KestrelHttpServer.Internal
{
    public class HttpFormFileCollection : List<HttpFormFile>
    {
        public HttpFormFile this[string name] => GetFile(name);

        public HttpFormFile GetFile(string name)
        {
            foreach (var file in this)
            {
                if (string.Equals(name, file.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return file;
                }
            }

            return null;
        }

        public IReadOnlyList<HttpFormFile> GetFiles(string name)
        {
            var files = new List<HttpFormFile>();

            foreach (var file in this)
            {
                if (string.Equals(name, file.Name, StringComparison.OrdinalIgnoreCase))
                {
                    files.Add(file);
                }
            }

            return files;
        }
    }
}