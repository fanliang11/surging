using Surging.Core.Caching;
using Surging.Core.CPlatform.Cache;
using Surging.Core.ProxyGenerator.Interceptors;
using System.Linq;
using System.Threading.Tasks;

namespace Surging.Core.System.Intercept
{
    /// <summary>
    /// 缓存拦截器
    /// </summary>
    public class CacheProviderInterceptor : CacheInterceptor
    {
        #region 方法

        /// <summary>
        /// The Intercept
        /// </summary>
        /// <param name="invocation">The invocation<see cref="ICacheInvocation"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public override async Task Intercept(ICacheInvocation invocation)
        {
            var attribute =
                 invocation.Attributes.Where(p => p is InterceptMethodAttribute)
                 .Select(p => p as InterceptMethodAttribute).FirstOrDefault();
            var cacheKey = invocation.CacheKey == null ? attribute.Key :
                string.Format(attribute.Key ?? "", invocation.CacheKey);
            var l2CacheKey = invocation.CacheKey == null ? attribute.L2Key :
                 string.Format(attribute.L2Key ?? "", invocation.CacheKey);
            await CacheIntercept(attribute, cacheKey, invocation, l2CacheKey, attribute.EnableL2Cache);
        }

        /// <summary>
        /// The CacheIntercept
        /// </summary>
        /// <param name="attribute">The attribute<see cref="InterceptMethodAttribute"/></param>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="invocation">The invocation<see cref="ICacheInvocation"/></param>
        /// <param name="l2Key">The l2Key<see cref="string"/></param>
        /// <param name="enableL2Cache">The enableL2Cache<see cref="bool"/></param>
        /// <returns>The <see cref="Task"/></returns>
        private async Task CacheIntercept(InterceptMethodAttribute attribute, string key, ICacheInvocation invocation, string l2Key, bool enableL2Cache)
        {
            ICacheProvider cacheProvider = null;
            switch (attribute.Mode)
            {
                case CacheTargetType.Redis:
                    {
                        cacheProvider = CacheContainer.GetService<ICacheProvider>(string.Format("{0}.{1}",
                           attribute.CacheSectionType.ToString(), CacheTargetType.Redis.ToString()));
                        break;
                    }
                case CacheTargetType.MemoryCache:
                    {
                        cacheProvider = CacheContainer.GetService<ICacheProvider>(CacheTargetType.MemoryCache.ToString());
                        break;
                    }
            }
            if (cacheProvider != null && !enableL2Cache) await Invoke(cacheProvider, attribute, key, invocation);
            else if (cacheProvider != null && enableL2Cache)
            {
                var l2CacheProvider = CacheContainer.GetService<ICacheProvider>(CacheTargetType.MemoryCache.ToString());
                if (l2CacheProvider != null) await Invoke(cacheProvider, l2CacheProvider, l2Key, attribute, key, invocation);
            }
        }

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <param name="cacheProvider">The cacheProvider<see cref="ICacheProvider"/></param>
        /// <param name="l2cacheProvider">The l2cacheProvider<see cref="ICacheProvider"/></param>
        /// <param name="l2Key">The l2Key<see cref="string"/></param>
        /// <param name="attribute">The attribute<see cref="InterceptMethodAttribute"/></param>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="invocation">The invocation<see cref="ICacheInvocation"/></param>
        /// <returns>The <see cref="Task"/></returns>
        private async Task Invoke(ICacheProvider cacheProvider, ICacheProvider l2cacheProvider, string l2Key, InterceptMethodAttribute attribute, string key, ICacheInvocation invocation)
        {
            switch (attribute.Method)
            {
                case CachingMethod.Get:
                    {
                        var retrunValue = await cacheProvider.GetFromCacheFirst(l2cacheProvider, l2Key, key, async () =>
                         {
                             await invocation.Proceed();
                             return invocation.ReturnValue;
                         }, invocation.ReturnType, attribute.Time);
                        invocation.ReturnValue = retrunValue;
                        break;
                    }
                default:
                    {
                        await invocation.Proceed();
                        var keys = attribute.CorrespondingKeys.Select(correspondingKey => string.Format(correspondingKey, invocation.CacheKey)).ToList();
                        keys.ForEach(cacheProvider.RemoveAsync);
                        break;
                    }
            }
        }

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <param name="cacheProvider">The cacheProvider<see cref="ICacheProvider"/></param>
        /// <param name="attribute">The attribute<see cref="InterceptMethodAttribute"/></param>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="invocation">The invocation<see cref="ICacheInvocation"/></param>
        /// <returns>The <see cref="Task"/></returns>
        private async Task Invoke(ICacheProvider cacheProvider, InterceptMethodAttribute attribute, string key, ICacheInvocation invocation)
        {
            switch (attribute.Method)
            {
                case CachingMethod.Get:
                    {
                        var retrunValue = await cacheProvider.GetFromCacheFirst(key, async () =>
                        {
                            await invocation.Proceed();
                            return invocation.ReturnValue;
                        }, invocation.ReturnType, attribute.Time);
                        invocation.ReturnValue = retrunValue;
                        break;
                    }
                default:
                    {
                        await invocation.Proceed();
                        var keys = attribute.CorrespondingKeys.Select(correspondingKey => string.Format(correspondingKey, invocation.CacheKey)).ToList();
                        keys.ForEach(cacheProvider.RemoveAsync);
                        break;
                    }
            }
        }

        #endregion 方法
    }
}