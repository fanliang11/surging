using Surging.Core.Swagger_V5.Swagger.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Swagger_V5
{
   public  class AppConfig
    {
        public static Info SwaggerOptions
        {
            get; internal set;
        }

        public static DocumentConfiguration SwaggerConfig
        {
            get; internal set;
        }
    }
}
