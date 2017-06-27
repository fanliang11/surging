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
AddRabbitMqAdapt()//基于rabbitmq的消费的服务适配<br/>
增加订阅功能：<br/>
` ``C#
 ServiceLocator.GetService<<ISubscriptionAdapt>>().SubscribeAt();
 ` ``
 <br/>
<br/>
IDE:Visual Studio 2017
<br/>
框架：.NET core 1.1
<br/>
如有任何问题可以加入QQ群：615562965

