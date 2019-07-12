using Microsoft.Extensions.DependencyInjection;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Support.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Support.Implementation
{
    /// <summary>
    /// Defines the <see cref="ServiceCommandProvider" />
    /// </summary>
    public class ServiceCommandProvider : ServiceCommandBase
    {
        #region 字段

        /// <summary>
        /// Defines the _serviceCommand
        /// </summary>
        private readonly ConcurrentDictionary<string, ServiceCommand> _serviceCommand = new ConcurrentDictionary<string, ServiceCommand>();

        /// <summary>
        /// Defines the _serviceEntryManager
        /// </summary>
        private readonly IServiceEntryManager _serviceEntryManager;

        /// <summary>
        /// Defines the _serviceProvider
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceCommandProvider"/> class.
        /// </summary>
        /// <param name="serviceEntryManager">The serviceEntryManager<see cref="IServiceEntryManager"/></param>
        /// <param name="serviceProvider">The serviceProvider<see cref="IServiceProvider"/></param>
        public ServiceCommandProvider(IServiceEntryManager serviceEntryManager, IServiceProvider serviceProvider)
        {
            _serviceEntryManager = serviceEntryManager;
            _serviceProvider = serviceProvider;
            var manager = serviceProvider.GetService<IServiceCommandManager>();
            if (manager != null)
            {
                manager.Changed += ServiceCommandManager_Removed;
                manager.Removed += ServiceCommandManager_Removed;
                manager.Created += ServiceCommandManager_Add;
            }
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The ConvertServiceCommand
        /// </summary>
        /// <param name="command">The command<see cref="CommandAttribute"/></param>
        /// <returns>The <see cref="ServiceCommand"/></returns>
        public ServiceCommand ConvertServiceCommand(CommandAttribute command)
        {
            var result = new ServiceCommand();
            if (command != null)
            {
                result = new ServiceCommand
                {
                    CircuitBreakerForceOpen = command.CircuitBreakerForceOpen,
                    ExecutionTimeoutInMilliseconds = command.ExecutionTimeoutInMilliseconds,
                    FailoverCluster = command.FailoverCluster,
                    Injection = command.Injection,
                    ShuntStrategy = command.ShuntStrategy,
                    RequestCacheEnabled = command.RequestCacheEnabled,
                    Strategy = command.Strategy,
                    InjectionNamespaces = command.InjectionNamespaces,
                    BreakeErrorThresholdPercentage = command.BreakeErrorThresholdPercentage,
                    BreakerForceClosed = command.BreakerForceClosed,
                    BreakerRequestVolumeThreshold = command.BreakerRequestVolumeThreshold,
                    BreakeSleepWindowInMilliseconds = command.BreakeSleepWindowInMilliseconds,
                    MaxConcurrentRequests = command.MaxConcurrentRequests
                };
            }
            return result;
        }

        /// <summary>
        /// The ConvertServiceCommand
        /// </summary>
        /// <param name="command">The command<see cref="ServiceCommandDescriptor"/></param>
        /// <returns>The <see cref="ServiceCommand"/></returns>
        public ServiceCommand ConvertServiceCommand(ServiceCommandDescriptor command)
        {
            var result = new ServiceCommand();
            if (command != null)
            {
                result = new ServiceCommand
                {
                    CircuitBreakerForceOpen = command.CircuitBreakerForceOpen,
                    ExecutionTimeoutInMilliseconds = command.ExecutionTimeoutInMilliseconds,
                    FailoverCluster = command.FailoverCluster,
                    Injection = command.Injection,
                    RequestCacheEnabled = command.RequestCacheEnabled,
                    Strategy = command.Strategy,
                    ShuntStrategy = command.ShuntStrategy,
                    InjectionNamespaces = command.InjectionNamespaces,
                    BreakeErrorThresholdPercentage = command.BreakeErrorThresholdPercentage,
                    BreakerForceClosed = command.BreakerForceClosed,
                    BreakerRequestVolumeThreshold = command.BreakerRequestVolumeThreshold,
                    BreakeSleepWindowInMilliseconds = command.BreakeSleepWindowInMilliseconds,
                    MaxConcurrentRequests = command.MaxConcurrentRequests
                };
            }
            return result;
        }

        /// <summary>
        /// The GetCommand
        /// </summary>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <returns>The <see cref="ValueTask{ServiceCommand}"/></returns>
        public override async ValueTask<ServiceCommand> GetCommand(string serviceId)
        {
            var result = _serviceCommand.GetValueOrDefault(serviceId);
            if (result == null)
            {
                var task = GetCommandAsync(serviceId);
                return task.IsCompletedSuccessfully ? task.Result : await task;
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        /// The GetCommandAsync
        /// </summary>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <returns>The <see cref="Task{ServiceCommand}"/></returns>
        public async Task<ServiceCommand> GetCommandAsync(string serviceId)
        {
            var result = new ServiceCommand();
            var manager = _serviceProvider.GetService<IServiceCommandManager>();
            if (manager == null)
            {
                var command = (from q in _serviceEntryManager.GetEntries()
                               let k = q.Attributes
                               where k.OfType<CommandAttribute>().Count() > 0 && q.Descriptor.Id == serviceId
                               select k.OfType<CommandAttribute>().FirstOrDefault()).FirstOrDefault();
                result = ConvertServiceCommand(command);
            }
            else
            {
                var commands = await manager.GetServiceCommandsAsync();
                result = ConvertServiceCommand(commands.Where(p => p.ServiceId == serviceId).FirstOrDefault());
            }
            _serviceCommand.AddOrUpdate(serviceId, result, (s, r) => result);
            return result;
        }

        /// <summary>
        /// The ServiceCommandManager_Add
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="ServiceCommandEventArgs"/></param>
        public void ServiceCommandManager_Add(object sender, ServiceCommandEventArgs e)
        {
            _serviceCommand.GetOrAdd(e.Command.ServiceId, e.Command);
        }

        /// <summary>
        /// The ServiceCommandManager_Removed
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="ServiceCommandEventArgs"/></param>
        private void ServiceCommandManager_Removed(object sender, ServiceCommandEventArgs e)
        {
            ServiceCommand value;
            _serviceCommand.TryRemove(e.Command.ServiceId, out value);
        }

        #endregion 方法
    }
}