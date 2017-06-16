using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.System.Module
{
   public class ContainerBuilderWrapper
    {
        /// <summary>
        /// 获取内部容器构建对象。
        /// </summary>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2015/12/4</para>
        /// </remarks>
        public ContainerBuilder ContainerBuilder { get; private set; }

        /// <summary>
        /// 初始化一个新的 <see cref="ContainerBuilderWrapper"/> 类实例。
        /// </summary>
        /// <param name="builder">容器构建对象。</param>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2015/12/4</para>
        /// </remarks>
        public ContainerBuilderWrapper(ContainerBuilder builder)
        {
            ContainerBuilder = builder;
        }

        /// <summary>
        /// 构建容器。
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2015/12/4</para>
        /// </remarks>
        public IContainer Build()
        {
            return ContainerBuilder.Build();
        }
    }
}
