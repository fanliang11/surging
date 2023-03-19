using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ProxyGenerator.Interceptors.Implementation.Metadatas
{
  [AttributeUsage(AttributeTargets.Method, Inherited = false)]
  public  class ServiceCacheIntercept : ServiceIntercept
    {
        #region 字段

        #endregion

        #region 构造函数
        /// <summary>
        /// 初始化一个新的<c>InterceptMethodAttribute</c>类型。
        /// </summary>
        /// <param name="method">缓存方式。</param>
        public ServiceCacheIntercept(CachingMethod method)
        {
            this.Method = method;
        }
        /// <summary>
        /// 初始化一个新的<c>InterceptMethodAttribute</c>类型。
        /// </summary>
        /// <param name="method">缓存方式。</param>
        /// <param name="correspondingMethodNames">与当前缓存方式相关的方法名称。注：此参数仅在缓存方式为Remove时起作用。</param>
        public ServiceCacheIntercept(CachingMethod method, params string[] correspondingMethodNames)
            : this(method)
        {
            this.CorrespondingKeys = correspondingMethodNames;
        }

         internal ServiceCacheIntercept(string [] serviceInterceptItem)
        {
            Key = serviceInterceptItem[0];
            L2Key= serviceInterceptItem[1];
            EnableL2Cache = serviceInterceptItem[2] == "1" ? true : false ;
           Enum.TryParse<CacheTargetType>(serviceInterceptItem[3],out CacheTargetType mode);
            Mode = mode;
            CacheSectionType = serviceInterceptItem[4];
                Enum.TryParse<CachingMethod>(serviceInterceptItem[5], out CachingMethod method);
            Method = method;
            Force=  serviceInterceptItem[6]== "1" ? true : false; ;
            Time = Convert.ToInt32(serviceInterceptItem[7]);
            if(!string.IsNullOrEmpty(serviceInterceptItem[8]))
            {
                CorrespondingKeys = serviceInterceptItem[8].Split(",");
            }
        }
        #endregion

        #region 公共属性
        /// <summary>
        /// 有效时间
        /// </summary>
        public int Time { get; set; } = 60;
        /// <summary>
        /// 采用什么进行缓存
        /// </summary>
        public CacheTargetType Mode { get; set; } = CacheTargetType.MemoryCache;

        ///// <summary>
        ///// 设置SectionType
        ///// </summary>
        public string CacheSectionType
        {
            get;
            set;
        } = "";

        public string L2Key 
        {
            get; set;
        }= "";

        public bool EnableL2Cache
        {
            get; set;
        }

        public string Key { get; set; } = "";
        /// <summary>
        /// 获取或设置缓存方式。
        /// </summary>
        public CachingMethod Method { get; set; } 

        /// <summary>
        /// 获取或设置一个<see cref="Boolean"/>值，该值表示当缓存方式为Put时，是否强制将值写入缓存中。
        /// </summary>
        public bool Force { get; set; }
        /// <summary>
        /// 获取或设置与当前缓存方式相关的方法名称。注：此参数仅在缓存方式为Remove时起作用。
        /// </summary>
        public string[] CorrespondingKeys { get; set; }
        protected override string MetadataId { get; set; } = "Cache";

        public override void Apply(ServiceDescriptor descriptor)
        {
            descriptor.Intercept(MetadataId).Key(Key, MetadataId)
                .L2Key(L2Key, MetadataId)
                .EnableL2Cache(EnableL2Cache, MetadataId)
                .Mode(Mode, MetadataId)
                .CacheSectionType(CacheSectionType, MetadataId)
                .Method(Method, MetadataId)
                .Force(Force, MetadataId)
                .CacheTime(Time, MetadataId)
                .CorrespondingKeys(CorrespondingKeys, MetadataId);
        }

        #endregion
    }
}
