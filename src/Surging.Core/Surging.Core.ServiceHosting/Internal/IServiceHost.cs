using Autofac;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.ServiceHosting.Internal
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IServiceHost" />
    /// </summary>
    public interface IServiceHost : IDisposable
    {
        #region 方法

        /// <summary>
        /// The Initialize
        /// </summary>
        /// <returns>The <see cref="IContainer"/></returns>
        IContainer Initialize();

        /// <summary>
        /// The Run
        /// </summary>
        /// <returns>The <see cref="IDisposable"/></returns>
        IDisposable Run();

        #endregion 方法
    }

    #endregion 接口
}