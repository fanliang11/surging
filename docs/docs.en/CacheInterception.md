surging is based on .net core language high-performance distributed microservices framework,
Clients can enable caching to intercept client calls to the server,The following example enables caching

Code Examples
=============

Create an Interceptor that inherits CacheInterceptor:
```c#
 public class CacheProviderInterceptor : CacheInterceptor
    {
        public override async Task Intercept(IInvocation invocation)
        {
            var attribute =
                 invocation.Attributes.Where(p => p is InterceptMethodAttribute)
                 .Select(p => p as InterceptMethodAttribute).FirstOrDefault();
            var cacheKey = invocation.CacheKey == null ? attribute.Key :
                string.Format(attribute.Key ?? "", invocation.CacheKey);
            await CacheIntercept(attribute, cacheKey, invocation);
        }

        private async Task CacheIntercept(InterceptMethodAttribute attribute, string key, IInvocation invocation)
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
            if (cacheProvider != null) await Invoke(cacheProvider, attribute, key, invocation);
        }

        private async Task Invoke(ICacheProvider cacheProvider, InterceptMethodAttribute attribute, string key, IInvocation invocation)
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
    }
```

Configure the interception code as follows:
```c#
builder.AddMicroService(option =>
                    {
                        option.AddClient();
                        option.AddClientIntercepted(typeof(CacheProviderInterceptor));//Configure Interceptor
                        //option.UseZooKeeperManager(new ConfigInfo("127.0.0.1:2181"));
                        option.UseConsulManager(new ConfigInfo("127.0.0.1:8500"));
                        option.UseDotNettyTransport();
                        option.UseRabbitMQTransport();
                        option.AddCache();
                        //option.UseKafkaMQTransport(kafkaOption =>
                        //{
                        //    kafkaOption.Servers = "127.0.0.1";
                        //});
                        //option.UseProtoBufferCodec();
                        option.UseMessagePackCodec();
                        builder.Register(p => new CPlatformContainer(ServiceLocator.Current));
                    });
```
## Next steps
