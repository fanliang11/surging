using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GateWay.WebApi
{
    /// <summary>  
    /// 隐藏接口，不生成到swagger文档展示  
    /// </summary>  
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]

    public partial class HiddenApiAttribute : Attribute { }
    public class HiddenApiFilter : IDocumentFilter
    {
        /// <summary>
        /// 重写Apply方法，移除隐藏接口的生成  
        /// </summary>
        /// <param name="swaggerDoc"></param>
        /// <param name="context"></param>
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (ApiDescriptionGroup apiDescriptionGroup in context.ApiDescriptionsGroups.Items)
            {
                foreach (var apiDescription in apiDescriptionGroup.Items)
                {
               if(apiDescription.ControllerAttributes().First(c => c is HiddenApiAttribute)!=null) //if (Enumerable.OfType<HiddenApiAttribute>(apiDescription.ControllerAttributes().Where(c=>c is HiddenApiAttribute).ToList())
                {
                    string key = "/" + apiDescription.RelativePath;
                    if (key.Contains("?"))
                    {
                        int idx = key.IndexOf("?", StringComparison.Ordinal);
                        key = key.Substring(0, idx);
                    }
                        swaggerDoc.Paths.Remove(key);
                }
                }

            }
        }
    }
}
