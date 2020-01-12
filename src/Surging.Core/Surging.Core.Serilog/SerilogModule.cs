using Microsoft.Extensions.Logging;
using Serilog;
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
            var section = AppConfig.GetSection("Serilog");

            var logger = new LoggerConfiguration().ReadFrom.Configuration(section)
                //.WriteTo.RollingFile(new ElasticsearchJsonFormatter(renderMessageTemplate:false),"c:/logs/log-{Date}.log")
                //.WriteTo.Logger(config => { 
                //    config.Filter.ByIncludingOnly(evt=>evt.Level== Serilog.Events.LogEventLevel.Information).WriteTo.RollingFile()
                //})
                .CreateLogger();
            
            serviceProvider.GetInstances<ILoggerFactory>().AddSerilog(logger);
            serviceProvider.GetInstances<IApplicationLifetime>().ApplicationStopped.Register(Log.CloseAndFlush);
        }

        //public override void Dispose()
        //{
        //    //Log.CloseAndFlush();
        //    base.Dispose();
        //}
    }
}
