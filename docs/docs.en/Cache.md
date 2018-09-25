surging is based on .net core language high-performance distributed microservices framework,
Clients can enable caching to intercept client calls to the server,The following example enables caching

Code Examples
=============

Clients enable caching through the following configuration:
```c#
{ 
    "CachingSettings": [
      {
        "Id": "SurgingCache",
        "Class": "Surging.Core.Caching.RedisCache.RedisContext,Surging.Core.Caching",
        "Properties": [
          {
            "Name": "appRuleFile",
            "Ref": "rule"
          },
          {
            "Name": "dataContextPool",
            "Ref": "ddls_sample",
            "Maps": [
              {
                "Name": "Redis",
                "Properties": [
                  {
                    "value": "127.0.0.1:6379::1"
                  }
                ]
              }
            ]
          },
          {
            "Name": "defaultExpireTime",
            "value": "120"
          },
          {
            "Name": "connectTimeout",
            "Value": "120"
          },
          {
            "Name": "minSize",
            "Value": "1"
          },
          {
            "Name": "maxSize",
            "Value": "10"
          }
        ]
      }
    ]
}
```
* The configuration section "CachingSettings" property "Id" is the ICacheProvider instance ID
* Configuration section "Maps" property "Name" is what cache
* Configuration section "Maps" property Properties is a cache server list
* Configuration section "Properties" property "defaultExpireTime" value of 120 is the cache default lost effective time is 120 seconds
* Configuration section "Properties" property "connectTimeout" value of 120 is connection cache server Timeout time is 120 seconds
* Configuration section "Properties" property "minSize" value of 1 is the minimum thread pool is 1
* Configuration section "Properties" property "maxSize" value of 10 is the maximum thread pool is 10

Open the cache can be In the interface method attribute "Command" property RequestCacheEnabled is set to true:
```c#
[ServiceBundle("api/{Service}")]
public interface IUserService :Surging.Core.CPlatform.Ioc.IServiceKey
{
	[Command(RequestCacheEnabled =true)]
	Task<string> SayHello(string username);
}
```
## Next steps

* [How to cache interception](https://github.com/dotnetcore/surging/blob/master/docs/docs.en/CacheInterception.md)

