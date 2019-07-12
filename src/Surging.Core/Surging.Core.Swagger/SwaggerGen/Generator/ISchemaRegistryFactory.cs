namespace Surging.Core.SwaggerGen
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="ISchemaRegistryFactory" />
    /// </summary>
    public interface ISchemaRegistryFactory
    {
        #region 方法

        /// <summary>
        /// The Create
        /// </summary>
        /// <returns>The <see cref="ISchemaRegistry"/></returns>
        ISchemaRegistry Create();

        #endregion 方法
    }

    #endregion 接口
}