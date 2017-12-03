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
  Task<string> SayHello(string username)
  {
    return Task.FromResult($"'{username}', Hello!");
  }
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
