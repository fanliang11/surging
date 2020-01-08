using Surging.Apm.Skywalking.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Surging.Core.CPlatform.Utilities;

namespace Surging.Apm.Skywalking.Core.Common
{
    internal class HostingEnvironmentProvider : IEnvironmentProvider
    {
        public string EnvironmentName { get; }

        public HostingEnvironmentProvider()
        {
            EnvironmentName ="";
        }
    }
}
