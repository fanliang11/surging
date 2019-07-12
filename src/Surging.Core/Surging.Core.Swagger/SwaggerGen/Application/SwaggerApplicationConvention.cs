using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Surging.Core.SwaggerGen
{
    /// <summary>
    /// Defines the <see cref="SwaggerApplicationConvention" />
    /// </summary>
    public class SwaggerApplicationConvention : IApplicationModelConvention
    {
        #region 方法

        /// <summary>
        /// The Apply
        /// </summary>
        /// <param name="application">The application<see cref="ApplicationModel"/></param>
        public void Apply(ApplicationModel application)
        {
            application.ApiExplorer.IsVisible = true;
        }

        #endregion 方法
    }
}