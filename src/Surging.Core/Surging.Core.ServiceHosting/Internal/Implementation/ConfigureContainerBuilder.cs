using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Surging.Core.ServiceHosting.Internal.Implementation
{
    /// <summary>
    /// Defines the <see cref="ConfigureContainerBuilder" />
    /// </summary>
    public class ConfigureContainerBuilder
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureContainerBuilder"/> class.
        /// </summary>
        /// <param name="configureContainerMethod">The configureContainerMethod<see cref="MethodInfo"/></param>
        public ConfigureContainerBuilder(MethodInfo configureContainerMethod)
        {
            MethodInfo = configureContainerMethod;
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
        /// <returns>The <see cref="Action{object}"/></returns>
        public Action<object> Build(object instance) => container => Invoke(instance, container);

        /// <summary>
        /// The GetContainerType
        /// </summary>
        /// <returns>The <see cref="Type"/></returns>
        public Type GetContainerType()
        {
            var parameters = MethodInfo.GetParameters();
            if (parameters.Length != 1)
            {
                throw new InvalidOperationException($"{MethodInfo.Name} 方法必须有一个参数");
            }
            return parameters[0].ParameterType;
        }

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <param name="instance">The instance<see cref="object"/></param>
        /// <param name="container">The container<see cref="object"/></param>
        private void Invoke(object instance, object container)
        {
            if (MethodInfo == null)
            {
                return;
            }
            var arguments = new object[1] { container };
            MethodInfo.Invoke(instance, arguments);
        }

        #endregion 方法
    }
}