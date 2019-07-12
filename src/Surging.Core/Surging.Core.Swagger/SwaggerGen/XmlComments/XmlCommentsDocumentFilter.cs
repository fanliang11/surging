using Microsoft.AspNetCore.Mvc.Controllers;
using Surging.Core.Swagger;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;

namespace Surging.Core.SwaggerGen
{
    /// <summary>
    /// Defines the <see cref="XmlCommentsDocumentFilter" />
    /// </summary>
    public class XmlCommentsDocumentFilter : IDocumentFilter
    {
        #region 常量

        /// <summary>
        /// Defines the MemberXPath
        /// </summary>
        private const string MemberXPath = "/doc/members/member[@name='{0}']";

        /// <summary>
        /// Defines the SummaryTag
        /// </summary>
        private const string SummaryTag = "summary";

        #endregion 常量

        #region 字段

        /// <summary>
        /// Defines the _xmlNavigator
        /// </summary>
        private readonly XPathNavigator _xmlNavigator;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlCommentsDocumentFilter"/> class.
        /// </summary>
        /// <param name="xmlDoc">The xmlDoc<see cref="XPathDocument"/></param>
        public XmlCommentsDocumentFilter(XPathDocument xmlDoc)
        {
            _xmlNavigator = xmlDoc.CreateNavigator();
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Apply
        /// </summary>
        /// <param name="swaggerDoc">The swaggerDoc<see cref="SwaggerDocument"/></param>
        /// <param name="context">The context<see cref="DocumentFilterContext"/></param>
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            // Collect (unique) controller names and types in a dictionary
            var controllerNamesAndTypes = context.ApiDescriptions
                .Select(apiDesc => apiDesc.ActionDescriptor as ControllerActionDescriptor)
                .SkipWhile(actionDesc => actionDesc == null)
                .GroupBy(actionDesc => actionDesc.ControllerName)
                .ToDictionary(grp => grp.Key, grp => grp.Last().ControllerTypeInfo.AsType());

            foreach (var nameAndType in controllerNamesAndTypes)
            {
                var memberName = XmlCommentsMemberNameHelper.GetMemberNameForType(nameAndType.Value);
                var typeNode = _xmlNavigator.SelectSingleNode(string.Format(MemberXPath, memberName));

                if (typeNode != null)
                {
                    var summaryNode = typeNode.SelectSingleNode(SummaryTag);
                    if (summaryNode != null)
                    {
                        if (swaggerDoc.Tags == null)
                            swaggerDoc.Tags = new List<Tag>();

                        swaggerDoc.Tags.Add(new Tag
                        {
                            Name = nameAndType.Key,
                            Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml)
                        });
                    }
                }
            }
        }

        #endregion 方法
    }
}