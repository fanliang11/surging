using Newtonsoft.Json.Serialization;
using Surging.Core.Swagger;
using System.Reflection;
using System.Xml.XPath;

namespace Surging.Core.SwaggerGen
{
    /// <summary>
    /// Defines the <see cref="XmlCommentsSchemaFilter" />
    /// </summary>
    public class XmlCommentsSchemaFilter : ISchemaFilter
    {
        #region 常量

        /// <summary>
        /// Defines the ExampleXPath
        /// </summary>
        private const string ExampleXPath = "example";

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
        /// Initializes a new instance of the <see cref="XmlCommentsSchemaFilter"/> class.
        /// </summary>
        /// <param name="xmlDoc">The xmlDoc<see cref="XPathDocument"/></param>
        public XmlCommentsSchemaFilter(XPathDocument xmlDoc)
        {
            _xmlNavigator = xmlDoc.CreateNavigator();
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Apply
        /// </summary>
        /// <param name="schema">The schema<see cref="Schema"/></param>
        /// <param name="context">The context<see cref="SchemaFilterContext"/></param>
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            var jsonObjectContract = context.JsonContract as JsonObjectContract;
            if (jsonObjectContract == null) return;

            var memberName = XmlCommentsMemberNameHelper.GetMemberNameForType(context.SystemType);
            var typeNode = _xmlNavigator.SelectSingleNode(string.Format(MemberXPath, memberName));

            if (typeNode != null)
            {
                var summaryNode = typeNode.SelectSingleNode(SummaryTag);
                if (summaryNode != null)
                    schema.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);
            }

            if (schema.Properties == null) return;
            foreach (var entry in schema.Properties)
            {
                var jsonProperty = jsonObjectContract.Properties[entry.Key];
                if (jsonProperty == null) continue;

                if (jsonProperty.TryGetMemberInfo(out MemberInfo memberInfo))
                {
                    ApplyPropertyComments(entry.Value, memberInfo);
                }
            }
        }

        /// <summary>
        /// The ApplyPropertyComments
        /// </summary>
        /// <param name="propertySchema">The propertySchema<see cref="Schema"/></param>
        /// <param name="memberInfo">The memberInfo<see cref="MemberInfo"/></param>
        private void ApplyPropertyComments(Schema propertySchema, MemberInfo memberInfo)
        {
            var memberName = XmlCommentsMemberNameHelper.GetMemberNameForMember(memberInfo);
            var memberNode = _xmlNavigator.SelectSingleNode(string.Format(MemberXPath, memberName));
            if (memberNode == null) return;

            var summaryNode = memberNode.SelectSingleNode(SummaryTag);
            if (summaryNode != null)
            {
                propertySchema.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);
            }

            var exampleNode = memberNode.SelectSingleNode(ExampleXPath);
            if (exampleNode != null)
                propertySchema.Example = XmlCommentsTextHelper.Humanize(exampleNode.InnerXml);
        }

        #endregion 方法
    }
}