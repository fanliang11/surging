using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;

namespace Surging.Core.SwaggerGen
{
    /// <summary>
    /// Defines the <see cref="XmlCommentsOperationFilter" />
    /// </summary>
    public class XmlCommentsOperationFilter : IOperationFilter
    {
        #region 常量

        /// <summary>
        /// Defines the MemberXPath
        /// </summary>
        private const string MemberXPath = "/doc/members/member[@name='{0}']";

        /// <summary>
        /// Defines the ParamXPath
        /// </summary>
        private const string ParamXPath = "param[@name='{0}']";

        /// <summary>
        /// Defines the RemarksXPath
        /// </summary>
        private const string RemarksXPath = "remarks";

        /// <summary>
        /// Defines the ResponsesXPath
        /// </summary>
        private const string ResponsesXPath = "response";

        /// <summary>
        /// Defines the SummaryXPath
        /// </summary>
        private const string SummaryXPath = "summary";

        #endregion 常量

        #region 字段

        /// <summary>
        /// Defines the _xmlNavigator
        /// </summary>
        private readonly XPathNavigator _xmlNavigator;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlCommentsOperationFilter"/> class.
        /// </summary>
        /// <param name="xmlDoc">The xmlDoc<see cref="XPathDocument"/></param>
        public XmlCommentsOperationFilter(XPathDocument xmlDoc)
        {
            _xmlNavigator = xmlDoc.CreateNavigator();
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Apply
        /// </summary>
        /// <param name="operation">The operation<see cref="Operation"/></param>
        /// <param name="context">The context<see cref="OperationFilterContext"/></param>
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (context.MethodInfo == null) return;

            // If method is from a constructed generic type, look for comments from the generic type method
            var targetMethod = context.MethodInfo.DeclaringType.IsConstructedGenericType
                ? GetGenericTypeMethodOrNullFor(context.MethodInfo)
                : context.MethodInfo;

            if (targetMethod == null) return;

            var memberName = XmlCommentsMemberNameHelper.GetMemberNameForMethod(targetMethod);
            var methodNode = _xmlNavigator.SelectSingleNode(string.Format(MemberXPath, memberName));

            if (methodNode != null)
            {
                ApplyMethodXmlToOperation(operation, methodNode);
                ApplyParamsXmlToActionParameters(operation.Parameters, methodNode, context.ServiceEntry);
                ApplyResponsesXmlToResponses(operation.Responses, methodNode.Select(ResponsesXPath));
            }

            if (context.ApiDescription != null)
                // Special handling for parameters that are bound to model properties
                ApplyPropertiesXmlToPropertyParameters(operation.Parameters, context.ApiDescription);
            else
                ApplyPropertiesXmlToPropertyParameters(operation.Parameters, context.ServiceEntry);
        }

        /// <summary>
        /// The ApplyMethodXmlToOperation
        /// </summary>
        /// <param name="operation">The operation<see cref="Operation"/></param>
        /// <param name="methodNode">The methodNode<see cref="XPathNavigator"/></param>
        private void ApplyMethodXmlToOperation(Operation operation, XPathNavigator methodNode)
        {
            var summaryNode = methodNode.SelectSingleNode(SummaryXPath);
            if (summaryNode != null)
                operation.Summary = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);

            var remarksNode = methodNode.SelectSingleNode(RemarksXPath);
            if (remarksNode != null)
                operation.Description = XmlCommentsTextHelper.Humanize(remarksNode.InnerXml);
        }

        /// <summary>
        /// The ApplyParamsXmlToActionParameters
        /// </summary>
        /// <param name="parameters">The parameters<see cref="IList{IParameter}"/></param>
        /// <param name="methodNode">The methodNode<see cref="XPathNavigator"/></param>
        /// <param name="apiDescription">The apiDescription<see cref="ApiDescription"/></param>
        private void ApplyParamsXmlToActionParameters(
            IList<IParameter> parameters,
            XPathNavigator methodNode,
            ApiDescription apiDescription)
        {
            if (parameters == null) return;

            foreach (var parameter in parameters)
            {
                // Check for a corresponding action parameter?
                var actionParameter = apiDescription.ActionDescriptor.Parameters
                    .FirstOrDefault(p => parameter.Name.Equals(
                        (p.BindingInfo?.BinderModelName ?? p.Name), StringComparison.OrdinalIgnoreCase));
                if (actionParameter == null) continue;

                var paramNode = methodNode.SelectSingleNode(string.Format(ParamXPath, actionParameter.Name));
                if (paramNode != null)
                    parameter.Description = XmlCommentsTextHelper.Humanize(paramNode.InnerXml);
            }
        }

        /// <summary>
        /// The ApplyParamsXmlToActionParameters
        /// </summary>
        /// <param name="parameters">The parameters<see cref="IList{IParameter}"/></param>
        /// <param name="methodNode">The methodNode<see cref="XPathNavigator"/></param>
        /// <param name="serviceEntry">The serviceEntry<see cref="ServiceEntry"/></param>
        private void ApplyParamsXmlToActionParameters(
         IList<IParameter> parameters,
         XPathNavigator methodNode,
         ServiceEntry serviceEntry)
        {
            if (parameters == null) return;

            foreach (var parameter in parameters)
            {
                // Check for a corresponding action parameter?
                var methodInfo = serviceEntry.Type.GetTypeInfo().DeclaredMethods.Where(p => p.Name == serviceEntry.MethodName).FirstOrDefault();
                var actionParameter = methodInfo.GetParameters()
                 .FirstOrDefault(p => parameter.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase));
                if (actionParameter == null) continue;

                var paramNode = methodNode.SelectSingleNode(string.Format(ParamXPath, actionParameter.Name));
                if (paramNode != null)
                    parameter.Description = XmlCommentsTextHelper.Humanize(paramNode.InnerXml);
            }
        }

        /// <summary>
        /// The ApplyPropertiesXmlToPropertyParameters
        /// </summary>
        /// <param name="parameters">The parameters<see cref="IList{IParameter}"/></param>
        /// <param name="apiDescription">The apiDescription<see cref="ApiDescription"/></param>
        private void ApplyPropertiesXmlToPropertyParameters(
            IList<IParameter> parameters,
            ApiDescription apiDescription)
        {
            if (parameters == null) return;

            foreach (var parameter in parameters)
            {
                // Check for a corresponding  API parameter (from ApiExplorer) that's property-bound?
                var propertyParam = apiDescription.ParameterDescriptions
                    .Where(p => p.ModelMetadata?.ContainerType != null && p.ModelMetadata?.PropertyName != null)
                    .FirstOrDefault(p => parameter.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase));
                if (propertyParam == null) continue;

                var metadata = propertyParam.ModelMetadata;
                var memberInfo = metadata.ContainerType.GetMember(metadata.PropertyName).FirstOrDefault();
                if (memberInfo == null) continue;

                var memberName = XmlCommentsMemberNameHelper.GetMemberNameForMember(memberInfo);
                var memberNode = _xmlNavigator.SelectSingleNode(string.Format(MemberXPath, memberName));
                if (memberNode == null) continue;

                var summaryNode = memberNode.SelectSingleNode(SummaryXPath);
                if (summaryNode != null)
                    parameter.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);
            }
        }

        /// <summary>
        /// The ApplyPropertiesXmlToPropertyParameters
        /// </summary>
        /// <param name="parameters">The parameters<see cref="IList{IParameter}"/></param>
        /// <param name="serviceEntry">The serviceEntry<see cref="ServiceEntry"/></param>
        private void ApplyPropertiesXmlToPropertyParameters(
    IList<IParameter> parameters,
    ServiceEntry serviceEntry)
        {
            if (parameters == null) return;

            foreach (var parameter in parameters)
            {
                var methodInfo = serviceEntry.Type.GetTypeInfo().DeclaredMethods.Where(p => p.Name == serviceEntry.MethodName).FirstOrDefault();
                var propertyParam = methodInfo.GetParameters()
                 .FirstOrDefault(p => parameter.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase));
                if (propertyParam == null) continue;
                var memberInfo = propertyParam.Member;
                if (memberInfo == null) continue;

                var memberName = XmlCommentsMemberNameHelper.GetMemberNameForMember(memberInfo);
                var memberNode = _xmlNavigator.SelectSingleNode(string.Format(MemberXPath, memberName));
                if (memberNode == null) continue;

                var summaryNode = memberNode.SelectSingleNode(SummaryXPath);
                if (summaryNode != null)
                    parameter.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);
            }
        }

        /// <summary>
        /// The ApplyResponsesXmlToResponses
        /// </summary>
        /// <param name="responses">The responses<see cref="IDictionary{string, Response}"/></param>
        /// <param name="responseNodes">The responseNodes<see cref="XPathNodeIterator"/></param>
        private void ApplyResponsesXmlToResponses(IDictionary<string, Response> responses, XPathNodeIterator responseNodes)
        {
            while (responseNodes.MoveNext())
            {
                var code = responseNodes.Current.GetAttribute("code", "");
                var response = responses.ContainsKey(code)
                    ? responses[code]
                    : responses[code] = new Response();

                response.Description = XmlCommentsTextHelper.Humanize(responseNodes.Current.InnerXml);
            }
        }

        /// <summary>
        /// The GetGenericTypeMethodOrNullFor
        /// </summary>
        /// <param name="constructedTypeMethod">The constructedTypeMethod<see cref="MethodInfo"/></param>
        /// <returns>The <see cref="MethodInfo"/></returns>
        private MethodInfo GetGenericTypeMethodOrNullFor(MethodInfo constructedTypeMethod)
        {
            var constructedType = constructedTypeMethod.DeclaringType;
            var genericTypeDefinition = constructedType.GetGenericTypeDefinition();

            // Retrieve list of candidate methods that match name and parameter count
            var candidateMethods = genericTypeDefinition.GetMethods()
                .Where(m =>
                {
                    return (m.Name == constructedTypeMethod.Name)
                        && (m.GetParameters().Length == constructedTypeMethod.GetParameters().Length);
                });

            // If inconclusive, just return null
            return (candidateMethods.Count() == 1) ? candidateMethods.First() : null;
        }

        #endregion 方法
    }
}