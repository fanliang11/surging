using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks;
using Surging.Core.CPlatform.Utilities; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ApiGateWay.ServiceDiscovery.Implementation
{
    /// <summary>
    /// 服务注册
    /// </summary>
    public class ServiceRegisterProvider : IServiceRegisterProvider
    {
        public async  Task<IEnumerable<ServiceAddressModel>> GetAddressAsync(string condition = null)
        {
            var result = new List<ServiceAddressModel>();
            var registerConfig = AppConfig.Register;
            var addresses = registerConfig.Address.Split(",");
            if (addresses.Length > 1)
            {
                foreach (var address in addresses)
                {

                    var addr = ConvertAddressModel(address);
                    result.Add(new ServiceAddressModel
                    {
                        Address = addr,
                        IsHealth = await ServiceLocator.GetService<IHealthCheckService>().IsHealth(addr)
                    });

                }
            }
            else
            {
                var address = ConvertAddressModel(registerConfig.Address);
                if (address != null)
                {
                    var ipAddress = address as IpAddressModel;

                    result.Add(new ServiceAddressModel
                    {
                        Address = ipAddress,
                        IsHealth = await ServiceLocator.GetService<IHealthCheckService>().IsHealth(ipAddress)
                    });
                }
            }
            return result;
        }

        public AddressModel ConvertAddressModel(string connection)
        {
            var address = connection.Split(":");
            if (address.Length > 1)
            {
                int port;
                int.TryParse(address[1], out port);
                return new IpAddressModel(address[0], port);
            }
            return null;
        }
    }
}
