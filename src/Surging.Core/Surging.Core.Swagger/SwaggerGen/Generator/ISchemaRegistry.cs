using Surging.Core.Swagger;
using System;
using System.Collections.Generic;
namespace Surging.Core.SwaggerGen
{
    public interface ISchemaRegistry
    {
        Schema GetOrRegister(Type type);

        Schema GetOrRegister(string parmName, Type type);

        IDictionary<string, Schema> Definitions { get; }
    }
}
