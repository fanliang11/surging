using Autofac;
using Autofac.Core.Lifetime;
using Surging.Core.CPlatform.Exceptions;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Surging.Core.CPlatform.Module
{
    /// <summary>
    /// Defines the <see cref="AbstractModule" />
    /// </summary>
    public abstract class AbstractModule : Autofac.Module, IDisposable
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractModule"/> class.
        /// </summary>
        public AbstractModule()
        {
            ModuleName = this.GetType().Name;
            TypeName = this.GetType().BaseType.Name;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Builder
        /// 容器创建包装属性
        /// </summary>
        public ContainerBuilderWrapper Builder { get; set; }

        /// <summary>
        /// Gets or sets the Components
        /// 组件
        /// </summary>
        public List<Component> Components { get; set; }

        /// <summary>
        /// Gets or sets the Description
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Enable
        /// 是否可用（控制模块是否加载）
        /// </summary>
        public bool Enable { get; set; } = true;

        /// <summary>
        /// Gets or sets the Identifier
        /// 唯一标识guid
        /// </summary>
        public Guid Identifier { get; set; }

        /// <summary>
        /// Gets or sets the ModuleName
        /// 模块名
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// Gets or sets the Title
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the TypeName
        /// 类型名
        /// </summary>
        public string TypeName { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Dispose
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// The Initialize
        /// </summary>
        /// <param name="serviceProvider">The serviceProvider<see cref="AppModuleContext"/></param>
        public virtual void Initialize(AppModuleContext serviceProvider)
        {
            Dispose();
        }

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
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

        /// <summary>
        /// 验证模块
        /// </summary>
        public virtual void ValidateModule()
        {
            if (this.Identifier == Guid.Empty || string.IsNullOrEmpty(this.ModuleName) || string.IsNullOrEmpty(this.TypeName)
                || string.IsNullOrEmpty(this.Title))
            {
                throw new CPlatformException("模块属性：Identifier，ModuleName，TypeName，Title 是必须的不能为空！");
            }

            Regex regex = new Regex(@"^[a-zA-Z][a-zA-Z0-9_]*$");
            if (!regex.IsMatch(this.ModuleName))
            {
                throw new CPlatformException("模块属性：ModuleName 必须为字母开头数字或下划线的组合！");
            }
        }

        /// <summary>
        /// The RegisterComponents
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        internal virtual void RegisterComponents(ContainerBuilderWrapper builder)
        {
            if (Components != null)
            {
                Components.ForEach(component =>
                {
                    //服务类型
                    Type serviceType = Type.GetType(component.ServiceType, true);
                    //实现类型
                    Type implementType = Type.GetType(component.ImplementType, true);
                    //组件生命周期
                    switch (component.LifetimeScope)
                    {
                        //依赖创建
                        case LifetimeScope.InstancePerDependency:
                            //如果是泛型
                            if (serviceType.GetTypeInfo().IsGenericType || implementType.GetTypeInfo().IsGenericType)
                            {
                                //注册泛型
                                builder.RegisterGeneric(implementType).As(serviceType).InstancePerDependency();
                            }
                            else
                            {
                                builder.RegisterType(implementType).As(serviceType).InstancePerDependency();
                            }
                            break;

                        case LifetimeScope.SingleInstance://单例
                            if (serviceType.GetTypeInfo().IsGenericType || implementType.GetTypeInfo().IsGenericType)
                            {
                                builder.RegisterGeneric(implementType).As(serviceType).SingleInstance();
                            }
                            else
                            {
                                builder.RegisterType(implementType).As(serviceType).SingleInstance();
                            }
                            break;

                        default://默认依赖创建
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
        /// 重写autofac load方法
        /// 判断组件是否可用，并注册模块组件
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder)
        {
            try
            {
                base.Load(builder);
                Builder = new ContainerBuilderWrapper(builder);
                if (Enable)//如果可用
                {
                    //注册创建容器
                    RegisterBuilder(Builder);
                    //注册组件
                    RegisterComponents(Builder);
                }
            }
            catch (Exception ex)
            {
                throw new CPlatformException(string.Format("注册模块组件类型时发生错误：{0}", ex.Message));
            }
        }

        /// <summary>
        /// The RegisterBuilder
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        protected virtual void RegisterBuilder(ContainerBuilderWrapper builder)
        {
        }

        #endregion 方法
    }
}