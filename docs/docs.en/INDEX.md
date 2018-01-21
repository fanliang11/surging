surging is based on .net core language high-performance distributed microservices framework,It can be deployed in clusters without complex operations
It was created by individuals for building microservices, which are deployed in the cloud and subsequently move the code to dotnetcore

Code Examples
=============

Create an interface that inherits IServiceKey:
```c#
[ServiceBundle("api/{Service}")]
public interface IUserService :Surging.Core.CPlatform.Ioc.IServiceKey
{
  Task<string> SayHello(string username);
}
```

To provide the implementation of this interface, you must inherit ServiceBase or ProxyServiceBase:
```c#
[ModuleName("User")]
public class UserService : ProxyServiceBase, IUserService
{
    public UserService(UserRepository repository)
    {
        this._repository = repository;
    }
	
    Task<string> SayHello(string username)
    {
	return Task.FromResult($"'{username}', Hello!");
    }
}
```

Dependency injection Repository, you must inherit BaseRepository:
```c#
 public class UserRepository: BaseRepository
{
}
```

Call in the following way:
```c#
// Get  a reference to the IUserService with instance ID "user".
var user = ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy<IUserService>("User");

// Send request and await the response.
Console.WriteLine(await user.SayHello("fanly"));
```

## Next steps

* [How to configure microservices](https://github.com/dotnetcore/surging/blob/master/docs/docs.en/ConfigMicroservices.md)
* [How to Dependency injection](https://github.com/dotnetcore/surging/blob/master/docs/docs.en/DependencyInjection.md)
* [How to use the cache]()
* [How to set service fuse protection]()
* [How to set up gateway access]()
