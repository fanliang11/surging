# surging 是一个分布式微服务框架,提供高性能RPC远程服务调用，采用Zookeeper作为surging服务的注册中心，集成了哈希，随机，轮询作为负载均衡的算法，RPC集成采用的是netty框架，采用异步传输。
<br />
增加EventBus服务的文件配置：<br/>
 new ConfigurationBuilder()<br/>
.SetBasePath(AppContext.BaseDirectory)<br/>
 .AddEventBusFile("eventBusSettings.json", optional: false);<br/>
增加EventBus服务的依赖注入：<br/>
  new ContainerBuilder().RegisterServiceBus();<br/>
增加rabbitmq 服务配置：<br/>
UseRabbitMQTransport() //rabbitmq 服务配置<br/>
AddRabbitMQAdapt()//基于rabbitmq的消费的服务适配<br/>
增加订阅功能：
<br/>
 ServiceLocator.GetService< ISubscriptionAdapt >().SubscribeAt();
 <br/>
增加服务容错、服务容错降级、服务强制降级


* 增加容错策略Injection，脚本注入：

<br/>

```c#
[Command(Strategy= StrategyType.Injection ,Injection = @"return null;")]
```    

 <br/>
 
```C#  
[Command(Strategy= StrategyType.Injection ,Injection = @"return 
Task.FromResult(new Surging.IModuleServices.Common.Models.UserModel
         {
            Name=""fanly"",
            Age=18
         });",InjectionNamespaces =new string[] { "Surging.IModuleServices.Common"})] 
```


* 增加容错策略Injection，本地模块注入：   

<br/>

```C#  
[Command(Strategy= StrategyType.Injection ,Injection = @"return true;")] 
```

<br/>

增加缓存降级，怎么使用？
<br/>
在业务接口方法上添加如下特性
<br/>

```C#  
   [Command(Strategy= StrategyType.Failover,FailoverCluster =3,RequestCacheEnabled =true)]  //RequestCacheEnabled =true 就是启用缓存
```

<br/>
怎么拦截获取缓存
 <br/>
在业务接口方法上添加如下特性
 <br/>
 
```C#  
 [InterceptMethod(CachingMethod.Get, Key = "GetUser_id_{0}", Mode = CacheTargetType.Redis, Time = 480)]
```
    
<br/>
怎么拦截删除缓存
 <br/>
在业务接口方法上添加如下特性
 <br/>
 
```C#  
  [InterceptMethod(CachingMethod.Remove, "GetUser_id_{0}", "GetUserName_name_{0}", Mode = CacheTargetType.Redis)]
```
      
<br/>
怎么添加缓存KEY
   <br/>
在业务模型属性上添加，如下特性，可以支持多个
   <br/>
   
```C# 
[CacheKey(1)]
```
        
<br/>
配置拦截器
<br/>
   
```C# 
 .AddClientIntercepted(typeof(CacheProviderInterceptor))
```

IDE:Visual Studio 2017 15.3 Preview ,vscode
<br/>
框架：.NET core 2.0
<br/>
如有任何问题可以加入QQ群：615562965
<br/>
[博客园]:https://www.cnblogs.com/fanliang11
