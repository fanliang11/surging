using Microsoft.Extensions.Logging;
using RulesEngine.Models;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.ProxyGenerator;
using Surging.Core.ServiceHosting.Extensions.Rules;
using Surging.Core.ServiceHosting.Extensions.Runtime;
using Surging.IModuleServices.Common;
using Surging.IModuleServices.Common.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Modules.Common.Domain
{
    public class WorkService : BackgroundServiceBehavior, IWorkService, ISingleInstance
    {
        private readonly ILogger<WorkService> _logger;
        private readonly Queue<Tuple<Message, RulesEngine.RulesEngine, SchedulerRuleWorkflow>> _queue = new Queue<Tuple<Message, RulesEngine.RulesEngine, SchedulerRuleWorkflow>>();
        private readonly ConcurrentDictionary<string, DateTime> _keyValuePairs = new ConcurrentDictionary<string, DateTime>();
        private readonly IServiceProxyProvider _serviceProxyProvider;
        private AtomicLong _atomic=new AtomicLong(1);
        private const int EXECSIZE = 1;
        private CancellationToken _token;

        public WorkService(ILogger<WorkService> logger, IServiceProxyProvider serviceProxyProvider)
        {
            _logger = logger;
            _serviceProxyProvider = serviceProxyProvider;
            /*   var script = @"parser
                               .Weekdays().SecondAt(3).Between(""8:00"", ""22:00"")";*/
            var script = @"parser
                              .TimeZone(""utc"")
                               .When(
                              function(lastExecTime){
                return DateUtils.IsToday(lastExecTime);
            }).Skip(
             function(lastExecTime){
                return DateUtils.IsWeekend(lastExecTime);
            }).Weekdays().SecondAt(3).Between(""8:00"", ""23:30"")";
            var ruleWorkflow = GetSchedulerRuleWorkflow(script);
            var messageId = Guid.NewGuid().ToString();
            _keyValuePairs.AddOrUpdate(messageId, DateTime.Now, (key, value) => DateTime.Now);
            _queue.Enqueue(new Tuple<Message, RulesEngine.RulesEngine, SchedulerRuleWorkflow>(new Message() { MessageId= messageId,Config=new SchedulerConfig() {  IsPersistence=true} }, GetRuleEngine(ruleWorkflow), ruleWorkflow));

        }

        public  Task<bool> AddWork(Message message)
        {
            var ruleWorkflow = GetSchedulerRuleWorkflow(message.Config.Script);
            _keyValuePairs.AddOrUpdate(message.MessageId, DateTime.Now, (key, value) => DateTime.Now);
            _queue.Enqueue(new Tuple<Message, RulesEngine.RulesEngine, SchedulerRuleWorkflow>(message, GetRuleEngine(ruleWorkflow), ruleWorkflow));
            return Task.FromResult(true);
        }

        protected override async  Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _token = stoppingToken;
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now); 
                _queue.TryDequeue(out Tuple<Message, RulesEngine.RulesEngine, SchedulerRuleWorkflow>? message);
                if (message != null)
                {
                    var parser = await GetParser(message.Item3, message.Item2);
                    await PayloadSubscribe(parser, message.Item1, message.Item2, message.Item3);
                    _keyValuePairs.TryGetValue(message.Item1.MessageId, out DateTime dateTime);
                    parser.Build(dateTime == DateTime.MinValue ? DateTime.Now : dateTime);
                }
                if (!_token.IsCancellationRequested && (message == null || _atomic.GetAndAdd(1) == EXECSIZE))
                {
                    _atomic = new AtomicLong(1);
                    await Task.Delay(1000, stoppingToken);

                }
            }
            catch (Exception ex){
                _logger.LogError("WorkService execute error, message：{message} ,trace info:{trace} ", ex.Message, ex.StackTrace);
            }
        }

        public async Task StartAsync()
        {
            if (_token.IsCancellationRequested)
            { 
                await base.StartAsync(_token);
            }
        }

        public async Task StopAsync()
        {
            if (!_token.IsCancellationRequested)
            {
               await  base.StopAsync(_token);
            }
        }

        private async Task PayloadSubscribe(RulePipePayloadParser parser, Message message, RulesEngine.RulesEngine rulesEngine, SchedulerRuleWorkflow ruleWorkflow)
        {
            parser.HandlePayload().Subscribe(async (temperature) =>
            {
                try
                {
                    if (temperature)
                    {
                       await  ExecuteByPlanAsyn(message);
                        _logger.LogInformation("Worker exec at: {time}", DateTimeOffset.Now);

                    }
                }
                catch (Exception ex) { }
                finally
                {
                    if (message.Config.IsPersistence || (!temperature && !message.Config.IsPersistence))
                        _queue.Enqueue(new Tuple<Message, RulesEngine.RulesEngine, SchedulerRuleWorkflow>(message, rulesEngine, ruleWorkflow));

                }
            });
        }

        private async Task<bool> ExecuteByPlanAsyn(Message message)
        {
            var result = false;
            var isExec = true;
            try
            {
                if (!string.IsNullOrEmpty(message.RoutePath))
                {
                    var serviceResult = await _serviceProxyProvider.Invoke<object>(message.Parameters, message.RoutePath, message.ServiceKey);
                    bool.TryParse(serviceResult?.ToString(), out result);
                    isExec = true;
                }
            }
            catch { }
            finally
            {
                if (isExec && message.Config.IsPersistence)
                    _keyValuePairs.AddOrUpdate(message.MessageId, DateTime.Now, (key, value) => DateTime.Now);
                else if (!message.Config.IsPersistence)
                    _keyValuePairs.TryRemove(message.MessageId, out DateTime dateTime);
            }
            return result;
        }

        private async Task<RulePipePayloadParser> GetParser(SchedulerRuleWorkflow ruleWorkflow, RulesEngine.RulesEngine engine)
        {
            var payloadParser = new RulePipePayloadParser();
            var ruleResult = await engine.ExecuteActionWorkflowAsync(ruleWorkflow.WorkflowName, ruleWorkflow.RuleName, new RuleParameter[] { new RuleParameter("parser", payloadParser) });
            if (ruleResult.Exception != null && _logger.IsEnabled(LogLevel.Error))
                _logger.LogError(ruleResult.Exception, ruleResult.Exception.Message);
            return payloadParser;
        }

        private RulesEngine.RulesEngine GetRuleEngine(SchedulerRuleWorkflow ruleWorkFlow)
        {
            var reSettingsWithCustomTypes = new ReSettings { CustomTypes = new Type[] { typeof(RulePipePayloadParser) } };
            var result = new RulesEngine.RulesEngine(new Workflow[] { ruleWorkFlow.GetWorkflow() }, null, reSettingsWithCustomTypes);
            return result;
        }

        private SchedulerRuleWorkflow GetSchedulerRuleWorkflow(string script)
        {
            var result = new SchedulerRuleWorkflow("1==1");
            if (!string.IsNullOrEmpty(script))
            {
                result = new SchedulerRuleWorkflow(script);
            }
            return result;
        }
    }
}
