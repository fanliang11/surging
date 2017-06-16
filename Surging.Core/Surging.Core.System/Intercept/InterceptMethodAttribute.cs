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
        int _time = 60;
        CacheTargetType _mode = CacheTargetType.MemoryCache;
        CachingMethod _method;
        string[] _correspondingKeys;

        #endregion

        #region 构造函数
        /// <summary>
        /// 初始化一个新的<c>InterceptMethodAttribute</c>类型。
        /// </summary>
        /// <param name="method">缓存方式。</param>
        public InterceptMethodAttribute(CachingMethod method)
        {
            this._method = method;
        }
        /// <summary>
        /// 初始化一个新的<c>InterceptMethodAttribute</c>类型。
        /// </summary>
        /// <param name="method">缓存方式。</param>
        /// <param name="correspondingMethodNames">与当前缓存方式相关的方法名称。注：此参数仅在缓存方式为Remove时起作用。</param>
        public InterceptMethodAttribute(CachingMethod method, params string[] correspondingMethodNames)
            : this(method)
        {
            this._correspondingKeys = correspondingMethodNames;
        }
        #endregion

        #region 公共属性
        /// <summary>
        /// 有效时间
        /// </summary>
        public int Time
        {
            get { return _time; }
            set { _time = value; }
        }
        /// <summary>
        /// 采用什么进行缓存
        /// </summary>
        public CacheTargetType Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        ///// <summary>
        ///// 设置SectionType
        ///// </summary>
        public SectionType CacheSectionType
        {
            get;
            set;
        }

        public string Key { get; set; }
        /// <summary>
        /// 获取或设置缓存方式。
        /// </summary>
        public CachingMethod Method
        {
            get
            {
                return _method;
            }
            set { _method = value; }
        }

        /// <summary>
        /// 获取或设置一个<see cref="Boolean"/>值，该值表示当缓存方式为Put时，是否强制将值写入缓存中。
        /// </summary>
        public bool Force { get; set; }
        /// <summary>
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

        #endregion

    }
}
