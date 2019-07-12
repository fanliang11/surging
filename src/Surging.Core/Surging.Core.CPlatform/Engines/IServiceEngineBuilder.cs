using Autofac;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Engines
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IServiceEngineBuilder" />
    /// </summary>
    public interface IServiceEngineBuilder
    {
        #region 方法

        /// <summary>
        /// The Build
        /// </summary>
        /// <param name="serviceContainer">The serviceContainer<see cref="ContainerBuilder"/></param>
        void Build(ContainerBuilder serviceContainer);

        /// <summary>
        /// The ReBuild
        /// </summary>
        /// <param name="serviceContainer">The serviceContainer<see cref="ContainerBuilder"/></param>
        /// <returns>The <see cref="ValueTuple{List{Type},IEnumerable{string}}?"/></returns>
        ValueTuple<List<Type>, IEnumerable<string>>? ReBuild(ContainerBuilder serviceContainer);

        #endregion 方法
    }

    #endregion 接口
}