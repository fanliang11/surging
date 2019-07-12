using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Surging.Core.ServiceHosting.Startup;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.ServiceHosting.Internal.Implementation
{
    /// <summary>
    /// Defines the <see cref="ServiceHost" />
    /// </summary>
    public class ServiceHost : IServiceHost
    {
        #region 字段

        /// <summary>
        /// Defines the _builder
        /// </summary>
        private readonly ContainerBuilder _builder;

        /// <summary>
        /// Defines the _hostingServiceProvider
        /// </summary>
        private readonly IServiceProvider _hostingServiceProvider;

        /// <summary>
        /// Defines the _hostLifetime
        /// </summary>
        private readonly IHostLifetime _hostLifetime;

        /// <summary>
        /// Defines the _mapServicesDelegates
        /// </summary>
        private readonly List<Action<IContainer>> _mapServicesDelegates;

        /// <summary>
        /// Defines the _applicationLifetime
        /// </summary>
        private IApplicationLifetime _applicationLifetime;

        /// <summary>
        /// Defines the _applicationServices
        /// </summary>
        private IContainer _applicationServices;

        /// <summary>
        /// Defines the _startup
        /// </summary>
        private IStartup _startup;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceHost"/> class.
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilder"/></param>
        /// <param name="hostingServiceProvider">The hostingServiceProvider<see cref="IServiceProvider"/></param>
        /// <param name="hostLifetime">The hostLifetime<see cref="IHostLifetime"/></param>
        /// <param name="mapServicesDelegate">The mapServicesDelegate<see cref="List{Action{IContainer}}"/></param>
        public ServiceHost(ContainerBuilder builder,
            IServiceProvider hostingServiceProvider,
            IHostLifetime hostLifetime,
             List<Action<IContainer>> mapServicesDelegate)
        {
            _builder = builder;
            _hostingServiceProvider = hostingServiceProvider;
            _hostLifetime = hostLifetime;
            _mapServicesDelegates = mapServicesDelegate;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Dispose
        /// </summary>
        public void Dispose()
        {
            (_hostingServiceProvider as IDisposable)?.Dispose();
        }

        /// <summary>
        /// The Initialize
        /// </summary>
        /// <returns>The <see cref="IContainer"/></returns>
        public IContainer Initialize()
        {
            if (_applicationServices == null)
            {
                _applicationServices = BuildApplication();
            }
            return _applicationServices;
        }

        /// <summary>
        /// The Run
        /// </summary>
        /// <returns>The <see cref="IDisposable"/></returns>
        public IDisposable Run()
        {
            RunAsync().GetAwaiter().GetResult();
            return this;
        }

        /// <summary>
        /// The RunAsync
        /// </summary>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task RunAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_applicationServices != null)
                MapperServices(_applicationServices);

            if (_hostLifetime != null)
            {
                _applicationLifetime = _hostingServiceProvider.GetService<IApplicationLifetime>();
                await _hostLifetime.WaitForStartAsync(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                _applicationLifetime?.NotifyStarted();
            }
        }

        /// <summary>
        /// The StopAsync
        /// </summary>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var cts = new CancellationTokenSource(2000))
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken))
            {
                var token = linkedCts.Token;
                _applicationLifetime?.StopApplication();
                token.ThrowIfCancellationRequested();
                await _hostLifetime.StopAsync(token);
                _applicationLifetime?.NotifyStopped();
            }
        }

        /// <summary>
        /// The BuildApplication
        /// </summary>
        /// <returns>The <see cref="IContainer"/></returns>
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

        /// <summary>
        /// The EnsureApplicationServices
        /// </summary>
        private void EnsureApplicationServices()
        {
            if (_applicationServices == null)
            {
                EnsureStartup();
                _applicationServices = _startup.ConfigureServices(_builder);
            }
        }

        /// <summary>
        /// The EnsureStartup
        /// </summary>
        private void EnsureStartup()
        {
            if (_startup != null)
            {
                return;
            }

            _startup = _hostingServiceProvider.GetRequiredService<IStartup>();
        }

        /// <summary>
        /// The MapperServices
        /// </summary>
        /// <param name="mapper">The mapper<see cref="IContainer"/></param>
        private void MapperServices(IContainer mapper)
        {
            foreach (var mapServices in _mapServicesDelegates)
            {
                mapServices(mapper);
            }
        }

        #endregion 方法
    }
}