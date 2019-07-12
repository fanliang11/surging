using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Support
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IServiceCommandProvider" />
    /// </summary>
    public interface IServiceCommandProvider
    {
        #region 方法

        /// <summary>
        /// The GetCommand
        /// </summary>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <returns>The <see cref="ValueTask{ServiceCommand}"/></returns>
        ValueTask<ServiceCommand> GetCommand(string serviceId);

        /// <summary>
        /// The Run
        /// </summary>
        /// <param name="text">The text<see cref="string"/></param>
        /// <param name="InjectionNamespaces">The InjectionNamespaces<see cref="string[]"/></param>
        /// <returns>The <see cref="Task{object}"/></returns>
        Task<object> Run(string text, params string[] InjectionNamespaces);

        #endregion 方法
    }

    #endregion 接口
}