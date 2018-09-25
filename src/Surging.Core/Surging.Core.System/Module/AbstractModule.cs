using Autofac;
using Surging.Core.Common.ServicesException;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Surging.Core.System.Module
{
    /// <summary>
    /// 抽象模块业务模块和系统模块的基类。
    /// </summary>
    /// <remarks>
    /// 	<para>创建：范亮</para>
    /// 	<para>日期：2015/12/4</para>
    /// </remarks>
    public abstract class AbstractModule : Autofac.Module
    {
        #region 实例属性

        public ContainerBuilderWrapper Builder { get; set; }
        /// <summary>
        /// 获取或设置模块标识符 GUID 。
        /// </summary>
        /// <value>
        /// 模块全局标识符 GUID 。
        /// </value>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2015/12/4</para>
        /// </remarks>
        public Guid Identifier { get; set; }

        /// <summary>
        /// 获取或设置模块名称(对应视图目录名称)唯一键。
        /// </summary>
        /// <value>
        /// 模块的名称需大小写字母组合。
        /// </value>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2015/12/4</para>
        /// </remarks>
   
        public string ModuleName { get; set; }

        /// <summary>
        /// 获取或设置模块类型名称(包含程序集名称的限定名)。
        /// </summary>
        /// <value>
        /// 模块类型名称。
        /// </value>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2015/12/4</para>
        /// </remarks>
    
        public string TypeName { get; set; }

        /// <summary>
        /// 获取或设置模块标题文本。
        /// </summary>
        /// <value>
        /// 标题文本。
        /// </value>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2015/12/4</para>
        /// </remarks>
      
        public string Title { get; set; }

        /// <summary>
        /// 获取或设置模块功能描述。
        /// </summary>
        /// <value>
        /// 模块功能描述文本。
        /// </value>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2015/12/4</para>
        /// </remarks>
        public string Description { get; set; }

        /// <summary>
        /// 获取或设置模块组件(定义了接口+实现类)列表。
        /// </summary>
        /// <value>
        /// 组件列表 List 泛型集合。
        /// </value>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2015/12/4</para>
        /// </remarks>
        public List<Component> Components { get; set; }

        #endregion

        #region 实例方法

        /// <summary>
        /// 初始化模块，该操作在应用程序启动时执行。
        /// </summary>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2015/12/4</para>
        /// </remarks>    
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// 加载组件到依赖注入容器。
        /// </summary>
        /// <param name="builder">容器构建对象。</param>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2015/12/4</para>
        /// </remarks>
        protected override void Load(ContainerBuilder builder)
        {
            try
            {
                base.Load(builder);
                Builder = new ContainerBuilderWrapper(builder);
                RegisterBuilder(Builder);
                RegisterComponents(Builder);
            }
            catch (Exception ex)
            {
                throw new ServiceException(string.Format("注册模块组件类型时发生错误：{0}", ex.Message));
            }
        }

        /// <summary>
        /// 注册构建。
        /// </summary>
        /// <param name="builder">容器构建对象。</param>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2015/12/4</para>
        /// </remarks>
        protected virtual void RegisterBuilder(ContainerBuilderWrapper builder)
        {
        }

        /// <summary>
        /// 注册组件到依赖注入容器。
        /// </summary>
        /// <param name="builder">容器构建对象。</param>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2015/12/4</para>
        /// </remarks>
        internal virtual void RegisterComponents(ContainerBuilderWrapper builder)
        {
            if (Components != null)
            {
                Components.ForEach(component =>
                {
                    Type serviceType = Type.GetType(component.ServiceType, true);
                    Type implementType = Type.GetType(component.ImplementType, true);
                    switch (component.LifetimeScope)
                    {
                        case LifetimeScope.InstancePerDependency:
                            if (serviceType.GetTypeInfo().IsGenericType || implementType.GetTypeInfo().IsGenericType)
                            {
                                builder.RegisterGeneric(implementType).As(serviceType).InstancePerDependency();
                            }
                            else
                            {
                                builder.RegisterType(implementType).As(serviceType).InstancePerDependency();
                            }
                            break;
                        case LifetimeScope.SingleInstance:
                            if (serviceType.GetTypeInfo().IsGenericType || implementType.GetTypeInfo().IsGenericType)
                            {
                                builder.RegisterGeneric(implementType).As(serviceType).SingleInstance();
                            }
                            else
                            {
                                builder.RegisterType(implementType).As(serviceType).SingleInstance();
                            }
                            break;
                        default:
                            if (serviceType.GetTypeInfo().IsGenericType || implementType.GetTypeInfo().IsGenericType)
                            {
                                builder.RegisterGeneric(implementType).As(serviceType).InstancePerDependency();
                            }
                            else
                            {
                                builder.RegisterType(implementType).As(serviceType).InstancePerDependency();
                            }
                            break;
                    }

                });
            }
        }

        /// <summary>
        /// 验证校验模块。
        /// </summary>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2015/12/4</para>
        /// </remarks>
        public virtual void ValidateModule()
        {
            if (this.Identifier == Guid.Empty || string.IsNullOrEmpty(this.ModuleName) || string.IsNullOrEmpty(this.TypeName)
                || string.IsNullOrEmpty(this.Title))
            {
                throw new ServiceException("模块属性：Identifier，ModuleName，TypeName，Title 是必须的不能为空！");
            }

            Regex regex = new Regex(@"^[a-zA-Z][a-zA-Z0-9_]*$");
            if (!regex.IsMatch(this.ModuleName))
            {
                throw new ServiceException("模块属性：ModuleName 必须为字母开头数字或下划线的组合！");
            }
        }

        /// <summary>
        /// 获取模块的字符串文本描述信息。
        /// </summary>
        /// <returns>
        /// 返回模块对象的字符串文本描述信息。
        /// </returns>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2015/12/4</para>
        /// </remarks>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("标识符：{0}", Identifier);
            sb.AppendLine();
            sb.AppendFormat("模块名：{0}", ModuleName);
            sb.AppendLine();
            sb.AppendFormat("类型名：{0}", TypeName);
            sb.AppendLine();
            sb.AppendFormat("标题：{0}", Title);
            sb.AppendLine();
            sb.AppendFormat("描述：{0}", Description);
            sb.AppendLine();
            sb.AppendFormat("组件详细 {0}个", Components.Count);
            sb.AppendLine();
            Components.ForEach(c =>
            {
                sb.AppendLine(c.ToString());
            });
            return sb.ToString();
        }

        #endregion
    }
}
