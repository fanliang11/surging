using Autofac;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Surging.Core.ServiceHosting.Internal.Implementation
{
    public class ConfigureBuilder
    {
        public ConfigureBuilder(MethodInfo configure)
        {
            MethodInfo = configure;
        }

        public MethodInfo MethodInfo { get; }

        public Action<IContainer> Build(object instance) => builder => Invoke(instance, builder);

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
    }
}