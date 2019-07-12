using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Surging.Core.KestrelHttpServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Surging.Core.Stage.Internal.Implementation
{
    /// <summary>
    /// Defines the <see cref="WebServerListener" />
    /// </summary>
    public class WebServerListener : IWebServerListener
    {
        #region 字段

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<WebServerListener> _logger;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServerListener"/> class.
        /// </summary>
        /// <param name="logger">The logger<see cref="ILogger{WebServerListener}"/></param>
        public WebServerListener(ILogger<WebServerListener> logger)
        {
            _logger = logger;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The GetPaths
        /// </summary>
        /// <param name="virtualPaths">The virtualPaths<see cref="string[]"/></param>
        /// <returns>The <see cref="string[]"/></returns>
        private string[] GetPaths(params string[] virtualPaths)
        {
            var result = new List<string>();
            string rootPath = string.IsNullOrEmpty(CPlatform.AppConfig.ServerOptions.RootPath) ?
                AppContext.BaseDirectory : CPlatform.AppConfig.ServerOptions.RootPath;
            foreach (var virtualPath in virtualPaths)
            {
                var path = Path.Combine(rootPath, virtualPath);
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"准备查找路径{path}下的证书。");
                if (Directory.Exists(path))
                {
                    var dirs = Directory.GetDirectories(path);
                    result.AddRange(dirs.Select(dir => Path.Combine(rootPath, virtualPath, new DirectoryInfo(dir).Name)));
                }
                else
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug($"未找到路径：{path}。");
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// The Listen
        /// </summary>
        /// <param name="context">The context<see cref="WebHostContext"/></param>
        void IWebServerListener.Listen(WebHostContext context)
        {
            var httpsPorts = AppConfig.Options.HttpsPort?.Split(",") ?? new string[] { "443" };
            var httpPorts = AppConfig.Options.HttpPorts?.Split(",");
            if (AppConfig.Options.EnableHttps)
            {
                foreach (var httpsPort in httpsPorts)
                {
                    int.TryParse(httpsPort, out int port);
                    if (string.IsNullOrEmpty(AppConfig.Options.HttpsPort)) port = 443;
                    if (port > 0)
                    {
                        context.KestrelOptions.Listen(context.Address, port, listOptions =>
                    {
                        X509Certificate2 certificate2 = null;
                        var fileName = AppConfig.Options.CertificateFileName;
                        var password = AppConfig.Options.CertificatePassword;
                        if (fileName != null && password != null)
                        {
                            var pfxFile = Path.Combine(AppContext.BaseDirectory, AppConfig.Options.CertificateFileName);
                            if (File.Exists(pfxFile))
                                certificate2 = new X509Certificate2(pfxFile, AppConfig.Options.CertificatePassword);
                            else
                            {
                                var paths = GetPaths(AppConfig.Options.CertificateLocation);
                                foreach (var path in paths)
                                {
                                    pfxFile = Path.Combine(path, AppConfig.Options.CertificateFileName);
                                    if (File.Exists(pfxFile))
                                        certificate2 = new X509Certificate2(pfxFile, AppConfig.Options.CertificatePassword);
                                }
                            }
                        }
                        listOptions = certificate2 == null ? listOptions.UseHttps() : listOptions.UseHttps(certificate2);
                    });
                    }
                }
            }

            if (httpPorts != null)
            {
                foreach (var httpPort in httpPorts)
                {
                    int.TryParse(httpPort, out int port);
                    if (port > 0)
                        context.KestrelOptions.Listen(context.Address, port);
                }
            }
        }

        #endregion 方法
    }
}