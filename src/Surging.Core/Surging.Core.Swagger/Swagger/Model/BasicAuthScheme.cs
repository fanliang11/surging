namespace Surging.Core.Swagger
{
    /// <summary>
    /// Defines the <see cref="BasicAuthScheme" />
    /// </summary>
    public class BasicAuthScheme : SecurityScheme
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicAuthScheme"/> class.
        /// </summary>
        public BasicAuthScheme()
        {
            Type = "basic";
        }

        #endregion 构造函数
    }
}