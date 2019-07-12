using Surging.Core.Swagger;
using System;
using System.Collections.Generic;

namespace Surging.Core.SwaggerGen
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="ISchemaRegistry" />
    /// </summary>
    public interface ISchemaRegistry
    {
        #region 属性

        /// <summary>
        /// Gets the Definitions
        /// </summary>
        IDictionary<string, Schema> Definitions { get; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The GetOrRegister
        /// </summary>
        /// <param name="parmName">The parmName<see cref="string"/></param>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="Schema"/></returns>
        Schema GetOrRegister(string parmName, Type type);

        /// <summary>
        /// The GetOrRegister
        /// </summary>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="Schema"/></returns>
        Schema GetOrRegister(Type type);

        #endregion 方法
    }

    #endregion 接口
}