using Surging.Core.Caching;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.System.Intercept
{
    /// <summary>
    /// 设置判断缓存拦截方法的特性类
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface)]
    public class InterceptMethodAttribute : Attribute
    {
        #region 字段

        /// <summary>
        /// Defines the _correspondingKeys
        /// </summary>
        internal string[] _correspondingKeys;

        /// <summary>
        /// Defines the _method
        /// </summary>
        internal CachingMethod _method;

        /// <summary>
        /// Defines the _mode
        /// </summary>
        internal CacheTargetType _mode = CacheTargetType.MemoryCache;

        /// <summary>
        /// Defines the _time
        /// </summary>
        internal int _time = 60;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptMethodAttribute"/> class.
        /// </summary>
        /// <param name="method">缓存方式。</param>
        public InterceptMethodAttribute(CachingMethod method)
        {
            this._method = method;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptMethodAttribute"/> class.
        /// </summary>
        /// <param name="method">缓存方式。</param>
        /// <param name="correspondingMethodNames">与当前缓存方式相关的方法名称。注：此参数仅在缓存方式为Remove时起作用。</param>
        public InterceptMethodAttribute(CachingMethod method, params string[] correspondingMethodNames)
            : this(method)
        {
            this._correspondingKeys = correspondingMethodNames;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the CacheSectionType
        /// 设置SectionType
        /// </summary>
        public SectionType CacheSectionType { get; set; }

        /// <summary>
        /// Gets or sets the CorrespondingKeys
        /// 获取或设置与当前缓存方式相关的方法名称。注：此参数仅在缓存方式为Remove时起作用。
        /// </summary>
        public string[] CorrespondingKeys
        {
            get
            {
                return _correspondingKeys;
            }
            set
            {
                _correspondingKeys = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether EnableL2Cache
        /// </summary>
        public bool EnableL2Cache { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Force
        /// 获取或设置一个<see cref="Boolean"/>值，该值表示当缓存方式为Put时，是否强制将值写入缓存中。
        /// </summary>
        public bool Force { get; set; }

        /// <summary>
        /// Gets or sets the Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the L2Key
        /// </summary>
        public string L2Key { get; set; }

        /// <summary>
        /// Gets or sets the Method
        /// 获取或设置缓存方式。
        /// </summary>
        public CachingMethod Method
        {
            get { return _method; }
            set { _method = value; }
        }

        /// <summary>
        /// Gets or sets the Mode
        /// 采用什么进行缓存
        /// </summary>
        public CacheTargetType Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        /// <summary>
        /// Gets or sets the Time
        /// 有效时间
        /// </summary>
        public int Time
        {
            get { return _time; }
            set { _time = value; }
        }

        #endregion 属性
    }
}