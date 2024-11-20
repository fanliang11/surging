using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.KestrelHttpServer.Abstractions
{
   public  class PhysicalFileResult : FileResult
    {
        private string _fileName;
         
        public PhysicalFileResult(string fileName, string contentType)
            : this(fileName, MediaTypeHeaderValue.Parse(contentType))
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }
        }
         
        public PhysicalFileResult(string fileName, MediaTypeHeaderValue contentType)
            : base(contentType?.ToString())
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            FileName = fileName;
        }
         
        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _fileName = value;
            }
        }
         
        public override Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var executor = context.HttpContext.RequestServices.GetRequiredService<PhysicalFileResultExecutor>();
            return executor.ExecuteAsync(context, this);
        }
    }
}
