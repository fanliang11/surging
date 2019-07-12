using Newtonsoft.Json;
using System.Collections.Generic;

namespace Surging.Core.Swagger
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IParameter" />
    /// </summary>
    public interface IParameter
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Description
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Gets the Extensions
        /// </summary>
        Dictionary<string, object> Extensions { get; }

        /// <summary>
        /// Gets or sets the In
        /// </summary>
        string In { get; set; }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Required
        /// </summary>
        bool Required { get; set; }

        #endregion 属性
    }

    #endregion 接口

    /// <summary>
    /// Defines the <see cref="BodyParameter" />
    /// </summary>
    public class BodyParameter : IParameter
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="BodyParameter"/> class.
        /// </summary>
        public BodyParameter()
        {
            In = "body";
            Extensions = new Dictionary<string, object>();
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets the Extensions
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> Extensions { get; private set; }

        /// <summary>
        /// Gets or sets the In
        /// </summary>
        public string In { get; set; }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Required
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Gets or sets the Schema
        /// </summary>
        public Schema Schema { get; set; }

        #endregion 属性
    }

    /// <summary>
    /// Defines the <see cref="Contact" />
    /// </summary>
    public class Contact
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Url
        /// </summary>
        public string Url { get; set; }

        #endregion 属性
    }

    /// <summary>
    /// Defines the <see cref="DocumentConfiguration" />
    /// </summary>
    public class DocumentConfiguration
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Info
        /// </summary>
        public Info Info { get; set; } = null;

        /// <summary>
        /// Gets or sets the Options
        /// </summary>
        public DocumentOptions Options { get; set; } = null;

        #endregion 属性
    }

    /// <summary>
    /// Defines the <see cref="DocumentOptions" />
    /// </summary>
    public class DocumentOptions
    {
        #region 属性

        /// <summary>
        /// Gets or sets a value indicating whether IgnoreFullyQualified
        /// </summary>
        public bool IgnoreFullyQualified { get; set; }

        /// <summary>
        /// Gets or sets the IngressName
        /// </summary>
        public string IngressName { get; set; }

        #endregion 属性
    }

    /// <summary>
    /// Defines the <see cref="ExternalDocs" />
    /// </summary>
    public class ExternalDocs
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Url
        /// </summary>
        public string Url { get; set; }

        #endregion 属性
    }

    /// <summary>
    /// Defines the <see cref="Header" />
    /// </summary>
    public class Header : PartialSchema
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Description
        /// </summary>
        public string Description { get; set; }

        #endregion 属性
    }

    /// <summary>
    /// Defines the <see cref="Info" />
    /// </summary>
    public class Info
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="Info"/> class.
        /// </summary>
        public Info()
        {
            Extensions = new Dictionary<string, object>();
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Contact
        /// </summary>
        public Contact Contact { get; set; }

        /// <summary>
        /// Gets or sets the Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets the Extensions
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> Extensions { get; private set; }

        /// <summary>
        /// Gets or sets the License
        /// </summary>
        public License License { get; set; }

        /// <summary>
        /// Gets or sets the TermsOfService
        /// </summary>
        public string TermsOfService { get; set; }

        /// <summary>
        /// Gets or sets the Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the Version
        /// </summary>
        public string Version { get; set; }

        #endregion 属性
    }

    /// <summary>
    /// Defines the <see cref="License" />
    /// </summary>
    public class License
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Url
        /// </summary>
        public string Url { get; set; }

        #endregion 属性
    }

    /// <summary>
    /// Defines the <see cref="NonBodyParameter" />
    /// </summary>
    public class NonBodyParameter : PartialSchema, IParameter
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the In
        /// </summary>
        public string In { get; set; }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Required
        /// </summary>
        public bool Required { get; set; }

        #endregion 属性
    }

    /// <summary>
    /// Defines the <see cref="Operation" />
    /// </summary>
    public class Operation
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="Operation"/> class.
        /// </summary>
        public Operation()
        {
            Extensions = new Dictionary<string, object>();
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Consumes
        /// </summary>
        public IList<string> Consumes { get; set; }

        /// <summary>
        /// Gets or sets the Deprecated
        /// </summary>
        public bool? Deprecated { get; set; }

        /// <summary>
        /// Gets or sets the Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets the Extensions
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> Extensions { get; private set; }

        /// <summary>
        /// Gets or sets the ExternalDocs
        /// </summary>
        public ExternalDocs ExternalDocs { get; set; }

        /// <summary>
        /// Gets or sets the OperationId
        /// </summary>
        public string OperationId { get; set; }

        /// <summary>
        /// Gets or sets the Parameters
        /// </summary>
        public IList<IParameter> Parameters { get; set; }

        /// <summary>
        /// Gets or sets the Produces
        /// </summary>
        public IList<string> Produces { get; set; }

        /// <summary>
        /// Gets or sets the Responses
        /// </summary>
        public IDictionary<string, Response> Responses { get; set; }

        /// <summary>
        /// Gets or sets the Schemes
        /// </summary>
        public IList<string> Schemes { get; set; }

        /// <summary>
        /// Gets or sets the Security
        /// </summary>
        public IList<IDictionary<string, IEnumerable<string>>> Security { get; set; }

        /// <summary>
        /// Gets or sets the Summary
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the Tags
        /// </summary>
        public IList<string> Tags { get; set; }

        #endregion 属性
    }

    /// <summary>
    /// Defines the <see cref="PartialSchema" />
    /// </summary>
    public class PartialSchema
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="PartialSchema"/> class.
        /// </summary>
        public PartialSchema()
        {
            Extensions = new Dictionary<string, object>();
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the CollectionFormat
        /// </summary>
        public string CollectionFormat { get; set; }

        /// <summary>
        /// Gets or sets the Default
        /// </summary>
        public object Default { get; set; }

        /// <summary>
        /// Gets or sets the Enum
        /// </summary>
        public IList<object> Enum { get; set; }

        /// <summary>
        /// Gets or sets the ExclusiveMaximum
        /// </summary>
        public bool? ExclusiveMaximum { get; set; }

        /// <summary>
        /// Gets or sets the ExclusiveMinimum
        /// </summary>
        public bool? ExclusiveMinimum { get; set; }

        /// <summary>
        /// Gets the Extensions
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> Extensions { get; private set; }

        /// <summary>
        /// Gets or sets the Format
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets the Items
        /// </summary>
        public PartialSchema Items { get; set; }

        /// <summary>
        /// Gets or sets the Maximum
        /// </summary>
        public double? Maximum { get; set; }

        /// <summary>
        /// Gets or sets the MaxItems
        /// </summary>
        public int? MaxItems { get; set; }

        /// <summary>
        /// Gets or sets the MaxLength
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the Minimum
        /// </summary>
        public double? Minimum { get; set; }

        /// <summary>
        /// Gets or sets the MinItems
        /// </summary>
        public int? MinItems { get; set; }

        /// <summary>
        /// Gets or sets the MinLength
        /// </summary>
        public int? MinLength { get; set; }

        /// <summary>
        /// Gets or sets the MultipleOf
        /// </summary>
        public int? MultipleOf { get; set; }

        /// <summary>
        /// Gets or sets the Pattern
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the UniqueItems
        /// </summary>
        public bool? UniqueItems { get; set; }

        #endregion 属性
    }

    /// <summary>
    /// Defines the <see cref="PathItem" />
    /// </summary>
    public class PathItem
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="PathItem"/> class.
        /// </summary>
        public PathItem()
        {
            Extensions = new Dictionary<string, object>();
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Delete
        /// </summary>
        public Operation Delete { get; set; }

        /// <summary>
        /// Gets the Extensions
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> Extensions { get; private set; }

        /// <summary>
        /// Gets or sets the Get
        /// </summary>
        public Operation Get { get; set; }

        /// <summary>
        /// Gets or sets the Head
        /// </summary>
        public Operation Head { get; set; }

        /// <summary>
        /// Gets or sets the Options
        /// </summary>
        public Operation Options { get; set; }

        /// <summary>
        /// Gets or sets the Parameters
        /// </summary>
        public IList<IParameter> Parameters { get; set; }

        /// <summary>
        /// Gets or sets the Patch
        /// </summary>
        public Operation Patch { get; set; }

        /// <summary>
        /// Gets or sets the Post
        /// </summary>
        public Operation Post { get; set; }

        /// <summary>
        /// Gets or sets the Put
        /// </summary>
        public Operation Put { get; set; }

        /// <summary>
        /// Gets or sets the Ref
        /// </summary>
        [JsonProperty("$ref")]
        public string Ref { get; set; }

        #endregion 属性
    }

    /// <summary>
    /// Defines the <see cref="Response" />
    /// </summary>
    public class Response
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="Response"/> class.
        /// </summary>
        public Response()
        {
            Extensions = new Dictionary<string, object>();
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Examples
        /// </summary>
        public object Examples { get; set; }

        /// <summary>
        /// Gets the Extensions
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> Extensions { get; private set; }

        /// <summary>
        /// Gets or sets the Headers
        /// </summary>
        public IDictionary<string, Header> Headers { get; set; }

        /// <summary>
        /// Gets or sets the Schema
        /// </summary>
        public Schema Schema { get; set; }

        #endregion 属性
    }

    /// <summary>
    /// Defines the <see cref="Schema" />
    /// </summary>
    public class Schema
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="Schema"/> class.
        /// </summary>
        public Schema()
        {
            Extensions = new Dictionary<string, object>();
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the AdditionalProperties
        /// </summary>
        public Schema AdditionalProperties { get; set; }

        /// <summary>
        /// Gets or sets the AllOf
        /// </summary>
        public IList<Schema> AllOf { get; set; }

        /// <summary>
        /// Gets or sets the Default
        /// </summary>
        public object Default { get; set; }

        /// <summary>
        /// Gets or sets the Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Discriminator
        /// </summary>
        public string Discriminator { get; set; }

        /// <summary>
        /// Gets or sets the Enum
        /// </summary>
        public IList<object> Enum { get; set; }

        /// <summary>
        /// Gets or sets the Example
        /// </summary>
        public object Example { get; set; }

        /// <summary>
        /// Gets or sets the ExclusiveMaximum
        /// </summary>
        public bool? ExclusiveMaximum { get; set; }

        /// <summary>
        /// Gets or sets the ExclusiveMinimum
        /// </summary>
        public bool? ExclusiveMinimum { get; set; }

        /// <summary>
        /// Gets the Extensions
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> Extensions { get; private set; }

        /// <summary>
        /// Gets or sets the ExternalDocs
        /// </summary>
        public ExternalDocs ExternalDocs { get; set; }

        /// <summary>
        /// Gets or sets the Format
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets the Items
        /// </summary>
        public Schema Items { get; set; }

        /// <summary>
        /// Gets or sets the Maximum
        /// </summary>
        public double? Maximum { get; set; }

        /// <summary>
        /// Gets or sets the MaxItems
        /// </summary>
        public int? MaxItems { get; set; }

        /// <summary>
        /// Gets or sets the MaxLength
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the MaxProperties
        /// </summary>
        public int? MaxProperties { get; set; }

        /// <summary>
        /// Gets or sets the Minimum
        /// </summary>
        public double? Minimum { get; set; }

        /// <summary>
        /// Gets or sets the MinItems
        /// </summary>
        public int? MinItems { get; set; }

        /// <summary>
        /// Gets or sets the MinLength
        /// </summary>
        public int? MinLength { get; set; }

        /// <summary>
        /// Gets or sets the MinProperties
        /// </summary>
        public int? MinProperties { get; set; }

        /// <summary>
        /// Gets or sets the MultipleOf
        /// </summary>
        public int? MultipleOf { get; set; }

        /// <summary>
        /// Gets or sets the Pattern
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// Gets or sets the Properties
        /// </summary>
        public IDictionary<string, Schema> Properties { get; set; }

        /// <summary>
        /// Gets or sets the ReadOnly
        /// </summary>
        public bool? ReadOnly { get; set; }

        /// <summary>
        /// Gets or sets the Ref
        /// </summary>
        [JsonProperty("$ref")]
        public string Ref { get; set; }

        /// <summary>
        /// Gets or sets the Required
        /// </summary>
        public IList<string> Required { get; set; }

        /// <summary>
        /// Gets or sets the Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the UniqueItems
        /// </summary>
        public bool? UniqueItems { get; set; }

        /// <summary>
        /// Gets or sets the Xml
        /// </summary>
        public Xml Xml { get; set; }

        #endregion 属性
    }

    /// <summary>
    /// Defines the <see cref="SwaggerDocument" />
    /// </summary>
    public class SwaggerDocument
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerDocument"/> class.
        /// </summary>
        public SwaggerDocument()
        {
            Extensions = new Dictionary<string, object>();
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the BasePath
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// Gets or sets the Consumes
        /// </summary>
        public IList<string> Consumes { get; set; }

        /// <summary>
        /// Gets or sets the Definitions
        /// </summary>
        public IDictionary<string, Schema> Definitions { get; set; }

        /// <summary>
        /// Gets the Extensions
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> Extensions { get; private set; }

        /// <summary>
        /// Gets or sets the ExternalDocs
        /// </summary>
        public ExternalDocs ExternalDocs { get; set; }

        /// <summary>
        /// Gets or sets the Host
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the Info
        /// </summary>
        public Info Info { get; set; }

        /// <summary>
        /// Gets or sets the Parameters
        /// </summary>
        public IDictionary<string, IParameter> Parameters { get; set; }

        /// <summary>
        /// Gets or sets the Paths
        /// </summary>
        public IDictionary<string, PathItem> Paths { get; set; }

        /// <summary>
        /// Gets or sets the Produces
        /// </summary>
        public IList<string> Produces { get; set; }

        /// <summary>
        /// Gets or sets the Responses
        /// </summary>
        public IDictionary<string, Response> Responses { get; set; }

        /// <summary>
        /// Gets or sets the Schemes
        /// </summary>
        public IList<string> Schemes { get; set; }

        /// <summary>
        /// Gets or sets the Security
        /// </summary>
        public IList<IDictionary<string, IEnumerable<string>>> Security { get; set; }

        /// <summary>
        /// Gets or sets the SecurityDefinitions
        /// </summary>
        public IDictionary<string, SecurityScheme> SecurityDefinitions { get; set; }

        /// <summary>
        /// Gets the Swagger
        /// </summary>
        public string Swagger
        {
            get { return "2.0"; }
        }

        /// <summary>
        /// Gets or sets the Tags
        /// </summary>
        public IList<Tag> Tags { get; set; }

        #endregion 属性
    }

    /// <summary>
    /// Defines the <see cref="Tag" />
    /// </summary>
    public class Tag
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="Tag"/> class.
        /// </summary>
        public Tag()
        {
            Extensions = new Dictionary<string, object>();
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets the Extensions
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> Extensions { get; private set; }

        /// <summary>
        /// Gets or sets the ExternalDocs
        /// </summary>
        public ExternalDocs ExternalDocs { get; set; }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get; set; }

        #endregion 属性
    }

    /// <summary>
    /// Defines the <see cref="Xml" />
    /// </summary>
    public class Xml
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Attribute
        /// </summary>
        public bool? Attribute { get; set; }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Namespace
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Gets or sets the Prefix
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Gets or sets the Wrapped
        /// </summary>
        public bool? Wrapped { get; set; }

        #endregion 属性
    }
}