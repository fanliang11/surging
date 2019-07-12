namespace Surging.Core.Swagger
{
    /// <summary>
    /// Defines the <see cref="ApiKeyScheme" />
    /// </summary>
    public class ApiKeyScheme : SecurityScheme
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiKeyScheme"/> class.
        /// </summary>
        public ApiKeyScheme()
        {
            Type = "apiKey";
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the In
        /// </summary>
        public string In { get; set; }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get; set; }

        #endregion 属性
    }
}