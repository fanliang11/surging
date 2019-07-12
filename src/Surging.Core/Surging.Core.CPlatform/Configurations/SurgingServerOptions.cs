using Surging.Core.CPlatform.Support;
using System;
using System.Collections.Generic;
using System.Net;

namespace Surging.Core.CPlatform.Configurations
{
    /// <summary>
    /// Defines the <see cref="SurgingServerOptions" />
    /// </summary>
    public partial class SurgingServerOptions : ServiceCommand
    {
        #region 属性

        /// <summary>
        /// Gets or sets the DisconnTimeInterval
        /// </summary>
        public int DisconnTimeInterval { get; set; } = 60;

        /// <summary>
        /// Gets or sets a value indicating whether EnableRouteWatch
        /// </summary>
        public bool EnableRouteWatch { get; set; }

        /// <summary>
        /// Gets or sets the Ip
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// Gets or sets the IpEndpoint
        /// </summary>
        public IPEndPoint IpEndpoint { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsModulePerLifetimeScope
        /// </summary>
        public bool IsModulePerLifetimeScope { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Libuv
        /// </summary>
        public bool Libuv { get; set; } = false;

        /// <summary>
        /// Gets or sets the MappingIP
        /// </summary>
        public string MappingIP { get; set; }

        /// <summary>
        /// Gets or sets the MappingPort
        /// </summary>
        public int MappingPort { get; set; }

        /// <summary>
        /// Gets or sets the NotRelatedAssemblyFiles
        /// </summary>
        public string NotRelatedAssemblyFiles { get; set; }

        /// <summary>
        /// Gets or sets the Packages
        /// </summary>
        public List<ModulePackage> Packages { get; set; } = new List<ModulePackage>();

        /// <summary>
        /// Gets or sets the Port
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the Ports
        /// </summary>
        public ProtocolPortOptions Ports { get; set; } = new ProtocolPortOptions();

        /// <summary>
        /// Gets or sets the Protocol
        /// </summary>
        public CommunicationProtocol Protocol { get; set; }

        /// <summary>
        /// Gets or sets the RelatedAssemblyFiles
        /// </summary>
        public string RelatedAssemblyFiles { get; set; } = "";

        /// <summary>
        /// Gets or sets a value indicating whether ReloadOnChange
        /// </summary>
        public bool ReloadOnChange { get; set; } = false;

        /// <summary>
        /// Gets or sets the RootPath
        /// </summary>
        public string RootPath { get; set; }

        /// <summary>
        /// Gets or sets the SoBacklog
        /// </summary>
        public int SoBacklog { get; set; } = 8192;

        /// <summary>
        /// Gets or sets the Token
        /// </summary>
        public string Token { get; set; } = "True";

        /// <summary>
        /// Gets or sets the WanIp
        /// </summary>
        public string WanIp { get; set; }

        /// <summary>
        /// Gets or sets the WatchInterval
        /// </summary>
        public double WatchInterval { get; set; } = 20d;

        /// <summary>
        /// Gets or sets the WebRootPath
        /// </summary>
        public string WebRootPath { get; set; } = AppContext.BaseDirectory;

        #endregion 属性
    }
}