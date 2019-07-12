using Autofac;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Module
{
    /// <summary>
    /// Defines the <see cref="ContainerBuilderWrapper" />
    /// </summary>
    public class ContainerBuilderWrapper
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerBuilderWrapper"/> class.
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilder"/></param>
        public ContainerBuilderWrapper(ContainerBuilder builder)
        {
            ContainerBuilder = builder;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the ContainerBuilder
        /// </summary>
        public ContainerBuilder ContainerBuilder { get; private set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Build
        /// </summary>
        /// <returns>The <see cref="IContainer"/></returns>
        public IContainer Build()
        {
            return ContainerBuilder.Build();
        }

        #endregion 方法
    }
}