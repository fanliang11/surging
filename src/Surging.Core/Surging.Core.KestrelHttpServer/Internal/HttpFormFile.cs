using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.KestrelHttpServer.Internal
{
    [MessagePackObject]
    public class HttpFormFile
    {

        [SerializationConstructor]
        public HttpFormFile(long length, string name, string fileName,byte[] file)
        {
            Length = length;
            Name = name;
            FileName = fileName;
            File = file;
        }
        [Key(0)]
        public long Length { get; }

        [Key(1)]
        public string Name { get; }

        [Key(2)]
        public string FileName { get; }

        [Key(3)]
        public byte[] File { get; } 
    }
}
