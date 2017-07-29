using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Support.Attributes;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Surging.Core.CPlatform.Support.Implementation
{
    public class ServiceCommandProvider : ServiceCommandBase
    {
        private readonly IServiceEntryManager _serviceEntryManager;
        private readonly ConcurrentDictionary<string, ServiceCommand> _serviceCommand = new ConcurrentDictionary<string, ServiceCommand>();

        public ServiceCommandProvider(IServiceEntryManager serviceEntryManager)
        {
            _serviceEntryManager = serviceEntryManager;
        }
        
        public override ServiceCommand GetCommand(string serviceId)
        {
            var result = _serviceCommand.GetValueOrDefault(serviceId);
            if (result == null)
            {
                var command = (from q in _serviceEntryManager.GetEntries()
                               let k = q.Attributes
                               where k.OfType<CommandAttribute>().Count() > 0 && q.Descriptor.Id==serviceId
                               select k.OfType<CommandAttribute>().FirstOrDefault()).FirstOrDefault();
                result = ConvertServiceCommand(command);
                _serviceCommand.AddOrUpdate(serviceId, result, (s, r) => result);
            }
            return result;
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
    }
}
