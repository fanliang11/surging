//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Text;
//using Serilog;
//using Serilog.Core;
//using Microsoft.Extensions.Configuration;

//namespace Surging.Core.Serilog
//{
//    public class SerilogProvider : ILoggerProvider
//    {
//        private readonly ConcurrentDictionary<string, Logger> _loggers = new ConcurrentDictionary<string, Logger>();
//        private readonly IConfiguration _config;

//        public SerilogProvider(IConfiguration configuration)
//        {
//            _config = configuration;
//        }

//        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
//        {
//            var log = new LoggerConfiguration()
//                .ReadFrom.Configuration(_config)
//                .CreateLogger();
//            throw new NotImplementedException();
//        }

//        public void Dispose()
//        {
//            _loggers.Clear();
//        }
//    }
//}
