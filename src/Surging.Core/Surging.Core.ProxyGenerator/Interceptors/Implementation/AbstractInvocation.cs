using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ProxyGenerator.Interceptors.Implementation
{
    /// <summary>
    /// Defines the <see cref="AbstractInvocation" />
    /// </summary>
    public abstract class AbstractInvocation : IInvocation, ICacheInvocation
    {
        #region 字段

        /// <summary>
        /// Defines the proxyObject
        /// </summary>
        protected readonly object proxyObject;

        /// <summary>
        /// Defines the _arguments
        /// </summary>
        private readonly IDictionary<string, object> _arguments;

        /// <summary>
        /// Defines the _attributes
        /// </summary>
        private readonly List<Attribute> _attributes;

        /// <summary>
        /// Defines the _cacheKey
        /// </summary>
        private readonly string[] _cacheKey;

        /// <summary>
        /// Defines the _returnType
        /// </summary>
        private readonly Type _returnType;

        /// <summary>
        /// Defines the _serviceId
        /// </summary>
        private readonly string _serviceId;

        /// <summary>
        /// Defines the _returnValue
        /// </summary>
        protected object _returnValue;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractInvocation"/> class.
        /// </summary>
        /// <param name="arguments">The arguments<see cref="IDictionary{string, object}"/></param>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <param name="cacheKey">The cacheKey<see cref="string[]"/></param>
        /// <param name="attributes">The attributes<see cref="List{Attribute}"/></param>
        /// <param name="returnType">The returnType<see cref="Type"/></param>
        /// <param name="proxy">The proxy<see cref="object"/></param>
        protected AbstractInvocation(
          IDictionary<string, object> arguments,
           string serviceId,
            string[] cacheKey,
            List<Attribute> attributes,
            Type returnType,
            object proxy
            )
        {
            _arguments = arguments;
            _serviceId = serviceId;
            _cacheKey = cacheKey;
            _attributes = attributes;
            _returnType = returnType;
            proxyObject = proxy;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the Arguments
        /// </summary>
        public IDictionary<string, object> Arguments => _arguments;

        /// <summary>
        /// Gets the Attributes
        /// </summary>
        public List<Attribute> Attributes => _attributes;

        /// <summary>
        /// Gets the CacheKey
        /// </summary>
        public string[] CacheKey => _cacheKey;

        /// <summary>
        /// Gets the Proxy
        /// </summary>
        public object Proxy => proxyObject;

        /// <summary>
        /// Gets the ReturnType
        /// </summary>
        public Type ReturnType => _returnType;

        /// <summary>
        /// Gets the ServiceId
        /// </summary>
        public string ServiceId => _serviceId;

        /// <summary>
        /// Gets or sets the ReturnValue
        /// </summary>
        object IInvocation.ReturnValue { get => _returnValue; set => _returnValue = value; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Proceed
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        public abstract Task Proceed();

        /// <summary>
        /// The SetArgumentValue
        /// </summary>
        /// <param name="index">The index<see cref="int"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        public void SetArgumentValue(int index, object value)
        {
            throw new NotImplementedException();
        }

        #endregion 方法
    }
}