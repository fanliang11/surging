using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.KestrelHttpServer
{
    public abstract class FileResult: ActionResult
    {
        private string _fileDownloadName;
        
        protected FileResult(string contentType)
        {
            if (contentType == null)
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            ContentType = contentType;
        }
        
        public string ContentType { get; }

       
        public string FileDownloadName
        {
            get { return _fileDownloadName ?? string.Empty; }
            set { _fileDownloadName = value; }
        }
    }
}
