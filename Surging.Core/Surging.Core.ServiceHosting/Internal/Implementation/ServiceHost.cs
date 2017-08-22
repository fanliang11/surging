using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Surging.Core.ServiceHosting.Startup;

namespace Surging.Core.ServiceHosting.Internal.Implementation
{
    public class ServiceHost : IServiceHost
    {
        private readonly ContainerBuilder _builder;
        private IStartup _startup;
        private IContainer _applicationServices;
        private readonly IServiceProvider _hostingServiceProvider;

        public ServiceHost(ContainerBuilder builder,
            IServiceProvider hostingServiceProvider)
        {
            _builder = builder;
            _hostingServiceProvider = hostingServiceProvider;
        }

        public void Dispose()
        {

        }

        public void Initialize()
        {
            if (_applicationServices == null)
            {
                _applicationServices = BuildApplication();
            }
        }

        private void EnsureApplicationServices()
        {
            if (_applicationServices == null)
            {
                EnsureStartup();
                _applicationServices = _startup.ConfigureServices(_builder);
            }
        }

        private void EnsureStartup()
        {
            if (_startup != null)
            {
                return;
            }

            _startup = _hostingServiceProvider.GetRequiredService<IStartup>();
        }

        private IContainer BuildApplication()
        {
            try
            {
                EnsureApplicationServices();
                Action<IContainer> configure = _startup.Configure;
                if (_applicationServices == null)
                    _applicationServices = _builder.Build();
                configure(_applicationServices);
                return _applicationServices;
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("应用程序启动异常: " + ex.ToString());
                throw;
            }
        }
    }
}
