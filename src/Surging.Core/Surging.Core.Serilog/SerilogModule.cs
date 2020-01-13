using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Elasticsearch;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Module;
using Surging.Core.ServiceHosting.Internal;

namespace Surging.Core.Serilog
{
    public class SerilogModule: EnginePartModule
    {
        public override void Initialize(AppModuleContext context)
        {
            var serviceProvider = context.ServiceProvoider;
            base.Initialize(context);

            var logger = new LoggerConfiguration().ReadFrom.Configuration(AppConfig.Configuration)
                //.WriteTo.RollingFile(new ElasticsearchJsonFormatter(renderMessageTemplate:false),"c:/logs/log-{Date}.log")
                //.WriteTo.Logger(config =>
                //{
                //    //config.Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Information).ReadFrom.Configuration(AppConfig.Configuration.GetSection("Information"))
                //})
                .CreateLogger();
            
            serviceProvider.GetInstances<ILoggerFactory>().AddSerilog(logger);
            serviceProvider.GetInstances<IApplicationLifetime>().ApplicationStopped.Register(Log.CloseAndFlush);
        }
    }
}
