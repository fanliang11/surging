using Surging.Core.ProxyGenerator.Implementation;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ProxyGenerator.Interceptors.Implementation
{
    /// <summary>
    /// Defines the <see cref="ActionInvocation" />
    /// </summary>
    public class ActionInvocation : AbstractInvocation
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionInvocation"/> class.
        /// </summary>
        /// <param name="arguments">The arguments<see cref="IDictionary{string, object}"/></param>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <param name="cacheKey">The cacheKey<see cref="string[]"/></param>
        /// <param name="attributes">The attributes<see cref="List{Attribute}"/></param>
        /// <param name="returnType">The returnType<see cref="Type"/></param>
        /// <param name="proxy">The proxy<see cref="object"/></param>
        protected ActionInvocation(
             IDictionary<string, object> arguments,
           string serviceId,
            string[] cacheKey,
            List<Attribute> attributes,
            Type returnType,
            object proxy
            ) : base(arguments, serviceId, cacheKey, attributes, returnType, proxy)
        {
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Proceed
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        public override async Task Proceed()
        {
            try
            {
                if (_returnValue == null)
                    _returnValue = await (Proxy as ServiceProxyBase).CallInvoke(this);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion 方法
    }
}