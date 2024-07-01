using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.KestrelHttpServer.Internal
{
   public class HttpFormFile
    {
        public HttpFormFile(long length, string name, string fileName,byte[] file)
        {
            Length = length;
            Name = name;
            FileName = fileName;
            File = file;
        }
        public long Length { get; }
        
        public string Name { get; }
        
        public string FileName { get; }

        public byte[] File { get; } 
    }
}
