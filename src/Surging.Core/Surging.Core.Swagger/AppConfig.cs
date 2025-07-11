using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Swagger
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
