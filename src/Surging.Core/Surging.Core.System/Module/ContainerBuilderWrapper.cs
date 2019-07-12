using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.System.Module
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
        /// <param name="builder">容器构建对象。</param>
        public ContainerBuilderWrapper(ContainerBuilder builder)
        {
            ContainerBuilder = builder;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the ContainerBuilder
        /// 获取内部容器构建对象。
        /// </summary>
        public ContainerBuilder ContainerBuilder { get; private set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// 构建容器。
        /// </summary>
        /// <returns></returns>
        public IContainer Build()
        {
            return ContainerBuilder.Build();
        }

        #endregion 方法
    }
}