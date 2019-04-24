using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Text;

namespace Surging.Core.KestrelHttpServer.Internal
{
    public class FileStreamResult : FileResult
    {
        private Stream _fileStream;
         
        public FileStreamResult(Stream fileStream, string contentType)
            : this(fileStream, MediaTypeHeaderValue.Parse(contentType))
        {
        }
         
        public FileStreamResult(Stream fileStream, MediaTypeHeaderValue contentType)
            : base(contentType?.ToString())
        {
            if (fileStream == null)
            {
                throw new ArgumentNullException(nameof(fileStream));
            }

            FileStream = fileStream;
        }
        
        public Stream FileStream
        {
            get => _fileStream;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _fileStream = value;
            }
        }

    }
}
