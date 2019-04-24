using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Support.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Support.Implementation
{
    public class ServiceCommandProvider : ServiceCommandBase
    {
        private readonly IServiceEntryManager _serviceEntryManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<string, ServiceCommand> _serviceCommand = new ConcurrentDictionary<string, ServiceCommand>();

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

        public override async  ValueTask<ServiceCommand> GetCommand(string serviceId)
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

        private void ServiceCommandManager_Removed(object sender, ServiceCommandEventArgs e)
        {
            ServiceCommand value;
            _serviceCommand.TryRemove(e.Command.ServiceId, out value);
        }

        public void ServiceCommandManager_Add(object sender, ServiceCommandEventArgs e)
        { 
            _serviceCommand.GetOrAdd(e.Command.ServiceId, e.Command);
        }

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
    }
}
