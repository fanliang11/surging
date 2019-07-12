using Surging.Core.System.Module.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.System.Module
{
    /// <summary>
    /// 业务模块基类
    /// </summary>
    [ModuleDescription("13C13D64-693A-4D1E-93EB-B2E1465C24C7", "业务基础模块", "业务基础模块")]
    public class BusinessModule : AbstractModule
    {
        #region 方法

        /// <summary>
        /// 初始化模块，该操作在应用程序启动时执行。
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// 注册组件到依赖注入容器。
        /// </summary>
        /// <param name="builder">容器构建对象。</param>
        internal override void RegisterComponents(ContainerBuilderWrapper builder)
        {
            base.RegisterComponents(builder);
        }

        #endregion 方法
    }
}