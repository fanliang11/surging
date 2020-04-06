using Surging.Core.Caching;
using Surging.Core.CPlatform.Cache;
using Surging.Core.CPlatform.Routing;
using Surging.Core.ProxyGenerator.Interceptors;
using Surging.Core.ProxyGenerator.Interceptors.Implementation;
using System.Linq;
using System.Threading.Tasks;
using Metadatas =Surging.Core.ProxyGenerator.Interceptors.Implementation.Metadatas;
namespace Surging.Core.System.Intercept
{
    /// <summary>
    /// 缓存拦截器
    /// </summary>
    public class CacheProviderInterceptor : IInterceptor
    {
        private readonly IInterceptorProvider _interceptorProvider; 
        private readonly IServiceRouteProvider _serviceRouteProvider;
        public CacheProviderInterceptor(IInterceptorProvider interceptorProvider, IServiceRouteProvider serviceRouteProvider)
        {
            _interceptorProvider = interceptorProvider;
            _serviceRouteProvider = serviceRouteProvider;
        }

        public  async Task Intercept(IInvocation invocation)
        {
         
            var route= await _serviceRouteProvider.Locate(invocation.ServiceId);
            var cacheMetadata = route.ServiceDescriptor.GetCacheIntercept("Cache");
            if (cacheMetadata != null)
            {
                var keyValues = _interceptorProvider.GetCacheKeyVaule(invocation.Arguments);
                var cacheKey = keyValues == null ? cacheMetadata.Key :
                    string.Format(cacheMetadata.Key ?? "", keyValues);
                var l2CacheKey = keyValues == null ? cacheMetadata.L2Key :
                     string.Format(cacheMetadata.L2Key ?? "", keyValues);
                await CacheIntercept(cacheMetadata, cacheKey, keyValues, invocation, l2CacheKey, cacheMetadata.EnableL2Cache);
            }
        }

        private async Task CacheIntercept(Metadatas.ServiceCacheIntercept attribute, string key, string[] keyVaules,IInvocation invocation,string l2Key,bool enableL2Cache)
        {
            ICacheProvider cacheProvider = null;
            switch (attribute.Mode)
            {
                case Metadatas.CacheTargetType.Redis:
                    {
                        cacheProvider = CacheContainer.GetService<ICacheProvider>(string.Format("{0}.{1}",
                           attribute.CacheSectionType.ToString(), CacheTargetType.Redis.ToString()));
                        break;
                    }
                case Metadatas.CacheTargetType.MemoryCache:
                    {
                        cacheProvider = CacheContainer.GetService<ICacheProvider>(CacheTargetType.MemoryCache.ToString());
                        break;
                    }
            }
            if (cacheProvider != null && !enableL2Cache) await Invoke(cacheProvider, attribute, key,keyVaules, invocation);
            else if(cacheProvider != null && enableL2Cache)
            {
                var l2CacheProvider = CacheContainer.GetService<ICacheProvider>(CacheTargetType.MemoryCache.ToString());
                if(l2CacheProvider !=null) await Invoke(cacheProvider,l2CacheProvider,l2Key, attribute, key, keyVaules, invocation);
            }
        }

        private async Task Invoke(ICacheProvider cacheProvider, Metadatas.ServiceCacheIntercept attribute, string key, string[] keyVaules, IInvocation invocation)
        {

            switch (attribute.Method)
            {
                case Metadatas.CachingMethod.Get:
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
                        var keys = attribute.CorrespondingKeys.Select(correspondingKey => string.Format(correspondingKey,keyVaules)).ToList();
                        keys.ForEach(cacheProvider.RemoveAsync);
                        break;
                    }
            }
        }


        private async Task Invoke(ICacheProvider cacheProvider, ICacheProvider l2cacheProvider,string l2Key, Metadatas.ServiceCacheIntercept attribute, string key, string[] keyVaules, IInvocation invocation)
        {

            switch (attribute.Method)
            {
                case Metadatas.CachingMethod.Get:
                    {
                        var retrunValue = await cacheProvider.GetFromCacheFirst(l2cacheProvider, l2Key,key, async () =>
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
                        var keys = attribute.CorrespondingKeys.Select(correspondingKey => string.Format(correspondingKey, keyVaules)).ToList();
                        keys.ForEach(cacheProvider.RemoveAsync);
                        break;
                    }
            }
        }
    }
}
