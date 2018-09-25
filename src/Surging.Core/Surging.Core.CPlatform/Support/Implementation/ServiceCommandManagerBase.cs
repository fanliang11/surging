using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Support.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Support.Implementation
{

    /// <summary>
    /// 服务命令事件参数。
    /// </summary>
    public class ServiceCommandEventArgs
    {
        public ServiceCommandEventArgs(ServiceCommandDescriptor serviceCommand)
        {
            Command = serviceCommand;
        }

        /// <summary>
        /// 服务命令信息。
        /// </summary>
        public ServiceCommandDescriptor Command { get; private set; }
    }

    /// <summary>
    /// 服务命令变更事件参数。
    /// </summary>
    public class ServiceCommandChangedEventArgs : ServiceCommandEventArgs
    {
        public ServiceCommandChangedEventArgs(ServiceCommandDescriptor serviceCommand, ServiceCommandDescriptor oldServiceCommand) : base(serviceCommand)
        {
            OldServiceCommand = oldServiceCommand;
        }

        /// <summary>
        /// 旧的服务命令信息。
        /// </summary>
        public ServiceCommandDescriptor OldServiceCommand { get; set; }
    }

    public abstract class ServiceCommandManagerBase : IServiceCommandManager
    {
        private readonly ISerializer<string> _serializer;
        private readonly IServiceEntryManager _serviceEntryManager;
        private EventHandler<ServiceCommandEventArgs> _created;
        private EventHandler<ServiceCommandEventArgs> _removed;
        private EventHandler<ServiceCommandChangedEventArgs> _changed;
        protected ServiceCommandManagerBase(ISerializer<string> serializer, IServiceEntryManager serviceEntryManager)
        {
            _serializer = serializer;
            _serviceEntryManager = serviceEntryManager;
        }

        #region Implementation of IServiceRouteManager

        /// <summary>
        /// 服务命令被创建。
        /// </summary>
        public event EventHandler<ServiceCommandEventArgs> Created
        {
            add { _created += value; }
            remove { _created -= value; }
        }

        /// <summary>
        /// 服务命令被删除。
        /// </summary>
        public event EventHandler<ServiceCommandEventArgs> Removed
        {
            add { _removed += value; }
            remove { _removed -= value; }
        }

        /// <summary>
        /// 服务命令被修改。
        /// </summary>
        public event EventHandler<ServiceCommandChangedEventArgs> Changed
        {
            add { _changed += value; }
            remove { _changed -= value; }
        }

        /// <summary>
        /// 获取所有可用的服务命令信息。
        /// </summary>
        /// <returns>服务命令集合。</returns>
        public abstract Task<IEnumerable<ServiceCommandDescriptor>> GetServiceCommandsAsync();

        protected abstract Task InitServiceCommandsAsync(IEnumerable<ServiceCommandDescriptor> routes);


        public virtual async Task SetServiceCommandsAsync()
        {
            List<ServiceCommandDescriptor> serviceCommands = new List<ServiceCommandDescriptor>();
            await Task.Run(() =>
            {
                var commands = (from q in _serviceEntryManager.GetEntries()
                                let k = q.Attributes
                                select new { ServiceId = q.Descriptor.Id, Command = k.OfType<CommandAttribute>().FirstOrDefault() }).ToList();
                commands.ForEach(command => serviceCommands.Add(ConvertServiceCommand(command.ServiceId, command.Command)));
                InitServiceCommandsAsync(serviceCommands);
            });
        }

        /// <summary>
        /// 清空所有的服务命令。
        /// </summary>
        /// <returns>一个任务。</returns>
        public abstract Task ClearAsync();

        #endregion Implementation of IServiceRouteManager

        /// <summary>
        /// 设置服务命令。
        /// </summary>
        /// <param name="routes">服务命令集合。</param>
        /// <returns>一个任务。</returns>
        public abstract Task SetServiceCommandsAsync(IEnumerable<ServiceCommandDescriptor> routes);

        protected void OnCreated(params ServiceCommandEventArgs[] args)
        {
            if (_created == null)
                return;

            foreach (var arg in args)
                _created(this, arg);
        }

        protected void OnChanged(params ServiceCommandChangedEventArgs[] args)
        {
            if (_changed == null)
                return;

            foreach (var arg in args)
                _changed(this, arg);
        }

        protected void OnRemoved(params ServiceCommandEventArgs[] args)
        {
            if (_removed == null)
                return;

            foreach (var arg in args)
                _removed(this, arg);
        }

        private ServiceCommandDescriptor ConvertServiceCommand(string serviceId, CommandAttribute command)
        {
            var result = new ServiceCommandDescriptor() { ServiceId = serviceId };
            if (command != null)
            {
                result = new ServiceCommandDescriptor
                {
                    ServiceId = serviceId,
                    CircuitBreakerForceOpen = command.CircuitBreakerForceOpen,
                    ExecutionTimeoutInMilliseconds = command.ExecutionTimeoutInMilliseconds,
                    FailoverCluster = command.FailoverCluster,
                    Injection = command.Injection,
                    RequestCacheEnabled = command.RequestCacheEnabled,
                    Strategy = command.Strategy,
                    ShuntStrategy=command.ShuntStrategy,
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