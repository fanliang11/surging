using System;

namespace Surging.Core.Swagger_V5.SwaggerGen
{
    public class SwaggerGeneratorException : Exception
    {
        public SwaggerGeneratorException(string message) : base(message)
        { }

        public SwaggerGeneratorException(string message, Exception innerException) : base(message, innerException)
        { }
    }
}