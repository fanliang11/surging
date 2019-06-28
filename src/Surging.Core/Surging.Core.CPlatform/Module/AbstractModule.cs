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
    public abstract class AbstractModule : Autofac.Module,IDisposable
    {
        #region 实例属性

        public ContainerBuilderWrapper Builder { get; set; }
   
        public Guid Identifier { get; set; }


        public string ModuleName { get; set; }


        public string TypeName { get; set; }

    

        public string Title { get; set; }

        public bool Enable { get; set; } = true;


        public string Description { get; set; }
        
        public List<Component> Components { get; set; }

        #endregion

        #region 构造函数
        public AbstractModule()
        {
            ModuleName = this.GetType().Name;
            TypeName = this.GetType().BaseType.Name;
        }
        #endregion

        #region 实例方法

        public virtual void Initialize(AppModuleContext serviceProvider)
        {
            Dispose();
        }
        
        protected override  void Load(ContainerBuilder builder)
        {
            try
            {
                base.Load(builder);
                Builder = new ContainerBuilderWrapper(builder);
                if (Enable)
                {
                    RegisterBuilder(Builder);
                    RegisterComponents(Builder);
                    
                }
            }
            catch (Exception ex)
            {
                throw new CPlatformException(string.Format("注册模块组件类型时发生错误：{0}", ex.Message));
            }
        }

       
        protected virtual void RegisterBuilder(ContainerBuilderWrapper builder)
        {
        }
         

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

        public virtual void Dispose()
        {
        }

        #endregion
    }
}
