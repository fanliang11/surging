using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Surging.Core.Swagger_V5.SwaggerGen
{
    public class SwaggerApplicationConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            application.ApiExplorer.IsVisible = true;
        }
    }
}