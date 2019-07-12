using Autofac;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Surging.Core.ServiceHosting.Internal.Implementation
{
    /// <summary>
    /// Defines the <see cref="ConfigureBuilder" />
    /// </summary>
    public class ConfigureBuilder
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureBuilder"/> class.
        /// </summary>
        /// <param name="configure">The configure<see cref="MethodInfo"/></param>
        public ConfigureBuilder(MethodInfo configure)
        {
            MethodInfo = configure;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the MethodInfo
        /// </summary>
        public MethodInfo MethodInfo { get; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Build
        /// </summary>
        /// <param name="instance">The instance<see cref="object"/></param>
        /// <returns>The <see cref="Action{IContainer}"/></returns>
        public Action<IContainer> Build(object instance) => builder => Invoke(instance, builder);

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <param name="instance">The instance<see cref="object"/></param>
        /// <param name="builder">The builder<see cref="IContainer"/></param>
        private void Invoke(object instance, IContainer builder)
        {
            using (var scope = builder.BeginLifetimeScope())
            {
                var parameterInfos = MethodInfo.GetParameters();
                var parameters = new object[parameterInfos.Length];
                for (var index = 0; index < parameterInfos.Length; index++)
                {
                    var parameterInfo = parameterInfos[index];
                    if (parameterInfo.ParameterType == typeof(IContainer))
                    {
                        parameters[index] = builder;
                    }
                    else
                    {
                        try
                        {
                            parameters[index] = scope.Resolve(parameterInfo.ParameterType);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(string.Format(
                                "无法解析的服务类型: '{0}'参数： '{1}' 方法： '{2}' 类型 '{3}'.",
                                parameterInfo.ParameterType.FullName,
                                parameterInfo.Name,
                                MethodInfo.Name,
                                MethodInfo.DeclaringType.FullName), ex);
                        }
                    }
                }
                MethodInfo.Invoke(instance, parameters);
            }
        }

        #endregion 方法
    }
}